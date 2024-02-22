using Css.SecureProfile;
using Css.SecureProfile.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using United.Mobile.DataAccess.CSLSerivce;
using United.Mobile.DataAccess.MPSignIn;
using United.Mobile.Model.Common;
using United.Mobile.Model.CSLModels;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.Internal.Exception;
using United.Mobile.Model.MPSignIn.Common;
using United.Service.Presentation.SecurityResponseModel;
using United.Services.Customer.Common;
using United.Services.ProfileValidation.Common;
using United.Utility.Helper;
using Genre = United.Service.Presentation.CommonModel.Genre;

namespace United.Common.Helper.Profile
{
    public class MileagePlusCSL : IMileagePlusCSL
    {
        private readonly ICacheLog<MileagePlusCSL> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICustomerDataService _customerDataService;
        private readonly ICustomerProfileService _customerProfileService;
        private readonly IMemberInfoRecommendationService _memberInfoRecommendationService;
        private readonly IUtilitiesService _profileValidationService;
        private readonly IMemberProfileService _memberProfileService;
        private readonly IMPSecurityQuestionsService _mPSecurityQuestionsService;
        private readonly IMPSecurityCheckDetailsService _mPSecurityCheckDetailsService;
        private readonly IHeaders _headers;

        public MileagePlusCSL(IConfiguration configuration
             , ICacheLog<MileagePlusCSL> logger
            , ICustomerDataService customerDataService
            , ICustomerProfileService customerProfileService
            , IMemberInfoRecommendationService memberInfoRecommendationService
            , IUtilitiesService profileValidationService
            , IMemberProfileService memberProfileService
            , IMPSecurityQuestionsService mPSecurityQuestionsService
            , IMPSecurityCheckDetailsService mPSecurityCheckDetailsService
            , IHeaders headers)
        {
            _configuration = configuration;
            _logger = logger;
            _customerDataService = customerDataService;
            _customerProfileService = customerProfileService;
            _memberInfoRecommendationService = memberInfoRecommendationService;
            _mPSecurityQuestionsService = mPSecurityQuestionsService;
            _profileValidationService = profileValidationService;
            _memberProfileService = memberProfileService;
            _mPSecurityCheckDetailsService = mPSecurityCheckDetailsService;
            _headers = headers;
        }

        public async Task<bool> ValidateMileagePlusNames(MOBMPSignInNeedHelpRequest request, string token, string sessionId, int appId, string appVersion, string deviceId)
        {

            List<ValidateMileagePlusNameRequest> ValidateMPNameRequestList = new List<ValidateMileagePlusNameRequest>();
            ValidateMPNameRequestList.Add(new ValidateMileagePlusNameRequest
            {
                FirstName = request.MPSignInNeedHelpItems.NeedHelpSignInInfo.First,
                LastName = request.MPSignInNeedHelpItems.NeedHelpSignInInfo.Last,
                MiddleName = request.MPSignInNeedHelpItems.NeedHelpSignInInfo.Middle,
                MileagePlusId = request.MPSignInNeedHelpItems.MileagePlusNumber,
                Suffix = request.MPSignInNeedHelpItems.NeedHelpSignInInfo.Suffix,
                Title = request.MPSignInNeedHelpItems.NeedHelpSignInInfo.Title,
                UseStartsWithNameLogic = false,
                ValidateMileagePlusId = true
            });
            ValidateMileagePlusNamesRequest ValidateMPNameRequest = new ValidateMileagePlusNamesRequest
            {
                DataSetting = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices"),
                LangCode = "en-US",
                ValidateMileagePlusNameRequests = ValidateMPNameRequestList
            };
            string path = "ValidateMileagePlusNames";
            string jsonRequest = JsonConvert.SerializeObject(ValidateMPNameRequest);
            _logger.LogInformation("GetWrongAnswersFlag - Request for {@clientRequest} ", JsonConvert.SerializeObject(jsonRequest));

            var response = await _profileValidationService.ValidateMileagePlusNames<United.Services.ProfileValidation.Common.ValidateMileagePlusNamesResponse>(token, jsonRequest, sessionId, path).ConfigureAwait(false);
            if (response != null)
            {

                if (response != null && response.Status.Equals(United.Services.ProfileValidation.Common.Constants.StatusType.Success)) // As discussed with Venkat, result sould return only one match
                {
                    _logger.LogInformation("GetWrongAnswersFlag - response for {response}", JsonConvert.SerializeObject(response));
                    return true;
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
                                errorMessage = errorMessage + " " + error.Message;
                            }
                        }
                        _logger.LogError("GetWrongAnswersFlag - error for {response}", JsonConvert.SerializeObject(response));
                        throw new MOBUnitedException(errorMessage);
                    }

