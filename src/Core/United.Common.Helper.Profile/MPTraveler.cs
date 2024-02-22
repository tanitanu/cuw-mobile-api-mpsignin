using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using United.Definition;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.CSLSerivce;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.Internal.Exception;
using United.Mobile.Model.PNRManagement;
using United.Mobile.Model.Shopping;
using United.Service.Presentation.CommonModel;
using United.Services.Customer.Common;
using United.Services.FlightShopping.Common.Extensions;
using United.Utility.Enum;
using United.Utility.Helper;
using JsonSerializer = United.Utility.Helper.DataContextJsonSerializer;
using Reservation = United.Mobile.Model.Shopping.Reservation;
using Traveler = United.Services.Customer.Common.Traveler;

namespace United.Common.Helper.Profile
{
    public class MPTraveler : IMPTraveler
    {
        private readonly IConfiguration _configuration;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly ICustomerDataService _customerDataService;
        private readonly ICacheLog<MPTraveler> _logger;
        private readonly IPNRRetrievalService _pNRRetrievalService;
        private readonly IProfileCreditCard _profileCreditCard;
        private readonly ILoyaltyUCBService _loyaltyUCBService;
        private readonly IDynamoDBUtility _dynamoDBUtility;
        private bool IsCorpBookingPath = false;
        private bool IsArrangerBooking = false;
        private readonly IHeaders _headers;

        public MPTraveler(IConfiguration configuration
            , ISessionHelperService sessionHelperService
            , ICustomerDataService mPEnrollmentService
            , ICacheLog<MPTraveler> logger
            , IPNRRetrievalService pNRRetrievalService
            , IProfileCreditCard profileCreditCard
            , ILoyaltyUCBService loyaltyUCBService
            , IHeaders headers
            , IDynamoDBUtility dynamoDBUtility
            )
        {
            _configuration = configuration;
            _sessionHelperService = sessionHelperService;
            _customerDataService = mPEnrollmentService;
            _logger = logger;
            _pNRRetrievalService = pNRRetrievalService;
            _profileCreditCard = profileCreditCard;
            _loyaltyUCBService = loyaltyUCBService;
            _headers = headers;
            _dynamoDBUtility = dynamoDBUtility;
        }

