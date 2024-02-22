using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.CSLSerivce;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Exception;
using United.Services.ProfileValidation.Common;
using United.Utility.Helper;

namespace United.Common.Helper.Profile
{
    public class CustomerProfile : ICustomerProfile
    {
        private readonly IConfiguration _configuration;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly ICacheLog<CustomerProfile> _logger;
        private readonly ICustomerDataService _customerDataService;
        private readonly ICustomerPreferencesService _customerPreferencesService;
        private readonly IUtilitiesService _utilitiesService;
        private readonly IMPTraveler _mpTraveler;
        private readonly ICorporateProfile _corpProfile;
        private readonly IHeaders _headers;

        public CustomerProfile(
             IConfiguration configuration
            , ISessionHelperService sessionHelperService
            , ICustomerDataService mPEnrollmentService
            , ICacheLog<CustomerProfile> logger
            , ICustomerPreferencesService customerPreferencesService
            , IMPTraveler mPTraveler
            , IUtilitiesService utilitiesService
            , ICorporateProfile corpProfile
            , IHeaders headers
            )
        {
            _configuration = configuration;
            _sessionHelperService = sessionHelperService;
            _customerDataService = mPEnrollmentService;
            _logger = logger;
            _customerPreferencesService = customerPreferencesService;
            _utilitiesService = utilitiesService;
            _mpTraveler = mPTraveler;
            _corpProfile = corpProfile;
            _headers = headers;
        }

        public async Task<List<MOBCPProfile>> PopulateProfiles(string sessionId, string mileagePlusNumber, int customerId, List<Services.Customer.Common.Profile> profiles, MOBCPProfileRequest request, bool getMPSecurityDetails = false, string path = "", MOBApplication application = null)
        {
            List<MOBCPProfile> mobProfiles = null;
            if (profiles != null && profiles.Count > 0)
            {
                CSLProfile persistedCSLProfile = new CSLProfile();
                persistedCSLProfile = await _sessionHelperService.GetSession<CSLProfile>(sessionId, persistedCSLProfile.ObjectName, new List<string>() { sessionId, persistedCSLProfile.ObjectName }).ConfigureAwait(false);
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

                foreach (var profile in profiles)
                {
                    if (profile.Travelers != null && profile.Travelers.Count > 0)
                    {
                        MOBCPProfile mobProfile = new MOBCPProfile();
                        mobProfile.AirportCode = profile.AirportCode;
                        mobProfile.AirportNameLong = profile.AirportNameLong;
                        mobProfile.AirportNameShort = profile.AirportNameShort;
                        mobProfile.Description = profile.Description;
                        mobProfile.Key = profile.Key;
                        mobProfile.LanguageCode = profile.LanguageCode;
                        mobProfile.ProfileId = profile.ProfileId;
                        mobProfile.ProfileMembers = PopulateProfileMembers(profile.ProfileMembers);
                        mobProfile.ProfileOwnerId = profile.ProfileOwnerId;
                        mobProfile.ProfileOwnerKey = profile.ProfileOwnerKey;

                        //Kirti - code breaking due to DLL update so commented 
                        //mobProfile.QuickCreditCardKey = profile.QuickCreditCardKey;
                        //mobProfile.QuickCreditCardNumber = profile.QuickCreditCardNum;

                        mobProfile.QuickCustomerId = profile.QuickCustomerId;
                        mobProfile.QuickCustomerKey = profile.QuickCustomerKey;
                        if (_configuration.GetValue<bool>("CorporateConcurBooking"))
                        {
                            mobProfile.CorporateData = _corpProfile.PopulateCorporateData(profile.CorporateData, application);
                        }
                        bool isProfileOwnerTSAFlagOn = false;
                        var tupleRes = await _mpTraveler.PopulateTravelers(profile.Travelers, mileagePlusNumber, isProfileOwnerTSAFlagOn, false, request, sessionId, getMPSecurityDetails, path).ConfigureAwait(false);
                        mobProfile.Travelers = tupleRes.mobTravelersOwnerFirstInList;
                        mobProfile.SavedTravelersMPList = tupleRes.savedTravelersMPList;
                        mobProfile.IsProfileOwnerTSAFlagON = tupleRes.isProfileOwnerTSAFlagOn;
                        if (mobProfile != null)
                        {
                            mobProfile.DisclaimerList = await _mpTraveler.GetProfileDisclaimerList().ConfigureAwait(false);
                        }
                        mobProfiles.Add(mobProfile);
                    }
                }
            }

            return mobProfiles;
        }

