using Css.SecureProfile;
using Css.SecureProfile.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using United.Definition;
using United.Mobile.DataAccess.CSLSerivce;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Exception;
using United.Service.Presentation.CommonModel;
using United.Service.Presentation.SecurityResponseModel;
using United.Services.ProfileValidation.Common;
using United.Utility.Helper;

namespace United.Common.Helper.Profile
{
    public class SecurityQuestion : ISecurityQuestion
    {
        private readonly ICacheLog<SecurityQuestion> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMPSecurityQuestionsService _mPSecurityQuestionsService;
        private readonly IUtilitiesService _utilitiesService;
        private readonly IDynamoDBUtility _dynamoDBUtility;
        private readonly IHeaders _headers;

        public SecurityQuestion(ICacheLog<SecurityQuestion> logger
            , IConfiguration configuration
            , IMPSecurityQuestionsService mPSecurityQuestionsService
            , IUtilitiesService utilitiesService
            , IDynamoDBUtility dynamoDBUtility
            , IHeaders headers
        )
        {
            _logger = logger;
            _configuration = configuration;
            _mPSecurityQuestionsService = mPSecurityQuestionsService;
            _utilitiesService = utilitiesService;
            _dynamoDBUtility = dynamoDBUtility;
            _headers = headers;
        }

      

        private List<Securityquestion> ConvertToCSSSecurityList(Collection<Service.Presentation.SecurityModel.Question> _CSLQuestionsCollection)
        {
            List<Securityquestion> _CSSQuestionsList = new List<Securityquestion>();
            List<Mobile.Model.Common.Answer> _CSSAnsList = new List<Mobile.Model.Common.Answer>();
            //int QuestionID = 1;
            //int AnsID = 1;
            foreach (var _CSLQst in _CSLQuestionsCollection)
            {
                if (_CSLQst != null && _CSLQst.Answers != null && _CSLQst.Answers.Count > 0)
                {
                    _CSSAnsList = new List<Mobile.Model.Common.Answer>();
                    //AnsID = 1;
                    foreach (var _CSLAns in _CSLQst.Answers)
                    {
                        Mobile.Model.Common.Answer _CSSAns = new Mobile.Model.Common.Answer
                        {
                            //AnswerId = AnsID,
                            AnswerKey = _CSLAns.AnswerKey,
                            AnswerText = _CSLAns.AnswerText,
                            QuestionKey = _CSLAns.QuestionKey,
                            //QuestionId = QuestionID
                        };
                        //++AnsID;
                        _CSSAnsList.Add(_CSSAns);
                    }
                    Securityquestion _CSSQtn = new Securityquestion
                    {
                        //QuestionId = QuestionID,
                        QuestionKey = _CSLQst.QuestionKey,
                        QuestionText = _CSLQst.QuestionText,
                        Used = _CSLQst.IsUsed,
                        Answers = _CSSAnsList
                    };
                    _CSSQuestionsList.Add(_CSSQtn);
                    //++QuestionID;
                }
            }

            return _CSSQuestionsList;
        }