        #region Methods
        public bool IsCorporateLeisureFareSelected(List<MOBSHOPTrip> trips)
        {
            string corporateFareText = _configuration.GetValue<string>("FSRLabelForCorporateLeisure") ?? string.Empty;
            if (trips != null)
            {
                return trips.Any(
                   x =>
                       x.FlattenedFlights.Any(
                           f =>
                               f.Flights.Any(
                                   fl =>
                                       fl.CorporateFareIndicator ==
                                       corporateFareText.ToString())));
            }

            return false;
        }
        public async Task<(List<MOBCPTraveler> mobTravelersOwnerFirstInList, bool isProfileOwnerTSAFlagOn, List<MOBKVP> savedTravelersMPList)> PopulateTravelers(List<Traveler> travelers, string mileagePluNumber, bool isProfileOwnerTSAFlagOn, bool isGetCreditCardDetailsCall, MOBCPProfileRequest request, string sessionid, bool getMPSecurityDetails = false, string path = "")
        {
            var savedTravelersMPList = new List<MOBKVP>();
            List<MOBCPTraveler> mobTravelers = null;
            List<MOBCPTraveler> mobTravelersOwnerFirstInList = null;
            MOBCPTraveler profileOwnerDetails = new MOBCPTraveler();
            if (travelers != null && travelers.Count > 0)
            {
                mobTravelers = new List<MOBCPTraveler>();
                int i = 0;
                var persistedReservation = await PersistedReservation(request).ConfigureAwait(false);

                foreach (Traveler traveler in travelers)
                {
                    #region
                    MOBCPTraveler mobTraveler = new MOBCPTraveler();
                    mobTraveler.PaxIndex = i; i++;
                    mobTraveler.CustomerId = traveler.CustomerId;
                    if (_configuration.GetValue<bool>("NGRPAwardCalendarMP2017Switch"))
                    {
                        mobTraveler.CustomerMetrics = PopulateCustomerMetrics(traveler.CustomerMetrics);
                    }
                    if (traveler.BirthDate != null)
                    {
                        mobTraveler.BirthDate = GeneralHelper.FormatDateOfBirth(traveler.BirthDate.GetValueOrDefault());
                    }
                    if (_configuration.GetValue<bool>("EnableNationalityAndCountryOfResidence"))
                    {
                        if (persistedReservation != null && persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.InfoNationalityAndResidence != null
                            && persistedReservation.ShopReservationInfo2.InfoNationalityAndResidence.IsRequireNationalityAndResidence)
                        {
                            if (string.IsNullOrEmpty(traveler.CountryOfResidence) || string.IsNullOrEmpty(traveler.Nationality))
                            {
                                mobTraveler.Message = _configuration.GetValue<string>("SavedTravelerInformationNeededMessage");
                            }
                        }
                        mobTraveler.Nationality = traveler.Nationality;
                        mobTraveler.CountryOfResidence = traveler.CountryOfResidence;
                    }

                    mobTraveler.FirstName = traveler.FirstName;
                    mobTraveler.GenderCode = traveler.GenderCode;
                    mobTraveler.IsDeceased = traveler.IsDeceased;
                    mobTraveler.IsExecutive = traveler.IsExecutive;
                    mobTraveler.IsProfileOwner = traveler.IsProfileOwner;
                    mobTraveler.Key = traveler.Key;
                    mobTraveler.LastName = traveler.LastName;
                    mobTraveler.MiddleName = traveler.MiddleName;
                    mobTraveler.MileagePlus = PopulateMileagePlus(traveler.MileagePlus);
                    if (mobTraveler.MileagePlus != null)
                    {
                        mobTraveler.MileagePlus.MpCustomerId = traveler.CustomerId;

                        if (request != null && IncludeTravelBankFOP(request.Application.Id, request.Application.Version.Major))
                        {
                            Session session = new Session();
                            string cslLoyaltryBalanceServiceResponse = await _loyaltyUCBService.GetLoyaltyBalance(request.Token, request.MileagePlusNumber, request.SessionId).ConfigureAwait(false);
                            if (!string.IsNullOrEmpty(cslLoyaltryBalanceServiceResponse))
                            {
                                United.TravelBank.Model.BalancesDataModel.BalanceResponse PlusPointResponse = JsonSerializer.NewtonSoftDeserialize<United.TravelBank.Model.BalancesDataModel.BalanceResponse>(cslLoyaltryBalanceServiceResponse);
                                United.TravelBank.Model.BalancesDataModel.Balance tbbalance = PlusPointResponse.Balances.FirstOrDefault(tb => tb.ProgramCurrencyType == United.TravelBank.Model.TravelBankConstants.ProgramCurrencyType.UBC);
                                if (tbbalance != null && tbbalance.TotalBalance > 0)
                                {
                                    mobTraveler.MileagePlus.TravelBankBalance = (double)tbbalance.TotalBalance;
                                }
                            }
                        }
                    }
                    mobTraveler.OwnerFirstName = traveler.OwnerFirstName;
                    mobTraveler.OwnerLastName = traveler.OwnerLastName;
                    mobTraveler.OwnerMiddleName = traveler.OwnerMiddleName;
                    mobTraveler.OwnerSuffix = traveler.OwnerSuffix;
                    mobTraveler.OwnerTitle = traveler.OwnerTitle;
                    mobTraveler.ProfileId = traveler.ProfileId;
                    mobTraveler.ProfileOwnerId = traveler.ProfileOwnerId;
                    bool isTSAFlagOn = false;
                    if (traveler.SecureTravelers != null && traveler.SecureTravelers.Count > 0)
                    {
                        if (request == null)
                        {
                            request = new MOBCPProfileRequest();
                            request.SessionId = string.Empty;
                            request.DeviceId = string.Empty;
                            request.Application = new MOBApplication() { Id = 0 };
                        }
                        else if (request.Application == null)
                        {
                            request.Application = new MOBApplication() { Id = 0 };
                        }
                        mobTraveler.SecureTravelers = PopulatorSecureTravelers(traveler.SecureTravelers, ref isTSAFlagOn, i >= 2, request.SessionId, request.Application.Id, request.DeviceId);
                        if (mobTraveler.SecureTravelers != null && mobTraveler.SecureTravelers.Count > 0)
                        {
                            mobTraveler.RedressNumber = mobTraveler.SecureTravelers[0].RedressNumber;
                            mobTraveler.KnownTravelerNumber = mobTraveler.SecureTravelers[0].KnownTravelerNumber;
                        }
                    }
                    mobTraveler.IsTSAFlagON = isTSAFlagOn;
                    if (mobTraveler.IsProfileOwner)
                    {
                        isProfileOwnerTSAFlagOn = isTSAFlagOn;
                    }
                    mobTraveler.Suffix = traveler.Suffix;
                    mobTraveler.Title = traveler.Title;
                    mobTraveler.TravelerTypeCode = traveler.TravelerTypeCode;
                    mobTraveler.TravelerTypeDescription = traveler.TravelerTypeDescription;
                    //mobTraveler.PTCDescription = Utility.GetPaxDescription(traveler.TravelerTypeCode);
                    if (persistedReservation != null && persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.TravelerTypes != null
                        && persistedReservation.ShopReservationInfo2.TravelerTypes.Count > 0)
                    {
                        if (traveler.BirthDate != null)
                        {
                            if (EnableYADesc() && persistedReservation.ShopReservationInfo2.IsYATravel)
                            {
                                mobTraveler.PTCDescription = GetYAPaxDescByDOB();
                            }
                            else
                            {
                                mobTraveler.PTCDescription = GetPaxDescriptionByDOB(traveler.BirthDate.ToString(), persistedReservation.Trips[0].FlattenedFlights[0].Flights[0].DepartDate);
                            }
                        }
                    }
                    else
                    {
                        if (EnableYADesc() && persistedReservation != null && persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.IsYATravel)
                        {
                            mobTraveler.PTCDescription = GetYAPaxDescByDOB();
                        }
                    }
                    mobTraveler.TravelProgramMemberId = traveler.TravProgramMemberId;
                    if (traveler != null)
                    {
                        if (traveler.MileagePlus != null)
                        {
                            mobTraveler.CurrentEliteLevel = traveler.MileagePlus.CurrentEliteLevel;
                            //mobTraveler.AirRewardPrograms = GetTravelerLoyaltyProfile(traveler.AirPreferences, traveler.MileagePlus.CurrentEliteLevel);
                        }
                    }
                    else if (_configuration.GetValue<bool>("BugFixToggleFor17M") && request != null && !string.IsNullOrEmpty(request.SessionId))
                    {
                        //    mobTraveler.CurrentEliteLevel = GetCurrentEliteLevel(mileagePluNumber);//**// Need to work on this with a test scenario with a Saved Traveler added MP Account with a Elite Status. Try to Add a saved traveler(with MP WX664656) to MP Account VW344781
                        /// 195113 : Booking - Travel Options -mAPP: Booking: PA tile is displayed for purchase in Customize screen for Elite Premier member travelling and Login with General member
                        /// Srini - 12/04/2017
                        /// Calling getprofile for each traveler to get elite level for a traveler, who hav mp#
                        mobTraveler.MileagePlus = await GetCurrentEliteLevelFromAirPreferences(traveler.AirPreferences, request.SessionId).ConfigureAwait(false);
                        if (mobTraveler != null)
                        {
                            if (mobTraveler.MileagePlus != null)
                            {
                                mobTraveler.CurrentEliteLevel = mobTraveler.MileagePlus.CurrentEliteLevel;
                            }
                        }
                    }
                    mobTraveler.AirRewardPrograms = GetTravelerLoyaltyProfile(traveler.AirPreferences, mobTraveler.CurrentEliteLevel);
                    mobTraveler.Phones = PopulatePhones(traveler.Phones, true);

                    if (mobTraveler.IsProfileOwner)
                    {
                        // These Phone and Email details for Makre Reseravation Phone and Email reason is mobTraveler.Phones = PopulatePhones(traveler.Phones,true) will get only day of travel contacts to register traveler & edit traveler.
                        mobTraveler.ReservationPhones = PopulatePhones(traveler.Phones, false);
                        mobTraveler.ReservationEmailAddresses = PopulateEmailAddresses(traveler.EmailAddresses, false);

                        // Added by Hasnan - #53484. 10/04/2017
                        // As per the Bug 53484:PINPWD: iOS and Android - Phone number is blank in RTI screen after booking from newly created account.
                        // If mobTraveler.Phones is empty. Then it newly created account. Thus returning mobTraveler.ReservationPhones as mobTraveler.Phones.
                        if (!_configuration.GetValue<bool>("EnableDayOfTravelEmail") || string.IsNullOrEmpty(path) || !path.ToUpper().Equals("BOOKING"))
                        {
                            if (mobTraveler.Phones.Count == 0)
                            {
                                mobTraveler.Phones = mobTraveler.ReservationPhones;
                            }
                        }
                        #region Corporate Leisure(ProfileOwner must travel)//Client will use the IsMustRideTraveler flag to auto select the travel and not allow to uncheck the profileowner on the SelectTraveler Screen.
                        if (_configuration.GetValue<bool>("EnableCorporateLeisure"))
                        {
                            if (persistedReservation?.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.TravelType == TravelType.CLB.ToString() && IsCorporateLeisureFareSelected(persistedReservation.Trips))
                            {
                                mobTraveler.IsMustRideTraveler = true;
                            }
                        }
                        #endregion Corporate Leisure
                    }
                    if (mobTraveler.IsProfileOwner && request == null) //**PINPWD//mobTraveler.IsProfileOwner && request == null Means GetProfile and Populate is for MP PIN PWD Path
                    {
                        mobTraveler.ReservationEmailAddresses = PopulateAllEmailAddresses(traveler.EmailAddresses);
                    }
                    mobTraveler.AirPreferences = PopulateAirPrefrences(traveler.AirPreferences);
                    if (request?.Application?.Version != null && string.IsNullOrEmpty(request?.Flow) && IsInternationalBillingAddress_CheckinFlowEnabled(request.Application))
                    {
                        try
                        {
                            MOBShoppingCart mobShopCart = new MOBShoppingCart();
                            mobShopCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, mobShopCart.ObjectName, new List<string> { request.SessionId, mobShopCart.ObjectName }).ConfigureAwait(false);
                            if (mobShopCart != null && !string.IsNullOrEmpty(mobShopCart.Flow) && mobShopCart.Flow == FlowType.CHECKIN.ToString())
                            {
                                request.Flow = mobShopCart.Flow;
                            }
                        }
                        catch { }
                    }
                    mobTraveler.Addresses = PopulateTravelerAddresses(traveler.Addresses, request?.Application, request?.Flow);

                    if (_configuration.GetValue<bool>("EnableDayOfTravelEmail") && !string.IsNullOrEmpty(path) && path.ToUpper().Equals("BOOKING"))
                    {
                        mobTraveler.EmailAddresses = PopulateEmailAddresses(traveler.EmailAddresses, true);
                    }
                    else
                    if (!getMPSecurityDetails)
                    {
                        mobTraveler.EmailAddresses = PopulateEmailAddresses(traveler.EmailAddresses, false);
                    }
                    else
                    {
                        mobTraveler.EmailAddresses = PopulateMPSecurityEmailAddresses(traveler.EmailAddresses);
                    }
                    mobTraveler.CreditCards = IsCorpBookingPath ? await _profileCreditCard.PopulateCorporateCreditCards(traveler.CreditCards, isGetCreditCardDetailsCall, mobTraveler.Addresses, persistedReservation, sessionid).ConfigureAwait(false) : await _profileCreditCard.PopulateCreditCards(traveler.CreditCards, isGetCreditCardDetailsCall, mobTraveler.Addresses, sessionid).ConfigureAwait(false);

                    //if ((mobTraveler.IsTSAFlagON && string.IsNullOrEmpty(mobTraveler.Title)) || string.IsNullOrEmpty(mobTraveler.FirstName) || string.IsNullOrEmpty(mobTraveler.LastName) || string.IsNullOrEmpty(mobTraveler.GenderCode) || string.IsNullOrEmpty(mobTraveler.BirthDate)) //|| mobTraveler.Phones == null || (mobTraveler.Phones != null && mobTraveler.Phones.Count == 0)
                    if (mobTraveler.IsTSAFlagON || string.IsNullOrEmpty(mobTraveler.FirstName) || string.IsNullOrEmpty(mobTraveler.LastName) || string.IsNullOrEmpty(mobTraveler.GenderCode) || string.IsNullOrEmpty(mobTraveler.BirthDate)) //|| mobTraveler.Phones == null || (mobTraveler.Phones != null && mobTraveler.Phones.Count == 0)
                    {
                        mobTraveler.Message = _configuration.GetValue<string>("SavedTravelerInformationNeededMessage");
                    }
                    if (mobTraveler.IsProfileOwner)
                    {
                        profileOwnerDetails = mobTraveler;
                    }
                    else
                    {
                        #region
                        if (mobTraveler.AirRewardPrograms != null && mobTraveler.AirRewardPrograms.Count > 0)
                        {
                            var airRewardProgramList = (from program in mobTraveler.AirRewardPrograms
                                                        where program.CarrierCode.ToUpper().Trim() == "UA"
                                                        select program).ToList();

                            if (airRewardProgramList != null && airRewardProgramList.Count > 0)
                            {
                                savedTravelersMPList.Add(new MOBKVP() { Key = mobTraveler.CustomerId.ToString(), Value = airRewardProgramList[0].MemberId });
                            }
                        }
                        #endregion
                        mobTravelers.Add(mobTraveler);
                    }
                    #endregion
                }
            }
            mobTravelersOwnerFirstInList = new List<MOBCPTraveler>();
            mobTravelersOwnerFirstInList.Add(profileOwnerDetails);
            if (!IsCorpBookingPath || IsArrangerBooking)
            {
                mobTravelersOwnerFirstInList.AddRange(mobTravelers);
            }

