using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.Profile;
using United.Definition;
using United.Mobile.DataAccess.Common;
using United.Mobile.Model.Booking;
using United.Mobile.Model.Common;
using United.Mobile.Model.Common.Enum;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.Internal.Exception;
using United.Mobile.Model.MPSignIn;
using United.Utility.Helper;

namespace United.Mobile.MPAccountProfile.Domain
{
    public class MPAccountProfileBusiness : IMPAccountProfileBusiness
    {
        private readonly ICacheLog<MPAccountProfileBusiness> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICustomerProfile _customerProfile;
        private readonly IMerchandizingServices _merchandizingServices;
        private readonly IDPService _dpService;
        private readonly IMileagePlus _mileagePlus;
        private readonly IShoppingSessionHelper _shoppingSessionHelper;
        private readonly IHeaders _headers;
        private readonly ICachingService _cachingService;
        private readonly IDynamoDBUtility _dynamoDBUtility;

        public MPAccountProfileBusiness(ICacheLog<MPAccountProfileBusiness> logger
            , IConfiguration configuration
            , ICustomerProfile customerProfile
            , IMerchandizingServices merchandizingServices
            , IDPService dpService
            , IMileagePlus mileagePlus
            , IShoppingSessionHelper shoppingSessionHelper
            , IHeaders headers
            , ICachingService cachingService
            , IDynamoDBUtility dynamoDBUtility)
        {
            _logger = logger;
            _configuration = configuration;
            _customerProfile = customerProfile;
            _merchandizingServices = merchandizingServices;
            _dpService = dpService;
            _mileagePlus = mileagePlus;
            _shoppingSessionHelper = shoppingSessionHelper;
            _headers = headers;
            _cachingService = cachingService;
            _dynamoDBUtility = dynamoDBUtility;
        }

        public async Task<MOBContactUsResponse> GetContactUsDetails(MOBContactUsRequest request)
        {
            var ListNames = new List<string> { "InternationalCallingCard" };
            MOBContactUsResponse response = new MOBContactUsResponse();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var title in ListNames)
            {
                stringBuilder.Append("'");
                stringBuilder.Append(title);
                stringBuilder.Append("'");
                stringBuilder.Append(",");
            }
            string reqTitles = stringBuilder.ToString().Trim(',');
            try
            {
                var CallingcardList = await _dynamoDBUtility.GetNewLegalDocumentsForTitles(reqTitles, request.TransactionId, true).ConfigureAwait(false);
                _logger.LogInformation("GetContactUsDetails-Business - LegalDocumentsForTitles Response {@legaldocumentsresponse} and {@transactionId}", JsonConvert.SerializeObject(CallingcardList), request.TransactionId);
                response.ContactUsUSACanada = GetContactUsUSACanada(request.MemberType, request.IsCEO, CallingcardList);
                response.ContactUSOutSideUSACanada = await GetContactUSOusideUSACanada(request.MemberType, request.IsCEO, CallingcardList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("GetContactUsDetails - OnPremSQLService-GetLegalDocumentsForTitles Error {@errorMessage} {@StackTrace}", ex.Message, ex.StackTrace);
            }

            return response;
        }

        public async Task<MOBCustomerPreferencesResponse> RetrieveCustomerPreferences(MOBCustomerPreferencesRequest request)
        {
            MOBCustomerPreferencesResponse response = new MOBCustomerPreferencesResponse();
            Session session = null;

            _logger.LogInformation("RetrieveCustomerPreferences {@clientRequest}", JsonConvert.SerializeObject(request));

            if (!string.IsNullOrEmpty(request.SessionId))
            {
                session = await _shoppingSessionHelper.GetShoppingSession(request.SessionId).ConfigureAwait(false);
                request.SessionId = session.SessionId;
            }
            else
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            response = await _customerProfile.RetrieveCustomerPreferences(request, session.Token).ConfigureAwait(false);
            response.SessionId = request.SessionId;
            response.TransactionId = request.TransactionId;
            response.LanguageCode = request.LanguageCode;

            return response;
        }