                    else
                    {
                        string exceptionMessage = _configuration.GetValue<string>("MPValidationErrorMessage") != null ? _configuration.GetValue<string>("MPValidationErrorMessage").ToString() : "The account information you entered is incorrect.";
                        if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && _configuration.GetValue<bool>("ReturnActualExceptionMessageBackForTesting"))

                        {
                            exceptionMessage = exceptionMessage + " response.Status not success and response.Errors.Count == 0 - at DAL ValidateMileagePlusNames";
                        }
                        throw new MOBUnitedException(exceptionMessage);
                    }
                }
            }
            else
            {
                string exceptionMessage = _configuration.GetValue<string>("MPValidationErrorMessage") != null ? _configuration.GetValue<string>("MPValidationErrorMessage").ToString() : "The account information you entered is incorrect.";
                if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && _configuration.GetValue<bool>("ReturnActualExceptionMessageBackForTesting"))
                {
                    exceptionMessage = exceptionMessage + " - due to jsonResponse is empty at DAL  UpdateWrongAnswersFlag";
                }
                throw new MOBUnitedException(exceptionMessage);
            }
        }

        public async Task<bool> ValidateMemberName(MOBMPSignInNeedHelpRequest request, string token, string sessionId, int appId, string appVersion, string deviceId)
        {
            var validateMemberNameRequest = new ValidateMemberNameRequest
            {
                FirstName = request.MPSignInNeedHelpItems.NeedHelpSignInInfo.First,
                LastName = request.MPSignInNeedHelpItems.NeedHelpSignInInfo.Last,
                MiddleName = request.MPSignInNeedHelpItems.NeedHelpSignInInfo.Middle ?? string.Empty,
                Suffix = request.MPSignInNeedHelpItems.NeedHelpSignInInfo.Suffix ?? string.Empty
            };

            string path = $"/validatemembername/LoyaltyId/{request.MPSignInNeedHelpItems.MileagePlusNumber}";
            string jsonRequest = JsonConvert.SerializeObject(validateMemberNameRequest);
            _logger.LogInformation("GetWrongAnswersFlag - Request for {@clientRequest} ", JsonConvert.SerializeObject(jsonRequest));

            var responseJson = await _memberProfileService.ValidateMemberName(token, jsonRequest, sessionId, path).ConfigureAwait(false);
            var response = JsonConvert.DeserializeObject<ValidateMemberNameResponse>(responseJson);

            if (response != null && !string.IsNullOrEmpty(response.Data.MatchResult))
            {
                return response.Data?.MatchResult.ToUpper() == "MATCH";
            }
            else
            {
                if (response.Errors != null && response.Errors.Any())
                {
                    string exceptionMessage = _configuration.GetValue<string>("MPValidationErrorMessage") != null ? _configuration.GetValue<string>("MPValidationErrorMessage").ToString() : "The account information you entered is incorrect.";
                    throw new MOBUnitedException(exceptionMessage);
                }
                else
                {
                    return false;
                }
            }
        }
        public MOBMPAccountValidation ValidateAccount(int applicationId, string deviceId, string appVersion, string transactionId, string mileagePlusNumber, string pinCode, Session shopTokenSession, bool validAuthTokenHashPinCheckFailed)
        {
            #region
            MOBMPAccountValidation accountValidation = new MOBMPAccountValidation();
            //UAWSAccountValidation.wsAccountResponse response = new UAWSAccountValidation.wsAccountResponse();
            //United.Mobile.DAL.Providers.CSL.Authentication.AuthenticateCSSToken(applicationId, deviceId, appVersion, transactionId, shopTokenSession, mileagePlusNumber, pinCode, LogEntries, levelSwitch, ref accountValidation, validAuthTokenHashPinCheckFailed);
            return accountValidation;
            #endregion
        }
        public async Task<bool> GetWrongAnswersFlag(bool answeredQuestionsIncorrectly, string token, string sessionId, int appId, string appVersion, string deviceId, int customerID = 0, string loyaltyId = null)
        {
            UpdateWrongAnswersFlagRequest request = new UpdateWrongAnswersFlagRequest();
            var response = new SearchResponse();
            request.AnsweredQuestionsIncorrectly = answeredQuestionsIncorrectly;
            request.DataSetting = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices");
            request.LoyaltyId = loyaltyId.ToUpper().Trim();
            string path = "/GetWrongAnswersFlag";
            string jsonRequest = JsonConvert.SerializeObject(request);

            _logger.LogInformation("GetWrongAnswersFlag - Request for {@clientRequest}", JsonConvert.SerializeObject(jsonRequest));

            if (_configuration.GetValue<bool>("EnableUCBVersion_UrlChanges"))
                response = await _customerDataService.InsertMPEnrollment<SearchResponse>(token, jsonRequest, _headers.ContextValues.SessionId, path).ConfigureAwait(false);
            else
                response = await _mPSecurityCheckDetailsService.InsertMPEnrollment<SearchResponse>(token, jsonRequest, _headers.ContextValues.SessionId, path).ConfigureAwait(false);

            if (response != null)
            {               

                if (response != null && response.Status.Equals(United.Services.Customer.Common.Constants.StatusType.Success)) // As discussed with Venkat, result sould return only one match
                {
                    _logger.LogInformation("GetWrongAnswersFlag - Response for {response}", JsonConvert.SerializeObject(response));
                    return response.Result;
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
                                errorMessage = errorMessage + " " + error.Message;
                            }
                        }
                        _logger.LogError("GetWrongAnswersFlag - error for {response}", JsonConvert.SerializeObject(response));
                        throw new MOBUnitedException(errorMessage);
                    }

                    else
                    {
                        string exceptionMessage = _configuration.GetValue<string>("UnableToGetAnswersFlag").ToString();
                        if (_configuration.GetValue<bool>("ReturnActualExceptionMessageBackForTesting"))
                        {
                            exceptionMessage = exceptionMessage + " response.Status not success and response.Errors.Count == 0 - at DAL GetWrongAnswersFlag";
                        }
                        throw new MOBUnitedException(exceptionMessage);
                    }
                }
            }
            else
            {
                string exceptionMessage = _configuration.GetValue<string>("UnableToGetAnswersFlag").ToString();
                if (_configuration.GetValue<bool>("ReturnActualExceptionMessageBackForTesting"))
                {
                    exceptionMessage = exceptionMessage + " - due to jsonResponse is empty at DAL  GetWrongAnswersFlag";
                }
                throw new MOBUnitedException(exceptionMessage);
            }

        }
        public async Task<bool> UpdateWrongAnswersFlag(bool answeredQuestionsIncorrectly, string token, string sessionId, int appId, string appVersion, string deviceId, int customerID = 0, string loyaltyId = null)
        {
            United.Services.Customer.Common.UpdateWrongAnswersFlagRequest request = new UpdateWrongAnswersFlagRequest();
            request.AnsweredQuestionsIncorrectly = answeredQuestionsIncorrectly;
            request.DataSetting = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString();
            request.LoyaltyId = loyaltyId.ToUpper().Trim();
            string path = "/UpdateWrongAnswersFlag";
            string jsonRequest = DataContextJsonSerializer.Serialize<United.Services.Customer.Common.UpdateWrongAnswersFlagRequest>(request);
            var response = new SaveResponse();

            if (_configuration.GetValue<bool>("EnableUCBVersion_UrlChanges"))
                response = await _customerDataService.ValidateCustomerData<United.Services.Customer.Common.SaveResponse>(token, jsonRequest, _headers.ContextValues.SessionId, path).ConfigureAwait(false);
            else
                response = await _mPSecurityCheckDetailsService.ValidateCustomerData<United.Services.Customer.Common.SaveResponse>(token, jsonRequest, _headers.ContextValues.SessionId, path).ConfigureAwait(false);

            if (response != null)
            {
                if (response != null && response.Status.Equals(United.Services.Customer.Common.Constants.StatusType.Success)) // As discussed with Venkat, result sould return only one match
                {
                    return true;
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
                                errorMessage = errorMessage + " " + error.Message;
                            }
                        }

                        throw new MOBUnitedException(errorMessage);
                    }

                    else
                    {
                        string exceptionMessage = _configuration.GetValue<string>("UnableToUpdateAnswersFlag").ToString();
                        if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && Convert.ToBoolean(_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting").ToString()))
                        {
                            exceptionMessage = exceptionMessage + " response.Status not success and response.Errors.Count == 0 - at DAL UpdateWrongAnswersFlag";
                        }
                        throw new MOBUnitedException(exceptionMessage);
                    }
                }
            }
            else
            {
                string exceptionMessage = _configuration.GetValue<string>("UnableToUpdateAnswersFlag").ToString();
                if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && Convert.ToBoolean(_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting").ToString()))
                {
                    exceptionMessage = exceptionMessage + " - due to jsonResponse is empty at DAL  UpdateWrongAnswersFlag";
                }
                throw new MOBUnitedException(exceptionMessage);
            }
        }
        public async Task<bool> SearchMPAccount(MOBMPSignInNeedHelpRequest needHelpRequest, string token, string sessionId, int appId, string appVersion, string deviceId)
        {
            bool isMpFound = false;
            SearchRequest searchObj = new SearchRequest();

            searchObj.DataSetting = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString();
            searchObj.LangCode = needHelpRequest.LanguageCode;
            searchObj.EmailAddress = needHelpRequest.MPSignInNeedHelpItems.EmailAddress; //enrollmentRequest.MPEnrollmentDetails.ContactInformation.EmailAddress;
            searchObj.FirstName = needHelpRequest.MPSignInNeedHelpItems.NeedHelpSignInInfo.First;//enrollmentRequest.MPEnrollmentDetails.PersonalInformation.FirstName;
            searchObj.LastName = needHelpRequest.MPSignInNeedHelpItems.NeedHelpSignInInfo.Last;//enrollmentRequest.MPEnrollmentDetails.PersonalInformation.LastName;
            searchObj.MiddleName = needHelpRequest.MPSignInNeedHelpItems.NeedHelpSignInInfo.Middle;//enrollmentRequest.MPEnrollmentDetails.PersonalInformation.MiddleName;
            searchObj.GetAdditionalInfo = false;
            searchObj.ReturnSurvivorOnly = false;
            searchObj.SendMpIdReminderEmail = true;
            searchObj.Suffix = needHelpRequest.MPSignInNeedHelpItems.NeedHelpSignInInfo.Suffix;//enrollmentRequest.MPEnrollmentDetails.PersonalInformation.Suffix;
            searchObj.Title = needHelpRequest.MPSignInNeedHelpItems.NeedHelpSignInInfo.Title;//enrollmentRequest.MPEnrollmentDetails.PersonalInformation.Title;

            if (_configuration.GetValue<bool>("BugFixToggleFor18C"))
            {
                searchObj.ExactMatch = true;
            }
            string path = "/GetSearchAccount";

            string jsonRequest = DataContextJsonSerializer.Serialize<SearchRequest>(searchObj);

            _logger.LogInformation("SearchMPAccount - Request for GetSearchAccount {@clientRequest}", JsonConvert.SerializeObject(jsonRequest) );

            var response = await _customerDataService.InsertMPEnrollment<SearchResponse>(token, jsonRequest, _headers.ContextValues.SessionId, path).ConfigureAwait(false);

            if (response != null)
            {
                _logger.LogInformation("SearchMPAccount {@clientResponse}", JsonConvert.SerializeObject(response));

                if (response != null && response.Status.Equals(United.Services.Customer.Common.Constants.StatusType.Success) && response.Travelers.Count == 1) // As discussed with Venkat, result sould return only one match
                {

                    isMpFound = true;
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
                                errorMessage = errorMessage + " " + error.Message;
                            }
                        }

                        throw new MOBUnitedException(errorMessage);
                    }
                    else
                    {
                        isMpFound = false;
                    }
                }
            }
            else
            {
                string exceptionMessage = _configuration.GetValue<string>("UnableToSearchMPErrorMessage").ToString();
                if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && Convert.ToBoolean(_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting").ToString()))
                {
                    exceptionMessage = exceptionMessage + " - due to jsonResponse is empty at DAL  SearchMPAccount";
                }
                throw new MOBUnitedException(exceptionMessage);
            }
            return isMpFound;
        }

        public async Task<(List<MOBCustomerSearchDetail>, bool)> SearchCustomer(MOBMPSignInNeedHelpRequest request, string token, string sessionId, int appId, string appVersion, string deviceId)
        {
            MOBCustomerSearchResponse response;
            var needHelpItems = request.MPSignInNeedHelpItems;
            var signInInfo = needHelpItems.NeedHelpSignInInfo;

            var path = "/CustomerSearch?searchCount=5";

            if (!string.IsNullOrEmpty(signInInfo.Last))
                path += $"&lastName={signInInfo.Last}";
            if (!string.IsNullOrEmpty(signInInfo.First))
                path += $"&firstName={signInInfo.First}";
            if (!string.IsNullOrEmpty(signInInfo.DateOfBirth))
                path += $"&birthDate={signInInfo.DateOfBirth}";

            if (!string.IsNullOrEmpty(needHelpItems.EmailAddress))
                path += $"&emailAddress={needHelpItems.EmailAddress}";
            if (!string.IsNullOrEmpty(needHelpItems.MileagePlusNumber))
                path += $"&loyaltyId={needHelpItems.MileagePlusNumber}";

            //string jsonRequest = DataContextJsonSerializer.Serialize(request);
            string jsonResponse = await _customerProfileService.Search(token, _headers.ContextValues.SessionId, path).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                response = JsonConvert.DeserializeObject<MOBCustomerSearchResponse>(jsonResponse);
            }
            else
            {
                return (new List<MOBCustomerSearchDetail>(), false);
            }

            return (response.CustomerSearchDetails, response.Errors.Any());
        }

        public async Task<MOBCustomerMPSearchResponse> SearchMPNumber(MOBMPSignInNeedHelpRequest request, string token, string sessionId)
        {
            var needHelpItems = request.MPSignInNeedHelpItems;
            var signInInfo = needHelpItems.NeedHelpSignInInfo;

            string path = "/SearchMPNumber?";

            if (!string.IsNullOrEmpty(needHelpItems.EmailAddress))
                path += $"&email={needHelpItems.EmailAddress}";
            if (!string.IsNullOrEmpty(signInInfo.DateOfBirth))
                path += $"&dob={signInInfo.DateOfBirth}";

            var jsonRequest = JsonConvert.SerializeObject(request);

            var jsonResponse = await _customerProfileService.SearchMPNumber(path, token, sessionId);
            var customerMPSearchResponse = JsonConvert.DeserializeObject<MOBCustomerMPSearchResponse>(jsonResponse);

            return customerMPSearchResponse;
        }

        public async Task<MOBCustomerAllMPSearchResponse> SearchAllMPNumbers(MOBMPSignInNeedHelpRequest request, string token, string sessionId)
        {
            var needHelpItems = request.MPSignInNeedHelpItems;
            var signInInfo = needHelpItems.NeedHelpSignInInfo;

            string path = "/SearchAllMPNumbers?";

            if (!string.IsNullOrEmpty(needHelpItems.EmailAddress))
                path += $"&email={needHelpItems.EmailAddress}";
            if (!string.IsNullOrEmpty(signInInfo.DateOfBirth))
                path += $"&dob={signInInfo.DateOfBirth}";

            var jsonRequest = JsonConvert.SerializeObject(request);

            var jsonResponse = await _customerProfileService.SearchMPNumber(path, token, sessionId);
            var response = JsonConvert.DeserializeObject<MOBCustomerAllMPSearchResponse>(jsonResponse);

            return response;
        }

        public async Task<MOBMemberInfoRecommendationResponse> RecommendedMPNumbers(List<string> mpNumbers, string token, string sessionId)
        {
            string path = "/recommended";
            string jsonRequest = JsonConvert.SerializeObject(new { MileagePlusIds = mpNumbers });
            
            var jsonResponse = await _memberInfoRecommendationService.RecommendedMPNumbers(path, jsonRequest, token, sessionId);
            var response = JsonConvert.DeserializeObject<MOBMemberInfoRecommendationResponse>(jsonResponse);

            return response;
        }

        public async Task<bool> ShuffleSavedSecurityQuestions(string token, string sessionId, int appId, string appVersion, string deviceId, string MileagePlusID, int customerID = 0, string loyaltyId = null)
        {
            bool StatusFlag = false;
            if (_configuration.GetValue<bool>("EnableDPToken"))
            {
                string path = string.Format("/ShuffleSavedSecurityQuestions");
                Collection<Genre> dbTokens = new Collection<Genre> { new Genre { Key = "DBEnvironment", Value = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString() }, new Genre { Key = "LangCode", Value = "en-US" } };

                Service.Presentation.SecurityRequestModel.ShuffleSavedSecurityQuestionsRequest request = new Service.Presentation.SecurityRequestModel.ShuffleSavedSecurityQuestionsRequest
                {
                    CustomerId = customerID,
                    MileagePlusId = MileagePlusID,
                    Tokens = dbTokens

                };
                string jsonRequest = DataContextJsonSerializer.Serialize<Service.Presentation.SecurityRequestModel.ShuffleSavedSecurityQuestionsRequest>(request);

                _logger.LogInformation("MPSignInNeedHelp {@clientResponse}", JsonConvert.SerializeObject(jsonRequest));

                var response = await _mPSecurityQuestionsService.ValidateSecurityAnswer(token, jsonRequest, _headers.ContextValues.SessionId, path).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(response))
                {
                    var jsonResponse = JsonConvert.DeserializeObject<ShuffleSavedSecurityQuestionsResponse>(response);
                    _logger.LogInformation("ShuffleSavedSecurityQuestions - Response {@clientResponse}", JsonConvert.SerializeObject(response));

                    if (!string.IsNullOrEmpty(jsonResponse.ExceptionMessage))
                    {
                        _logger.LogError("ShuffleSavedSecurityQuestions - exception {exception}", JsonConvert.SerializeObject(jsonResponse.ExceptionMessage));
                    }
                    StatusFlag = jsonResponse.Success;
                }
            }
            else
            {
                string url = _configuration.GetValue<string>("CssSecureProfileURL").ToString();
                List<Metadata> dbTokens = new List<Metadata> { new Metadata("DBEnvironment", _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString()), new Metadata("LangCode", "en-US") };

                var request = new Css.SecureProfile.Types.ShuffleSavedSecurityQuestionsRequest();

                request.CustomerId = customerID;
                request.MileagePlusId = MileagePlusID;
                request.Tokens = dbTokens;

                string jsonRequest = JsonConvert.SerializeObject(request);
                SecureProfileClient proxy = new SecureProfileClient(url, token);
                ShuffleSavedSecurityQuestionsCallWrapper wrapper = proxy.ShuffleSavedSecurityQuestions(customerID, MileagePlusID, dbTokens);

                //string jsonResponse = HttpHelper.Post(url, "application/json; charset=utf-8", token, jsonRequest);

                if (!string.IsNullOrEmpty(wrapper.ExceptionMessage))
                {
                    _logger.LogError("ShuffleSavedSecurityQuestions - Response from ShuffleSavedSecurityQuestions {Trace}", wrapper.ExceptionMessage);
                }
                StatusFlag = wrapper.Success;
            }

            return StatusFlag;
        }
    }
}