            return (mobTravelersOwnerFirstInList, isGetCreditCardDetailsCall, savedTravelersMPList);
        }
        private List<MOBEmail> PopulateMPSecurityEmailAddresses(List<Services.Customer.Common.Email> emailAddresses)
        {
            #region
            List<MOBEmail> mobEmailAddresses = new List<MOBEmail>();
            if (emailAddresses != null && emailAddresses.Count > 0)
            {
                MOBEmail primaryEmailAddress = null;
                int co = 0;
                foreach (Services.Customer.Common.Email email in emailAddresses)
                {
                    if (email.EffectiveDate <= DateTime.UtcNow && email.DiscontinuedDate >= DateTime.UtcNow)
                    {
                        #region As per Wade Change want to filter out to return only Primary email to client if not primary phone exist return the first one from the list.So lot of work at client side will save time. Sep 15th 2014
                        co = co + 1;
                        MOBEmail e = new MOBEmail();
                        e.Key = email.Key;
                        e.Channel = new MOBChannel();
                        e.Channel.ChannelCode = email.ChannelCode;
                        e.Channel.ChannelDescription = email.ChannelCodeDescription;
                        e.Channel.ChannelTypeCode = email.ChannelTypeCode.ToString();
                        e.Channel.ChannelTypeDescription = email.ChannelTypeDescription;
                        e.EmailAddress = email.EmailAddress;
                        e.IsDefault = email.IsDefault;
                        e.IsPrimary = email.IsPrimary;
                        e.IsPrivate = email.IsPrivate;
                        e.IsDayOfTravel = email.IsDayOfTravel;
                        if (email.IsPrimary)
                        {
                            primaryEmailAddress = new MOBEmail();
                            primaryEmailAddress = e;
                            break;
                        }
                        #endregion
                    }
                }
                if (primaryEmailAddress != null)
                {
                    mobEmailAddresses.Add(primaryEmailAddress);
                }
            }
            return mobEmailAddresses;
            #endregion
        }
        public List<MOBAddress> PopulateTravelerAddresses(List<United.Services.Customer.Common.Address> addresses, MOBApplication application = null, string flow = null)
        {
            #region

            var mobAddresses = new List<Mobile.Model.Common.MOBAddress>();
            if (addresses != null && addresses.Count > 0)
            {
                bool isCorpAddressPresent = false;
                if (_configuration.GetValue<bool>("CorporateConcurBooking") && IsCorpBookingPath)
                {
                    //As per Business / DotCom Kalpen; we are removing the condition for checking the Effectivedate and Discontinued date
                    var corpIndex = addresses.FindIndex(x => x.ChannelTypeDescription != null && x.ChannelTypeDescription.ToLower() == "corporate" && x.AddressLine1 != null && x.AddressLine1.Trim() != "");
                    if (corpIndex >= 0)
                        isCorpAddressPresent = true;

                }
                foreach (United.Services.Customer.Common.Address address in addresses)
                {
                    if (_configuration.GetValue<bool>("CorporateConcurBooking"))
                    {
                        if (isCorpAddressPresent && address.ChannelTypeDescription.ToLower() == "corporate" &&
                            (_configuration.GetValue<bool>("USPOSCountryCodes_ByPass") || IsInternationalBilling(application, address.CountryCode, flow)))
                        {
                            var a = new Mobile.Model.Common.MOBAddress();
                            a.Key = address.Key;
                            a.Channel = new MOBChannel();
                            a.Channel.ChannelCode = address.ChannelCode;
                            a.Channel.ChannelDescription = address.ChannelCodeDescription;
                            a.Channel.ChannelTypeCode = address.ChannelTypeCode.ToString();
                            a.Channel.ChannelTypeDescription = address.ChannelTypeDescription;
                            a.ApartmentNumber = address.AptNum;
                            a.Channel = new MOBChannel();
                            a.Channel.ChannelCode = address.ChannelCode;
                            a.Channel.ChannelDescription = address.ChannelCodeDescription;
                            a.Channel.ChannelTypeCode = address.ChannelTypeCode.ToString();
                            a.Channel.ChannelTypeDescription = address.ChannelTypeDescription;
                            a.City = address.City;
                            a.CompanyName = address.CompanyName;
                            a.Country = new MOBCountry();
                            a.Country.Code = address.CountryCode;
                            a.Country.Name = address.CountryName;
                            a.JobTitle = address.JobTitle;
                            a.Line1 = address.AddressLine1;
                            a.Line2 = address.AddressLine2;
                            a.Line3 = address.AddressLine3;
                            a.State = new Mobile.Model.Common.MOBState();
                            a.State.Code = address.StateCode;
                            a.IsDefault = address.IsDefault;
                            a.IsPrivate = address.IsPrivate;
                            a.PostalCode = address.PostalCode;
                            if (address.ChannelTypeDescription.ToLower().Trim() == "corporate")
                            {
                                a.IsPrimary = true;
                                a.IsCorporate = true; // MakeIsCorporate true inorder to disable the edit on client
                            }
                            // Make IsPrimary true inorder to select the corpaddress by default

                            if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                            {
                                a.IsValidForTPIPurchase = IsValidAddressForTPIpayment(address.CountryCode);

                                if (a.IsValidForTPIPurchase)
                                {
                                    a.IsValidForTPIPurchase = IsValidSateForTPIpayment(address.StateCode);
                                }
                            }
                            mobAddresses.Add(a);
                        }
                    }


                    if (address.EffectiveDate <= DateTime.UtcNow && address.DiscontinuedDate >= DateTime.UtcNow)
                    {
                        if (_configuration.GetValue<bool>("USPOSCountryCodes_ByPass") || IsInternationalBilling(application, address.CountryCode, flow)) //##Kirti - allow only US addresses 
                        {
                            var a = new Mobile.Model.Common.MOBAddress();
                            a.Key = address.Key;
                            a.Channel = new MOBChannel();
                            a.Channel.ChannelCode = address.ChannelCode;
                            a.Channel.ChannelDescription = address.ChannelCodeDescription;
                            a.Channel.ChannelTypeCode = address.ChannelTypeCode.ToString();
                            a.Channel.ChannelTypeDescription = address.ChannelTypeDescription;
                            a.ApartmentNumber = address.AptNum;
                            a.Channel = new MOBChannel();
                            a.Channel.ChannelCode = address.ChannelCode;
                            a.Channel.ChannelDescription = address.ChannelCodeDescription;
                            a.Channel.ChannelTypeCode = address.ChannelTypeCode.ToString();
                            a.Channel.ChannelTypeDescription = address.ChannelTypeDescription;
                            a.City = address.City;
                            a.CompanyName = address.CompanyName;
                            a.Country = new MOBCountry();
                            a.Country.Code = address.CountryCode;
                            a.Country.Name = address.CountryName;
                            a.JobTitle = address.JobTitle;
                            a.Line1 = address.AddressLine1;
                            a.Line2 = address.AddressLine2;
                            a.Line3 = address.AddressLine3;
                            a.State = new Mobile.Model.Common.MOBState();
                            a.State.Code = address.StateCode;
                            //a.State.Name = address.StateName;
                            a.IsDefault = address.IsDefault;
                            a.IsPrimary = address.IsPrimary;
                            a.IsPrivate = address.IsPrivate;
                            a.PostalCode = address.PostalCode;
                            //Adding this check for corporate addresses to gray out the Edit button on the client
                            //if (address.ChannelTypeDescription.ToLower().Trim() == "corporate")
                            //{
                            //    a.IsCorporate = true;
                            //}
                            if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                            {
                                a.IsValidForTPIPurchase = IsValidAddressForTPIpayment(address.CountryCode);

                                if (a.IsValidForTPIPurchase)
                                {
                                    a.IsValidForTPIPurchase = IsValidSateForTPIpayment(address.StateCode);
                                }
                            }
                            mobAddresses.Add(a);
                        }
                    }
                }
            }
            return mobAddresses;
            #endregion
        }
        public bool IsInternationalBilling(MOBApplication application, string countryCode, string flow)
        {
            bool _isIntBilling = IsInternationalBillingAddress_CheckinFlowEnabled(application);
            if (_isIntBilling && flow?.ToLower() == FlowType.CHECKIN.ToString().ToLower()) // need to enable Int Billing address only in Checkin flow
            {
                //check for multiple countries
                return _isIntBilling;
            }
            else
            {
                if (string.IsNullOrEmpty(countryCode))
                    return false;
                //Normal Code as usual
                return _configuration.GetValue<string>("USPOSCountryCodes").Contains(countryCode);
            }
        }

        private bool IncludeTravelBankFOP(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableTravelBankFOP")
                && GeneralHelper.IsApplicationVersionGreater
                (appId, appVersion, "AndroidTravelBankFOPVersion", "iPhoneTravelBankFOPVersion", "", "", true, _configuration);
        }