        public async Task<List<Securityquestion>> GetMPPinPwdSavedSecurityQuestions(string token, int customerId, string mileagePlusNumber, string sessionId, int appId, string appVersion, string deviceId)
        {
            List<Securityquestion> securityQuestions = null;

            if (_configuration.GetValue<bool>("EnableDPToken"))
            {
                string path = string.Format("/GetSavedSecurityQuestions");

                Collection<Genre> dbTokens = new Collection<Genre> { new Genre { Key = "DBEnvironment", Value = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString() }, new Genre { Key = "LangCode", Value = "en-US" } };

                Service.Presentation.SecurityRequestModel.SavedSecurityQuestionsRequest _SavedSecurityQuestionsRequest = new Service.Presentation.SecurityRequestModel.SavedSecurityQuestionsRequest
                {
                    CustomerId = customerId,
                    MileagePlusId = mileagePlusNumber,
                    Tokens = dbTokens
                };

                string jsonRequest = JsonConvert.SerializeObject(_SavedSecurityQuestionsRequest);
                var response = await _mPSecurityQuestionsService.ValidateSecurityAnswer(token, jsonRequest, sessionId, path).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(response))
                {
                    var jsonResponse = JsonConvert.DeserializeObject<SavedSecurityQuestionsResponse>(response);
                    if (jsonResponse?.Questions != null && string.IsNullOrEmpty(jsonResponse.ExceptionCode))
                    {
                        _logger.LogInformation("GetMPPinPwdSavedSecurityQuestions {@cslResponse}", response);
                    }
                    else
                    {
                        string exceptionmessage = string.IsNullOrEmpty(jsonResponse.ExceptionCode) ? jsonResponse.ExceptionCode : "The account information you entered is incorrect.";
                        exceptionmessage = _configuration.GetValue<string>("ValidateMPSignInGetSavedSecurityQuestionsErrorMessage") != null && _configuration.GetValue<string>("ValidateMPSignInGetSavedSecurityQuestionsErrorMessage") != "" ? _configuration.GetValue<string>("ValidateMPSignInGetSavedSecurityQuestionsErrorMessage").ToString() : "The account information you entered is incorrect.";
                        _logger.LogError("GetMPPinPwdSavedSecurityQuestions error {@cslResponse}", response);
                        throw new MOBUnitedException(exceptionmessage);
                    }
                    securityQuestions = ConvertToCSSSecurityList(jsonResponse.Questions);
                }
            }
            else
            {
                string url = _configuration.GetValue<string>("CssSecureProfileURL").ToString();

                List<Metadata> dbTokens = new List<Metadata> { new Metadata("DBEnvironment", _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString()), new Metadata("LangCode", "en-US") };

                SecureProfileClient proxy = new SecureProfileClient(url, token);
                GetSavedSecurityQuestionsCallWrapper savedQuestions = proxy.GetSavedSecurityQuestions(customerId, mileagePlusNumber.ToUpper(), dbTokens);

                if (string.IsNullOrEmpty(savedQuestions.ExceptionMessage))
                {
                    securityQuestions = DataContextJsonSerializer.NewtonSoftDeserialize<List<Securityquestion>>(DataContextJsonSerializer.NewtonSoftSerializeToJson<List<Question>>(savedQuestions.Questions));

                    _logger.LogInformation("GetMPPinPwdSavedSecurityQuestions - Client Response for get SavedSecurityQuestions {Trace}", savedQuestions.Questions);
                }
                else
                {
                    throw new MOBUnitedException(savedQuestions.ExceptionMessage);
                }
            }
            return securityQuestions;
        }
        public async Task<bool> ValidateSecurityAnswer(string questionKey, string answerKey, string mileagePlusNumber, string token, string langCode, string sessionId, int appId, string appVersion, string deviceId)
        {
            bool isAnswerValidated = false;

            string path = string.Format("/ValidateSecurityAnswer");

            Collection<Service.Presentation.CommonModel.Genre> dbTokens = new Collection<Service.Presentation.CommonModel.Genre> { new Service.Presentation.CommonModel.Genre { Key = "DBEnvironment", Value = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString() }, new Service.Presentation.CommonModel.Genre { Key = "LangCode", Value = "en-US" } };

            Service.Presentation.SecurityRequestModel.ValidateSecurityAnswerRequest _ValidateSecurityAnswerRequest = new Service.Presentation.SecurityRequestModel.ValidateSecurityAnswerRequest
            {
                AnswerKey = answerKey,
                MileagePlusId = mileagePlusNumber,
                QuestionKey = questionKey,
                Tokens = dbTokens,
                //tobechanged csstodp
                IsFirstAnswer = false
            };

            string jsonRequest = DataContextJsonSerializer.Serialize<Service.Presentation.SecurityRequestModel.ValidateSecurityAnswerRequest>(_ValidateSecurityAnswerRequest);

            string jsonResponse = await _mPSecurityQuestionsService.ValidateSecurityAnswer(token, jsonRequest, sessionId, path).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var response = JsonConvert.DeserializeObject<ValidateSecurityAnswerResponse>(jsonResponse);
                if (response != null && string.IsNullOrEmpty(response.ExceptionCode))
                {
                    _logger.LogInformation("ValidateSecurityAnswer {@cslResponse} {@mileagePlusNumber}", jsonResponse, mileagePlusNumber);
                }
                else
                {
                    string exceptionmessage = string.IsNullOrEmpty(response.ExceptionCode) ? response.ExceptionCode : "Unable to Validate Device.";
                    _logger.LogError("ValidateSecurityAnswer exception {@cslResponse} {@mileagePlusNumber}", jsonResponse, mileagePlusNumber);
                    throw new MOBUnitedException(exceptionmessage);
                }
                isAnswerValidated = response.IsCorrectAnswer;
            }