        public async Task<MOBMPAccountSummaryResponse> GetAccountSummaryWithMemberCardPremierActivity(MOBMPAccountValidationRequest request)
        {
            MOBMPAccountSummaryResponse response = new MOBMPAccountSummaryResponse();

            if (_configuration.GetValue<bool>("MyAccountForceUpdateToggle") == true
                && _mileagePlus.IsPremierStatusTrackerSupportedVersion(request.Application.Id, request.Application.Version.Major) == false)
            {
                string myAccountForceUpdate = !string.IsNullOrEmpty(_configuration.GetValue<string>("MyAccountForceUpdateMessage")) ? _configuration.GetValue<string>("MyAccountForceUpdateMessage") : "A newer version of the United Airlines app is available. Please update to the new version to continue.";
                throw new MOBUnitedException(myAccountForceUpdate);
            }

            string dpToken = string.Empty;
            string authToken = string.Empty;
            bool validRequest = false;
            var tupleRes = await _shoppingSessionHelper.ValidateHashPinAndGetAuthToken(request.MileagePlusNumber, request.HashValue, request.Application.Id, request.DeviceId, request.Application.Version.Major, authToken, request.TransactionId, request.SessionId).ConfigureAwait(false);
            validRequest = tupleRes.returnValue;
            authToken = tupleRes.validAuthToken;
            if (!validRequest)
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("bugBountySessionExpiredMsg"));
            }
            dpToken = await _dpService.GetAnonymousToken(request.Application.Id, request.DeviceId, _configuration).ConfigureAwait(false);
            response.OPAccountSummary = null;
            Service.Presentation.ProductResponseModel.Subscription merchOut = null;

            if (_configuration.GetValue<bool>("EnableUCBPhase1_MobilePhase1Changes"))
            {
                var tuple = await _mileagePlus.GetAccountSummaryWithPremierActivityV2(request, true, dpToken).ConfigureAwait(false);
                response.OPAccountSummary = tuple.mpSummary;
                merchOut = tuple.merchOut;
            }

            response.isUASubscriptionsAvailable = false;

            #region GetUASubscriptions
            string channelId = string.Empty;
            string channelName = string.Empty;
            if (_configuration.GetValue<bool>("EnabledMERCHChannels"))
            {
                string merchChannel = "MOBMYRES";
                _merchandizingServices.SetMerchandizeChannelValues(merchChannel, ref channelId, ref channelName);
            }

            if(merchOut != null)
            {
                response.UASubscriptions = _merchandizingServices.GetUASubscriptions(request.MileagePlusNumber, merchOut);
            }
            else
            {
                response.UASubscriptions = await _merchandizingServices.GetUASubscriptions(request.MileagePlusNumber, 1, request.TransactionId, channelId, channelName, dpToken).ConfigureAwait(false);
            }

            if (response.UASubscriptions != null && response.UASubscriptions.SubscriptionTypes != null && response.UASubscriptions.SubscriptionTypes.Count > 0)
            {
                response.isUASubscriptionsAvailable = true;
            }

            #endregion

            #region GetTSAFLAGSTATUS
            //MPSignIn Services TurnOn, as observered the dynamo IsTSAFlaggedAccount failing 
            //And UI team confirmed this flag is not used
            response.OPAccountSummary.IsMPAccountTSAFlagON = false;// await _dynamoDBUtility.IsTSAFlaggedAccount(request.MileagePlusNumber, request.TransactionId).ConfigureAwait(false);//utility
            if (!_configuration.GetValue<bool>("DisableAssigningIsMPAccountTSAFlagON"))
            {
                if (response.OPAccountSummary.IsMPAccountTSAFlagON)
                {
                    if (_configuration.GetValue<bool>("NewServieCall_GetProfile_AllTravelerData"))
                    {
                        try
                        {
                            response.OPAccountSummary.IsMPAccountTSAFlagON = await _mileagePlus.GetProfile_AllTravelerData(request.MileagePlusNumber, request.TransactionId, dpToken, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false);
                        }
                        catch (System.Exception ex)
                        {
                            _logger.LogError("GetAccountSummaryMemberCardPremierActivity Error {message} {StackTrace}", ex.Message, ex.StackTrace);
                        }
                    }
                    else
                    {
                        MOBProfileRequest temp_request = new MOBProfileRequest
                        {
                            AccessCode = string.Empty,
                            LanguageCode = "en-US",
                            MileagePlusNumber = request.MileagePlusNumber,
                            PinCode = "****",
                            SessionID = request.TransactionId
                        };
                        temp_request.MileagePlusNumber = temp_request.MileagePlusNumber;  //the setter should force to uppercase
                        temp_request.IncludeSecureTravelers = true;
                        temp_request.TransactionId = "FromAccountSummaryCall";
                    }
                    if (response.OPAccountSummary.IsMPAccountTSAFlagON)
                    {
                        response.OPAccountSummary.TSAMessage = _configuration.GetValue<string>("TSAAccountMessage");
                    }
                }
            }
            #endregion