        private bool IsValidSateForTPIpayment(string stateCode)
        {
            return !string.IsNullOrEmpty(stateCode) && !string.IsNullOrEmpty(_configuration.GetValue<string>("ExcludeUSStateCodesForTripInsurance")) && !_configuration.GetValue<string>("ExcludeUSStateCodesForTripInsurance").Contains(stateCode.ToUpper().Trim());
        }
        private bool IsValidAddressForTPIpayment(string countryCode)
        {
            return !string.IsNullOrEmpty(countryCode) && countryCode.ToUpper().Trim() == "US";
        }
        public List<MOBPrefAirPreference> PopulateAirPrefrences(List<United.Services.Customer.Common.AirPreference> airPreferences)
        {
            List<MOBPrefAirPreference> mobAirPrefs = new List<MOBPrefAirPreference>();
            if (airPreferences != null && airPreferences.Count > 0)
            {
                foreach (United.Services.Customer.Common.AirPreference pref in airPreferences)
                {
                    MOBPrefAirPreference mobAirPref = new MOBPrefAirPreference();
                    mobAirPref.AirportCode = pref.AirportCode;
                    mobAirPref.AirportCode = pref.AirportNameLong;
                    mobAirPref.AirportNameShort = pref.AirportNameShort;
                    mobAirPref.AirPreferenceId = pref.AirPreferenceId;
                    mobAirPref.ClassDescription = pref.ClassDescription;
                    mobAirPref.ClassId = pref.ClassId;
                    mobAirPref.CustomerId = pref.CustomerId;
                    mobAirPref.EquipmentCode = pref.EquipmentCode;
                    mobAirPref.EquipmentDesc = pref.EquipmentDesc;
                    mobAirPref.EquipmentId = pref.EquipmentId;
                    mobAirPref.IsActive = pref.IsActive;
                    mobAirPref.IsSelected = pref.IsSelected;
                    mobAirPref.IsNew = pref.IsNew;
                    mobAirPref.Key = pref.Key;
                    mobAirPref.LanguageCode = pref.LanguageCode;
                    mobAirPref.MealCode = pref.MealCode;
                    mobAirPref.MealDescription = pref.MealDescription;
                    mobAirPref.MealId = pref.MealId;
                    mobAirPref.NumOfFlightsDisplay = pref.NumOfFlightsDisplay;
                    mobAirPref.ProfileId = pref.ProfileId;
                    mobAirPref.SearchPreferenceDescription = pref.SearchPreferenceDescription;
                    mobAirPref.SearchPreferenceId = pref.SearchPreferenceId;
                    mobAirPref.SeatFrontBack = pref.SeatFrontBack;
                    mobAirPref.SeatSide = pref.SeatSide;
                    mobAirPref.SeatSideDescription = pref.SeatSideDescription;
                    mobAirPref.VendorCode = pref.VendorCode;
                    mobAirPref.VendorDescription = pref.VendorDescription;
                    mobAirPref.VendorId = pref.VendorId;
                    mobAirPref.AirRewardPrograms = GetAirRewardPrograms(pref.AirRewardPrograms);
                    mobAirPref.SpecialRequests = GetTravelerSpecialRequests(pref.SpecialRequests);
                    mobAirPref.ServiceAnimals = GetTravelerServiceAnimals(pref.ServiceAnimals);

                    mobAirPrefs.Add(mobAirPref);
                }
            }
            return mobAirPrefs;
        }
        private List<MOBPrefRewardProgram> GetAirRewardPrograms(List<United.Services.Customer.Common.RewardProgram> programs)
        {
            List<MOBPrefRewardProgram> mobAirRewardsProgs = new List<MOBPrefRewardProgram>();
            if (programs != null && programs.Count > 0)
            {
                foreach (United.Services.Customer.Common.RewardProgram pref in programs)
                {
                    MOBPrefRewardProgram mobAirRewardsProg = new MOBPrefRewardProgram();
                    mobAirRewardsProg.CustomerId = Convert.ToInt32(pref.CustomerId);
                    mobAirRewardsProg.ProfileId = Convert.ToInt32(pref.ProfileId);
                    //mobAirRewardsProg.ProgramCode = pref.ProgramCode;
                    //mobAirRewardsProg.ProgramDescription = pref.ProgramDescription;
                    mobAirRewardsProg.ProgramMemberId = pref.ProgramMemberId;
                    mobAirRewardsProg.VendorCode = pref.VendorCode;
                    mobAirRewardsProg.VendorDescription = pref.VendorDescription;
                    mobAirRewardsProgs.Add(mobAirRewardsProg);
                }
            }
            return mobAirRewardsProgs;
        }
        private List<MOBPrefSpecialRequest> GetTravelerSpecialRequests(List<United.Services.Customer.Common.SpecialRequest> specialRequests)
        {
            List<MOBPrefSpecialRequest> mobSpecialRequests = new List<MOBPrefSpecialRequest>();
            if (specialRequests != null && specialRequests.Count > 0)
            {
                foreach (United.Services.Customer.Common.SpecialRequest req in specialRequests)
                {
                    MOBPrefSpecialRequest mobSpecialRequest = new MOBPrefSpecialRequest();
                    mobSpecialRequest.AirPreferenceId = req.AirPreferenceId;
                    mobSpecialRequest.SpecialRequestId = req.SpecialRequestId;
                    mobSpecialRequest.SpecialRequestCode = req.SpecialRequestCode;
                    mobSpecialRequest.Key = req.Key;
                    mobSpecialRequest.LanguageCode = req.LanguageCode;
                    mobSpecialRequest.Description = req.Description;
                    mobSpecialRequest.Priority = req.Priority;
                    mobSpecialRequest.IsNew = req.IsNew;
                    mobSpecialRequest.IsSelected = req.IsSelected;
                    mobSpecialRequests.Add(mobSpecialRequest);
                }
            }
            return mobSpecialRequests;
        }
        private List<MOBPrefServiceAnimal> GetTravelerServiceAnimals(List<ServiceAnimal> serviceAnimals)
        {
            var results = new List<MOBPrefServiceAnimal>();

            if (serviceAnimals == null || !serviceAnimals.Any())
                return results;

            results = serviceAnimals.Select(x => new MOBPrefServiceAnimal
            {
                AirPreferenceId = x.AirPreferenceId,
                ServiceAnimalId = x.ServiceAnimalId,
                ServiceAnimalIdDesc = x.ServiceAnimalDesc,
                ServiceAnimalTypeId = x.ServiceAnimalTypeId,
                ServiceAnimalTypeIdDesc = x.ServiceAnimalTypeDesc,
                Key = x.Key,
                Priority = x.Priority,
                IsNew = x.IsNew,
                IsSelected = x.IsSelected
            }).ToList();

            return results;
        }
        private List<MOBEmail> PopulateAllEmailAddresses(List<Services.Customer.Common.Email> emailAddresses)
        {
            #region
            List<MOBEmail> mobEmailAddresses = new List<MOBEmail>();
            if (emailAddresses != null && emailAddresses.Count > 0)
            {
                MOBEmail primaryEmailAddress = null;
                int co = 0;
                foreach (Services.Customer.Common.Email email in emailAddresses)
                {
                    if (email.EffectiveDate <= DateTime.UtcNow && email.DiscontinuedDate >= DateTime.UtcNow)
                    {
                        #region As per Wade Change want to filter out to return only Primary email to client if not primary phone exist return the first one from the list.So lot of work at client side will save time. Sep 15th 2014
                        co = co + 1;
                        MOBEmail e = new MOBEmail();
                        e.Key = email.Key;
                        e.Channel = new MOBChannel();
                        e.Channel.ChannelCode = email.ChannelCode;
                        e.Channel.ChannelDescription = email.ChannelCodeDescription;
                        e.Channel.ChannelTypeCode = email.ChannelTypeCode.ToString();
                        e.Channel.ChannelTypeDescription = email.ChannelTypeDescription;
                        e.EmailAddress = email.EmailAddress;
                        e.IsDefault = email.IsDefault;
                        e.IsPrimary = email.IsPrimary;
                        e.IsPrivate = email.IsPrivate;
                        e.IsDayOfTravel = email.IsDayOfTravel;
                        mobEmailAddresses.Add(e);
                        #endregion
                    }
                }
            }
            return mobEmailAddresses;
            #endregion
        }
        public List<MOBEmail> PopulateEmailAddresses(List<Services.Customer.Common.Email> emailAddresses, bool onlyDayOfTravelContact)
        {
            #region
            List<MOBEmail> mobEmailAddresses = new List<MOBEmail>();
            bool isCorpEmailPresent = false;

            if (_configuration.GetValue<bool>("CorporateConcurBooking") && IsCorpBookingPath)
            {
                //As per Business / DotCom Kalpen; we are removing the condition for checking the Effectivedate and Discontinued date
                var corpIndex = emailAddresses.FindIndex(x => x.ChannelTypeDescription != null && x.ChannelTypeDescription.ToLower() == "corporate" && x.EmailAddress != null && x.EmailAddress.Trim() != "");
                if (corpIndex >= 0)
                    isCorpEmailPresent = true;

            }

            if (emailAddresses != null && emailAddresses.Count > 0)
            {
                MOBEmail primaryEmailAddress = null;
                int co = 0;
                foreach (Services.Customer.Common.Email email in emailAddresses)
                {
                    if (_configuration.GetValue<bool>("CorporateConcurBooking"))
                    {
                        if (isCorpEmailPresent && !onlyDayOfTravelContact && email.ChannelTypeDescription.ToLower() == "corporate")
                        {
                            primaryEmailAddress = new MOBEmail();
                            email.IsPrimary = true;
                            primaryEmailAddress.Key = email.Key;
                            primaryEmailAddress.Channel = new MOBChannel();
                            primaryEmailAddress.EmailAddress = email.EmailAddress;
                            primaryEmailAddress.Channel.ChannelCode = email.ChannelCode;
                            primaryEmailAddress.Channel.ChannelDescription = email.ChannelCodeDescription;
                            primaryEmailAddress.Channel.ChannelTypeCode = email.ChannelTypeCode.ToString();
                            primaryEmailAddress.Channel.ChannelTypeDescription = email.ChannelTypeDescription;
                            primaryEmailAddress.IsDefault = email.IsDefault;
                            primaryEmailAddress.IsPrimary = email.IsPrimary;
                            primaryEmailAddress.IsPrivate = email.IsPrivate;
                            primaryEmailAddress.IsDayOfTravel = email.IsDayOfTravel;
                            if (!email.IsDayOfTravel)
                            {
                                break;
                            }

                        }
                        else if (isCorpEmailPresent && !onlyDayOfTravelContact && email.ChannelTypeDescription.ToLower() != "corporate")
                        {
                            continue;
                        }
                    }
                    //Fix for CheckOut ArgNull Exception - Empty EmailAddress with null EffectiveDate & DiscontinuedDate for Corp Account Revenue Booking (MOBILE-9873) - Shashank : Added OR condition to allow CorporateAccount ProfileOwner.
                    if ((email.EffectiveDate <= DateTime.UtcNow && email.DiscontinuedDate >= DateTime.UtcNow) ||
                            (!_configuration.GetValue<bool>("DisableCheckforCorpAccEmail") && email.ChannelTypeDescription.ToLower() == "corporate"
                            && email.IsProfileOwner == true && primaryEmailAddress.IsNullOrEmpty()))
                    {
                        #region As per Wade Change want to filter out to return only Primary email to client if not primary phone exist return the first one from the list.So lot of work at client side will save time. Sep 15th 2014
                        co = co + 1;
                        MOBEmail e = new MOBEmail();
                        e.Key = email.Key;
                        e.Channel = new MOBChannel();
                        e.EmailAddress = email.EmailAddress;
                        e.Channel.ChannelCode = email.ChannelCode;
                        e.Channel.ChannelDescription = email.ChannelCodeDescription;
                        e.Channel.ChannelTypeCode = email.ChannelTypeCode.ToString();
                        e.Channel.ChannelTypeDescription = email.ChannelTypeDescription;
                        e.IsDefault = email.IsDefault;
                        e.IsPrimary = email.IsPrimary;
                        e.IsPrivate = email.IsPrivate;
                        e.IsDayOfTravel = email.IsDayOfTravel;
                        if (email.IsDayOfTravel)
                        {
                            primaryEmailAddress = new MOBEmail();
                            primaryEmailAddress = e;
                            if (onlyDayOfTravelContact)
                            {
                                break;
                            }
                        }
                        if (!onlyDayOfTravelContact)
                        {
                            if (email.IsPrimary)
                            {
                                primaryEmailAddress = new MOBEmail();
                                primaryEmailAddress = e;
                                break;
                            }
                            else if (co == 1)
                            {
                                primaryEmailAddress = new MOBEmail();
                                primaryEmailAddress = e;
                            }
                        }
                        #endregion
                    }
                }
                if (primaryEmailAddress != null)
                {
                    mobEmailAddresses.Add(primaryEmailAddress);
                }
            }
            return mobEmailAddresses;
            #endregion
        }
        public List<MOBCPPhone> PopulatePhones(List<United.Services.Customer.Common.Phone> phones, bool onlyDayOfTravelContact)
        {
            List<MOBCPPhone> mobCPPhones = new List<MOBCPPhone>();
            bool isCorpPhonePresent = false;


            if (_configuration.GetValue<bool>("CorporateConcurBooking") && IsCorpBookingPath)
            {
                var corpIndex = phones.FindIndex(x => x.ChannelTypeDescription != null && x.ChannelTypeDescription.ToLower() == "corporate" && x.PhoneNumber != null && x.PhoneNumber != "");
                if (corpIndex >= 0)
                    isCorpPhonePresent = true;
            }


            if (phones != null && phones.Count > 0)
            {
                MOBCPPhone primaryMobCPPhone = null;
                CultureInfo ci = GeneralHelper.EnableUSCultureInfo();
                int co = 0;
                foreach (United.Services.Customer.Common.Phone phone in phones)
                {
                    #region As per Wade Change want to filter out to return only Primary Phone to client if not primary phone exist return the first one from the list.So lot of work at client side will save time. Sep 15th 2014
                    MOBCPPhone mobCPPhone = new MOBCPPhone();
                    co = co + 1;

                    mobCPPhone.AreaNumber = phone.AreaNumber;
                    mobCPPhone.PhoneNumber = phone.PhoneNumber;

                    mobCPPhone.Attention = phone.Attention;
                    mobCPPhone.ChannelCode = phone.ChannelCode;
                    mobCPPhone.ChannelCodeDescription = phone.ChannelCodeDescription;
                    mobCPPhone.ChannelTypeCode = Convert.ToString(phone.ChannelTypeCode);
                    mobCPPhone.ChannelTypeDescription = phone.ChannelTypeDescription;
                    mobCPPhone.ChannelTypeDescription = phone.ChannelTypeDescription;
                    mobCPPhone.ChannelTypeSeqNumber = phone.ChannelTypeSeqNum;
                    mobCPPhone.CountryCode = phone.CountryCode;
                    //mobCPPhone.CountryCode = GetAccessCode(phone.CountryCode);
                    mobCPPhone.CountryPhoneNumber = phone.CountryPhoneNumber;
                    mobCPPhone.CustomerId = phone.CustomerId;
                    mobCPPhone.Description = phone.Description;
                    mobCPPhone.DiscontinuedDate = Convert.ToString(phone.DiscontinuedDate);
                    mobCPPhone.EffectiveDate = Convert.ToString(phone.EffectiveDate);
                    mobCPPhone.ExtensionNumber = phone.ExtensionNumber;
                    mobCPPhone.IsPrimary = phone.IsPrimary;
                    mobCPPhone.IsPrivate = phone.IsPrivate;
                    mobCPPhone.IsProfileOwner = phone.IsProfileOwner;
                    mobCPPhone.Key = phone.Key;
                    mobCPPhone.LanguageCode = phone.LanguageCode;
                    mobCPPhone.PagerPinNumber = phone.PagerPinNumber;
                    mobCPPhone.SharesCountryCode = phone.SharesCountryCode;
                    mobCPPhone.WrongPhoneDate = Convert.ToString(phone.WrongPhoneDate);
                    if (phone.PhoneDevices != null && phone.PhoneDevices.Count > 0)
                    {
                        mobCPPhone.DeviceTypeCode = phone.PhoneDevices[0].CommDeviceTypeCode;
                        mobCPPhone.DeviceTypeDescription = phone.PhoneDevices[0].CommDeviceTypeDescription;
                    }
                    mobCPPhone.IsDayOfTravel = phone.IsDayOfTravel;

                    if (_configuration.GetValue<bool>("CorporateConcurBooking"))
                    {
                        #region
                        if (IsCorpBookingPath && isCorpPhonePresent && !onlyDayOfTravelContact && phone.ChannelTypeDescription.ToLower() == "corporate")
                        {
                            //return the corporate phone number
                            primaryMobCPPhone = new MOBCPPhone();
                            mobCPPhone.IsPrimary = true;
                            primaryMobCPPhone = mobCPPhone;
                            break;

                        }
                        if (IsCorpBookingPath && isCorpPhonePresent && !onlyDayOfTravelContact && phone.ChannelTypeDescription.ToLower() != "corporate")
                        {
                            //There is corporate phone number present, continue till corporate phone number is found
                            continue;
                        }
                        #endregion
                    }

                    if (phone.IsDayOfTravel)
                    {
                        primaryMobCPPhone = new MOBCPPhone();
                        primaryMobCPPhone = mobCPPhone;// Only day of travel contact should be returned to use at Edit Traveler
                        if (onlyDayOfTravelContact)
                        {
                            break;
                        }
                    }
                    if (!onlyDayOfTravelContact)
                    {
                        if (phone.IsPrimary)
                        {
                            primaryMobCPPhone = new MOBCPPhone();
                            primaryMobCPPhone = mobCPPhone;
                            break;
                        }
                        else if (co == 1)
                        {
                            primaryMobCPPhone = new MOBCPPhone();
                            primaryMobCPPhone = mobCPPhone;
                        }
                    }
                    #endregion
                }
                if (primaryMobCPPhone != null)
                {
                    mobCPPhones.Add(primaryMobCPPhone);
                }
                GeneralHelper.DisableUSCultureInfo(ci);
            }
            return mobCPPhones;
        }
        private List<MOBBKLoyaltyProgramProfile> GetTravelerLoyaltyProfile(List<United.Services.Customer.Common.AirPreference> airPreferences, int currentEliteLevel)
        {
            List<MOBBKLoyaltyProgramProfile> programs = new List<MOBBKLoyaltyProgramProfile>();
            //if(airPreferences != null && airPreferences.Count > 0 && airPreferences[0].AirRewardPrograms != null && airPreferences[0].AirRewardPrograms.Count > 0) 
            if (airPreferences != null && airPreferences.Count > 0)
            {
                #region
                List<United.Services.Customer.Common.AirPreference> airPreferencesList = new List<Services.Customer.Common.AirPreference>();
                airPreferencesList = (from item in airPreferences
                                      where item.AirRewardPrograms != null && item.AirRewardPrograms.Count > 0
                                      select item).ToList();
                //foreach(United.Services.Customer.Common.RewardProgram rewardProgram in airPreferences[0].AirRewardPrograms) 
                if (airPreferencesList != null && airPreferencesList.Count > 0)
                {
                    foreach (United.Services.Customer.Common.RewardProgram rewardProgram in airPreferencesList[0].AirRewardPrograms)
                    {
                        MOBBKLoyaltyProgramProfile airRewardProgram = new MOBBKLoyaltyProgramProfile();
                        airRewardProgram.ProgramId = rewardProgram.ProgramID.ToString();
                        airRewardProgram.ProgramName = rewardProgram.Description;
                        airRewardProgram.MemberId = rewardProgram.ProgramMemberId;
                        airRewardProgram.CarrierCode = rewardProgram.VendorCode;
                        if (airRewardProgram.CarrierCode.Trim().Equals("UA"))
                        {
                            airRewardProgram.MPEliteLevel = currentEliteLevel;
                        }
                        airRewardProgram.RewardProgramKey = rewardProgram.Key;
                        programs.Add(airRewardProgram);
                    }
                }
                #endregion
            }
            return programs;
        }
        public async Task<MOBCPMileagePlus> GetCurrentEliteLevelFromAirPreferences(List<AirPreference> airPreferences, string sessionid)
        {
            MOBCPMileagePlus mobCPMileagePlus = null;
            if (_configuration.GetValue<bool>("BugFixToggleFor17M") &&
                airPreferences != null &&
                airPreferences.Count > 0 &&
                airPreferences[0].AirRewardPrograms != null &&
                airPreferences[0].AirRewardPrograms.Count > 0)
            {
                mobCPMileagePlus = await GetCurrentEliteLevelFromAirRewardProgram(airPreferences, sessionid).ConfigureAwait(false);
            }

            return mobCPMileagePlus;
        }
        private async Task<MOBCPMileagePlus> GetCurrentEliteLevelFromAirRewardProgram(List<AirPreference> airPreferences, string sessionid)
        {
            MOBCPMileagePlus mobCPMileagePlus = null;
            var airRewardProgram = airPreferences[0].AirRewardPrograms[0];
            if (!string.IsNullOrEmpty(airRewardProgram.ProgramMemberId))
            {
                Session session = new Session();
                session = await _sessionHelperService.GetSession<Session>(_headers.ContextValues.SessionId, session.ObjectName, new List<string>() { _headers.ContextValues.SessionId, session.ObjectName }).ConfigureAwait(false);

                MOBCPProfileRequest request = new MOBCPProfileRequest();
                request.CustomerId = 0;
                request.MileagePlusNumber = airRewardProgram.ProgramMemberId;
                United.Services.Customer.Common.ProfileRequest profileRequest = GetProfileRequest(request);
                string jsonRequest = JsonSerializer.Serialize<United.Services.Customer.Common.ProfileRequest>(profileRequest);
                string url = string.Format("/GetProfile");

                //Utility utility = new Utility();
                var jsonresponse = await MakeHTTPPostAndLogIt(session.SessionId, session.DeviceID, "GetProfileForTravelerToGetEliteLevel", session.AppID, string.Empty, session.Token, url, jsonRequest).ConfigureAwait(false);
                mobCPMileagePlus = GetOwnerEliteLevelFromCslResponse(jsonresponse);
            }
            return mobCPMileagePlus;
        }