        public async Task<MOBCustomerPreferencesResponse> RetrieveCustomerPreferences(MOBCustomerPreferencesRequest request, string token)
        {
            MOBCustomerPreferencesResponse mobResponse = new MOBCustomerPreferencesResponse();

            try
            {
                var response = await _customerPreferencesService.GetCustomerPreferences<United.Services.Loyalty.Preferences.Common.AirPreferenceCompositeDataModel>(token, request.MileagePlusNumber, _headers.ContextValues.SessionId).ConfigureAwait(false);

                if (response != null)
                {
                    if ((response != null) && (response.ExpertMode != null) && (response.ExpertMode.GETexpertModeData != null))
                    {
                        mobResponse.IsExpertModeEnabled = response.ExpertMode.GETexpertModeData.ExpertModeFlag;
                    }
                }
            }
            catch (System.Net.WebException wex)
            {
                if (wex.Response != null)
                {
                    var errorResponse = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                    _logger.LogError("RetrieveCustomerPreferences-Exception {errorResponse}", GeneralHelper.RemoveCarriageReturn(errorResponse));
                    throw new System.Exception(wex.Message);

                }
            }

            return mobResponse;
        }

        private List<MOBCPProfileMember> PopulateProfileMembers(List<United.Services.Customer.Common.ProfileMember> profileMembers)
        {
            List<MOBCPProfileMember> mobProfileMembers = null;

            if (profileMembers != null && profileMembers.Count > 0)
            {
                mobProfileMembers = new List<MOBCPProfileMember>();
                foreach (var profileMember in profileMembers)
                {
                    MOBCPProfileMember mobProfileMember = new MOBCPProfileMember();
                    mobProfileMember.CustomerId = profileMember.CustomerId;
                    mobProfileMember.Key = profileMember.Key;
                    mobProfileMember.LanguageCode = profileMember.LanguageCode;
                    mobProfileMember.ProfileId = profileMember.ProfileId;

                    mobProfileMembers.Add(mobProfileMember);
                }
            }

            return mobProfileMembers;
        }

        private United.Services.Customer.Common.ProfileRequest GetEmailIDTFAProfileRequest(MOBMPPINPWDValidateRequest mobPINPWDProfileRequest)
        {
            United.Services.Customer.Common.ProfileRequest request = new United.Services.Customer.Common.ProfileRequest();
            request.DataSetting = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString();
            List<string> requestStringList = new List<string>();
            requestStringList.Add("EmailAddresses");
            request.DataToLoad = requestStringList;
            request.LoyaltyId = mobPINPWDProfileRequest.MileagePlusNumber;
            request.MemberCustomerIdsToLoad = new List<int>();
            request.LangCode = "en-US";
            return request;
        }

