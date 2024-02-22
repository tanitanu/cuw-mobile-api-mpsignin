using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using United.CorporateDirect.Models.CustomerProfile;
using United.Definition.CSLModels.CustomerProfile;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.CSLSerivce;
using United.Mobile.DataAccess.MPSignIn;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.CSLModels;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.Internal.Exception;
using United.Mobile.Model.Shopping;
using United.Utility.AppVersion;
using United.Utility.Enum;
using United.Utility.Helper;
using Address = United.Mobile.Model.CSLModels.Address;
using Email = United.Mobile.Model.CSLModels.Email;
using Phone = United.Mobile.Model.CSLModels.Phone;

namespace United.Common.Helper.Profile
{
    public class UCBProfile : IUCBProfile
    {
        private readonly ICacheLog<UCBProfile> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly ICorporateGetService _corporateGetService;
        private readonly IUtility _utility;
        private readonly IProfileCreditCard _profileCreditCard;
        private readonly ILoyaltyUCBService _loyaltyBalanceServices;
        private readonly IUCBProfileService _profileService;
        private readonly IMPTraveler _mPTraveler;
        private bool IsCorpBookingPath = false;
        private bool IsArrangerBooking = false;
        private readonly IFeatureToggles _featureToggles;

        public UCBProfile(ICacheLog<UCBProfile> logger
            , IConfiguration configuration
            , ISessionHelperService sessionHelperService
            , ICorporateGetService corporateGetService
            , IUtility utility
            , IProfileCreditCard profileCreditCard
            , ILoyaltyUCBService loyaltyBalanceServices
            , IUCBProfileService profileService
            , IMPTraveler mPTraveler
            , IFeatureToggles featureToggles)
        {
            _logger = logger;
            _configuration = configuration;
            _sessionHelperService = sessionHelperService;
            _corporateGetService = corporateGetService;
            _utility = utility;
            _profileCreditCard = profileCreditCard;
            _loyaltyBalanceServices = loyaltyBalanceServices;
            _profileService = profileService;
            _mPTraveler = mPTraveler;
            _featureToggles = featureToggles;
        }
        public async Task<List<MOBCPProfile>> GetProfileV2(MOBCPProfileRequest request, bool getMPSecurityDetails = false)
        {
            if (request == null)
            {
                throw new MOBUnitedException("Profile request cannot be null.");
            }
            United.Mobile.Model.Shopping.Reservation persistedReservation = !getMPSecurityDetails ? PersistedReservation(request).Result : null;

            List<MOBCPProfile> profiles = null;

            if (persistedReservation != null)
            {
                if (persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.IsYATravel)
                {
                    request.ProfileOwnerOnly = true;//For young Adult profile owner only allowed to book ticket.
                }
            }

            var response = await GetProfileDetails(request, getMPSecurityDetails, persistedReservation?.ShopReservationInfo?.IsCorporateBooking == true).ConfigureAwait(false);

            if (response != null && response.Data != null)
            {
                if (persistedReservation != null)
                {
                    if (persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.IsYATravel)
                    {
                        GetProfileOwner(response);
                    }
                }
                if (getMPSecurityDetails)
                {
                    GetProfileOwner(response);
                }
                profiles = await PopulateProfilesV2(request.SessionId, request.MileagePlusNumber, request.CustomerId, response.Data.Travelers, request, path: request.Path, application: request.Application, isCorporateBooking: persistedReservation?.ShopReservationInfo?.IsCorporateBooking == true, getMPSecurityDetails: getMPSecurityDetails).ConfigureAwait(false);
            }
            else
            {
                throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
            return profiles;
        }

        public virtual async Task<United.Mobile.Model.Shopping.Reservation> PersistedReservation(MOBCPProfileRequest request)
        {
            United.Mobile.Model.Shopping.Reservation persistedReservation = new United.Mobile.Model.Shopping.Reservation();
            if (request != null)
                persistedReservation = await _sessionHelperService.GetSession<United.Mobile.Model.Shopping.Reservation>(request.SessionId, persistedReservation.ObjectName, new List<string> { request.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);

            if (Convert.ToBoolean(_configuration.GetValue<string>("CorporateConcurBooking") ?? "false"))
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

        private async Task<CslResponse<TravelersProfileResponse>> GetProfileDetails(MOBCPProfileRequest request, bool getMPSecurityDetails = false, bool isCorporateBooking = false)
        {
            CslResponse<TravelersProfileResponse> cslTravelerProfileResponse = new CslResponse<TravelersProfileResponse>();
            cslTravelerProfileResponse = await MakeProfileTravelerServiceCall(request).ConfigureAwait(false);
            await MakeProfileOwnerServiceCall(request).ConfigureAwait(false);

            if (!getMPSecurityDetails)
            {
                await MakeProfileCreditCardsServiecall(request).ConfigureAwait(false);
            }
            if (getMPSecurityDetails || isCorporateBooking)
            {
                await MakeCorpProfileServiecall(request).ConfigureAwait(false);
            }
            if (!getMPSecurityDetails && isCorporateBooking)
            {
                await MakeCorpFopServiceCall(request).ConfigureAwait(false);
            }

            return cslTravelerProfileResponse;
        }

        private async Task<CslResponse<TravelersProfileResponse>> MakeProfileTravelerServiceCall(MOBCPProfileRequest request)
        {
            string jsonResponse = await _profileService.GetAllTravellers(request.Token, request.MileagePlusNumber, request.SessionId).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(jsonResponse))
            {
                CslResponse<TravelersProfileResponse> travelersProfileResponse = JsonConvert.DeserializeObject<CslResponse<TravelersProfileResponse>>(jsonResponse);
                return travelersProfileResponse;
            }
            else
            {
                throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
        }

        public async Task MakeProfileOwnerServiceCall(MOBCPProfileRequest request)
        {
            string jsonResponse = await _profileService.GetProfileOwner(request.Token, request.MileagePlusNumber, request.SessionId).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var ownerProfileResponse = JsonConvert.DeserializeObject<OwnerResponseModel>(jsonResponse);
                await _sessionHelperService.SaveSession<OwnerResponseModel>(ownerProfileResponse, request.SessionId, new List<string> { request.SessionId, "CSLGetProfileOwnerResponse" }, "CSLGetProfileOwnerResponse").ConfigureAwait(false);
            }
            else
            {
                throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
        }

        public async Task MakeProfileCreditCardsServiecall(MOBCPProfileRequest request)
        {
            string jsonResponse = await _profileService.GetCreditCardsData(request.Token, request.MileagePlusNumber, request.SessionId).ConfigureAwait(false);
            var creditCardDataResponse = JsonConvert.DeserializeObject<CslResponse<CreditCardDataReponseModel>>(jsonResponse);
            if (creditCardDataResponse != null && creditCardDataResponse.Data != null)
            {
                await _sessionHelperService.SaveSession<CslResponse<CreditCardDataReponseModel>>(creditCardDataResponse, request.SessionId, new List<string> { request.SessionId, "CSLGetProfileCreditCardsResponse" }, "CSLGetProfileCreditCardsResponse").ConfigureAwait(false);
            }
            else
            {
                throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
        }

        private async Task MakeCorpProfileServiecall(MOBCPProfileRequest request)
        {
            United.CorporateDirect.Models.CustomerProfile.CorporateProfileRequest corpProfileRequest = new United.CorporateDirect.Models.CustomerProfile.CorporateProfileRequest();
            corpProfileRequest.LoyaltyId = request.MileagePlusNumber;
            string jsonRequest = JsonConvert.SerializeObject(corpProfileRequest);
            string jsonResponse = await _corporateGetService.GetCorpProfileData(request.Token, request.SessionId, jsonRequest).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var corprofileResponse = JsonConvert.DeserializeObject<United.CorporateDirect.Models.CustomerProfile.CorpProfileResponse>(jsonResponse);
                if (corprofileResponse != null && (corprofileResponse.Errors == null || corprofileResponse.Errors.Count == 0))
                {
                    bool isEnableU4BCorporateBooking = request != null && request.Application != null ? IsEnableU4BCorporateBooking(request.Application.Id, request.Application.Version?.Major) : false;
                    string sessionId = isEnableU4BCorporateBooking ? request.DeviceId + request.MileagePlusNumber : request.SessionId;
                    await _sessionHelperService.SaveSession<CorpProfileResponse>(corprofileResponse, sessionId, new List<string> { sessionId, "CSLCorpProfileResponse" }, "CSLCorpProfileResponse").ConfigureAwait(false);

                }
            }
        }

        public async Task MakeCorpFopServiceCall(MOBCPProfileRequest request)
        {
            United.CorporateDirect.Models.CustomerProfile.CorporateProfileRequest corpProfileRequest = new United.CorporateDirect.Models.CustomerProfile.CorporateProfileRequest();
            corpProfileRequest.LoyaltyId = request.MileagePlusNumber;
            string jsonRequest = JsonConvert.SerializeObject(corpProfileRequest);
            string jsonResponse = await _corporateGetService.GetCorpFOPData(request.Token, request.SessionId, jsonRequest).ConfigureAwait(false);

            #region
            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var corpFOPResponse = JsonConvert.DeserializeObject<United.CorporateDirect.Models.CustomerProfile.CorpFopResponse>(jsonResponse);
                if (corpFOPResponse != null && (corpFOPResponse.Errors == null || corpFOPResponse.Errors.Count == 0))
                {
                    await _sessionHelperService.SaveSession<CorpFopResponse>(corpFOPResponse, request.SessionId, new List<string> { request.SessionId, "CSLCorpFopResponse" }, "CSLCorpFopResponse").ConfigureAwait(false);

                }
                else
                {
                    throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
            }
            else
            {
                throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
            #endregion


        }

        private void GetProfileOwner(CslResponse<TravelersProfileResponse> response)
        {
            if (response.Data.Travelers != null && response.Data.Travelers.Count > 0)
            {

                TravelerProfileResponse owner = response.Data.Travelers.First(t => t.Profile != null && t.Profile.ProfileOwnerIndicator);
                response.Data.Travelers = new List<TravelerProfileResponse>();
                response.Data.Travelers.Add(owner);
            }
        }

        private async Task<List<MOBCPProfile>> PopulateProfilesV2(string sessionId, string mileagePlusNumber, int customerId, List<TravelerProfileResponse> profilesTravelers, MOBCPProfileRequest request, bool getMPSecurityDetails = false, string path = "", MOBApplication application = null, bool isCorporateBooking = false)
        {
            List<MOBCPProfile> mobProfiles = null;
            if (profilesTravelers != null && profilesTravelers.Count > 0)
            {
                CSLProfile persistedCSLProfile = new CSLProfile();
                persistedCSLProfile = await _sessionHelperService.GetSession<CSLProfile>(sessionId, persistedCSLProfile.ObjectName, new List<string> { sessionId, persistedCSLProfile.ObjectName }).ConfigureAwait(false);
                if (persistedCSLProfile == null)
                {
                    persistedCSLProfile = new CSLProfile();
                    persistedCSLProfile.SessionId = sessionId;
                    persistedCSLProfile.MileagePlusNumber = mileagePlusNumber;
                    persistedCSLProfile.CustomerId = customerId;
                }
                if (persistedCSLProfile.Profiles == null)
                {
                    mobProfiles = new List<MOBCPProfile>();
                    persistedCSLProfile.Profiles = mobProfiles;
                }
                else
                {
                    mobProfiles = persistedCSLProfile.Profiles;
                }

                TravelerProfileResponse owner = profilesTravelers.First(t => t.Profile?.ProfileOwnerIndicator == true);
                if (owner != null /*&& owner.AirPreferences != null*/ && owner.Profile != null)
                {
                    MOBCPProfile mobProfile = new MOBCPProfile();
                    if (owner.AirPreferences != null)
                    {
                        mobProfile.AirportCode = owner.AirPreferences[0].AirportCode;
                        mobProfile.AirportNameLong = owner.AirPreferences[0].AirportNameLong;
                        mobProfile.AirportNameShort = owner.AirPreferences[0].AirportNameShort;
                    }
                    //  mobProfile.Description = profile.Description;No longer sent by new service
                    //  mobProfile.Key = owner.Profile.TravelerKey; No longer sent by new service
                    // mobProfile.LanguageCode = profile.LanguageCode; // No longer sent by new service
                    mobProfile.ProfileId = Convert.ToInt32(owner.Profile.ProfileId);
                    // mobProfile.ProfileMembers = PopulateProfileMembers(profile.ProfileMembers);No longer sent by new service itis just duplicate info 
                    mobProfile.ProfileOwnerId = Convert.ToInt32(owner.Profile.ProfileOwnerId);
                    mobProfile.ProfileOwnerKey = owner.Profile.TravelerKey;
                    //mobProfile.QuickCustomerId = profile.QuickCustomerId;No longer sent by new service
                    //mobProfile.QuickCustomerKey = profile.QuickCustomerKey;No longer sent by new service
                    if (isCorporateBooking || getMPSecurityDetails)
                    {
                        mobProfile.CorporateData = await PopulateCorporateData(request).ConfigureAwait(false);
                    }
                    bool isProfileOwnerTSAFlagOn = false;
                    List<MOBKVP> mpList = new List<MOBKVP>();
                    var tupleResponse = await PopulateTravelersV2(profilesTravelers, mileagePlusNumber, isProfileOwnerTSAFlagOn, false, request, mpList, request.SessionId, getMPSecurityDetails, path).ConfigureAwait(false);
                    mobProfile.Travelers = tupleResponse.mobTravelersOwnerFirstInList;
                    isProfileOwnerTSAFlagOn = tupleResponse.isProfileOwnerTSAFlagOn;
                    mpList = tupleResponse.savedTravelersMPList;

                    //mobProfile.Travelers = PopulateTravelersV2(profilesTravelers, mileagePlusNumber, ref isProfileOwnerTSAFlagOn, false, request, out mpList, request.SessionId, getMPSecurityDetails, path);
                    mobProfile.SavedTravelersMPList = mpList;
                    mobProfile.IsProfileOwnerTSAFlagON = isProfileOwnerTSAFlagOn;
                    if (mobProfile != null)
                    {
                        mobProfile.DisclaimerList = await _mPTraveler.GetProfileDisclaimerList().ConfigureAwait(false);
                    }
                    mobProfiles.Add(mobProfile);
                }
            }

            return mobProfiles;
        }

        private async Task<MOBCPCorporate> PopulateCorporateData(MOBCPProfileRequest request)
        {
            bool isEnableU4BCorporateBooking = request != null && request.Application != null ? IsEnableU4BCorporateBooking(request.Application.Id, request.Application.Version?.Major) : false;
            string sessionId = isEnableU4BCorporateBooking ? request.DeviceId + request.MileagePlusNumber : request.SessionId;

            United.CorporateDirect.Models.CustomerProfile.CorpProfileResponse _corprofileResponse = new United.CorporateDirect.Models.CustomerProfile.CorpProfileResponse();
            _corprofileResponse = await _sessionHelperService.GetSession<CorpProfileResponse>(sessionId, "CSLCorpProfileResponse", new List<string> { sessionId, "CSLCorpProfileResponse" }).ConfigureAwait(false);
            MOBCPCorporate profileCorporateData = new MOBCPCorporate();
            if (_corprofileResponse != null)
            {
                if (_corprofileResponse?.Profiles != null && _corprofileResponse.Profiles.Any() && (_corprofileResponse.Errors == null || _corprofileResponse.Errors.Count == 0))
                {
                    var corporateData = _corprofileResponse.Profiles[0].CorporateData;
                    if (corporateData != null && corporateData.IsValid)
                    {
                        profileCorporateData.CompanyName = corporateData.CompanyName;
                        profileCorporateData.DiscountCode = corporateData.DiscountCode;
                        profileCorporateData.FareGroupId = corporateData.FareGroupId;
                        profileCorporateData.IsValid = corporateData.IsValid;
                        profileCorporateData.VendorId = corporateData.VendorId;
                        profileCorporateData.VendorName = corporateData.VendorName;
                        profileCorporateData.LeisureDiscountCode = corporateData.LeisureCode;
                        if(IsEnableSuppressingCompanyNameForBusiness(request.Application.Id, request.Application.Version.Major))
                            profileCorporateData.IsPersonalized =  corporateData.IsPersonalized;

                        if (await _featureToggles.IsEnableU4BForMultipax(request.Application.Id, request.Application.Version?.Major))
                            profileCorporateData.IsMultiPaxAllowed = corporateData.IsMultiPaxAllowed;

                        if (corporateData.IsArranger == true)
                        {
                            profileCorporateData.NoOfTravelers = Convert.ToInt32((string.IsNullOrEmpty(_configuration.GetValue<string>("TravelArrangerCount"))) ? "1" : _configuration.GetValue<string>("TravelArrangerCount"));
                            profileCorporateData.CorporateBookingType = CORPORATEBOOKINGTYPE.TravelArranger.ToString();
                        }

                    }
                    await _sessionHelperService.SaveSession<CorpProfileResponse>(_corprofileResponse, sessionId, new List<string> { sessionId, "CSLCorpProfileResponse" }, "CSLCorpProfileResponse").ConfigureAwait(false);
                    return profileCorporateData;
                }
            }
            return null;
        }

        private bool IsEnableU4BCorporateBooking(int applicationId, string appVersion)
        {
            return _configuration.GetValue<bool>("Enable_eRes_EmergencyDeviationTraining_TravelTypes") &&
                GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableU4BCorporateBooking_AppVersion"), _configuration.GetValue<string>("IPhone_EnableU4BCorporateBooking_AppVersion"));
        }

        private async Task<(bool isProfileOwnerTSAFlagOn, List<MOBKVP> savedTravelersMPList, List<MOBCPTraveler> mobTravelersOwnerFirstInList)> PopulateTravelersV2(List<TravelerProfileResponse> travelers, string mileagePluNumber, bool isProfileOwnerTSAFlagOn, bool isGetCreditCardDetailsCall, MOBCPProfileRequest request, List<MOBKVP> savedTravelersMPList, string sessionid, bool getMPSecurityDetails = false, string path = "")
        {
            savedTravelersMPList = new List<MOBKVP>();
            List<MOBCPTraveler> mobTravelers = null;
            List<MOBCPTraveler> mobTravelersOwnerFirstInList = null;
            MOBCPTraveler profileOwnerDetails = new MOBCPTraveler();
            OwnerResponseModel profileOwnerResponse = new OwnerResponseModel();
            United.CorporateDirect.Models.CustomerProfile.CorpProfileResponse corpProfileResponse = new United.CorporateDirect.Models.CustomerProfile.CorpProfileResponse();
            if (travelers != null && travelers.Count > 0)
            {
                mobTravelers = new List<MOBCPTraveler>();
                int i = 0;
                var persistedReservation = !getMPSecurityDetails ? PersistedReservation(request).Result : new United.Mobile.Model.Shopping.Reservation();

                foreach (TravelerProfileResponse traveler in travelers)
                {
                    #region
                    MOBCPTraveler mobTraveler = new MOBCPTraveler();
                    mobTraveler.PaxIndex = i; i++;
                    mobTraveler.CustomerId = Convert.ToInt32(traveler.Profile?.CustomerId);
                    if (traveler.Profile?.ProfileOwnerIndicator == true)
                    {
                        profileOwnerResponse = await _sessionHelperService.GetSession<OwnerResponseModel>(request.SessionId, "CSLGetProfileOwnerResponse", new List<string> { request.SessionId, "CSLGetProfileOwnerResponse" }).ConfigureAwait(false);

                        if (_configuration.GetValue<bool>("EnabledPartnerCardsId") && profileOwnerResponse?.PartnerCards?.Data != null)
                        {
                            var cardTypes = profileOwnerResponse.PartnerCards.Data.Where(x => x.PartnerCode == "CH").Select(x => x.CardType).ToArray();
                            mobTraveler.PartnerRPCIds = string.Join("|", cardTypes);
                        }

                        mobTraveler.CustomerMetrics = PopulateCustomerMetrics(profileOwnerResponse);
                        mobTraveler.MileagePlus = PopulateMileagePlusV2(profileOwnerResponse, request.MileagePlusNumber);
                        mobTraveler.IsDeceased = profileOwnerResponse?.MileagePlus?.Data?.IsDeceased == true;
                        mobTraveler.EmployeeId = traveler.Profile?.EmployeeId;

                    }
                    if (traveler.Profile?.BirthDate != null)
                    {
                        mobTraveler.BirthDate = traveler.Profile?.BirthDate.ToString("MM/dd/yyyy");
                        if (mobTraveler.BirthDate == "01/01/0001")
                            mobTraveler.BirthDate = null;
                    }
                    if (_configuration.GetValue<bool>("EnableNationalityAndCountryOfResidence"))
                    {
                        if (persistedReservation != null && persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.InfoNationalityAndResidence != null
                            && persistedReservation.ShopReservationInfo2.InfoNationalityAndResidence.IsRequireNationalityAndResidence)
                        {
                            if (string.IsNullOrEmpty(traveler.CustomerAttributes?.CountryofResidence) || string.IsNullOrEmpty(traveler.CustomerAttributes?.Nationality))
                            {
                                mobTraveler.Message = _configuration.GetValue<string>("SavedTravelerInformationNeededMessage");
                            }
                        }
                        mobTraveler.Nationality = traveler.CustomerAttributes?.Nationality;
                        mobTraveler.CountryOfResidence = traveler.CustomerAttributes?.CountryofResidence;
                    }

                    mobTraveler.FirstName = traveler.Profile.FirstName;
                    mobTraveler.GenderCode = traveler.Profile?.Gender.ToString() == "Undefined" ? "" : traveler.Profile.Gender.ToString();
                    mobTraveler.IsProfileOwner = traveler.Profile.ProfileOwnerIndicator;
                    mobTraveler.Key = traveler.Profile.TravelerKey;
                    mobTraveler.LastName = traveler.Profile.LastName;
                    mobTraveler.MiddleName = traveler.Profile.MiddleName;

                    if (mobTraveler.MileagePlus != null)
                    {
                        mobTraveler.MileagePlus.MpCustomerId = Convert.ToInt32(traveler.Profile.CustomerId);

                        if (!getMPSecurityDetails && request != null && IncludeTravelBankFOP(request.Application.Id, request.Application.Version.Major))
                        {
                            Session session = await _sessionHelperService.GetSession<Session>(request.SessionId, new Session().ObjectName, new List<string> { request.SessionId, new Session().ObjectName }).ConfigureAwait(false);

                            string cslLoyaltryBalanceServiceResponse = await _loyaltyBalanceServices.GetLoyaltyBalance(session.Token, mobTraveler.MileagePlus.MileagePlusId, session.SessionId).ConfigureAwait(false);
                            if (!string.IsNullOrEmpty(cslLoyaltryBalanceServiceResponse))
                            {
                                United.TravelBank.Model.BalancesDataModel.BalanceResponse PlusPointResponse = JsonConvert.DeserializeObject<United.TravelBank.Model.BalancesDataModel.BalanceResponse>(cslLoyaltryBalanceServiceResponse);
                                United.TravelBank.Model.BalancesDataModel.Balance tbbalance = PlusPointResponse.Balances.FirstOrDefault(tb => tb.ProgramCurrencyType == United.TravelBank.Model.TravelBankConstants.ProgramCurrencyType.UBC);
                                if (tbbalance != null && tbbalance.TotalBalance > 0)
                                {
                                    mobTraveler.MileagePlus.TravelBankBalance = (double)tbbalance.TotalBalance;
                                }
                            }
                        }
                    }

                    mobTraveler.ProfileId = Convert.ToInt32(traveler.Profile.ProfileId);
                    mobTraveler.ProfileOwnerId = Convert.ToInt32(traveler.Profile.ProfileOwnerId);
                    bool isTSAFlagOn = false;
                    if (traveler.SecureTravelers != null)
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
                        mobTraveler.SecureTravelers = PopulatorSecureTravelersV2(traveler.SecureTravelers, ref isTSAFlagOn, i >= 2, request.SessionId, request.Application.Id, request.DeviceId);
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
                    mobTraveler.Suffix = traveler.Profile.Suffix;
                    mobTraveler.Title = traveler.Profile.Title;
                    mobTraveler.TravelerTypeCode = traveler.Profile?.TravelerTypeCode;
                    mobTraveler.TravelerTypeDescription = traveler.Profile?.TravelerTypeDescription;
                    if (persistedReservation != null && persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.TravelerTypes != null
                        && persistedReservation.ShopReservationInfo2.TravelerTypes.Count > 0)
                    {
                        if (traveler.Profile?.BirthDate != null)
                        {
                            if (EnableYADesc() && persistedReservation.ShopReservationInfo2.IsYATravel)
                            {
                                mobTraveler.PTCDescription = GetYAPaxDescByDOB();
                            }
                            else
                            {
                                mobTraveler.PTCDescription = GetPaxDescriptionByDOB(traveler.Profile.BirthDate.ToString(), persistedReservation.Trips[0].FlattenedFlights[0].Flights[0].DepartDate);
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
                    //  mobTraveler.TravelProgramMemberId = traveler.Profile.TravProgramMemberId; No longer needed confirmed from service
                    if (traveler != null)
                    {
                        if (mobTraveler.MileagePlus != null)
                        {
                            mobTraveler.CurrentEliteLevel = mobTraveler.MileagePlus.CurrentEliteLevel;
                            //mobTraveler.AirRewardPrograms = GetTravelerLoyaltyProfile(traveler.AirPreferences, traveler.MileagePlus.CurrentEliteLevel);
                        }
                    }

                    mobTraveler.AirRewardPrograms = GetTravelerRewardPgrograms(traveler.RewardPrograms, mobTraveler.CurrentEliteLevel);
                    mobTraveler.Phones = PopulatePhonesV2(traveler, true);
                    if (mobTraveler.IsProfileOwner)
                    {
                        // These Phone and Email details for Makre Reseravation Phone and Email reason is mobTraveler.Phones = PopulatePhones(traveler.Phones,true) will get only day of travel contacts to register traveler & edit traveler.
                        mobTraveler.ReservationPhones = PopulatePhonesV2(traveler, false);
                        mobTraveler.ReservationEmailAddresses = PopulateEmailAddressesV2(traveler.Emails, false);

                        // Added by Hasnan - #53484. 10/04/2017
                        // As per the Bug 53484:PINPWD: iOS and Android - Phone number is blank in RTI screen after booking from newly created account.
                        // If mobTraveler.Phones is empty. Then it newly created account. Thus returning mobTraveler.ReservationPhones as mobTraveler.Phones.
                        if (_configuration.GetValue<bool>("EnableDayOfTravelEmail") || string.IsNullOrEmpty(path) || !path.ToUpper().Equals("BOOKING"))
                        {
                            if (mobTraveler.Phones.Count == 0)
                            {
                                mobTraveler.Phones = mobTraveler.ReservationPhones;
                            }
                        }
                        #region Corporate Leisure(ProfileOwner must travel)//Client will use the IsMustRideTraveler flag to auto select the travel and not allow to uncheck the profileowner on the SelectTraveler Screen.
                        if (_configuration.GetValue<bool>("EnableCorporateLeisure"))
                        {
                            if (persistedReservation?.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.TravelType == TravelType.CLB.ToString() && _mPTraveler.IsCorporateLeisureFareSelected(persistedReservation.Trips))
                            {
                                mobTraveler.IsMustRideTraveler = true;
                            }
                        }
                        #endregion Corporate Leisure
                    }
                    if (mobTraveler.IsProfileOwner && getMPSecurityDetails) //**PINPWD//mobTraveler.IsProfileOwner && request == null Means GetProfile and Populate is for MP PIN PWD Path
                    {
                        mobTraveler.ReservationEmailAddresses = PopulateAllEmailAddressesV2(traveler.Emails);
                    }
                    mobTraveler.AirPreferences = PopulateAirPrefrencesV2(traveler);
                    if (!getMPSecurityDetails && request?.Application?.Version != null && string.IsNullOrEmpty(request?.Flow) && _mPTraveler.IsInternationalBillingAddress_CheckinFlowEnabled(request.Application))
                    {
                        try
                        {
                            var mobShopCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, new MOBShoppingCart().ObjectName, new List<string> { request.SessionId, new MOBShoppingCart().ObjectName }).ConfigureAwait(false);
                            if (mobShopCart != null && !string.IsNullOrEmpty(mobShopCart.Flow) && mobShopCart.Flow == FlowType.CHECKIN.ToString())
                            {
                                request.Flow = mobShopCart.Flow;
                            }
                        }
                        catch { }
                    }
                    mobTraveler.Addresses = PopulateTravelerAddressesV2(traveler.Addresses, request?.Application, request?.Flow);

                    if (_configuration.GetValue<bool>("EnableDayOfTravelEmail") && !string.IsNullOrEmpty(path) && path.ToUpper().Equals("BOOKING"))
                    {
                        mobTraveler.EmailAddresses = PopulateEmailAddressesV2(traveler.Emails, true);
                    }
                    else
                    if (!getMPSecurityDetails)
                    {
                        mobTraveler.EmailAddresses = PopulateEmailAddressesV2(traveler.Emails, false);
                    }
                    else
                    {
                        mobTraveler.EmailAddresses = PopulateMPSecurityEmailAddressesV2(traveler.Emails);
                    }
                    if (mobTraveler.IsProfileOwner == true)
                    {
                        if (!getMPSecurityDetails)
                        {
                            bool isCardMandatory = false;
                            var corpCreditCards = new List<MOBCreditCard>();
                            if (mobTraveler.CreditCards == null)
                            {
                                mobTraveler.CreditCards = new List<MOBCreditCard>();
                            }

                            if (IsCorpBookingPath)
                            {
                                bool isEnableU4BCorporateBooking = request != null && request.Application != null ? IsEnableU4BCorporateBooking(request.Application.Id, request.Application.Version?.Major) : false;
                                string sessionId = isEnableU4BCorporateBooking ? request.DeviceId + request.MileagePlusNumber : request.SessionId;
                                corpProfileResponse = await _sessionHelperService.GetSession<CorpProfileResponse>(sessionid, "CSLCorpProfileResponse", new List<string> { sessionid, "CSLCorpProfileResponse" }).ConfigureAwait(false);
                                corpCreditCards = await _profileCreditCard.PopulateCorporateCreditCards(isGetCreditCardDetailsCall, mobTraveler.Addresses, persistedReservation, request).ConfigureAwait(false);
                                if (corpCreditCards != null && corpCreditCards.Any(s => s.IsMandatory == true))
                                {
                                    isCardMandatory = true;
                                    mobTraveler.CreditCards = corpCreditCards;
                                }
                            }
                            if (!isCardMandatory)
                            {
                                mobTraveler.CreditCards = await _profileCreditCard.PopulateCreditCards(isGetCreditCardDetailsCall, mobTraveler.Addresses, request).ConfigureAwait(false);
                                if (corpCreditCards != null && corpCreditCards.Count() > 0)
                                {
                                    mobTraveler.CreditCards.AddRange(corpCreditCards);
                                }
                            }
                        }
                        if (IsCorpBookingPath && corpProfileResponse?.Profiles != null && corpProfileResponse.Profiles.Count() > 0)
                        {
                            var corporateTraveler = corpProfileResponse?.Profiles[0].Travelers.FirstOrDefault();
                            if (corporateTraveler != null)
                            {
                                if (corporateTraveler.Addresses != null)
                                {
                                    var corporateAddress = PopulateCorporateTravelerAddresses(corporateTraveler.Addresses, request.Application, request.Flow);
                                    if (mobTraveler.Addresses == null)
                                        mobTraveler.Addresses = new List<MOBAddress>();
                                    mobTraveler.Addresses.AddRange(corporateAddress);
                                }
                                if (corporateTraveler.EmailAddresses != null)
                                {
                                    var corporateEmailAddresses = PopulateCorporateEmailAddresses(corporateTraveler.EmailAddresses, false);
                                    mobTraveler.ReservationEmailAddresses = new List<MOBEmail>();
                                    mobTraveler.ReservationEmailAddresses.AddRange(corporateEmailAddresses);
                                }
                                if (corporateTraveler.Phones != null)
                                {
                                    var corporatePhones = PopulateCorporatePhones(corporateTraveler.Phones, false);
                                    mobTraveler.ReservationPhones = new List<MOBCPPhone>();
                                    mobTraveler.ReservationPhones.AddRange(corporatePhones);
                                }
                                if (corporateTraveler.AirPreferences != null)
                                {
                                    var corporateAirpreferences = PopulateCorporateAirPrefrences(corporateTraveler.AirPreferences);
                                    if (mobTraveler.AirPreferences == null)
                                        mobTraveler.AirPreferences = new List<MOBPrefAirPreference>();
                                    mobTraveler.AirPreferences.AddRange(corporateAirpreferences);
                                }
                            }

                        }
                    }
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

            return (isProfileOwnerTSAFlagOn, savedTravelersMPList, mobTravelersOwnerFirstInList);
        }

        private MOBCPCustomerMetrics PopulateCustomerMetrics(OwnerResponseModel profileOwnerResponse)
        {
            if (profileOwnerResponse?.CustomerMetrics?.Data != null)
            {
                MOBCPCustomerMetrics travelerCustomerMetrics = new MOBCPCustomerMetrics();
                if (!String.IsNullOrEmpty(profileOwnerResponse.CustomerMetrics.Data.PTCCode))
                {
                    travelerCustomerMetrics.PTCCode = profileOwnerResponse.CustomerMetrics.Data.PTCCode;
                }
                return travelerCustomerMetrics;
            }
            return null;
        }

        private MOBCPMileagePlus PopulateMileagePlusV2(OwnerResponseModel profileOwnerResponse, string mileageplusId)
        {
            if (profileOwnerResponse?.MileagePlus?.Data != null)
            {
                MOBCPMileagePlus mileagePlus = null;
                var mileagePlusData = profileOwnerResponse.MileagePlus.Data;

                mileagePlus = new MOBCPMileagePlus();
                var balance = profileOwnerResponse.MileagePlus.Data.Balances?.FirstOrDefault(balnc => (int)balnc.Currency == 5);
                if (balance != null)
                    mileagePlus.AccountBalance = Convert.ToInt32(balance.Amount);
                mileagePlus.ActiveStatusCode = mileagePlusData.AccountStatus;
                mileagePlus.ActiveStatusDescription = mileagePlusData.AccountStatusDescription;
                mileagePlus.AllianceEliteLevel = mileagePlusData.StarAllianceTierLevel;
                mileagePlus.ClosedStatusCode = mileagePlusData.OpenClosedStatusCode;
                mileagePlus.ClosedStatusDescription = mileagePlusData.OpenClosedStatusDescription;
                mileagePlus.CurrentEliteLevel = mileagePlusData.MPTierLevel;
                if (mileagePlus.CurrentEliteLevelDescription != null)
                {
                    mileagePlus.CurrentEliteLevelDescription = mileagePlusData.MPTierLevelDescription.ToString().ToUpper() == "NON-ELITE" ? "General member" : mileagePlusData.MPTierLevelDescription;
                }
                mileagePlus.CurrentYearMoneySpent = mileagePlusData.CurrentYearMoneySpent;
                mileagePlus.EliteMileageBalance = Convert.ToInt32(mileagePlusData.EliteMileageBalance);
                // mileagePlus.EliteSegmentBalance = Convert.ToInt32(mileagePlusData.EliteSegmentBalance);No longer used confirmed from service
                // mileagePlus.EncryptedPin = mileagePlusData.EncryptedPin;No longer used confirmed from service
                mileagePlus.EnrollDate = mileagePlusData.EnrollDate.GetValueOrDefault().ToString("MM/dd/yyyy");
                mileagePlus.EnrollSourceCode = mileagePlusData.EnrollSourceCode;
                mileagePlus.EnrollSourceDescription = mileagePlusData.EnrollSourceDescription;
                // mileagePlus.FlexPqmBalance = onePass.FlexPQMBalance;
                mileagePlus.FutureEliteDescription = mileagePlusData.NextStatusLevelDescription;
                mileagePlus.FutureEliteLevel = mileagePlusData.NextStatusLevel;
                mileagePlus.InstantEliteExpirationDate = mileagePlusData.NextStatusLevelDescription;
                mileagePlus.IsCEO = mileagePlusData.CEO;
                mileagePlus.IsClosedPermanently = mileagePlusData.IsClosedPermanently;
                mileagePlus.IsClosedTemporarily = mileagePlusData.IsClosedTemporarily;
                // mileagePlus.IsCurrentTrialEliteMember = mileagePlusData.IsCurrentTrialEliteMember;No longer used confirmed from service
                // mileagePlus.IsFlexPqm = mileagePlusData.IsFlexPQM;
                // mileagePlus.IsInfiniteElite = mileagePlusData.IsInfiniteElite;
                // mileagePlus.IsLifetimeCompanion = mileagePlusData.IsLifetimeCompanion;
                mileagePlus.IsLockedOut = mileagePlusData.IsLockedOut;
                // mileagePlus.IsPresidentialPlus = onePass.IsPresidentialPlus;No longer used confirmed from service
                mileagePlus.IsUnitedClubMember = mileagePlusData.IsPClubMember;
                // mileagePlus.Key = onePass.Key;No longer used confirmed from service
                mileagePlus.LastActivityDate = mileagePlusData.LastActivityDate.GetValueOrDefault().ToString("MM/dd/yyyy");
                // mileagePlus.LastExpiredMile = Convert.ToInt32(mileagePlusData.LastExpiredMile);
                mileagePlus.LastFlightDate = mileagePlusData.LastFlightDate.GetValueOrDefault().ToString("MM/dd/yyyy");
                //mileagePlus.LastStatementBalance = Convert.ToInt32(mileagePlusData.LastStatementBalance);//This property is deprecated confirmed with service team 
                mileagePlus.LastStatementDate = mileagePlusData.LastStatementDate.GetValueOrDefault().ToString("MM/dd/yyyy");
                mileagePlus.LifetimeEliteMileageBalance = Convert.ToInt32(mileagePlusData.LifetimeMiles);
                // mileagePlus.MileageExpirationDate = mileagePlusData.MileageExpirationDate.GetValueOrDefault().ToString("MM/dd/yyyy");//This property is deprecated confirmed with service team
                mileagePlus.MileagePlusId = mileageplusId;
                // mileagePlus.MileagePlusPin = mileagePlusData.MileagePlusPIN;//No longer used confirmed from service
                // mileagePlus.NextYearEliteLevel = mileagePlusData.NextYearEliteLevel;
                // mileagePlus.NextYearEliteLevelDescription = mileagePlusData.NextYearEliteLevelDescription;
                // mileagePlus.PriorUnitedAccountNumber = mileagePlusData.PriorUnitedAccountNumber;
                // mileagePlus.StarAllianceEliteLevel = Convert.ToInt32(mileagePlusData.SkyTeamEliteLevelCode);//This property is deprecated confirmed with service team 
                //Need to call another service here 
                //if (!_configuration.GetValue<bool>("Keep_MREST_MP_EliteLevel_Expiration_Logic"))
                //{
                //    mileagePlus.PremierLevelExpirationDate = onePass.PremierLevelExpirationDate;
                //    if (onePass.CurrentYearInstantElite != null)
                //    {
                //        mileagePlus.InstantElite = new MOBInstantElite()
                //        {
                //            ConsolidatedCode = onePass.CurrentYearInstantElite.ConsolidatedCode,
                //            EffectiveDate = onePass.CurrentYearInstantElite.EffectiveDate != null ? onePass.CurrentYearInstantElite.EffectiveDate.ToString("MM/dd/yyyy") : string.Empty,
                //            EliteLevel = onePass.CurrentYearInstantElite.EliteLevel,
                //            EliteYear = onePass.CurrentYearInstantElite.EliteYear,
                //            ExpirationDate = onePass.CurrentYearInstantElite.ExpirationDate != null ? onePass.CurrentYearInstantElite.ExpirationDate.ToString("MM/dd/yyyy") : string.Empty,
                //            PromotionCode = onePass.CurrentYearInstantElite.PromotionCode
                //        };
                //    }
                //}
                return mileagePlus;
            }
            else
            {
                throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
        }

        private bool IncludeTravelBankFOP(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableTravelBankFOP")
                && GeneralHelper.IsApplicationVersionGreater
                (appId, appVersion, "AndroidTravelBankFOPVersion", "iPhoneTravelBankFOPVersion", "", "", true, _configuration);
        }

        private List<MOBCPSecureTraveler> PopulatorSecureTravelersV2(SecureTravelerResponseData secureTravelerResponseData, ref bool isTSAFlag, bool correctDate, string sessionID, int appID, string deviceID)
        {
            List<MOBCPSecureTraveler> mobSecureTravelers = null;

            if (secureTravelerResponseData?.SecureTraveler != null)
            {
                mobSecureTravelers = new List<MOBCPSecureTraveler>();
                var secureTraveler = secureTravelerResponseData.SecureTraveler;
                if (!_configuration.GetValue<bool>("DisableUCBKTNFix") && secureTraveler.DocumentType == null) //MOBILE-26294 : Before UCB Migration documentype used to be empty .But after UCB Migration we are getting it as Null Due to that we are not building the KTN number.Looks for the bug number for more details.
                {
                    secureTraveler.DocumentType = "";
                }
                if (secureTraveler.DocumentType != null && secureTraveler.DocumentType.Trim().ToUpper() != "X")
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
                    mobSecureTraveler.CustomerId = Convert.ToInt32(secureTraveler.CustomerId);
                    mobSecureTraveler.DecumentType = secureTraveler.DocumentType;
                    mobSecureTraveler.Description = secureTraveler.Description;
                    mobSecureTraveler.FirstName = secureTraveler.FirstName;
                    mobSecureTraveler.Gender = secureTraveler.Gender;
                    // mobSecureTraveler.Key = secureTraveler.Key;No longer needed confirmed from service
                    mobSecureTraveler.LastName = secureTraveler.LastName;
                    mobSecureTraveler.MiddleName = secureTraveler.MiddleName;
                    mobSecureTraveler.SequenceNumber = (int)secureTraveler.SequenceNumber;
                    mobSecureTraveler.Suffix = secureTraveler.Suffix;
                    if (secureTravelerResponseData.SupplementaryTravelInfos != null)
                    {
                        foreach (SupplementaryTravelDocsDataMembers supplementaryTraveler in secureTravelerResponseData.SupplementaryTravelInfos)
                        {
                            if (supplementaryTraveler.Type == "K")
                            {
                                mobSecureTraveler.KnownTravelerNumber = supplementaryTraveler.Number;
                            }
                            if (supplementaryTraveler.Type == "R")
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
                    }
                    else
                    {
                        mobSecureTravelers.Add(mobSecureTraveler);
                    }
                    #endregion
                }

            }
            return mobSecureTravelers;
        }

        private bool EnableYADesc(bool isReshop = false)
        {
            return _configuration.GetValue<bool>("EnableYoungAdultBooking") && _configuration.GetValue<bool>("EnableYADesc") && !isReshop;
        }

        private static string GetYAPaxDescByDOB()
        {
            return "Young adult (18-23)";
        }

        private static string GetPaxDescriptionByDOB(string date, string deptDateFLOF)
        {
            int age = GetAgeByDOB(date, deptDateFLOF);
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

        private static int GetAgeByDOB(string birthDate, string firstLOFDepDate)
        {
            var travelDate = DateTime.Parse(firstLOFDepDate);

            var birthDate1 = DateTime.Parse(birthDate);
            // Calculate the age.
            var age = travelDate.Year - birthDate1.Year;
            // Go back to the year the person was born in case of a leap year
            if (birthDate1 > travelDate.AddYears(-age)) age--;

            return age;
        }

        private List<MOBBKLoyaltyProgramProfile> GetTravelerRewardPgrograms(List<RewardProgramData> rewardPrograms, int currentEliteLevel)
        {
            List<MOBBKLoyaltyProgramProfile> programs = new List<MOBBKLoyaltyProgramProfile>();

            if (rewardPrograms != null && rewardPrograms.Count > 0)
            {
                foreach (RewardProgramData rewardProgram in rewardPrograms)
                {
                    MOBBKLoyaltyProgramProfile airRewardProgram = new MOBBKLoyaltyProgramProfile();
                    airRewardProgram.ProgramId = rewardProgram.ProgramId.ToString();
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
            return programs;
        }

        private List<MOBCPPhone> PopulatePhonesV2(TravelerProfileResponse traveler, bool onlyDayOfTravelContact)
        {
            List<MOBCPPhone> mobCPPhones = new List<MOBCPPhone>();
            var phones = traveler.Phones;
            if (phones != null && phones.Count > 0)
            {
                MOBCPPhone primaryMobCPPhone = null;
                int co = 0;
                foreach (Phone phone in phones)
                {
                    #region As per Wade Change want to filter out to return only Primary Phone to client if not primary phone exist return the first one from the list.So lot of work at client side will save time. Sep 15th 2014
                    MOBCPPhone mobCPPhone = new MOBCPPhone();
                    co = co + 1;
                    mobCPPhone.AreaNumber = phone.AreaCode;
                    mobCPPhone.PhoneNumber = phone.Number;
                    //mobCPPhone.Attention = phone.Attention;No longer needed confirmed from service
                    mobCPPhone.ChannelCode = "P";
                    mobCPPhone.ChannelCodeDescription = "Phone";
                    mobCPPhone.ChannelTypeCode = phone.Type.ToString();
                    mobCPPhone.ChannelTypeDescription = phone.TypeDescription;
                    mobCPPhone.ChannelTypeSeqNumber = phone.SequenceNumber;
                    mobCPPhone.CountryCode = phone.CountryCode;
                    mobCPPhone.CountryPhoneNumber = phone.CountryPhoneNumber;
                    mobCPPhone.CustomerId = Convert.ToInt32(traveler.Profile.CustomerId);
                    mobCPPhone.Description = phone.Remark;
                    mobCPPhone.DiscontinuedDate = Convert.ToString(phone.DiscontinuedDate);
                    mobCPPhone.EffectiveDate = Convert.ToString(phone.EffectiveDate);
                    mobCPPhone.ExtensionNumber = phone.ExtensionNumber;
                    mobCPPhone.IsPrimary = phone.PrimaryIndicator;
                    mobCPPhone.IsPrivate = phone.IsPrivate;
                    mobCPPhone.IsProfileOwner = traveler.Profile.ProfileOwnerIndicator;
                    mobCPPhone.Key = phone.Key;
                    mobCPPhone.LanguageCode = phone.LanguageCode;
                    mobCPPhone.WrongPhoneDate = Convert.ToString(phone.WrongPhoneDate);
                    mobCPPhone.DeviceTypeCode = phone.DeviceType.ToString();
                    mobCPPhone.DeviceTypeDescription = phone.TypeDescription;

                    mobCPPhone.IsDayOfTravel = phone.DayOfTravelNotification;

                    if (phone.DayOfTravelNotification)
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
                        if (phone.DayOfTravelNotification)
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
            }
            return mobCPPhones;
        }

        private List<MOBEmail> PopulateEmailAddressesV2(List<Email> emailAddresses, bool onlyDayOfTravelContact)
        {
            #region
            List<MOBEmail> mobEmailAddresses = new List<MOBEmail>();
            bool isCorpEmailPresent = false;

            if (emailAddresses != null && emailAddresses.Count > 0)
            {
                if (IsCorpBookingPath)
                {
                    //As per Business / DotCom Kalpen; we are removing the condition for checking the Effectivedate and Discontinued date
                    var corpIndex = emailAddresses.FindIndex(x => x.TypeDescription != null && x.TypeDescription.ToLower() == "corporate" && x.Address != null && x.Address.Trim() != "");
                    if (corpIndex >= 0)
                        isCorpEmailPresent = true;
                }

                MOBEmail primaryEmailAddress = null;
                int co = 0;
                foreach (Email email in emailAddresses)
                {

                    if (isCorpEmailPresent && !onlyDayOfTravelContact && email.TypeDescription.ToLower() == "corporate")
                    {
                        primaryEmailAddress = new MOBEmail();
                        email.PrimaryIndicator = true;
                        primaryEmailAddress.Key = email.Key;
                        primaryEmailAddress.Channel = new MOBChannel();
                        primaryEmailAddress.EmailAddress = email.Address;
                        primaryEmailAddress.Channel.ChannelCode = "E";
                        primaryEmailAddress.Channel.ChannelDescription = "Email";
                        primaryEmailAddress.Channel.ChannelTypeCode = email.Type.ToString();
                        primaryEmailAddress.Channel.ChannelTypeDescription = email.TypeDescription;
                        primaryEmailAddress.IsDefault = email.PrimaryIndicator;
                        primaryEmailAddress.IsPrimary = email.PrimaryIndicator;
                        primaryEmailAddress.IsPrivate = email.IsPrivate;
                        primaryEmailAddress.IsDayOfTravel = email.DayOfTravelNotification;
                        if (!email.DayOfTravelNotification)
                        {
                            break;
                        }

                    }
                    else if (isCorpEmailPresent && !onlyDayOfTravelContact && email.TypeDescription.ToLower() != "corporate")
                    {
                        continue;
                    }

                    //Fix for CheckOut ArgNull Exception - Empty EmailAddress with null EffectiveDate & DiscontinuedDate for Corp Account Revenue Booking (MOBILE-9873) - Shashank : Added OR condition to allow CorporateAccount ProfileOwner.
                    if ((email.EffectiveDate <= DateTime.UtcNow && email.DiscontinuedDate >= DateTime.UtcNow) ||
                            (email.TypeDescription.ToLower() == "corporate"
                            && email.PrimaryIndicator == true && primaryEmailAddress == null))  // check condition
                    {
                        #region As per Wade Change want to filter out to return only Primary email to client if not primary phone exist return the first one from the list.So lot of work at client side will save time. Sep 15th 2014
                        co = co + 1;
                        MOBEmail e = new MOBEmail();
                        e.Key = email.Key;
                        e.Channel = new MOBChannel();
                        e.EmailAddress = email.Address;
                        e.Channel.ChannelCode = "E";
                        e.Channel.ChannelDescription = "Email";
                        e.Channel.ChannelTypeCode = email.Type.ToString();
                        e.Channel.ChannelTypeDescription = email.TypeDescription;
                        e.IsDefault = email.PrimaryIndicator;
                        e.IsPrimary = email.PrimaryIndicator;
                        e.IsPrivate = email.IsPrivate;
                        e.IsDayOfTravel = email.DayOfTravelNotification;
                        if (email.DayOfTravelNotification)
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
                            if (email.PrimaryIndicator)
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

        private List<MOBEmail> PopulateAllEmailAddressesV2(List<Email> emailAddresses)
        {
            #region
            List<MOBEmail> mobEmailAddresses = new List<MOBEmail>();
            if (emailAddresses != null && emailAddresses.Count > 0)
            {
                int co = 0;
                foreach (Email email in emailAddresses)
                {
                    if (email.EffectiveDate <= DateTime.UtcNow && email.DiscontinuedDate >= DateTime.UtcNow)
                    {
                        #region As per Wade Change want to filter out to return only Primary email to client if not primary phone exist return the first one from the list.So lot of work at client side will save time. Sep 15th 2014
                        co = co + 1;
                        MOBEmail e = new MOBEmail();
                        e.Key = email.Key;
                        e.Channel = new MOBChannel();
                        e.Channel.ChannelCode = "E";
                        e.Channel.ChannelDescription = "Email";
                        e.Channel.ChannelTypeCode = email.Type.ToString();
                        e.Channel.ChannelTypeDescription = email.TypeDescription;
                        e.EmailAddress = email.Address;
                        e.IsDefault = email.PrimaryIndicator;
                        e.IsPrimary = email.PrimaryIndicator;
                        e.IsPrivate = email.IsPrivate;
                        e.IsDayOfTravel = email.DayOfTravelNotification;
                        mobEmailAddresses.Add(e);
                        #endregion
                    }
                }
            }
            return mobEmailAddresses;
            #endregion
        }

        private List<MOBPrefAirPreference> PopulateAirPrefrencesV2(TravelerProfileResponse traveler)
        {
            var airPreferences = traveler.AirPreferences;
            List<MOBPrefAirPreference> mobAirPrefs = new List<MOBPrefAirPreference>();
            if (airPreferences != null && airPreferences.Count > 0)
            {
                foreach (AirPreferenceDataModel pref in airPreferences)
                {
                    MOBPrefAirPreference mobAirPref = new MOBPrefAirPreference();
                    mobAirPref.AirportCode = pref.AirportCode;
                    mobAirPref.AirportCode = pref.AirportNameLong;
                    mobAirPref.AirportNameShort = pref.AirportNameShort;
                    mobAirPref.AirPreferenceId = pref.AirPreferenceId;
                    mobAirPref.ClassDescription = pref.ClassDescription;
                    mobAirPref.ClassId = pref.ClassID;
                    mobAirPref.CustomerId = traveler.Profile.CustomerId;
                    mobAirPref.EquipmentCode = pref.EquipmentCode;
                    mobAirPref.EquipmentDesc = pref.EquipmentDescription;
                    mobAirPref.EquipmentId = pref.EquipmentID;
                    mobAirPref.IsActive = true;//By default if it is returned it is active confirmed with service team
                    mobAirPref.IsSelected = true;// By default if it is returned it is active confirmed with service team
                    mobAirPref.IsNew = false;// By default if it is returned it is false confirmed with service team
                    mobAirPref.Key = pref.Key;
                    //mobAirPref.LanguageCode = pref.LanguageCode;No longer sent from service confirmed with them
                    mobAirPref.MealCode = pref.MealCode;
                    mobAirPref.MealDescription = pref.MealDescription;
                    mobAirPref.MealId = pref.MealId;
                    // mobAirPref.NumOfFlightsDisplay = pref.NumOfFlightsDisplay;No longer sent from service confirmed with them
                    mobAirPref.ProfileId = traveler.Profile.ProfileId;
                    mobAirPref.SearchPreferenceDescription = pref.SearchPreferenceDescription;
                    mobAirPref.SearchPreferenceId = pref.SearchPreferenceID;
                    //mobAirPref.SeatFrontBack = pref.SeatFrontBack;No longer sent from service confirmed with them
                    mobAirPref.SeatSide = pref.SeatSide;
                    mobAirPref.SeatSideDescription = pref.SeatSideDescription;
                    mobAirPref.VendorCode = pref.VendorCode;//Service confirmed we can hard code this as we dont have any other vendor it is always United airlines
                    mobAirPref.VendorDescription = pref.VendorDescription;//Service confirmed we can hard code this as we dont have any other vendor it is always United airlines
                    mobAirPref.VendorId = pref.VendorId;
                    mobAirPref.AirRewardPrograms = GetAirRewardPrograms(traveler);
                    // mobAirPref.SpecialRequests = GetTravelerSpecialRequests(pref.SpecialRequests);Client is not using this even we send this ..
                    // mobAirPref.ServiceAnimals = GetTravelerServiceAnimals(pref.ServiceAnimals);Client is not using this even we send this ..
                    mobAirPrefs.Add(mobAirPref);
                }
            }
            return mobAirPrefs;
        }

        private List<MOBPrefRewardProgram> GetAirRewardPrograms(TravelerProfileResponse traveler)
        {
            List<MOBPrefRewardProgram> mobAirRewardsProgs = new List<MOBPrefRewardProgram>();
            if (traveler?.RewardPrograms != null && traveler?.RewardPrograms.Count > 0)
            {
                foreach (RewardProgramData pref in traveler?.RewardPrograms)
                {
                    MOBPrefRewardProgram mobAirRewardsProg = new MOBPrefRewardProgram();
                    if (traveler?.Profile != null)
                    {
                        mobAirRewardsProg.CustomerId = traveler.Profile.CustomerId;
                        mobAirRewardsProg.ProfileId = traveler.Profile.ProfileId;
                    }
                    mobAirRewardsProg.ProgramMemberId = pref.ProgramMemberId;
                    mobAirRewardsProg.VendorCode = pref.VendorCode;
                    mobAirRewardsProg.VendorDescription = pref.VendorDescription;
                    mobAirRewardsProgs.Add(mobAirRewardsProg);
                }
            }
            return mobAirRewardsProgs;
        }

        private List<MOBAddress> PopulateTravelerAddressesV2(List<Address> addresses, MOBApplication application = null, string flow = null)
        {
            #region

            var mobAddresses = new List<MOBAddress>();
            if (addresses != null && addresses.Count > 0)
            {
                bool isCorpAddressPresent = false;
                //As per Business / DotCom Kalpen; we are removing the condition for checking the Effectivedate and Discontinued date
                var corpIndex = addresses.FindIndex(x => x.TypeDescription != null && x.TypeDescription.ToLower() == "corporate" && x.Line1 != null && x.Line1.Trim() != "");
                if (corpIndex >= 0)
                    isCorpAddressPresent = true;
                foreach (Address address in addresses)
                {

                    if (isCorpAddressPresent && address.TypeDescription.ToLower() == "corporate" &&
                              (_configuration.GetValue<bool>("USPOSCountryCodes_ByPass") || _mPTraveler.IsInternationalBilling(application, address.CountryCode, flow)))
                    {
                        var a = new MOBAddress();
                        a.Key = address.Key;
                        a.Channel = new MOBChannel();
                        a.Channel.ChannelCode = "A";
                        a.Channel.ChannelDescription = "Address";
                        a.Channel.ChannelTypeCode = address.Type.ToString();
                        a.Channel.ChannelTypeDescription = address.TypeDescription;
                        //a.ApartmentNumber = address.AptNum; No longer needed confirmed from service
                        a.City = address.City;
                        // a.CompanyName = address.CompanyName;No longer needed confirmed from service
                        a.Country = new MOBCountry();
                        a.Country.Code = address.CountryCode;
                        a.Country.Name = address.CountryName;
                        // a.JobTitle = address.JobTitle;No longer needed confirmed from service
                        a.Line1 = address.Line1;
                        a.Line2 = address.Line2;
                        a.Line3 = address.Line3;
                        a.State = new MOBState();
                        a.State.Code = address.StateCode;
                        a.IsDefault = address.PrimaryIndicator;
                        a.IsPrivate = address.IsPrivate;
                        a.PostalCode = address.PostalCode;
                        if (address.TypeDescription.ToLower().Trim() == "corporate")
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
                    if (address.EffectiveDate <= DateTime.UtcNow && address.DiscontinuedDate >= DateTime.UtcNow)
                    {
                        if (_configuration.GetValue<bool>("USPOSCountryCodes_ByPass") || _mPTraveler.IsInternationalBilling(application, address.CountryCode, flow)) //##Kirti - allow only US addresses 
                        {
                            var a = new MOBAddress();
                            a.Key = address.Key;
                            a.Channel = new MOBChannel();
                            a.Channel.ChannelCode = "A";
                            a.Channel.ChannelDescription = "Address";
                            a.Channel.ChannelTypeCode = address.Type.ToString();
                            a.Channel.ChannelTypeDescription = address.TypeDescription;
                            a.City = address.City;
                            a.Country = new MOBCountry();
                            a.Country.Code = address.CountryCode;
                            a.Country.Name = address.CountryName;
                            a.Line1 = address.Line1;
                            a.Line2 = address.Line2;
                            a.Line3 = address.Line3;
                            a.State = new MOBState();
                            a.State.Code = address.StateCode;
                            //a.State.Name = address.StateName;
                            a.IsDefault = address.PrimaryIndicator;
                            a.IsPrimary = address.PrimaryIndicator;
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

        private bool IsValidSateForTPIpayment(string stateCode)
        {
            return !string.IsNullOrEmpty(stateCode) && !string.IsNullOrEmpty(_configuration.GetValue<string>("ExcludeUSStateCodesForTripInsurance")) && !_configuration.GetValue<string>("ExcludeUSStateCodesForTripInsurance").Contains(stateCode.ToUpper().Trim());
        }
        private bool IsValidAddressForTPIpayment(string countryCode)
        {
            return !string.IsNullOrEmpty(countryCode) && countryCode.ToUpper().Trim() == "US";
        }

        private List<MOBEmail> PopulateMPSecurityEmailAddressesV2(List<Email> emailAddresses)
        {
            #region
            List<MOBEmail> mobEmailAddresses = new List<MOBEmail>();
            if (emailAddresses != null && emailAddresses.Count > 0)
            {
                MOBEmail primaryEmailAddress = null;
                int co = 0;
                foreach (Email email in emailAddresses)
                {
                    if (email.EffectiveDate <= DateTime.UtcNow && email.DiscontinuedDate >= DateTime.UtcNow)
                    {
                        #region As per Wade Change want to filter out to return only Primary email to client if not primary phone exist return the first one from the list.So lot of work at client side will save time. Sep 15th 2014
                        co = co + 1;
                        MOBEmail e = new MOBEmail();
                        e.Key = email.Key;
                        e.Channel = new MOBChannel();
                        e.Channel.ChannelCode = "E";
                        e.Channel.ChannelDescription = "Email";
                        e.Channel.ChannelTypeCode = email.Type.ToString();
                        e.Channel.ChannelTypeDescription = email.TypeDescription;
                        e.EmailAddress = email.Address;
                        e.IsDefault = email.PrimaryIndicator;
                        e.IsPrimary = email.PrimaryIndicator;
                        e.IsPrivate = email.IsPrivate;
                        e.IsDayOfTravel = email.DayOfTravelNotification;
                        if (email.PrimaryIndicator)
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

        private List<MOBAddress> PopulateCorporateTravelerAddresses(List<United.CorporateDirect.Models.CustomerProfile.Address> addresses, MOBApplication application = null, string flow = null)
        {
            #region
            List<MOBAddress> mobAddresses = new List<MOBAddress>();
            if (addresses != null && addresses.Count > 0)
            {

                foreach (United.CorporateDirect.Models.CustomerProfile.Address address in addresses)
                {
                    if ((_configuration.GetValue<bool>("USPOSCountryCodes_ByPass") || _mPTraveler.IsInternationalBilling(application, address.CountryCode, flow)))
                    {
                        MOBAddress a = new MOBAddress();
                        a.Key = address.Key;
                        a.Channel = new MOBChannel();
                        a.Channel.ChannelCode = address.ChannelCode;
                        a.Channel.ChannelDescription = address.ChannelCodeDescription;
                        a.Channel.ChannelTypeCode = address.ChannelTypeCode.ToString();
                        a.Channel.ChannelTypeDescription = address.ChannelTypeDescription;
                        a.City = address.City;
                        a.Country = new MOBCountry();
                        a.Country.Code = address.CountryCode;
                        a.Line1 = address.AddressLine1;
                        a.State = new MOBState();
                        a.State.Code = address.StateCode;
                        a.PostalCode = address.PostalCode;
                        a.IsPrimary = true;
                        a.IsCorporate = true;
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
            return mobAddresses;
            #endregion
        }

        private List<MOBEmail> PopulateCorporateEmailAddresses(List<United.CorporateDirect.Models.CustomerProfile.Email> emailAddresses, bool onlyDayOfTravelContact)
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
                foreach (United.CorporateDirect.Models.CustomerProfile.Email email in emailAddresses)
                {
                    if (_configuration.GetValue<bool>("CorporateConcurBooking"))
                    {
                        if (isCorpEmailPresent && !onlyDayOfTravelContact && email.ChannelTypeDescription.ToLower() == "corporate")
                        {
                            primaryEmailAddress = new MOBEmail();
                            primaryEmailAddress.Channel = new MOBChannel();
                            primaryEmailAddress.EmailAddress = email.EmailAddress;
                            primaryEmailAddress.Channel.ChannelCode = email.ChannelCode;
                            primaryEmailAddress.Channel.ChannelDescription = email.ChannelCodeDescription;
                            primaryEmailAddress.Channel.ChannelTypeCode = email.ChannelTypeCode.ToString();
                            primaryEmailAddress.Channel.ChannelTypeDescription = email.ChannelTypeDescription;
                            primaryEmailAddress.IsPrimary = true;
                            break;
                        }
                        else if (isCorpEmailPresent && !onlyDayOfTravelContact && email.ChannelTypeDescription.ToLower() != "corporate")
                        {
                            continue;
                        }
                    }
                    //Fix for CheckOut ArgNull Exception - Empty EmailAddress with null EffectiveDate & DiscontinuedDate for Corp Account Revenue Booking (MOBILE-9873) - Shashank : Added OR condition to allow CorporateAccount ProfileOwner.
                    if ((!_configuration.GetValue<bool>("DisableCheckforCorpAccEmail")
                            && email.IsProfileOwner == true && primaryEmailAddress != null))
                    {
                        #region As per Wade Change want to filter out to return only Primary email to client if not primary phone exist return the first one from the list.So lot of work at client side will save time. Sep 15th 2014
                        co = co + 1;
                        MOBEmail e = new MOBEmail();
                        e.Channel = new MOBChannel();
                        e.EmailAddress = email.EmailAddress;
                        e.Channel.ChannelCode = email.ChannelCode;
                        e.Channel.ChannelDescription = email.ChannelCodeDescription;
                        e.Channel.ChannelTypeCode = email.ChannelTypeCode.ToString();
                        e.Channel.ChannelTypeDescription = email.ChannelTypeDescription;

                        if (!onlyDayOfTravelContact)
                        {
                            if (co == 1)
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

        private List<MOBCPPhone> PopulateCorporatePhones(List<United.CorporateDirect.Models.CustomerProfile.Phone> phones, bool onlyDayOfTravelContact)
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
                int co = 0;
                foreach (United.CorporateDirect.Models.CustomerProfile.Phone phone in phones)
                {
                    #region As per Wade Change want to filter out to return only Primary Phone to client if not primary phone exist return the first one from the list.So lot of work at client side will save time. Sep 15th 2014
                    MOBCPPhone mobCPPhone = new MOBCPPhone();
                    co = co + 1;
                    mobCPPhone.PhoneNumber = phone.PhoneNumber;
                    mobCPPhone.ChannelCode = phone.ChannelCode;
                    mobCPPhone.ChannelCodeDescription = phone.ChannelCodeDescription;
                    mobCPPhone.ChannelTypeCode = Convert.ToString(phone.ChannelTypeCode);
                    mobCPPhone.ChannelTypeDescription = phone.ChannelTypeDescription;
                    mobCPPhone.ChannelTypeDescription = phone.ChannelTypeDescription;
                    mobCPPhone.ChannelTypeSeqNumber = 0;
                    mobCPPhone.CountryCode = phone.CountryCode;
                    mobCPPhone.IsProfileOwner = phone.IsProfileOwner;
                    if (phone.PhoneDevices != null && phone.PhoneDevices.Count > 0)
                    {
                        mobCPPhone.DeviceTypeCode = phone.PhoneDevices[0].CommDeviceTypeCode;
                    }


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

                    if (!onlyDayOfTravelContact)
                    {
                        if (co == 1)
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
            }
            return mobCPPhones;
        }

        private List<MOBPrefAirPreference> PopulateCorporateAirPrefrences(List<United.CorporateDirect.Models.CustomerProfile.AirPreference> airPreferences)
        {
            List<MOBPrefAirPreference> mobAirPrefs = new List<MOBPrefAirPreference>();
            if (airPreferences != null && airPreferences.Count > 0)
            {
                foreach (United.CorporateDirect.Models.CustomerProfile.AirPreference pref in airPreferences)
                {
                    MOBPrefAirPreference mobAirPref = new MOBPrefAirPreference();
                    mobAirPref.MealCode = pref.MealCode;
                    mobAirPrefs.Add(mobAirPref);
                }
            }
            return mobAirPrefs;
        }

        public async Task<TravelPolicy> GetCorporateTravelPolicy(MOBMPPINPWDValidateRequest request, Session session, MOBCPCorporate corporateData)
        {
            TravelPolicy corporatePolicy = new TravelPolicy();
            var corpProfileRequest = new CorporateProfileRequest();
            corpProfileRequest.LoyaltyId = request?.MileagePlusNumber;
            string jsonRequest = JsonConvert.SerializeObject(corpProfileRequest);
            var jsonResponse = await _corporateGetService.GetData<CorpPolicyResponse>(session.Token, session.SessionId, jsonRequest).ConfigureAwait(false);
            #region
            if (jsonResponse.response != null)
            {
                if (jsonResponse.response.Errors == null || jsonResponse.response.Errors.Count == 0)
                {
                    await _sessionHelperService.SaveSession<CorpPolicyResponse>(jsonResponse.response, request.DeviceId + request.MileagePlusNumber , new List<string> { request.DeviceId + request.MileagePlusNumber, "CSLCorporatePolicyResponse" }, "CSLCorporatePolicyResponse").ConfigureAwait(false);


                    if (jsonResponse.response.TravelPolicies != null && jsonResponse.response.TravelPolicies.Count > 0)
                    {
                        bool isU4BTravelAddOnPolicy = GeneralHelper.IsEnableU4BTravelAddONPolicy(request.Application.Id, request.Application.Version?.Major, _configuration);

                        //Build Cabin descriptions allowed based on travel policy
                        CorporateTravelPolicy travelPolicy = jsonResponse.response?.TravelPolicies?.FirstOrDefault();
                        CorporateTravelCabinRestriction travelCabinRestrictions = null;

                        if (isU4BTravelAddOnPolicy)
                            travelCabinRestrictions = travelPolicy?.TravelCabinRestrictions?.FirstOrDefault(x => x != null && !string.IsNullOrEmpty(x.TripTypeCode) && x.TripTypeCode == "DE");

                        if (travelCabinRestrictions == null)
                            travelCabinRestrictions = travelPolicy?.TravelCabinRestrictions?.FirstOrDefault();

                        string cabinNameAllowed = GetCabinNameFromCorpTravelPolicy(travelCabinRestrictions);
                        string cabinNameAllowedForLongTripDuration = string.Empty;
                        int duration = 0;
                        if (isU4BTravelAddOnPolicy && travelPolicy?.TravelCabinRestrictions != null && travelPolicy?.TravelCabinRestrictions.Count > 1) //If short trip/long trip duration is available
                        {
                            CorporateTravelCabinRestriction travelCabinRestrictionsForLongTripDuration = travelPolicy?.TravelCabinRestrictions?.FirstOrDefault(x => x != null && !string.IsNullOrEmpty(x.TripTypeCode) && x.TripTypeCode == "LT");
                            duration = travelCabinRestrictionsForLongTripDuration?.Duration != null ? Convert.ToInt32(travelCabinRestrictionsForLongTripDuration?.Duration) : 0;
                            if (duration > 0)
                                cabinNameAllowedForLongTripDuration = GetCabinNameFromCorpTravelPolicy(travelCabinRestrictionsForLongTripDuration);
                        }

                        List<CMSContentMessage> lstMessages = await _utility.GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("U4BCorporateContentMessageGroupName"), "U4BCorporateContentMessageCache").ConfigureAwait(false);

                        List<MOBMobileCMSContentMessages> travelPolicyTitle = null;
                        List<MOBMobileCMSContentMessages> travelPolicyMessage = null;
                        List<MOBMobileCMSContentMessages> travelPolicyBudget = null;
                        List<MOBMobileCMSContentMessages> travelPolicySeat = null;
                        List<MOBMobileCMSContentMessages> travelPolicyFooterMessage = null;
                        List<MOBMobileCMSContentMessages> travelPolicyButton = null;
                        List<MOBMobileCMSContentMessages> travelPolicyCorporateBusinessNamePersonalizedTitle = null;

                        bool isFarePlusTravelAddOnEnabled = isU4BTravelAddOnPolicy && jsonResponse.response.TravelPolicies.FirstOrDefault().IsAirfarePlusTravelAddOn;
                        bool hasDurationFromTravelPolicy = isU4BTravelAddOnPolicy && duration > 0 && !string.IsNullOrEmpty(cabinNameAllowedForLongTripDuration);

                        if (lstMessages != null && lstMessages.Count > 0)
                        {
                            string travelBudgetKey = isFarePlusTravelAddOnEnabled ? "TravelPolicyAddOn.Budget" : "TravelPolicy.Budget";
                            string travelPolicySeatKey = hasDurationFromTravelPolicy ? "TravelPolicyDuration.Seat" : "TravelPolicy.Seat";
                            travelPolicyTitle = _utility.GetSDLMessageFromList(lstMessages, "TravelPolicy.Title");
                            travelPolicyMessage = _utility.GetSDLMessageFromList(lstMessages, "TravelPolicy.Message");
                            travelPolicyBudget = _utility.GetSDLMessageFromList(lstMessages, travelBudgetKey);
                            travelPolicySeat = _utility.GetSDLMessageFromList(lstMessages, travelPolicySeatKey);
                            travelPolicyFooterMessage = _utility.GetSDLMessageFromList(lstMessages, "TravelPolicy.FooterMessage");
                            travelPolicyButton = _utility.GetSDLMessageFromList(lstMessages, "TravelPolicy.Button");
                            travelPolicyCorporateBusinessNamePersonalizedTitle = _utility.GetSDLMessageFromList(lstMessages, "TravelPolicy.CorporateBusinessNamePersonalizedTitle");
                        }

                        string corporateCompanyName = corporateData?.CompanyName;

                        corporatePolicy.TravelPolicyTitle = travelPolicyTitle?.FirstOrDefault()?.ContentShort;

                        if (IsEnableSuppressingCompanyNameForBusiness(request.Application.Id, request.Application.Version.Major))
                        {
                            corporatePolicy.TravelPolicyHeader = corporateData.IsPersonalized ? string.Format(travelPolicyTitle?.FirstOrDefault()?.ContentFull, corporateCompanyName) : travelPolicyCorporateBusinessNamePersonalizedTitle?.FirstOrDefault()?.ContentFull;
                            corporatePolicy.TravelPolicyBody = string.Format(travelPolicyMessage?.FirstOrDefault()?.ContentFull, corporateData.IsPersonalized ? corporateCompanyName : _configuration.GetValue<string>("TravelPolicySplashScreenBodyReplaceCompanyName"));
                        }
                        else
                        {
                            corporatePolicy.TravelPolicyHeader = _configuration.GetValue<bool>("EnableTravelPolicyMessageForOlderClients") ? travelPolicyCorporateBusinessNamePersonalizedTitle?.FirstOrDefault()?.ContentFull : string.Format(travelPolicyTitle?.FirstOrDefault()?.ContentFull, corporateCompanyName);
                            corporatePolicy.TravelPolicyBody = string.Format(travelPolicyMessage?.FirstOrDefault()?.ContentFull, _configuration.GetValue<bool>("EnableTravelPolicyMessageForOlderClients") ? _configuration.GetValue<string>("TravelPolicySplashScreenBodyReplaceCompanyName") : corporateCompanyName);
                        }

                        corporatePolicy.TravelPolicyContent = new List<MOBSection>();
                        bool isShowBudgetOnTravelPolicy = isU4BTravelAddOnPolicy ? travelPolicy?.MaximumBudget > 0 && (travelPolicy.IsAirfare || travelPolicy.IsAirfarePlusTravelAddOn) : travelPolicy?.MaximumBudget > 0;

                        if (isShowBudgetOnTravelPolicy)
                        {
                            string maximumBudget = isU4BTravelAddOnPolicy ? string.Format("{0:n0}", travelPolicy?.MaximumBudget) : travelPolicy?.MaximumBudget.ToString();
                            corporatePolicy.TravelPolicyContent.Add(new MOBSection
                            {
                                Text1 = travelPolicyBudget?.FirstOrDefault()?.ContentShort?.Split('|')?.FirstOrDefault(),
                                Text2 = string.Format(travelPolicyBudget?.FirstOrDefault()?.ContentFull, maximumBudget),
                                Text3 = travelPolicyBudget?.FirstOrDefault()?.ContentShort?.Split('|')?.LastOrDefault()
                            });
                        }
                        if (!string.IsNullOrEmpty(cabinNameAllowed))
                        {
                            corporatePolicy.TravelPolicyContent.Add(new MOBSection
                            {
                                Text1 = travelPolicySeat?.FirstOrDefault()?.ContentShort?.Split('|')?.FirstOrDefault(),
                                Text2 = hasDurationFromTravelPolicy ? string.Format(travelPolicySeat?.FirstOrDefault()?.ContentFull, duration, cabinNameAllowed, cabinNameAllowedForLongTripDuration) : string.Format(travelPolicySeat?.FirstOrDefault()?.ContentFull, cabinNameAllowed),
                                Text3 = travelPolicySeat?.FirstOrDefault()?.ContentShort?.Split('|')?.LastOrDefault()
                            });
                        }
                        corporatePolicy.TravelPolicyFooterMessage = travelPolicyFooterMessage?.FirstOrDefault()?.ContentFull;
                        corporatePolicy.TravelPolicyButton = new List<string>();
                        corporatePolicy.TravelPolicyButton.Add(travelPolicyButton?.FirstOrDefault()?.ContentFull);
                    }
                }
                else
                {
                    throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
            }
            else
            {
                throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
            #endregion
            return corporatePolicy;
        }

        public string GetCabinNameFromCorpTravelPolicy(CorporateTravelCabinRestriction travelCabinRestrictions)
        {
            string cabinNameAllowed = string.Empty;
            var U4BCorporateCabinTypes = _configuration.GetValue<string>("U4BCorporateCabinTypes").Split('|');

            //if (travelPolicy.IsBasicEconomyAllowed.Value)
            //    cabinNameAllowed = cabinNameAllowed + U4BCorporateCabinTypes[0];
            if (travelCabinRestrictions.IsEconomyAllowed.Value)
                cabinNameAllowed = !string.IsNullOrEmpty(cabinNameAllowed) ? cabinNameAllowed + ", " + U4BCorporateCabinTypes[1] : cabinNameAllowed + U4BCorporateCabinTypes[1];
            if (travelCabinRestrictions.IsPremiumEconomyAllowed.Value)
                cabinNameAllowed = !string.IsNullOrEmpty(cabinNameAllowed) ? cabinNameAllowed + ", " + U4BCorporateCabinTypes[2] : cabinNameAllowed + U4BCorporateCabinTypes[2];
            if (travelCabinRestrictions.IsBusinessFirstAllowed.Value)
                cabinNameAllowed = !string.IsNullOrEmpty(cabinNameAllowed) ? cabinNameAllowed + ", " + U4BCorporateCabinTypes[3] : cabinNameAllowed + U4BCorporateCabinTypes[3];
            return cabinNameAllowed;
        }

        public bool IsEnableSuppressingCompanyNameForBusiness(int applicationId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableSuppressingCompanyNameForBusiness")
                && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_IsEnableSuppressingCompanyNameForBusiness_AppVersion"), _configuration.GetValue<string>("IPhone_IsEnableSuppressingCompanyNameForBusiness_AppVersion"));
        }
    }
}