        private MOBCPMileagePlus GetOwnerEliteLevelFromCslResponse(string jsonresponse)
        {
            MOBCPMileagePlus mobCPMileagePlus = null;
            if (!string.IsNullOrEmpty(jsonresponse))
            {
                United.Services.Customer.Common.ProfileResponse response = JsonSerializer.Deserialize<United.Services.Customer.Common.ProfileResponse>(jsonresponse);
                if (response != null && response.Status.Equals(United.Services.Customer.Common.Constants.StatusType.Success) &&
                    response.Profiles != null &&
                    response.Profiles.Count > 0 &&
                    response.Profiles[0].Travelers != null &&
                    response.Profiles[0].Travelers.Exists(p => p.IsProfileOwner))
                {
                    var owner = response.Profiles[0].Travelers.First(p => p.IsProfileOwner);
                    if (owner != null & owner.MileagePlus != null)
                    {
                        mobCPMileagePlus = PopulateMileagePlus(owner.MileagePlus);
                    }
                }
            }

            return mobCPMileagePlus;
        }

        private async Task<string> MakeHTTPPostAndLogIt(string sessionId, string deviceId, string action, int applicationId, string appVersion, string token, string url, string jsonRequest, bool isXMLRequest = false)
        {
            string jsonResponse = string.Empty;

            string paypalCSLCallDurations = string.Empty;

            var response = await _customerDataService.GetCustomerData<United.Services.Customer.Common.ProfileRequest>(token, sessionId, jsonRequest).ConfigureAwait(false);
            //check
            return response.ToString();
        }