            #region GetStatusLiftBanner
            //Promotion endpoint not available
            //response.OPAccountSummary.statusLiftBanner = await _utility.GetStatusLiftBanner(request).ConfigureAwait(false);
            #endregion

            return response;

        }

        private MOBContactUsUSACanada GetContactUsUSACanada(MOBMemberType memberType, bool isCEO, List<MOBLegalDocument> CallingcardList)
        {
            List<MOBContactUSUSACanadaPhoneNumber> lstCOntactDetails = new List<MOBContactUSUSACanadaPhoneNumber>
            {
                new MOBContactUSUSACanadaPhoneNumber
                                {
                                    ContactUsDeskName =CallingcardList.Find(item => item.Title =="CallingCardReservationAssistanceText").LegalDocument,
                                    ContactUsDeskDescription =string.Empty,
                                    ContactUsDeskPhoneNumber =CallingcardList.Find(item => item.Title =="CallingCardReservationAssistanceCell").LegalDocument
                                },
                                 new MOBContactUSUSACanadaPhoneNumber
                                {
                                    ContactUsDeskName =CallingcardList.Find(item => item.Title =="CallingCardBaggageServiceText").LegalDocument,
                                    ContactUsDeskDescription =string.Empty,
                                    ContactUsDeskPhoneNumber =CallingcardList.Find(item => item.Title =="CallingCardBaggageServiceCell").LegalDocument
                                },
                                new MOBContactUSUSACanadaPhoneNumber
                                {
                                    ContactUsDeskName =CallingcardList.Find(item => item.Title =="CallingFlightArrivalDepartureText").LegalDocument,
                                    ContactUsDeskDescription =CallingcardList.Find(item => item.Title =="CallingAutoMatedInformationText").LegalDocument,
                                    ContactUsDeskPhoneNumber =CallingcardList.Find(item => item.Title =="CallingFlightArrivalDepartureCell").LegalDocument
                                }
            };
            List<MOBContactUSEmail> lstemailContact = new List<MOBContactUSEmail>
            {
                new MOBContactUSEmail
                {
                   ContactUsDeskEmailName= CallingcardList.Find(item => item.Title =="CallingPremier1KCustomerCareText").LegalDocument,
                    ContactUsDeskEmailAddress = CallingcardList.Find(item => item.Title =="CallingFlightArrivalDepartureEmail").LegalDocument
                }
            };

            if (memberType == MOBMemberType.Premier1K)
            {
                lstCOntactDetails.Insert(0, new MOBContactUSUSACanadaPhoneNumber
                {
                    ContactUsDeskName = CallingcardList.Find(item => item.Title == "CallingPremier1kAsissistanceText").LegalDocument,
                    ContactUsDeskDescription = CallingcardList.Find(item => item.Title == "CallingPremier1kAsissistanceDescription").LegalDocument,
                    ContactUsDeskPhoneNumber = CallingcardList.Find(item => item.Title == "CallingPremier1kAsissistanceCell").LegalDocument
                });
                lstCOntactDetails.Insert(3, new MOBContactUSUSACanadaPhoneNumber
                {
                    ContactUsDeskName = CallingcardList.Find(item => item.Title == "CallingMilagePlusAssistanceText").LegalDocument,
                    ContactUsDeskDescription = string.Empty,
                    ContactUsDeskPhoneNumber = CallingcardList.Find(item => item.Title == "CallingMilagePlusAssistanceCell").LegalDocument
                });
                return new MOBContactUsUSACanada(_configuration)
                {
                    USACanadaContactTypeEmail = new MOBContactUSContactTypeEmail
                    {
                        ContactType = CallingcardList.Find(item => item.Title == "DefaultEmailAddressContactType").LegalDocument,
                        EmailAddresses = lstemailContact
                    },
                    USACanadaContactTypePhone = new MOBContactUSUSACanadaContactTypePhone
                    {
                        ContactType = CallingcardList.Find(item => item.Title == "InternaitonPhoneContactType").LegalDocument,
                        PhoneNumbers = lstCOntactDetails
                    }
                };
            }
            if (memberType == MOBMemberType.PremierSilver || memberType == MOBMemberType.PremierGold || memberType == MOBMemberType.PremierPlatinium)
            {
                lstCOntactDetails.Insert(0, new MOBContactUSUSACanadaPhoneNumber
                {
                    ContactUsDeskName = CallingcardList.Find(item => item.Title == "CallingPremierPriorityAssistanceText").LegalDocument,
                    ContactUsDeskDescription = CallingcardList.Find(item => item.Title == "CallingPremierPriorityAssistanceDescription").LegalDocument,
                    ContactUsDeskPhoneNumber = CallingcardList.Find(item => item.Title == "CallingPremierPriorityAssistanceCell").LegalDocument
                });
                lstCOntactDetails.Insert(3, new MOBContactUSUSACanadaPhoneNumber
                {
                    ContactUsDeskName = CallingcardList.Find(item => item.Title == "CallingMilagePlusAssistanceText").LegalDocument,
                    ContactUsDeskDescription = string.Empty,
                    ContactUsDeskPhoneNumber = CallingcardList.Find(item => item.Title == "CallingMilagePlusAssistanceForSilverGoldPlatinumCell").LegalDocument
                });
                return new MOBContactUsUSACanada(_configuration)
                {
                    USACanadaContactTypePhone = new MOBContactUSUSACanadaContactTypePhone
                    {
                        ContactType = CallingcardList.Find(item => item.Title == "InternaitonPhoneContactType").LegalDocument,
                        PhoneNumbers = lstCOntactDetails
                    }
                };

            }
            else if (memberType == MOBMemberType.PremierGlobalServices)
            {
                lstCOntactDetails.Insert(0, new MOBContactUSUSACanadaPhoneNumber
                {
                    ContactUsDeskName = isCEO ? CallingcardList.Find(item => item.Title == "CallingCharimanCircleText").LegalDocument : CallingcardList.Find(item => item.Title == "CallingUnitedGlobalText").LegalDocument,
                    ContactUsDeskDescription = CallingcardList.Find(item => item.Title == "CallingCharimanCircleDescription").LegalDocument,
                    ContactUsDeskPhoneNumber = isCEO ? CallingcardList.Find(item => item.Title == "CallingCharimanCircleCell").LegalDocument : CallingcardList.Find(item => item.Title == "CallingUnitedGlobalCell").LegalDocument
                });

                lstCOntactDetails.Insert(3, new MOBContactUSUSACanadaPhoneNumber
                {
                    ContactUsDeskName = CallingcardList.Find(item => item.Title == "CallingMilagePlusAssistanceText").LegalDocument,
                    ContactUsDeskDescription = string.Empty,
                    ContactUsDeskPhoneNumber = isCEO ? CallingcardList.Find(item => item.Title == "CallingCharimanCircleCell").LegalDocument : CallingcardList.Find(item => item.Title == "CallingUnitedGlobalCell").LegalDocument
                });
                return new MOBContactUsUSACanada(_configuration)
                {
                    USACanadaContactTypeEmail = new MOBContactUSContactTypeEmail
                    {
                        ContactType = CallingcardList.Find(item => item.Title == "DefaultEmailAddressContactType").LegalDocument,
                        EmailAddresses = new List<MOBContactUSEmail>
                        {
                            new MOBContactUSEmail
                            {
                                ContactUsDeskEmailName = CallingcardList.Find(item => item.Title == "CallingFlightGlobalServicesText").LegalDocument,
                                ContactUsDeskEmailAddress = CallingcardList.Find(item => item.Title == "CallingFlightGlobalServicesEmail").LegalDocument
                            }
                        }
                    },
                    USACanadaContactTypePhone = new MOBContactUSUSACanadaContactTypePhone
                    {
                        ContactType = CallingcardList.Find(item => item.Title == "InternaitonPhoneContactType").LegalDocument,
                        PhoneNumbers = lstCOntactDetails
                    }
                };

            }
            else
            {
                lstCOntactDetails.Insert(0, new MOBContactUSUSACanadaPhoneNumber
                {
                    ContactUsDeskName = CallingcardList.Find(item => item.Title == "CallingTravelreservationText").LegalDocument,
                    ContactUsDeskDescription = CallingcardList.Find(item => item.Title == "CallingTravelreservationDescription").LegalDocument,
                    ContactUsDeskPhoneNumber = CallingcardList.Find(item => item.Title == "CallingTravelreservationCell").LegalDocument
                });

                lstCOntactDetails.Insert(3, new MOBContactUSUSACanadaPhoneNumber
                {
                    ContactUsDeskName = CallingcardList.Find(item => item.Title == "CallingMilagePlusAssistanceText").LegalDocument,
                    ContactUsDeskDescription = CallingcardList.Find(item => item.Title == "CallingAutoMatedInformationText").LegalDocument,
                    ContactUsDeskPhoneNumber = CallingcardList.Find(item => item.Title == "CallingMilagePlusGeneralAssistanceCell").LegalDocument
                });
                return new MOBContactUsUSACanada(_configuration)
                {

                    USACanadaContactTypePhone = new MOBContactUSUSACanadaContactTypePhone
                    {
                        ContactType = CallingcardList.Find(item => item.Title == "InternaitonPhoneContactType").LegalDocument,
                        PhoneNumbers = lstCOntactDetails
                    }
                };
            };
        }

        private async Task<MOBContactUSOutSideUSACanada> GetContactUSOusideUSACanada(MOBMemberType memberType, bool isCEO, List<MOBLegalDocument> CallingcardList)
        {
            MOBContactUSOutSideUSACanada outSideUSACanadaContacts = new MOBContactUSOutSideUSACanada(_configuration);
            if (memberType == MOBMemberType.Premier1K || memberType == MOBMemberType.PremierGlobalServices)
            {
                outSideUSACanadaContacts.HowToUseOutsideUSACanadaATTDirectAccessNumberDescription = CallingcardList.Find(item => item.Title == nameof(outSideUSACanadaContacts.HowToUseOutsideUSACanadaATTDirectAccessNumberDescription))?.LegalDocument;
                outSideUSACanadaContacts.OutSideUSACanadaContactATTTollFreeNumber = memberType == MOBMemberType.PremierGlobalServices ? isCEO ? CallingcardList.Find(item => item.Title == "CallingChairMenCircleTollFreeNumber")?.LegalDocument : CallingcardList.Find(item => item.Title == "CallingGlobalServicesTollFreeNumber")?.LegalDocument : memberType == MOBMemberType.Premier1K ? CallingcardList.Find(item => item.Title == "CallingPremier1KTollFreeNumber")?.LegalDocument : string.Empty;
                outSideUSACanadaContacts.DefaultEmailAddressContactType = CallingcardList.Find(item => item.Title == nameof(outSideUSACanadaContacts.DefaultEmailAddressContactType))?.LegalDocument;
                outSideUSACanadaContacts.SelectCountryDefaultText = CallingcardList.Find(item => item.Title == nameof(outSideUSACanadaContacts.SelectCountryDefaultText))?.LegalDocument;
                outSideUSACanadaContacts.CountryListDefaultSelection = CallingcardList.Find(item => item.Title == nameof(outSideUSACanadaContacts.CountryListDefaultSelection))?.LegalDocument;
                outSideUSACanadaContacts.InternaitonPhoneContactType = CallingcardList.Find(item => item.Title == nameof(outSideUSACanadaContacts.InternaitonPhoneContactType))?.LegalDocument;
                outSideUSACanadaContacts.SelectCountryFromListScreenText = CallingcardList.Find(item => item.Title == nameof(outSideUSACanadaContacts.SelectCountryFromListScreenText))?.LegalDocument;
                outSideUSACanadaContacts.ATTAccessNumberDialInfoText = CallingcardList.Find(item => item.Title == nameof(outSideUSACanadaContacts.ATTAccessNumberDialInfoText))?.LegalDocument;
                outSideUSACanadaContacts.InternationalDefaultEmailAddresses = memberType == MOBMemberType.PremierGlobalServices ?
                    new List<MOBContactUSEmail>
                    {
                        new MOBContactUSEmail
                        {
                            ContactUsDeskEmailName = CallingcardList.Find(item => item.Title == "CallingFlightGlobalServicesText").LegalDocument,
                            ContactUsDeskEmailAddress = CallingcardList.Find(item => item.Title == "CallingFlightGlobalServicesEmail").LegalDocument
                        }
                    } :
                    new List<MOBContactUSEmail>
                    {
                        new MOBContactUSEmail
                        {
                            ContactUsDeskEmailName = CallingcardList.Find(item => item.Title == "CallingPremier1KCustomerCareText").LegalDocument,
                            ContactUsDeskEmailAddress = CallingcardList.Find(item => item.Title == "CallingFlightArrivalDepartureEmail").LegalDocument
                        }
                    };
                outSideUSACanadaContacts.ContactUSLocationDescription = CallingcardList.Find(item => item.Title == "ContactUsLocationsDescription")?.LegalDocument;
                outSideUSACanadaContacts.ContactUSLocationHyperlink = CallingcardList.Find(item => item.Title == "ContactUsLocationsLink")?.LegalDocument;
                outSideUSACanadaContacts.ContactUSDirectAccessNumber = CallingcardList.Find(item => item.Title == "ContactUsDirectAccessNumbers")?.LegalDocument;
                outSideUSACanadaContacts.OutSideUSACanadaContactTypePhoneList = await GetInternationalCallingCard().ConfigureAwait(false);
                return outSideUSACanadaContacts;
            }
            else
            {
                return null;
            }
        }

        private async Task<List<MOBContactUSOusideUSACanadaContactTypePhone>> GetInternationalCallingCard()
        {
            List<MOBContactUSOusideUSACanadaContactTypePhone> items = null;
            List<MOBBKCountry> lstCountries = new List<MOBBKCountry>();
            var callingCard = await _cachingService.GetCache<List<CallingCard>>("CallingCard", _headers.ContextValues.TransactionId).ConfigureAwait(false);
            var callingCardList = JsonConvert.DeserializeObject<List<CallingCard>>(callingCard);
            #region
            List<string> lstPhoneNumber = null;
            MOBContactAccessNumber CityContact = null;

            foreach (var obj in callingCardList)
            {
                if (items == null)
                {
                    items = new List<MOBContactUSOusideUSACanadaContactTypePhone>();
                }
                MOBContactUSOusideUSACanadaContactTypePhone item = items.FirstOrDefault(c => c.Country.CountryCode == obj.CountryCode);

                lstPhoneNumber = new List<string>();
                if (obj.PhoneNumber.Trim() != string.Empty)
                {
                    lstPhoneNumber.Add(obj.PhoneNumber);
                }

                if (obj.PhoneNumber2.Trim() != string.Empty)
                {
                    lstPhoneNumber.Add(obj.PhoneNumber2);
                }

                if (obj.PhoneNumber3.ToString().Trim() != string.Empty)
                {
                    lstPhoneNumber.Add(obj.PhoneNumber3);
                }

                if (obj.PhoneNumber4.ToString().Trim() != string.Empty)
                {
                    lstPhoneNumber.Add(obj.PhoneNumber4);
                }

                CityContact = new MOBContactAccessNumber
                {
                    City = obj.CityName,
                    ATTDirectAccessNumbers = lstPhoneNumber
                };
                if (item == null)
                {
                    item = new MOBContactUSOusideUSACanadaContactTypePhone();
                    MOBBKCountry country = new MOBBKCountry();
                    item.Country = new MOBBKCountry
                    {
                        CountryCode = obj.CountryCode,
                        Name = obj.CountryName,
                        ShortName = obj.CountryLongName
                    };
                    item.ContactAccessNumberList = new List<MOBContactAccessNumber>
                    {
                        CityContact
                    };
                    items.Add(item);
                }
                else
                {

                    item.ContactAccessNumberList.Add(CityContact);
                    item.ContactAccessNumberList = item.ContactAccessNumberList.OrderBy(c => c.City).ToList();
                }

            }
            return items;

            #endregion
        }
    }
}


