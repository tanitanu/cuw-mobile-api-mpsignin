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
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.Internal.Exception;
using United.Service.Presentation.SecurityRequestModel;
using United.Service.Presentation.SecurityResponseModel;
using United.Services.Customer.Common;
using United.Utility.Helper;
using Genre = United.Service.Presentation.CommonModel.Genre;

namespace United.Common.Helper.Profile
{
    public class MileagePlusTFACSL : IMileagePlusTFACSL
    {
        private readonly ICacheLog<MileagePlusTFACSL> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICustomerDataService _customerDataService;
        private readonly IMPSecurityQuestionsService _mPSecurityQuestionsService;
        private readonly IMPSecurityCheckDetailsService _mPSecurityCheckDetailsService;
        private readonly IHeaders _headers;

        public MileagePlusTFACSL(ICacheLog<MileagePlusTFACSL> logger
            , IConfiguration configuration
            , IMPSecurityQuestionsService mPSecurityQuestionsService
            , ICustomerDataService customerDataService
            , IMPSecurityCheckDetailsService mPSecurityCheckDetailsService
            , IHeaders headers)
        {
            _logger = logger;
            _configuration = configuration;
            _mPSecurityQuestionsService = mPSecurityQuestionsService;
            _customerDataService = customerDataService;
            _mPSecurityCheckDetailsService = mPSecurityCheckDetailsService;
            _headers = headers;

        }

        public async Task<bool> GetTfaWrongAnswersFlag(string sessionid, string token, int customerId, string mileagePlusNumber, bool answeredQuestionsIncorrectly, string languageCode)
        {

            if (string.IsNullOrEmpty(mileagePlusNumber))
            {
                throw new MOBUnitedException("MPNumber request cannot be null.");
            }

            string path = string.Format("/GetTfaWrongAnswersFlag");

            United.Services.Customer.Common.UpdateWrongAnswersFlagRequest updatewronganswersflagrequest = new Services.Customer.Common.UpdateWrongAnswersFlagRequest
            {
                AnsweredQuestionsIncorrectly = answeredQuestionsIncorrectly,
                CustomerId = customerId,
                LoyaltyId = mileagePlusNumber,
                LangCode = languageCode,
                DataSetting = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices")
            };

            string jsonRequest = JsonConvert.SerializeObject(updatewronganswersflagrequest);

            var response = await _customerDataService.InsertMPEnrollment<SearchResponse>(token, jsonRequest, _headers.ContextValues.SessionId, path).ConfigureAwait(false);
            bool retValue = false;

            if (response != null)
            {
                retValue = response.Result;

                if (response != null && (response.Errors == null || response.Errors.Count() == 0))
                {
                    _logger.LogInformation("GetTfaWrongAnswersFlag {Response}", response);
                }
                else
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

                    string exceptionmessage = !string.IsNullOrEmpty(errorMessage) ? errorMessage : "Unable to Get TFA Wrong Answers Flag.";

                    _logger.LogError("GetTfaWrongAnswersFlag {@Exception}", GeneralHelper.RemoveCarriageReturn(exceptionmessage));
                    throw new MOBUnitedException(exceptionmessage);
                }
            }

            return retValue; // response;
        }

        private string GetDPRequestObject(int applicationID, string deviceId, string configSectionKey = "dpTokenRequest")
        {
            switch (applicationID)
            {
                case 1:
                    return _configuration.GetSection(configSectionKey).GetSection("ios").GetValue<string>("clientId");

                case 2:
                    return _configuration.GetSection(configSectionKey).GetSection("android").GetValue<string>("clientId");
                    
                default:
                    return default;
            }
        }