        private string GetPaxDescriptionByDOB(string date, string deptDateFLOF)
        {
            int age = TopHelper.GetAgeByDOB(date, deptDateFLOF);
            if ((18 <= age) && (age <= 64))
            {
                return "Adult (18-64)";
            }
            else
            if ((2 <= age) && (age < 5))
            {
                return "Child (2-4)";
            }
            else
            if ((5 <= age) && (age <= 11))
            {
                return "Child (5-11)";
            }
            else
            //if((12 <= age) && (age <= 17))
            //{

            //}
            if ((12 <= age) && (age <= 14))
            {
                return "Child (12-14)";
            }
            else
            if ((15 <= age) && (age <= 17))
            {
                return "Child (15-17)";
            }
            else
            if (65 <= age)
            {
                return "Senior (65+)";
            }
            else if (age < 2)
                return "Infant (under 2)";

            return string.Empty;
        }
        public string GetYAPaxDescByDOB()
        {
            return "Young adult (18-23)";
        }
        private bool EnableYADesc(bool isReshop = false)
        {
            return _configuration.GetValue<bool>("EnableYoungAdultBooking") && _configuration.GetValue<bool>("EnableYADesc") && !isReshop;
        }
        public List<MOBCPSecureTraveler> PopulatorSecureTravelers(List<United.Services.Customer.Common.SecureTraveler> secureTravelers, ref bool isTSAFlag, bool correctDate, string sessionID, int appID, string deviceID)
        {
            List<MOBCPSecureTraveler> mobSecureTravelers = null;
            try
            {
                if (secureTravelers != null && secureTravelers.Count > 0)
                {
                    mobSecureTravelers = new List<MOBCPSecureTraveler>();
                    int secureTravelerCount = 0;
                    foreach (var secureTraveler in secureTravelers)
                    {
                        if (secureTraveler.DocumentType != null && secureTraveler.DocumentType.Trim().ToUpper() != "X" && secureTravelerCount < 3)
                        {
                            #region
                            MOBCPSecureTraveler mobSecureTraveler = new MOBCPSecureTraveler();
                            if (correctDate)
                            {
                                DateTime tempBirthDate = secureTraveler.BirthDate.GetValueOrDefault().AddHours(1);
                                mobSecureTraveler.BirthDate = tempBirthDate.ToString("MM/dd/yyyy", CultureInfo.CurrentCulture);
                            }
                            else
                            {
                                mobSecureTraveler.BirthDate = secureTraveler.BirthDate.GetValueOrDefault().ToString("MM/dd/yyyy", CultureInfo.CurrentCulture);
                            }
                            mobSecureTraveler.CustomerId = secureTraveler.CustomerId;
                            mobSecureTraveler.DecumentType = secureTraveler.DocumentType;
                            mobSecureTraveler.Description = secureTraveler.Description;
                            mobSecureTraveler.FirstName = secureTraveler.FirstName;
                            mobSecureTraveler.Gender = secureTraveler.Gender;
                            mobSecureTraveler.Key = secureTraveler.Key;
                            mobSecureTraveler.LastName = secureTraveler.LastName;
                            mobSecureTraveler.MiddleName = secureTraveler.MiddleName;
                            mobSecureTraveler.SequenceNumber = secureTraveler.SequenceNumber;
                            mobSecureTraveler.Suffix = secureTraveler.Suffix;
                            if (secureTraveler.SupplementaryTravelInfos != null)
                            {
                                foreach (Services.Customer.Common.SupplementaryTravelInfo supplementaryTraveler in secureTraveler.SupplementaryTravelInfos)
                                {
                                    if (supplementaryTraveler.Type == Services.Customer.Common.Constants.SupplementaryTravelInfoNumberType.KnownTraveler)
                                    {
                                        mobSecureTraveler.KnownTravelerNumber = supplementaryTraveler.Number;
                                    }
                                    if (supplementaryTraveler.Type == Services.Customer.Common.Constants.SupplementaryTravelInfoNumberType.Redress)
                                    {
                                        mobSecureTraveler.RedressNumber = supplementaryTraveler.Number;
                                    }
                                }
                            }
                            if (!isTSAFlag && secureTraveler.DocumentType.Trim().ToUpper() == "U")
                            {
                                isTSAFlag = true;
                            }
                            if (secureTraveler.DocumentType.Trim().ToUpper() == "C" || secureTraveler.DocumentType.Trim() == "") // This is to get only Customer Cleared Secure Traveler records
                            {
                                mobSecureTravelers = new List<MOBCPSecureTraveler>();
                                mobSecureTravelers.Add(mobSecureTraveler);
                                secureTravelerCount = 4;
                            }
                            else
                            {
                                mobSecureTravelers.Add(mobSecureTraveler);
                                secureTravelerCount = secureTravelerCount + 1;
                            }
                            #endregion
                        }
                        else if (secureTravelerCount > 3)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("PopulatorSecureTravelers {@Exception} for {@SecureTravelers}", JsonConvert.SerializeObject(ex), JsonConvert.SerializeObject(secureTravelers));
            }

            return mobSecureTravelers;
        }
        private MOBCPMileagePlus PopulateMileagePlus(United.Services.Customer.Common.MileagePlus onePass)
        {
            MOBCPMileagePlus mileagePlus = null;
            if (onePass != null)
            {
                mileagePlus = new MOBCPMileagePlus();
                mileagePlus.AccountBalance = onePass.AccountBalance;
                mileagePlus.ActiveStatusCode = onePass.ActiveStatusCode;
                mileagePlus.ActiveStatusDescription = onePass.ActiveStatusDescription;
                mileagePlus.AllianceEliteLevel = onePass.AllianceEliteLevel;
                mileagePlus.ClosedStatusCode = onePass.ClosedStatusCode;
                mileagePlus.ClosedStatusDescription = onePass.ClosedStatusDescription;
                mileagePlus.CurrentEliteLevel = onePass.CurrentEliteLevel;
                if (onePass.CurrentEliteLevelDescription != null)
                {
                    mileagePlus.CurrentEliteLevelDescription = onePass.CurrentEliteLevelDescription.ToString().ToUpper() == "NON-ELITE" ? "General member" : onePass.CurrentEliteLevelDescription;
                }
                mileagePlus.CurrentYearMoneySpent = onePass.CurrentYearMoneySpent;
                mileagePlus.EliteMileageBalance = onePass.EliteMileageBalance;
                mileagePlus.EliteSegmentBalance = Convert.ToInt32(onePass.EliteSegmentBalance);
                //mileagePlus.EliteSegmentDecimalPlaceValue = onePass.elite;
                mileagePlus.EncryptedPin = onePass.EncryptedPin;
                mileagePlus.EnrollDate = onePass.EnrollDate.GetValueOrDefault().ToString("MM/dd/yyyy");
                mileagePlus.EnrollSourceCode = onePass.EnrollSourceCode;
                mileagePlus.EnrollSourceDescription = onePass.EnrollSourceDescription;
                mileagePlus.FlexPqmBalance = onePass.FlexPQMBalance;
                mileagePlus.FutureEliteDescription = onePass.FutureEliteLevelDescription;
                mileagePlus.FutureEliteLevel = onePass.FutureEliteLevel;
                mileagePlus.InstantEliteExpirationDate = onePass.FutureEliteLevelDescription;
                mileagePlus.IsCEO = onePass.IsCEO;
                mileagePlus.IsClosedPermanently = onePass.IsClosedPermanently;
                mileagePlus.IsClosedTemporarily = onePass.IsClosedTemporarily;
                mileagePlus.IsCurrentTrialEliteMember = onePass.IsCurrentTrialEliteMember;
                mileagePlus.IsFlexPqm = onePass.IsFlexPQM;
                mileagePlus.IsInfiniteElite = onePass.IsInfiniteElite;
                mileagePlus.IsLifetimeCompanion = onePass.IsLifetimeCompanion;
                mileagePlus.IsLockedOut = onePass.IsLockedOut;
                mileagePlus.IsPresidentialPlus = onePass.IsPresidentialPlus;
                mileagePlus.IsUnitedClubMember = onePass.IsPClubMember;
                mileagePlus.Key = onePass.Key;
                mileagePlus.LastActivityDate = onePass.LastActivityDate;
                mileagePlus.LastExpiredMile = onePass.LastExpiredMile;
                mileagePlus.LastFlightDate = onePass.LastFlightDate;
                mileagePlus.LastStatementBalance = onePass.LastStatementBalance;
                mileagePlus.LastStatementDate = onePass.LastStatementDate.GetValueOrDefault().ToString("MM/dd/yyyy");
                mileagePlus.LifetimeEliteMileageBalance = onePass.LifetimeEliteMileageBalance;
                mileagePlus.MileageExpirationDate = onePass.MileageExpirationDate;
                mileagePlus.MileagePlusId = onePass.MileagePlusId;
                mileagePlus.MileagePlusPin = onePass.MileagePlusPIN;
                mileagePlus.NextYearEliteLevel = onePass.NextYearEliteLevel;
                mileagePlus.NextYearEliteLevelDescription = onePass.NextYearEliteLevelDescription;
                mileagePlus.PriorUnitedAccountNumber = onePass.PriorUnitedAccountNumber;
                mileagePlus.StarAllianceEliteLevel = onePass.SkyTeamEliteLevelCode;
                if (!_configuration.GetValue<bool>("Keep_MREST_MP_EliteLevel_Expiration_Logic"))
                {
                    mileagePlus.PremierLevelExpirationDate = onePass.PremierLevelExpirationDate;
                    if (onePass.CurrentYearInstantElite != null)
                    {
                        mileagePlus.InstantElite = new MOBInstantElite()
                        {
                            ConsolidatedCode = onePass.CurrentYearInstantElite.ConsolidatedCode,
                            EffectiveDate = onePass.CurrentYearInstantElite.EffectiveDate != null ? onePass.CurrentYearInstantElite.EffectiveDate.ToString("MM/dd/yyyy") : string.Empty,
                            EliteLevel = onePass.CurrentYearInstantElite.EliteLevel,
                            EliteYear = onePass.CurrentYearInstantElite.EliteYear,
                            ExpirationDate = onePass.CurrentYearInstantElite.ExpirationDate != null ? onePass.CurrentYearInstantElite.ExpirationDate.ToString("MM/dd/yyyy") : string.Empty,
                            PromotionCode = onePass.CurrentYearInstantElite.PromotionCode
                        };
                    }
                }
            }

            return mileagePlus;
        }
        private MOBCPCustomerMetrics PopulateCustomerMetrics(United.Services.Customer.Common.CustomerMetrics customerMetrics)
        {
            MOBCPCustomerMetrics travelerCustomerMetrics = new MOBCPCustomerMetrics();
            if (customerMetrics != null && customerMetrics.PTCCode != null)
            {
                travelerCustomerMetrics.PTCCode = customerMetrics.PTCCode;
            }
            return travelerCustomerMetrics;
        }
        private async Task<Reservation> PersistedReservation(MOBCPProfileRequest request)
        {
            Reservation persistedReservation =
                new Reservation();
            if (request != null)
                persistedReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, persistedReservation.ObjectName, new List<string> { request.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);

            if (_configuration.GetValue<bool>("CorporateConcurBooking"))
            {
                if (persistedReservation != null && persistedReservation.ShopReservationInfo != null &&
                    persistedReservation.ShopReservationInfo.IsCorporateBooking)
                {
                    this.IsCorpBookingPath = true;
                }

                if (persistedReservation != null && persistedReservation.ShopReservationInfo2 != null &&
                    persistedReservation.ShopReservationInfo2.IsArrangerBooking)
                {
                    this.IsArrangerBooking = true;
                }
            }
            return persistedReservation;
        }

        public async Task<MOBUpdateTravelerInfoResponse> UpdateTravelerMPId(string deviceId, string accessCode, string recordLocator, string sessionId, string transactionId, string languageCode, int applicationId, string appVersion, string mileagePlusNumber, string firstName, string lastName, string sharesPosition, string token)
        {

            MOBUpdateTravelerInfoResponse response = new MOBUpdateTravelerInfoResponse();

            try
            {
                MOBUpdateTravelerInfoRequest request = CreateTravelerInfoRequest(deviceId, accessCode, recordLocator, sessionId, transactionId, languageCode, applicationId, appVersion, mileagePlusNumber, firstName, lastName, sharesPosition);

                if (request == null)
                    throw new MOBUnitedException("UpdateTravelerMPId request cannot be null.");

                MOBPNRPassenger travelerInfo = request.TravelersInfo[0]; //Fetching the first item from traveler Info

                List<United.Service.Presentation.ReservationModel.Traveler> travelerInfoMPKTNRequest = UpdateTravelerInfoMPRedressKTNRequest(request);
                string jsonRequest = JsonConvert.SerializeObject(travelerInfoMPKTNRequest);

                //MPNumber CSL Call
                await UpdateTravlerMPorFFInfo(response, request, token, jsonRequest, travelerInfo, true).ConfigureAwait(false);

                if (response.Exception != null)
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("AssociateMPNumberFailErrorMsg"));
                }
            }
            catch (System.Net.WebException wex)
            {
                if (wex.Response != null)
                {
                    var errorResponse = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                    throw new System.Exception(wex.Message);
                }
            }

            return response;
        }