        public async Task<MOBCPProfile> GeteMailIDTFAMPSecurityDetails(MOBMPPINPWDValidateRequest request, string token)
        {
            if (request == null)
            {
                throw new MOBUnitedException("Profile request cannot be null.");
            }
            MOBCPProfile mpSecurityCheckDetails = null;

            United.Services.Customer.Common.ProfileRequest profileRequest = GetEmailIDTFAProfileRequest(request);

            string jsonRequest = DataContextJsonSerializer.Serialize<United.Services.Customer.Common.ProfileRequest>(profileRequest);

            string path = "/GetProfile";

            var response = await _customerDataService.InsertMPEnrollment<Services.Customer.Common.ProfileResponse>(token, jsonRequest, _headers.ContextValues.SessionId, path).ConfigureAwait(false);

            if (response != null)
            {
                if (response != null && response.Status.Equals(United.Services.Customer.Common.Constants.StatusType.Success) && response.Profiles != null)
                {
                    MOBCPProfileRequest mobProfileRequest = null;
                    List<MOBCPProfile> profiles = await PopulateProfiles(Guid.NewGuid().ToString().ToUpper().Replace("-", ""), request.MileagePlusNumber, response.Profiles[0].CustomerId, response.Profiles, mobProfileRequest, false, application: request.Application).ConfigureAwait(false);
                    if (profiles != null && profiles.Count > 0)
                        mpSecurityCheckDetails = profiles[0];
                }
                else
                {
                    if (response.Errors != null && response.Errors.Count > 0)
                    {
                        string errorMessage = string.Empty;
                        foreach (var error in response.Errors)
                        {
                            if (!string.IsNullOrEmpty(error.UserFriendlyMessage))
                            {
                                errorMessage = errorMessage + " " + error.UserFriendlyMessage;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(error.Message) && error.Message.ToUpper().Trim().Contains("INVALID"))
                                {
                                    errorMessage = errorMessage + " " + "Invalid MileagePlusId " + request.MileagePlusNumber;
                                }
                                else
                                {
                                    errorMessage = errorMessage + " " + (error.MinorDescription != null ? error.MinorDescription : string.Empty);
                                }
                            }
                        }

                        throw new MOBUnitedException(errorMessage);
                    }
                    else
                    {
                        throw new MOBUnitedException("Unable to get profile.");
                    }
                }
            }
            else
            {
                throw new MOBUnitedException("Unable to get profile.");
            }

            return mpSecurityCheckDetails;
        }

        public bool EnableYoungAdult(bool isReshop = false)
        {
            return _configuration.GetValue<bool>("EnableYoungAdultBooking") && !isReshop;
        }

        public async Task<bool> ValidateMPNames(string token, string langCode, string title, string firstName, string middleName, string lastName, string suffix, string mileagePlusId, string sessionId, int appId, string appVersion, string deviceId)
        {
            bool isMpNamesValidated = false;

            United.Services.ProfileValidation.Common.ValidateMileagePlusNamesRequest validateMPRequest = new ValidateMileagePlusNamesRequest
            {
                DataSetting = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString(),
                LangCode = langCode,
                ValidateMileagePlusNameRequests = new List<ValidateMileagePlusNameRequest>()
            };
            validateMPRequest.ValidateMileagePlusNameRequests.Add(
                new ValidateMileagePlusNameRequest()
                {
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,
                    MileagePlusId = mileagePlusId,
                    Suffix = suffix,
                    Title = title,
                    UseStartsWithNameLogic = false, // ALM 27062 fix udpated UseStartsWithNameLogic = true to false to exactly match the name true will match the name starts with
                    ValidateMileagePlusId = true
                });

            string path = "ValidateMileagePlusNames";
            string jsonRequest = JsonConvert.SerializeObject(validateMPRequest);
            var response = await _utilitiesService.ValidateMileagePlusNames<ValidateMileagePlusNamesResponse>(token, jsonRequest, sessionId, path).ConfigureAwait(false);

            if (response != null)
            {
                if (response != null && response.Status.Equals(United.Services.ProfileValidation.Common.Constants.StatusType.Success) && response.ValidateMileagePlusNameResponses.Count > 0 &&
                    response.ValidateMileagePlusNameResponses[0].Status.Equals(United.Services.ProfileValidation.Common.Constants.StatusType.Success))
                {
                    isMpNamesValidated = true;
                }
                else
                {
                    string exceptionMessage = _configuration.GetValue<string>("ValidateMileagePlusNamesErrorMessage");

                    if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && _configuration.GetValue<bool>("ReturnActualExceptionMessageBackForTesting"))
                    {
                        exceptionMessage = exceptionMessage + " response.Status not success - at DAL ValidateMPNames";
                    }
                    throw new MOBUnitedException(exceptionMessage);
                }
            }

            return isMpNamesValidated;
        }
    }
}