        public async Task<bool> ValidateDevice(Session session, string appVersion, string languageCode)
        {
            bool StatusFlag = false;

            if (_configuration.GetValue<bool>("EnableDPToken"))
            {

                if (string.IsNullOrEmpty(session.MileagPlusNumber) || string.IsNullOrEmpty(session.DeviceID))
                {
                    _logger.LogInformation("ValidateDevice UnitedException {errormessage}", "MPNumber or DeviceID in sesion is null.");
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
                }

                Collection<Genre> dbTokens = new Collection<Genre> { new Genre { Key = "DBEnvironment", Value = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString() }, new Genre { Key = "LangCode", Value = languageCode } };
                Service.Presentation.SecurityRequestModel.ValidateDeviceRequest _validateDeviceRequest = new Service.Presentation.SecurityRequestModel.ValidateDeviceRequest
                {
                    ApplicationId = GetDPRequestObject(session.AppID, session.DeviceID),
                    CustomerId = session.CustomerID,
                    DeviceId = session.DeviceID,
                    MileagePlusId = session.MileagPlusNumber,
                    Tokens = dbTokens
                };

                string jsonRequest = JsonConvert.SerializeObject(_validateDeviceRequest);
                string path = string.Format("/ValidateDevice");

                string jsonResponse = await _mPSecurityQuestionsService.ValidateSecurityAnswer(session.Token, jsonRequest, session.SessionId, path).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    var response = JsonConvert.DeserializeObject<ValidateDeviceResponse>(jsonResponse);
                    if (response != null && string.IsNullOrEmpty(response.ExceptionCode))
                    {
                        _logger.LogInformation("ValidateDevice Response {@cslResponse}", jsonResponse);
                    }
                    else
                    {
                        string exceptionmessage = string.IsNullOrEmpty(response.ExceptionCode) ? response.ExceptionCode : "Unable to Validate Device.";
                        _logger.LogError("ValidateDevice Exception {@cslResponse}", jsonResponse);
                        throw new MOBUnitedException(exceptionmessage);
                    }

                    StatusFlag = response.IsAuthenticated;
                }

            }
            else
            {
                if (string.IsNullOrEmpty(session.MileagPlusNumber) || string.IsNullOrEmpty(session.DeviceID))
                {
                    _logger.LogError("ValidateDevice MOBUnitedException {exception}", "MPNumber or DeviceID in sesion is null.");
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
                }

                System.Guid appID = GetApplicationID(session.AppID, session.SessionId, "ValidateDevice");

                string url = string.Format("{0}", _configuration.GetValue<string>("CssSecureProfileURL"));

                List<Metadata> dbTokens = new List<Metadata> { new Metadata("DBEnvironment", _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString()), new Metadata("LangCode", languageCode) };
                SecureProfileClient proxy = new SecureProfileClient(url, session.Token);
                ValidateDeviceCallWrapper response = proxy.ValidateDevice(session.CustomerID, session.MileagPlusNumber, appID.ToString(), session.DeviceID, dbTokens);

                if (response != null && string.IsNullOrEmpty(response.ExceptionCode))
                {
                    _logger.LogInformation("ValidateDevice DeSerialized Response {response} ", GeneralHelper.RemoveCarriageReturn(response.ToString()));
                }
                else
                {
                    string exceptionmessage = string.IsNullOrEmpty(response.ExceptionCode) ? response.ExceptionCode : "Unable to Validate Device.";
                    _logger.LogWarning("ValidateDevice Exception {exception} ", exceptionmessage);
                    throw new MOBUnitedException(exceptionmessage);
                }

                StatusFlag = response.IsAuthenticated;
            }
            return StatusFlag;
        }
        public async Task<bool> AddDeviceAuthentication(Session session, string appVersion, string languageCode)
        {
            bool StatusFlag = false;

            if (_configuration.GetValue<bool>("EnableDPToken"))
            {

                if (string.IsNullOrEmpty(session.MileagPlusNumber) || string.IsNullOrEmpty(session.DeviceID))
                {
                    _logger.LogError("AddDeviceAuthentication {MOBUnitedException}", "MPNumber or DeviceID in sesion is null.");

                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                string originatingInfo = "::1", authenticationMethod = "sign-in", insertId = "";

                Collection<Genre> dbTokens = new Collection<Genre> { new Genre { Key = "DBEnvironment", Value = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices") }, new Genre { Key = "LangCode", Value = languageCode } };
                insertId = session.CustomerID.ToString();
                if (_configuration.GetValue<string>("PassDeviceIDForOriginatingInfo") != null
                    &&
                    Convert.ToBoolean(_configuration.GetValue<string>("PassDeviceIDForOriginatingInfo")))
                {
                    originatingInfo = session.DeviceID;
                }

                DeviceAuthenticationRequest _DeviceAuthRequest = new DeviceAuthenticationRequest
                {
                    ApplicationId = GetDPRequestObject(session.AppID, session.DeviceID),
                    AuthenticationMethod = authenticationMethod,
                    CustomerId = session.CustomerID,
                    DeviceId = session.DeviceID,
                    InsertId = insertId,
                    MileagePlusId = session.MileagPlusNumber,
                    OriginatingInfo = originatingInfo,
                    Tokens = dbTokens
                };
                string jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(_DeviceAuthRequest);
                string path = "/AddDeviceAuthentication";

                string jsonResponse = await _mPSecurityQuestionsService.AddDeviceAuthentication(session.Token, jsonRequest, session.SessionId, path).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    var response = JsonConvert.DeserializeObject<DeviceAuthenticationResponse>(jsonResponse);

                    if (response != null && string.IsNullOrEmpty(response.ExceptionCode))
                    {
                        _logger.LogInformation("AddDeviceAuthentication {DeSerializedResponse}", response);
                    }
                    else
                    {
                        string exceptionmessage = string.IsNullOrEmpty(response.ExceptionCode) ? response.ExceptionCode : "Unable to Add Device.";
                        _logger.LogError("AddDeviceAuthentication {Exception}", jsonResponse);
                        throw new MOBUnitedException(exceptionmessage);
                    }
                    StatusFlag = response.Success;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(session.MileagPlusNumber) || string.IsNullOrEmpty(session.DeviceID))
                {
                    _logger.LogError("AddDeviceAuthentication {MOBUnitedException}", "MPNumber or DeviceID in sesion is null.");
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }

                System.Guid cssApplicatinID = GetApplicationID(session.AppID, session.SessionId, "AddDeviceAuthentication");

                string url = string.Format("{0}", _configuration.GetValue<string>("CssSecureProfileURL"));
                string originatingInfo = "::1", authenticationMethod = "sign-in", insertId = "";

                List<Metadata> dbTokens = new List<Metadata> { new Metadata("DBEnvironment", _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices")), new Metadata("LangCode", languageCode) };
                SecureProfileClient proxy = new SecureProfileClient(url, session.Token);
                if (_configuration.GetValue<string>("PassDeviceIDForOriginatingInfo") != null
                    &&
                    Convert.ToBoolean(_configuration.GetValue<string>("PassDeviceIDForOriginatingInfo")))
                {
                    originatingInfo = session.DeviceID;
                }
                insertId = session.CustomerID.ToString();
                AddDeviceAuthenticationCallWrapper response = proxy.AddDeviceAuthentication(session.CustomerID, session.MileagPlusNumber, cssApplicatinID.ToString(), session.DeviceID, authenticationMethod, originatingInfo, insertId, dbTokens);

                //****Get Call Duration Code - Venkat 03/17/2015*******
                if (response != null && string.IsNullOrEmpty(response.ExceptionCode))
                {
                    _logger.LogInformation("AddDeviceAuthentication {DeSerializedResponse}", GeneralHelper.RemoveCarriageReturn(response.ToString()));
                }
                else
                {
                    string exceptionmessage = string.IsNullOrEmpty(response.ExceptionCode) ? response.ExceptionCode : "Unable to Add Device.";
                    _logger.LogError("AddDeviceAuthentication {Exception}", exceptionmessage);
                    throw new MOBUnitedException(exceptionmessage);
                }

                StatusFlag = response.Success;
            }
            return StatusFlag;
        }

        public async Task<List<Securityquestion>> GetMPPINPWDSecurityQuestions(string token, string sessionId, int appId, string appVersion, string deviceId)
        {
            List<Securityquestion> securityQuestions = new List<Securityquestion>();

            if (_configuration.GetValue<bool>("EnableDPToken"))
            {
                #region

                Collection<Genre> dbTokens = new Collection<Genre> { new Genre { Key = "DBEnvironment", Value = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices") }, new Genre { Key = "LangCode", Value = "en-US" } };

                Service.Presentation.SecurityRequestModel.SecurityQuestionsRequest _SecurityQuestionsRequest = new Service.Presentation.SecurityRequestModel.SecurityQuestionsRequest
                {
                    Tokens = dbTokens,
                };
                string jsonRequest = DataContextJsonSerializer.Serialize<Service.Presentation.SecurityRequestModel.SecurityQuestionsRequest>(_SecurityQuestionsRequest);

                string jsonResponse = await _mPSecurityQuestionsService.GetMPPINPWDSecurityQuestions(token, jsonRequest, sessionId).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    SecurityQuestionsResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<SecurityQuestionsResponse>(jsonResponse);

                    if (response != null && string.IsNullOrEmpty(response.ExceptionCode))
                    {
                        _logger.LogInformation("GetMPPINPWDSecurityQuestions - Response for GetAllSecurityQuestions {Response} {transactionId}", jsonResponse, _headers.ContextValues.TransactionId);
                    }
                    else
                    {
                        string exceptionmessage = string.IsNullOrEmpty(response.ExceptionCode) ? response.ExceptionCode : "Unable to Validate Device.";
                        _logger.LogError("GetMPPINPWDSecurityQuestions - Response for GetAllSecurityQuestions {exception} {transactionId}", Newtonsoft.Json.JsonConvert.SerializeObject(exceptionmessage), _headers.ContextValues.TransactionId);
                        throw new MOBUnitedException(exceptionmessage);
                    }
                    securityQuestions = ConvertToCSSSecurityList(response.SecurityQuestions);
                }
                #endregion
            }
            else
            {
                #region
                string url = _configuration.GetValue<string>("CslSecureProfileURL");

                _logger.LogInformation("GetMPPINPWDSecurityQuestions - URL for get Security Questions {url} {transactionId}", url, _headers.ContextValues.TransactionId);
                List<Metadata> dbTokens = new List<Metadata> { new Metadata("DBEnvironment", _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices")), new Metadata("LangCode", "en-US") };

                _logger.LogInformation("GetMPPINPWDSecurityQuestions - Request for get Security Questions {dbTokens} {transactionId}", dbTokens, _headers.ContextValues.TransactionId);

                SecureProfileClient proxy = new SecureProfileClient(url, token);
                GetAllSecurityQuestionsCallWrapper wrapper = proxy.GetAllSecurityQuestions(dbTokens);

                if (string.IsNullOrEmpty(wrapper.ExceptionMessage))
                {
                    _logger.LogInformation("GetMPPINPWDSecurityQuestions - Client Response for get Security Questions {securityQuestions} {transactionId}", securityQuestions, _headers.ContextValues.TransactionId);

                    securityQuestions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Securityquestion>>(Newtonsoft.Json.JsonConvert.SerializeObject(wrapper.SecurityQuestions));

                }
                else
                {
                    throw new MOBUnitedException(wrapper.ExceptionMessage);
                }
                #endregion
            }
            return securityQuestions;

        }
        private static List<Securityquestion> ConvertToCSSSecurityList(Collection<Service.Presentation.SecurityModel.Question> _CSLQuestionsCollection)
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
        private System.Guid GetApplicationID(int appId, string sessionid, string actionName)
        {
            #region Get Aplication Id

            System.Guid appID = new Guid("643e1e47-1242-4b6c-ab7e-64024e4bc84c"); // default App Id
            try
            {
                string[] cSSAuthenticationTokenServiceApplicationIDs = _configuration.GetValue<string>("CSSAuthenticationTokenServiceApplicationIDs").Split('|');
                foreach (string applicationID in cSSAuthenticationTokenServiceApplicationIDs)
                {
                    if (Convert.ToInt32(applicationID.Split('~')[0].ToString().ToUpper().Trim()) == appId)
                    {
                        appID = new Guid(applicationID.Split('~')[1].ToString().Trim());
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);
            }
            #endregion

            return appID;
        }
        public async Task<SaveResponse> SendForgotPasswordEmail(string sessionid, string token, string mileagePlusNumber, string emailAddress, string languageCode)
        {
            if (string.IsNullOrEmpty(mileagePlusNumber))
            {
                throw new MOBUnitedException("MPNumber request cannot be null.");
            }
            string path = string.Format("/SendForgotPasswordEmail");
            var response = new SaveResponse();
            United.Services.Customer.Common.SendForgotPasswordEmailRequest sendforgotpasswordemailrequest = new SendForgotPasswordEmailRequest
            {
                EmailAddress = emailAddress,
                MileagePlusId = mileagePlusNumber,
                LangCode = languageCode,
                DataSetting = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString()
            };

            string jsonRequest = DataContextJsonSerializer.Serialize<SendForgotPasswordEmailRequest>(sendforgotpasswordemailrequest);

            if (_configuration.GetValue<bool>("EnableUCBVersion_UrlChanges"))
                response = await _customerDataService.InsertMPEnrollment<SaveResponse>(token, jsonRequest, _headers.ContextValues.SessionId, path).ConfigureAwait(false);
            else
                response = await _mPSecurityCheckDetailsService.InsertMPEnrollment<SaveResponse>(token, jsonRequest, _headers.ContextValues.SessionId, path).ConfigureAwait(false);

            if (response != null)
            {
                if (response != null && (response.Errors == null || response.Errors.Count() == 0))
                {
                    _logger.LogInformation("SendForgotPasswordEmail {@Response}", GeneralHelper.RemoveCarriageReturn(JsonConvert.SerializeObject(response)));
                }
                else
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

                    string exceptionmessage = !string.IsNullOrEmpty(errorMessage) ? errorMessage : "Unable to Send Reset Account Email.";
                    exceptionmessage = GeneralHelper.RemoveCarriageReturn(exceptionmessage);
                    _logger.LogError("SendForgotPasswordEmail Exception {@exception} ", exceptionmessage);
                    throw new MOBUnitedException(exceptionmessage);
                }

            }

            return response;
        }
        public async Task<SaveResponse> SendResetAccountEmail(string sessionid, string token, int customerId, string mileagePlusNumber, string emailAddress, string languageCode)
        {
            if (string.IsNullOrEmpty(mileagePlusNumber))
            {
                throw new MOBUnitedException("MPNumber request cannot be null.");
            }
            string path = "/SendResetAccountEmail";
            var response = new SaveResponse();
            United.Services.Customer.Common.SendResetAccountEmailRequest SendResetAccountEmailRequest = new Services.Customer.Common.SendResetAccountEmailRequest
            {
                EmailAddress = emailAddress,
                MileagePlusId = mileagePlusNumber,
                LangCode = languageCode,
                DataSetting = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString()
            };

            string jsonRequest = DataContextJsonSerializer.Serialize<SendResetAccountEmailRequest>(SendResetAccountEmailRequest);

            if (_configuration.GetValue<bool>("EnableUCBVersion_UrlChanges"))
                response = await _customerDataService.InsertMPEnrollment<SaveResponse>(token, jsonRequest, _headers.ContextValues.SessionId, path).ConfigureAwait(false);
            else
                response = await _mPSecurityCheckDetailsService.InsertMPEnrollment<SaveResponse>(token, jsonRequest, _headers.ContextValues.SessionId, path).ConfigureAwait(false);

            if (response != null)
            {
                if (response != null && (response.Errors == null || response.Errors.Count() == 0))
                {
                    _logger.LogInformation("SendResetAccountEmail {@Response} {@mileagePlusNumber}", response, mileagePlusNumber);
                }
                else
                {
                    string errorMessage = string.Empty;
                    foreach (var error in response?.Errors)
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
                    string exceptionmessage = !string.IsNullOrEmpty(errorMessage) ? errorMessage : "Unable to Send Reset Account Email.";
                    _logger.LogError("SendResetAccountEmails error {@Exception} {@mileagePlusNumber} ", GeneralHelper.RemoveCarriageReturn(exceptionmessage), mileagePlusNumber);
                    throw new MOBUnitedException(exceptionmessage);
                }
            }

            return response;
        }
        public bool SignOutSession(string sessionid, string token, int appId)
        {

            bool signOutSuccess = false;
            if (_configuration.GetValue<bool>("EnableDPToken"))
            {
                //signOutSuccess = DataPower.DPAccessTokenFactory.RevokeDPToken(token, appId, logEntries, traceSwitch); ////**==>> WHY DO WE NEED TO SIGN OUT IF THE MP SIGN IN ON THAT DEVICE FIRST TIME AS ANY WAY WE ASK SECURITY QUESTIONS TO ALLOW CUSTOMER TO CONTINUE ( AS CUSTOMER MAY NOT MAY NOT SELECT REMEMBER ME) AS THIS dp REVOKE TOKEN CREATING LOTS OF ISSUES TO FUTHURE CONTINUE WITHSAME TOKEN WHICH WE SIGNED THE CUSTOMER. 
            }
            else
            {
                System.Guid appID = GetApplicationID(appId, sessionid, "SignOutSession");
                string url = string.Format("{0}", _configuration.GetValue<string>("CSSAuthenticationTokenGeneratorURL"));

                var client = new Css.ChannelProxy.Client(url);
                Css.Types.SignOutSessionCallWrapper response = client.SignOutSession(appID, new Guid(token));

                if (response.SignOutSessionOperationResult == Css.Types.SignOutSessionResult.Success &&
                    response.CallAuthenticationOperationResult == Css.Types.CallAuthenticationResult.Success &&
                    response.CallAuthorizationOperationResult == Css.Types.CallAuthorizationResult.Success &&
                    response.UseTokenValidationResult == Css.Types.UseTokenValidationResult.Valid)
                {
                    signOutSuccess = true; //**==>> Do this return true and default return false
                }

                if (response != null && string.IsNullOrEmpty(response.ExceptionMessage))
                {
                    _logger.LogInformation("SignOutSession {DeSerializedResponse}", response);
                }
                else
                {
                    string exceptionmessage = string.IsNullOrEmpty(response.ExceptionMessage) ? response.ExceptionMessage : "Unable to Signout Session.";
                    _logger.LogError("SignOutSession {Exception}", exceptionmessage);
                    throw new MOBUnitedException(exceptionmessage); //**==>> do not throw exception this is just to sign out if any exception no need to throw exception 
                }
            }

            return signOutSuccess;
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
                string jsonResponse = await _mPSecurityQuestionsService.ValidateSecurityAnswer(token, jsonRequest, _headers.ContextValues.SessionId, path).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    var response = JsonConvert.DeserializeObject<ShuffleSavedSecurityQuestionsResponse>(jsonResponse);
                    if (!string.IsNullOrEmpty(response.ExceptionMessage))
                    {
                        _logger.LogError("ShuffleSavedSecurityQuestions - Response from ShuffleSavedSecurityQuestions {Trace}", response.ExceptionMessage);
                    }
                    StatusFlag = response.Success;
                }
            }
            else
            {

                string url = _configuration.GetValue<string>("CssSecureProfileURL").ToString();

                List<Metadata> dbTokens = new List<Metadata> { new Metadata("DBEnvironment", _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices").ToString()), new Metadata("LangCode", "en-US") };

                Css.SecureProfile.Types.ShuffleSavedSecurityQuestionsRequest request = new Css.SecureProfile.Types.ShuffleSavedSecurityQuestionsRequest
                {
                    CustomerId = customerID,
                    MileagePlusId = MileagePlusID,
                    Tokens = dbTokens
                };

                string jsonRequest = DataContextJsonSerializer.Serialize<Css.SecureProfile.Types.ShuffleSavedSecurityQuestionsRequest>(request);

                SecureProfileClient proxy = new SecureProfileClient(url, token);

                ShuffleSavedSecurityQuestionsCallWrapper wrapper = proxy.ShuffleSavedSecurityQuestions(customerID, MileagePlusID, dbTokens);

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