        private MOBUpdateTravelerInfoRequest CreateTravelerInfoRequest(string deviceId, string accessCode, string recordLocator, string sessionId, string transactionId, string languageCode, int appId, string appVersion, string mileagePlusNumber, string firstName, string lastName, string sharesPosition)
        {
            MOBUpdateTravelerInfoRequest request = new MOBUpdateTravelerInfoRequest();

            request.AccessCode = accessCode;
            request.DeviceId = deviceId;
            request.LanguageCode = languageCode;
            request.SessionId = sessionId;
            request.TransactionId = transactionId;
            request.RecordLocator = recordLocator;
            request.Application = new MOBApplication();
            request.Application.Id = appId;
            request.Application.Version = new MOBVersion();
            request.Application.Version.Major = appVersion;
            request.TravelersInfo = new List<MOBPNRPassenger>();
            request.TravelersInfo.Add(new MOBPNRPassenger()
            {
                MileagePlus = new MOBCPMileagePlus()
                {
                    MileagePlusId = mileagePlusNumber
                },
                PassengerName = new MOBName()
                {
                    First = firstName,
                    Last = lastName
                },
                SharesGivenName = firstName,
                SHARESPosition = sharesPosition,
            });

            return request;
        }

        private List<United.Service.Presentation.ReservationModel.Traveler> UpdateTravelerInfoMPRedressKTNRequest(MOBUpdateTravelerInfoRequest request)
        {
            List<United.Service.Presentation.ReservationModel.Traveler>
                _TravellerList = new List<United.Service.Presentation.ReservationModel.Traveler>();


            request.TravelersInfo.ForEach(traveler =>
            {
                Service.Presentation.PersonModel.Person person;
                LoyaltyProgramProfile loyaltyProgramProfile;

                if ((string.IsNullOrEmpty(traveler.KnownTravelerNumber) == false || string.IsNullOrEmpty(traveler.RedressNumber) == false))
                {
                    person = new Service.Presentation.PersonModel.Person
                    {
                        Documents = new Collection<Service.Presentation.PersonModel.Document>() {
                        new Service.Presentation.PersonModel.Document {
                            KnownTravelerNumber = traveler.KnownTravelerNumber,
                            RedressNumber = traveler.RedressNumber
                        }
                    },
                        GivenName = (!string.IsNullOrEmpty(traveler.SharesGivenName)
                        && !string.Equals(traveler.SharesGivenName, traveler.PassengerName.First, StringComparison.OrdinalIgnoreCase))
                        ? traveler.SharesGivenName
                        : traveler.PassengerName.First,

                        Surname = traveler.PassengerName.Last,
                        InfantIndicator = "FALSE",
                        Key = traveler.SHARESPosition
                    };
                }
                else
                {
                    if (traveler.PassengerName != null)
                    {
                        person = new Service.Presentation.PersonModel.Person
                        {
                            GivenName = (!string.IsNullOrEmpty(traveler.SharesGivenName)
                        && !string.Equals(traveler.SharesGivenName, traveler.PassengerName.First, StringComparison.OrdinalIgnoreCase))
                        ? traveler.SharesGivenName
                        : traveler.PassengerName.First,

                            Surname = traveler.PassengerName.Last,
                            InfantIndicator = "FALSE",
                            Key = traveler.SHARESPosition
                        };
                    }
                    else
                    {
                        person = null;
                    }
                }

                if (traveler.MileagePlus != null && string.IsNullOrEmpty(traveler.MileagePlus.MileagePlusId) == false)
                {
                    loyaltyProgramProfile = new LoyaltyProgramProfile
                    {
                        LoyaltyProgramCarrierCode = "UA",
                        LoyaltyProgramMemberID = traveler.MileagePlus.MileagePlusId
                    };
                }
                else if (traveler.OaRewardPrograms != null)
                {
                    loyaltyProgramProfile = new LoyaltyProgramProfile
                    {
                        LoyaltyProgramCarrierCode = traveler.OaRewardPrograms[0].VendorCode,
                        LoyaltyProgramMemberID = traveler.OaRewardPrograms[0].ProgramMemberId

                    };
                }
                else
                {
                    loyaltyProgramProfile = null;
                }

                _TravellerList.Add(new United.Service.Presentation.ReservationModel.Traveler
                {
                    Person = person,
                    LoyaltyProgramProfile = loyaltyProgramProfile
                });

            });

            return _TravellerList;
        }