            return isAnswerValidated;
        }

        public async Task<bool> MPPinPwdValidatePassowrd(string token, string langCode, string password, string mpdId, string username, string email, string sessionId, int appId, string appVersion, string deviceId)
        {
            bool isPasswordValidated = false;

            United.Services.ProfileValidation.Common.ValidatePasswordRequest ProfileValidationReq = new ValidatePasswordRequest
            {
                DataSetting = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString(),
                Email = email,
                LangCode = langCode,
                MileagePlusId = mpdId,
                Password = password,
                UserName = username
            };

            string path = string.Format("ValidatePassword");

            string jsonRequest = DataContextJsonSerializer.Serialize<ValidatePasswordRequest>(ProfileValidationReq);
            var response = await _utilitiesService.ValidateMileagePlusNames<ValidatePasswordResponse>(token, jsonRequest, sessionId, path).ConfigureAwait(false);

            if (response != null)
            {
                if (response != null && response.Status.Equals(United.Services.ProfileValidation.Common.Constants.StatusType.Success) && response.ValidationResult == true)
                {
                    isPasswordValidated = true;
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
                        //string exceptionMessage = _configuration.GetValue<string>("UnableToValidatePasswordErrorMessage");
                        //if (ConfigurationManager.AppSettings["ReturnActualExceptionMessageBackForTesting"] != null && Convert.ToBoolean(ConfigurationManager.AppSettings["ReturnActualExceptionMessageBackForTesting"].ToString()))
                        //{
                        //    exceptionMessage = exceptionMessage + " response.Status not success and response.Errors.Count == 0 - at DAL MPPinPwdValidatePassowrd";
                        //}
                        //throw new MOBUnitedException(exceptionMessage);
                    }
                }
            }

            return isPasswordValidated;
        }

        public async Task<bool> MPPinPwdUpdateCustomerPassword(string token, string oldPassword, string newPassword, int customerId, string mileagePlusNumber, string langCode, string sessionId, int appId, string appVersion, string deviceId)
        {
            bool isPasswordUpdated = false;
            string path = "/UpdateCustomerPassword";

            Collection<Service.Presentation.CommonModel.Genre> dbTokens = new Collection<Service.Presentation.CommonModel.Genre> { new Service.Presentation.CommonModel.Genre { Key = "DBEnvironment", Value = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString() }, new Service.Presentation.CommonModel.Genre { Key = "LangCode", Value = "en-US" } };

            Service.Presentation.SecurityRequestModel.UpdateCustomerPasswordRequest _UpdateCustomerPasswordRequest = new Service.Presentation.SecurityRequestModel.UpdateCustomerPasswordRequest
            {

                CustomerId = customerId,
                MileagePlusId = mileagePlusNumber,
                Tokens = dbTokens,
                NewPassword = newPassword,
                OldPassword = oldPassword,
                WhoModifiedId = mileagePlusNumber
            };

            string jsonRequest = DataContextJsonSerializer.Serialize<Service.Presentation.SecurityRequestModel.UpdateCustomerPasswordRequest>(_UpdateCustomerPasswordRequest);

            string jsonResponse = await _mPSecurityQuestionsService.ValidateSecurityAnswer(token, jsonRequest, sessionId, path).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var response = JsonConvert.DeserializeObject<UpdateCustomerPasswordResponse>(jsonResponse);
                if (response != null && string.IsNullOrEmpty(response.ExceptionCode))
                {
                    _logger.LogInformation("MPPinPwdUpdateCustomerPassword - Response for MPPinPwdUpdateCustomerPassword {DeSerialized Response}", response);
                }
                else
                {
                    string exceptionmessage = string.IsNullOrEmpty(response.ExceptionCode) ? response.ExceptionCode : "Unable to Update Customer Password.";
                    _logger.LogError("MPPinPwdUpdateCustomerPassword - Response for MPPinPwdUpdateCustomerPassword {Exception}", jsonResponse);
                    throw new MOBUnitedException(exceptionmessage);
                }
                isPasswordUpdated = Convert.ToBoolean(response.Result);
            }
            else
            {

                string url = _configuration.GetValue<string>("CssSecureProfileURL").ToString();

                SecureProfileClient proxy = new SecureProfileClient(url, token);

                var dbToken = new List<Css.SecureProfile.Types.Metadata>();
                dbToken.Add(new Metadata() { Key = "DBEnvironment", Value = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices") });
                dbToken.Add(new Metadata() { Key = "LangCode", Value = langCode });


                UpdateCustomerPasswordCallWrapper wrapper = proxy.UpdateCustomerPassword(mileagePlusNumber, oldPassword, newPassword, mileagePlusNumber, dbToken);

                if (wrapper != null)
                {
                    if (wrapper.ExceptionMessage != null)
                    {
                        //result = wrapper.ExceptionMessage;
                        if (_configuration.GetValue<string>("PasswordUpdatedErrorMessage") == null)
                        {
                            throw new MOBUnitedException(wrapper.ExceptionMessage);
                        }
                        else
                        {
                            throw new MOBUnitedException(_configuration.GetValue<string>("PasswordUpdatedErrorMessage").ToString());
                        }
                    }
                    else
                    {
                        isPasswordUpdated = true;
                    }
                }
            }

            return isPasswordUpdated;
        }

        public async Task<List<MOBItem>> GetMPPINPWDTitleMessagesForMPAuth(List<string> titleList)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool isTermsnConditions = false;

            if (!isTermsnConditions)
            {
                foreach (var title in titleList)
                {
                    stringBuilder.Append("'");
                    stringBuilder.Append(title);
                    stringBuilder.Append("'");
                    stringBuilder.Append(",");
                }
            }
            else
            {
                stringBuilder.Append(titleList[0]);
            }

            string reqTitles = stringBuilder.ToString().Trim(',');
            var docs = new List<MOBLegalDocument>();
            try
            {
                docs = await _dynamoDBUtility.GetNewLegalDocumentsForTitles(reqTitles, _headers.ContextValues.TransactionId, true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("GetMPPINPWDTitleMessagesForMPAuth - OnPremSQLService-GetLegalDocumentsForTitles Error {message} {StackTrace}  ", ex.Message, ex.StackTrace);
            }
            List<MOBItem> messages = new List<MOBItem>();
            if (docs != null && docs.Count > 0)
            {
                foreach (MOBLegalDocument doc in docs)
                {
                    MOBItem item = new MOBItem
                    {
                        Id = doc.Title,
                        CurrentValue = doc.LegalDocument
                    };
                    messages.Add(item);
                }
            }
            return messages;
        }

        public async Task<bool> UnLockCustomerAccount(int customerId, string mileagePlusNumber, string token, string langCode, string sessionId, int appId, string appVersion, string deviceId)
        {
            bool isAccountUnLocked = false;

            string path = string.Format("/UnLockCustomerAccount");

            Collection<Service.Presentation.CommonModel.Genre> dbTokens = new Collection<Service.Presentation.CommonModel.Genre> { new Service.Presentation.CommonModel.Genre { Key = "DBEnvironment", Value = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString() }, new Service.Presentation.CommonModel.Genre { Key = "LangCode", Value = "en-US" } };

            Service.Presentation.SecurityRequestModel.UnLockCustomerAccountRequest _UnLockCustomerAccountRequest = new Service.Presentation.SecurityRequestModel.UnLockCustomerAccountRequest
            {
                CustomerId = customerId,
                MileagePlusId = mileagePlusNumber,
                Tokens = dbTokens,
                UpdateId = customerId.ToString(),
            };

            string jsonRequest = DataContextJsonSerializer.Serialize<Service.Presentation.SecurityRequestModel.UnLockCustomerAccountRequest>(_UnLockCustomerAccountRequest);

            var response = await _mPSecurityQuestionsService.ValidateSecurityAnswer(token, jsonRequest, sessionId, path).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(response))
            {
                var jsonResponse = JsonConvert.DeserializeObject<UnLockCustomerAccountResponse>(response);
                if (jsonResponse != null && string.IsNullOrEmpty(jsonResponse.ExceptionCode))
                {
                    _logger.LogInformation("UnLockCustomerAccount - Response for UnLockCustomerAccount {DeSerialized Response}", jsonResponse);
                }
                else
                {
                    string exceptionmessage = string.IsNullOrEmpty(jsonResponse.ExceptionCode) ? jsonResponse.ExceptionCode : "Unable to Validate Device.";
                    _logger.LogError("UnLockCustomerAccount - Response for UnLockCustomerAccount {Exception}", response);
                    throw new MOBUnitedException(exceptionmessage);
                }
                isAccountUnLocked = jsonResponse.Success;
            }

            return isAccountUnLocked;
        }

        public async Task<bool> LockCustomerAccount(int customerId, string mileagePlusNumber, string token, string langCode, string sessionId, int appId, string appVersion, string deviceId)
        {
            return await LockCustomerAccountWithSendEmailFlag(customerId, mileagePlusNumber, token, langCode, sessionId, appId, appVersion, deviceId, true).ConfigureAwait(false);
        }

        public async Task<bool> LockCustomerAccountWithSendEmailFlag(int customerId, string mileagePlusNumber, string token, string langCode, string sessionId, int appId, string appVersion, string deviceId, bool sendEmail)
        {
            bool isAccountLocked = false;
            if (_configuration.GetValue<bool>("EnableDPToken"))
            {
                string path = "/LockCustomerAccount";
                Collection<Service.Presentation.CommonModel.Genre> dbTokens = new Collection<Service.Presentation.CommonModel.Genre> { new Service.Presentation.CommonModel.Genre { Key = "DBEnvironment", Value = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString() }, new Service.Presentation.CommonModel.Genre { Key = "LangCode", Value = "en-US" } };

                Service.Presentation.SecurityRequestModel.LockCustomerAccountRequest _LockCustomerAccountRequest = new Service.Presentation.SecurityRequestModel.LockCustomerAccountRequest
                {
                    CustomerId = customerId,
                    MileagePlusId = mileagePlusNumber,
                    Tokens = dbTokens,
                    IsSendEmail = sendEmail,
                    UpdateId = customerId.ToString(),
                };

                string jsonRequest = DataContextJsonSerializer.Serialize<Service.Presentation.SecurityRequestModel.LockCustomerAccountRequest>(_LockCustomerAccountRequest);

                _logger.LogInformation("LockCustomerAccountWithSendEmailFlag {@clientRequest} {@mileagePlusNumber}", JsonConvert.SerializeObject(jsonRequest), mileagePlusNumber);

                string jsonResponse = await _mPSecurityQuestionsService.ValidateSecurityAnswer(token, jsonRequest, sessionId, path).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    var response = JsonConvert.DeserializeObject<LockCustomerAccountResponse>(jsonResponse);
                    if (response != null && string.IsNullOrEmpty(response.ExceptionCode))
                    {
                        _logger.LogInformation("LockCustomerAccountWithSendEmailFlag - Response for LockCustomerAccountWithSendEmailFlag {DeSerialized Response} {@mileagePlusNumber}", response, mileagePlusNumber);
                    }
                    else
                    {
                        string exceptionmessage = string.IsNullOrEmpty(response.ExceptionCode) ? response.ExceptionCode : "Unable to Validate Device.";
                        _logger.LogError("LockCustomerAccountWithSendEmailFlag - Response for LockCustomerAccount {Exception} {@mileagePlusNumber}", jsonResponse, mileagePlusNumber);
                        throw new MOBUnitedException(exceptionmessage);
                    }
                    isAccountLocked = response.Success;
                }
            }
            else
            {
                string url = _configuration.GetValue<string>("CssSecureProfileURL").ToString();

                List<Metadata> dbTokens = new List<Metadata> { new Metadata("DBEnvironment", _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString()), new Metadata("LangCode", "en-US") };

                SecureProfileClient proxy = new SecureProfileClient(url, token);

                LockCustomerAccountCallWrapper response = proxy.LockCustomerAccount(customerId, mileagePlusNumber, sendEmail, customerId.ToString(), dbTokens);

                if (string.IsNullOrEmpty(response.ExceptionMessage))
                {
                    isAccountLocked = response.Success;
                }
                else
                {
                    throw new MOBUnitedException(response.ExceptionMessage);
                }
            }
            return isAccountLocked;
        }
    }
}