        private async Task<MOBUpdateTravelerInfoResponse> UpdateTravlerMPorFFInfo
           (MOBUpdateTravelerInfoResponse response, MOBUpdateTravelerInfoRequest request, string Token, string cslJsonRequest, MOBPNRPassenger travelerInfo, bool associateMPidFlow = false)
        {
            var urlLoyaltyId = string.Empty;
            var jsonResponseLoyaltyId = string.Empty;

            bool hasMilagePlus = (travelerInfo.MileagePlus != null && !string.IsNullOrEmpty(travelerInfo.MileagePlus.MileagePlusId));
            bool hasRewardProgram = (travelerInfo.OaRewardPrograms != null && travelerInfo.OaRewardPrograms.Any() && !string.IsNullOrEmpty(travelerInfo.OaRewardPrograms[0].ProgramMemberId));

            try
            {
                if (hasMilagePlus || hasRewardProgram)
                {
                    urlLoyaltyId = string.Format("/{0}/Passengers/Loyalty?RetrievePNR=true&EndTransaction=true", request.RecordLocator);
                    //_configuration.GetValue(("ManageRes_EditTraveler"), request.RecordLocator));

                    jsonResponseLoyaltyId = await _pNRRetrievalService.UpdateTravelerInfo(Token, cslJsonRequest, urlLoyaltyId, request.SessionId).ConfigureAwait(false);
                }

                //If Response NOT-NULL Check for MP Number / FREQ Flyer Number 
                if (!string.IsNullOrEmpty(jsonResponseLoyaltyId))
                {
                    List<United.Service.Presentation.CommonModel.Message>
                       msgResponseLoyaltyId = JsonConvert.DeserializeObject<List<United.Service.Presentation.CommonModel.Message>>(jsonResponseLoyaltyId);

                    if (associateMPidFlow && msgResponseLoyaltyId != null && msgResponseLoyaltyId[0].Type.ToLower() == "failed")
                    {
                        response.Exception = new MOBException("9999", _configuration.GetValue<string>("AssociateMPNumberFailErrorMsg"));
                    }

                    if (msgResponseLoyaltyId != null && string.Equals
                        (msgResponseLoyaltyId[0].Status, "SHARE_RESPONSE", StringComparison.OrdinalIgnoreCase))
                    {
                        if ((hasRewardProgram && msgResponseLoyaltyId[0].Text.IndexOf(travelerInfo.OaRewardPrograms[0].ProgramMemberId.ToUpper()) == -1) ||
                            (hasMilagePlus && msgResponseLoyaltyId[0].Text.IndexOf(travelerInfo.MileagePlus.MileagePlusId.ToUpper()) == -1))
                        {
                            response.Exception = new MOBException("9999", _configuration.GetValue<string>("ValidateManageResMPNameMisMatchErrorMessage"));
                        }
                    }
                }
            }
            catch (System.Net.WebException wex)
            {
                if (wex.Response != null)
                {
                    var errorResponse = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();

                    //if (traceSwitch.TraceInfo)
                    //    LogEntries.Add(United.Logger.LogEntry.GetLogEntry<string>(request.SessionId, "UpdateTravelerInfo-MP/FF", "Exception", errorResponse));
                    //response.Exception = new MOBException("9999", _configuration.GetValue<string>("ValidateManageResMPNameMisMatchErrorMessage"));
                }
            }
            return response;
        }

        #endregion


        public bool IsInternationalBillingAddress_CheckinFlowEnabled(MOBApplication application)
        {
            if (_configuration.GetValue<bool>("EnableInternationalBillingAddress_CheckinFlow"))
            {
                if (application != null && GeneralHelper.IsApplicationVersionGreater(application.Id, application?.Version?.Major, "IntBillingCheckinFlowAndroidversion", "IntBillingCheckinFlowiOSversion", "", "", true, _configuration))
                {
                    return true;
                }
            }
            return false;
        }

        #region UCB Migration MobilePhase3
        public async Task<List<MOBTypeOption>> GetProfileDisclaimerList()
        {

            List<MOBLegalDocument> profileDisclaimerList = await GetLegalDocumentsForTitles("ProfileDisclamerList").ConfigureAwait(false);
            List<MOBTypeOption> disclaimerList = new List<MOBTypeOption>();
            List<MOBTypeOption> travelerDisclaimerTextList = new List<MOBTypeOption>();

            List<string> mappingTextList = _configuration.GetValue<string>("Booking20TravelerDisclaimerMapping").Split('~').ToList();
            foreach (string mappingText in mappingTextList)
            {
                string disclaimerTextTitle = mappingText.Split('=')[0].ToString().Trim();
                List<string> travelerTextTitleList = mappingText.Split('=')[1].ToString().Split('|').ToList();
                int co = 0;
                foreach (string travelerTextTile in travelerTextTitleList)
                {
                    if (profileDisclaimerList != null)
                    {
                        foreach (MOBLegalDocument legalDocument in profileDisclaimerList)
                        {
                            if (legalDocument.Title.ToUpper().Trim() == travelerTextTile.ToUpper().Trim())
                            {
                                MOBTypeOption typeOption = new MOBTypeOption();
                                co++;
                                typeOption.Key = disclaimerTextTitle + co.ToString();
                                typeOption.Value = legalDocument.LegalDocument;
                                travelerDisclaimerTextList.Add(typeOption);
                            }
                        }
                    }
                }
            }
            return travelerDisclaimerTextList;
        }

        private async Task<List<MOBLegalDocument>> GetLegalDocumentsForTitles(string titles)
        {
            var legalDocuments = new List<MOBLegalDocument>();
            legalDocuments = await _dynamoDBUtility.GetNewLegalDocumentsForTitles(titles, _headers.ContextValues.TransactionId, true).ConfigureAwait(false);

            if (legalDocuments == null)
            {
                legalDocuments = new List<United.Definition.MOBLegalDocument>();
            }
            return legalDocuments;
        }
        #endregion

        public United.Services.Customer.Common.ProfileRequest GetProfileRequest(MOBCPProfileRequest mobCPProfileRequest, bool getEmployeeIdFromCSLCustomerData = false)
        {
            United.Services.Customer.Common.ProfileRequest request = new United.Services.Customer.Common.ProfileRequest();
            request.DataSetting = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices");
            if (mobCPProfileRequest.CustomerId != 0)
            {
                request.CustomerId = mobCPProfileRequest.CustomerId;
            }

            List<string> requestStringList = new List<string>();
            if (mobCPProfileRequest.ProfileOwnerOnly)
            {
                requestStringList.Add("ProfileOwnerOnly");
                if (mobCPProfileRequest.IncludeCreditCards)
                {
                    requestStringList.Add("CreditCards");
                }
            }
            else
            {
                if (mobCPProfileRequest.IncludeAllTravelerData)
                {
                    requestStringList.Add("AllTravelerData");
                    requestStringList.Add("TravelerData");
                }
                else
                {
                    #region
                    requestStringList.Add("TravelerData");
                    if (mobCPProfileRequest.IncludeAddresses)
                    {
                        requestStringList.Add("Addresses");
                    }
                    if (mobCPProfileRequest.IncludeEmailAddresses)
                    {
                        requestStringList.Add("EmailAddresses");
                    }
                    if (mobCPProfileRequest.IncludePhones)
                    {
                        requestStringList.Add("Phones");
                    }
                    if (mobCPProfileRequest.IncludeSubscriptions)
                    {
                        requestStringList.Add("Subscriptions");
                    }
                    if (mobCPProfileRequest.IncludeTravelMarkets)
                    {
                        requestStringList.Add("TravelMarkets");
                    }
                    if (mobCPProfileRequest.IncludeCustomerProfitScore)
                    {
                        requestStringList.Add("CustomerProfitScore");
                    }
                    if (mobCPProfileRequest.IncludePets)
                    {
                        requestStringList.Add("Pets");
                    }
                    if (mobCPProfileRequest.IncludeCarPreferences)
                    {
                        requestStringList.Add("CarPreferences");
                    }
                    if (mobCPProfileRequest.IncludeDisplayPreferences)
                    {
                        requestStringList.Add("DisplayPreferences");
                    }
                    if (mobCPProfileRequest.IncludeHotelPreferences)
                    {
                        requestStringList.Add("HotelPreferences");
                    }
                    if (mobCPProfileRequest.IncludeAirPreferences)
                    {
                        requestStringList.Add("AirPreferences");
                    }
                    if (mobCPProfileRequest.IncludeContacts)
                    {
                        requestStringList.Add("Contacts");
                    }
                    if (mobCPProfileRequest.IncludePassports)
                    {
                        requestStringList.Add("Passports");
                    }
                    if (mobCPProfileRequest.IncludeSecureTravelers)
                    {
                        requestStringList.Add("SecureTravelers");
                    }
                    if (mobCPProfileRequest.IncludeFlexEQM)
                    {
                        requestStringList.Add("FlexEQM");
                    }
                    if (mobCPProfileRequest.IncludeCreditCards)
                    {
                        requestStringList.Add("CreditCards");
                    }
                    if (mobCPProfileRequest.IncludeServiceAnimals)
                    {
                        requestStringList.Add("ServiceAnimals");
                    }
                    if (mobCPProfileRequest.IncludeSpecialRequests)
                    {
                        requestStringList.Add("SpecialRequests");
                    }
                    if (mobCPProfileRequest.IncludePosCountyCode)
                    {
                        requestStringList.Add("PosCountyCode");
                    }
                    #endregion
                }
            }
            if (requestStringList.Count == 0)
            {
                requestStringList.Add("AllTravelerData"); // This option means return all the data
            }

            if (getEmployeeIdFromCSLCustomerData)
            {
                requestStringList.Add("EmployeeLinkage");
            }
            if (_configuration.GetValue<bool>("CorporateConcurBooking") && IsCorpBookingPath)
            {
                requestStringList.Add("CorporateCC");
            }
            request.DataToLoad = requestStringList;

            if (mobCPProfileRequest.ReturnAllSavedTravelers || mobCPProfileRequest.CustomerId == 0)
            {
                if (!string.IsNullOrEmpty(mobCPProfileRequest.MileagePlusNumber))
                {
                    request.LoyaltyId = mobCPProfileRequest.MileagePlusNumber;
                }
                else
                {
                    throw new MOBUnitedException("Profile Owner MileagePlus number is required.");
                }
            }
            else
            {
                request.MemberCustomerIdsToLoad = new List<int>();
                request.MemberCustomerIdsToLoad.Add(mobCPProfileRequest.CustomerId);
            }

            return request;
        }

    }
}