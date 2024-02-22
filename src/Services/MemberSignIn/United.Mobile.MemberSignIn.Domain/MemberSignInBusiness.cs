using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.Profile;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.CSLSerivce;
using United.Mobile.Model.Common;
using CSLModels = United.Mobile.Model.CSLModels;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.Internal.Common.CacheModels;
using United.Mobile.Model.Internal.Exception;
using United.Mobile.Model.MPSignIn.Common;
using United.Mobile.Model.OneClickEnrollment;
using United.Service.Presentation.CommonModel;
using United.Service.Presentation.ReservationResponseModel;
using United.Utility.Helper;

namespace United.Mobile.MemberSignIn.Domain
{
    public class MemberSignInBusiness : IMemberSignInBusiness
    {
        private readonly ICacheLog<MemberSignInBusiness> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly ICustomerProfile _customerProfile;
        private readonly IMPTraveler _mPTraveler;
        private readonly ILoyaltyAWSService _LoyaltyAWSService;
        private readonly IMileagePlusTFACSL _mileagePlusTFACSL;
        private readonly IMileagePlusCSL _mileagePlusCSL;
        private readonly IShoppingSessionHelper _shoppingSessionHelper;
        private readonly ISecurityQuestion _securityQuestion;
        private readonly IUtility _utility;
        private readonly ICatalog _catalog;
        private readonly IDynamoDBUtility _dynamoDBUtility;
        private readonly IRavenService _RavenService;
        private readonly IRavenCloudService _ravenCloudService;
        private readonly IContactPointService _contactPointService;


        public MemberSignInBusiness(
            ICacheLog<MemberSignInBusiness> logger
            , IConfiguration configuration
            , ISessionHelperService sessionHelperService
            , ICustomerProfile customerProfile
            , IMPTraveler mPTraveler
            , ILoyaltyAWSService LoyaltyAWSService
            , IMileagePlusTFACSL mileagePlusTFACSL
            , IMileagePlusCSL mileagePlusCSL
            , IShoppingSessionHelper shoppingSessionHelper
            , ISecurityQuestion securityQuestion
            , IUtility utility
            , ICatalog catalog
            , IDynamoDBUtility dynamoDBUtility
            , IRavenService RavenService
            , IContactPointService contactPointService
            , IRavenCloudService ravenCloudService
            )
        {
            _logger = logger;
            _configuration = configuration;
            _sessionHelperService = sessionHelperService;
            _customerProfile = customerProfile;
            _mPTraveler = mPTraveler;
            _LoyaltyAWSService = LoyaltyAWSService;
            _mileagePlusTFACSL = mileagePlusTFACSL;
            _mileagePlusCSL = mileagePlusCSL;
            _shoppingSessionHelper = shoppingSessionHelper;
            _securityQuestion = securityQuestion;
            _utility = utility;
            _catalog = catalog;
            _dynamoDBUtility = dynamoDBUtility;
            _RavenService = RavenService;
            _contactPointService = contactPointService;
            _ravenCloudService = ravenCloudService;
        }
        public async Task<MOBJoinMileagePlusEnrollmentResponse> OneClickEnrollment(MOBJoinMileagePlusEnrollmentRequest request)
        {
            MOBJoinMileagePlusEnrollmentResponse response = new MOBJoinMileagePlusEnrollmentResponse();


            #region Code for Creating Session and Token
            Session session = null;
            if (!string.IsNullOrEmpty(request.SessionId))
            {
                session = await _shoppingSessionHelper.GetShoppingSession(request.SessionId).ConfigureAwait(false);
            }
            else
            {
                session = await _shoppingSessionHelper.CreateShoppingSession(request.Application.Id, request.DeviceId, request.Application.Version.Major, request.TransactionId, string.Empty, string.Empty).ConfigureAwait(false);
            }

            request.TransactionId = session.SessionId;
            request.SessionId = session.SessionId;
            #endregion

            var reservationdetail = new United.Service.Presentation.ReservationResponseModel.ReservationDetail();
            var cslReservation = await _sessionHelperService.GetSession<United.Service.Presentation.ReservationResponseModel.ReservationDetail>(request.SessionId, reservationdetail.GetType().FullName, new List<string> { request.SessionId, reservationdetail.GetType().FullName }).ConfigureAwait(false);

            if (_configuration.GetValue<bool>("EnableUCBchange")
                && await _catalog.IsClientCatalogEnabled(request.Application.Id, _configuration.GetValue<string>("TurnOnUCBOnOneclickEnrollment").Split('|')).ConfigureAwait(false))
            {
                response = await OneClickUcbEnrollment(request, session, cslReservation).ConfigureAwait(false);
            }
            else
            {
                response = await OneClickEnrollment(request, session, cslReservation).ConfigureAwait(false);
            }

            return response;
        }



        public async Task<MOBMPSignInNeedHelpResponse> MPSignInNeedHelp(MOBMPSignInNeedHelpRequest request)
        {
            MOBMPSignInNeedHelpResponse response = new MOBMPSignInNeedHelpResponse
            {
                MileagePlusNumber = request.MileagePlusNumber,
                SessionID = request.SessionID
            };
            try
            {
                Session session = null;
                if (!string.IsNullOrEmpty(request.SessionID))
                {
                    session = await _shoppingSessionHelper.GetShoppingSession(request.SessionID).ConfigureAwait(false);
                }
                else
                {
                    //TFS 53624 - fix to block update password /bypass authentication by sending null session Id - pradeep
                    if (request.SecurityUpdateType == MOBMPSecurityUpdatePath.UpdatePassword)
                    {
                        _logger.LogInformation("MPSignInNeedHelp {SecurityUpdateType} NullSessionIdExceptionForPasswordUpdate {request}", request);
                        _logger.LogInformation("MPSignInNeedHelp - " + request.SecurityUpdateType.ToString(), " NullSessionIdExceptionForPasswordUpdate {request}", request, "Session Id not provided in request for update password operation");

                        throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

                    }
                    else
                    {
                        session = await _shoppingSessionHelper.CreateShoppingSession(request.Application.Id, request.DeviceId, request.Application.Version.Major, request.TransactionId, request.MileagePlusNumber, string.Empty).ConfigureAwait(false);
                    }
                }
                // Pre check to see if there is LOCK on the MP Account already
                if (_configuration.GetValue<bool>("EnableMPSigninPreLockCheck")
                   && (request.SecurityUpdateType == MOBMPSecurityUpdatePath.ForgotMPPassWord || request.SecurityUpdateType == MOBMPSecurityUpdatePath.ValidateSecurityQuestions)
                  && !(!await _mileagePlusTFACSL.GetTfaWrongAnswersFlag(session.SessionId, session.Token, session.CustomerID, request.MileagePlusNumber, false, request.LanguageCode).ConfigureAwait(false)
                  && !await _mileagePlusCSL.GetWrongAnswersFlag(true, session.Token, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId, 0, request.MileagePlusNumber).ConfigureAwait(false)))
                {
                    response.MPSignInNeedHelpDetails = new MOBMPSignInNeedHelpItemsDetails
                    {
                        NeedHelpMessages = await GetNeedHelpTitleMessages(MOBMPSecurityUpdatePath.AccountLocked, false, session.SessionId).ConfigureAwait(false)
                    };
                    response.NeedHelpSecurityPath = MOBMPSecurityUpdatePath.UnableToResetOnline;
                }
                else
                {
                    request.TransactionId = session.SessionId;
                    request.SessionID = session.SessionId;
                    response.SessionID = session.SessionId;
                    response.MPSignInNeedHelpDetails = new MOBMPSignInNeedHelpItemsDetails();
                    if (request != null && !string.IsNullOrEmpty(request.MileagePlusNumber))
                    {
                        request.MileagePlusNumber = request.MileagePlusNumber.Trim().ToUpper();
                    }

                    if (session.AppID == request.Application.Id && session.DeviceID.ToUpper().Trim() == request.DeviceId.ToUpper().Trim() && session.MileagPlusNumber.ToUpper().Trim() == request.MileagePlusNumber.ToUpper().Trim())
                    {
                        #region
                        var valid = false;
                        if (request.SecurityUpdateType == MOBMPSecurityUpdatePath.ForgotMPPassWord)
                        {
                            if (_configuration.GetValue<bool>("EnableDPToken"))
                            {
                                string exceptionMessage = _configuration.GetValue<string>("ErrorContactMileagePlus") != null ? _configuration.GetValue<string>("ErrorContactMileagePlus").ToString() : "Please contact the MileagePlus Service Center for assistance with your account.";
                                if (_configuration.GetValue<bool>("EnableMemberProfileServiceEndPoint"))
                                {
                                    valid = await _mileagePlusCSL.ValidateMemberName(request, session.Token, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false);
                                    if (!valid)
                                    {
                                        throw new MOBUnitedException(exceptionMessage);
                                    }
                                }
                                else
                                {
                                    valid = await _mileagePlusCSL.ValidateMileagePlusNames(request, session.Token, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false);
                                    if (!valid)
                                    {
                                        throw new MOBUnitedException(exceptionMessage);
                                    }
                                }                                
                            }
                            else
                            {
                                // Added by Hasnan - #58425. To determine if Account is locked/ closed/ Active.
                                MOBMPAccountValidation accountValidation = _mileagePlusCSL.ValidateAccount(request.Application.Id, request.DeviceId, request.Application.Version.Major, request.TransactionId, request.MileagePlusNumber, string.Empty, session, false);

                                // Added by Hasnan - #58425. Customers with closed accounts should not be able to reset password.
                                if (accountValidation.ClosedAccount)
                                {
                                    string exceptionMessage = _configuration.GetValue<string>("ErrorContactMileagePlus") != null ? _configuration.GetValue<string>("ErrorContactMileagePlus").ToString() : "Please contact the MileagePlus Service Center for assistance with your account.";
                                    throw new MOBUnitedException(exceptionMessage);
                                }
                            }

                            #region
                            // Call same /8.0/utilities/profilevalidation/api/ValidateMileagePlusNames csl
                            //if (await _customerProfile.ValidateMPNames(session.Token, request.LanguageCode, request.MPSignInNeedHelpItems.NeedHelpSignInInfo.Title,
                            //     request.MPSignInNeedHelpItems.NeedHelpSignInInfo.First, request.MPSignInNeedHelpItems.NeedHelpSignInInfo.Middle, request.MPSignInNeedHelpItems.NeedHelpSignInInfo.Last,
                            //     request.MPSignInNeedHelpItems.NeedHelpSignInInfo.Suffix, request.MileagePlusNumber, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false))
                           
                            if (valid)
                            {
                                #region
                                var securityQuestionsFromCSL = await _securityQuestion.GetMPPinPwdSavedSecurityQuestions(session.Token, 0, request.MileagePlusNumber, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false);//.GetRange(0, 2);
                                if (securityQuestionsFromCSL == null || securityQuestionsFromCSL.Count == 0)
                                {
                                    if (GeneralHelper.IsApplicationVersionGreaterorEqual(request.Application.Id, request.Application.Version.Major,
                                        _configuration.GetValue<string>("Android_ResendActivationEmail_AppVersion"),
                                        _configuration.GetValue<string>("iPhone_ResendActivationEmail_AppVersion")))
                                    {
                                        response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetActivationEmailTitleMessages(request.MileagePlusNumber, session);
                                        var primaryEmailResponse = await GetCustomerPrimaryEmail(request.MileagePlusNumber, session);
                                        response.MPSignInNeedHelpDetails.NeedHelpMessages.Find(m => m.Id == "Body1").CurrentValue
                                            = FormatNeedHelpEmailMessages(response.MPSignInNeedHelpDetails.NeedHelpMessages
                                            .Find(m => m.Id == "Body1").CurrentValue, primaryEmailResponse);
                                        response.NeedHelpSecurityPath = MOBMPSecurityUpdatePath.UpdateSecurityQuestions; 
                                        response.ErrorScreenType = MOBMPErrorScreenType.UnableToReset;
                                    }
                                    else
                                    {
                                        response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetNeedHelpTitleMessages(MOBMPSecurityUpdatePath.IncorrectUserDetails, false, session.SessionId).ConfigureAwait(false);
                                        response.NeedHelpSecurityPath = MOBMPSecurityUpdatePath.UnableToResetOnline;
                                    }
                                }
                                else
                                {
                                    bool resultTfaWrongAnswersFlag = await _mileagePlusTFACSL.GetTfaWrongAnswersFlag(session.SessionId, session.Token, session.CustomerID, request.MileagePlusNumber, false, request.LanguageCode).ConfigureAwait(false); //**==>> Change Provider to return only bool true or false. .
                                    if (!resultTfaWrongAnswersFlag && !await _mileagePlusCSL.GetWrongAnswersFlag(true, session.Token, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId, 0, request.MileagePlusNumber).ConfigureAwait(false))
                                    {
                                        int numberOfQuestionsToBeSent = _configuration.GetValue<int>("NumberOfQuestionsToBeSentToClinet");
                                        if (securityQuestionsFromCSL.Count >= numberOfQuestionsToBeSent)
                                        {
                                            response.MPSignInNeedHelpDetails.SecurityQuestions = securityQuestionsFromCSL.Take(numberOfQuestionsToBeSent).ToList(); // ALM 27489 PINPWD: Same security questions order is not displayed in mobile when compared with .COM
                                            var persistSecurityQuestionObject = new MPPINPWSecurityQuestionsValidation
                                            {
                                                RetryCount = 0,
                                                SecurityQuestionsFromCSL = securityQuestionsFromCSL,
                                                SecurityQuestionsSentToClient = response.MPSignInNeedHelpDetails.SecurityQuestions
                                            };
                                            await _sessionHelperService.SaveSession<MPPINPWSecurityQuestionsValidation>(persistSecurityQuestionObject, session.SessionId, new List<string> { session.SessionId, persistSecurityQuestionObject.ObjectName }, persistSecurityQuestionObject.ObjectName).ConfigureAwait(false);
                                            response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetNeedHelpTitleMessages(MOBMPSecurityUpdatePath.ValidateSecurityQuestions, false, session.SessionId).ConfigureAwait(false);
                                            response.NeedHelpSecurityPath = MOBMPSecurityUpdatePath.ValidateSecurityQuestions;
                                        }
                                        else
                                        {
                                            response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetNeedHelpTitleMessages(MOBMPSecurityUpdatePath.IncorrectUserDetails, false, session.SessionId).ConfigureAwait(false);
                                            response.NeedHelpSecurityPath = MOBMPSecurityUpdatePath.UnableToResetOnline;
                                        }
                                    }
                                    else
                                    {
                                        response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetNeedHelpTitleMessages(MOBMPSecurityUpdatePath.AccountLocked, false, session.SessionId).ConfigureAwait(false);
                                        response.NeedHelpSecurityPath = MOBMPSecurityUpdatePath.UnableToResetOnline;
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                string exceptionMessage = _configuration.GetValue<string>("ValidateMileagePlusNamesErrorMessage").ToString();
                                throw new MOBUnitedException(exceptionMessage);
                            }
                            #endregion
                        }
                        else if (request.SecurityUpdateType == MOBMPSecurityUpdatePath.ValidateSecurityQuestions)
                        {
                            #region
                            bool answeredAllCorrect = true;

                            #region All CorrectAnswersCheck
                            var questionsFromPersist = await _sessionHelperService.GetSession<MPPINPWSecurityQuestionsValidation>(request.SessionID, new MPPINPWSecurityQuestionsValidation().ObjectName, new List<string> { request.SessionID, new MPPINPWSecurityQuestionsValidation().ObjectName }).ConfigureAwait(false);

                            if (questionsFromPersist != null)
                            {
                                questionsFromPersist.RetryCount = questionsFromPersist.RetryCount + 1;
                            }
                            int correctlyAnsweredQuestions = 0;
                            foreach (var question in request.MPSignInNeedHelpItems.AnsweredSecurityQuestions)
                            {
                                #region
                                foreach (var answer in question.Answers)
                                {
                                    if (await _securityQuestion.ValidateSecurityAnswer(answer.QuestionKey, answer.AnswerKey, request.MileagePlusNumber, session.Token, request.LanguageCode, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false))
                                    {
                                        correctlyAnsweredQuestions = correctlyAnsweredQuestions + 1;
                                    }
                                    else
                                    {
                                        answeredAllCorrect = false;
                                    }
                                }
                                #endregion
                            }
                            #endregion

                            #region TFA Validation & Respose Block
                            bool validatedevicecallwrapper = await _mileagePlusTFACSL.ValidateDevice(session, request.Application.Version.Major, request.LanguageCode).ConfigureAwait(false);//**==>> Change Provider to return only bool true or false.
                            bool isTFAVersion = GeneralHelper.ValidateNonTFAVersion(request.Application.Id, request.Application.Version.Major, _configuration);

                            if (isTFAVersion && ((answeredAllCorrect && !validatedevicecallwrapper) || (!answeredAllCorrect && correctlyAnsweredQuestions == 0 && validatedevicecallwrapper)))
                            {
                                response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetNeedHelpTitleMessages(request.Application.Id == 6 ? "MP_PIN_PWD_TFA_FORGOT_PASSWORD_TITLES_WINDOWS_APP" : "MP_PIN_PWD_TFA_FORGOT_PASSWORD_TITLES_ALL", session.SessionId).ConfigureAwait(false);

                                MOBMPPINPWDValidateRequest mobmppinpwdvalidaterequest = new MOBMPPINPWDValidateRequest
                                {
                                    Application = request.Application,
                                    MileagePlusNumber = request.MileagePlusNumber,
                                    TransactionId = request.TransactionId,
                                    DeviceId = request.DeviceId
                                };
                                MOBCPProfile mobcpprofile = await _customerProfile.GeteMailIDTFAMPSecurityDetails(mobmppinpwdvalidaterequest, session.Token).ConfigureAwait(false);


                                if (mobcpprofile != null)
                                {
                                    string strEmail = GetProfileEmailAddress(mobcpprofile);
                                    string maskedEmail = MaskEmailAddress(strEmail) + ".";
                                    response.MPSignInNeedHelpDetails.NeedHelpMessages.Add(new MOBItem() { CurrentValue = maskedEmail, Id = "Body3", SaveToPersist = false });
                                    if (!string.IsNullOrEmpty(strEmail))
                                    {
                                        await _mileagePlusTFACSL.SendForgotPasswordEmail(session.SessionId, session.Token, request.MileagePlusNumber, strEmail, request.LanguageCode).ConfigureAwait(false);
                                    }
                                    response.NeedHelpSecurityPath = MOBMPSecurityUpdatePath.TFAForgotPasswordEmail;
                                }

                                if (!answeredAllCorrect && correctlyAnsweredQuestions == 0 && validatedevicecallwrapper)
                                {
                                    await _securityQuestion.LockCustomerAccountWithSendEmailFlag(0, request.MileagePlusNumber, session.Token, request.LanguageCode, request.SessionID, request.Application.Id, request.Application.Version.Major, request.DeviceId, false).ConfigureAwait(false);
                                    await _mileagePlusCSL.UpdateWrongAnswersFlag(true, session.Token, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId, 0, request.MileagePlusNumber).ConfigureAwait(false);
                                }
                                //Reordering to present the same questions across all channels
                                await _mileagePlusCSL.ShuffleSavedSecurityQuestions(session.Token, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId, request.MileagePlusNumber).ConfigureAwait(false);

                            }
                            #endregion
                            else
                            {
                                if (!answeredAllCorrect)
                                {
                                    #region
                                    if (questionsFromPersist.RetryCount < 2 && correctlyAnsweredQuestions > 0)
                                    {
                                        var questionToBeSent = new List<Securityquestion>();
                                        questionToBeSent = questionsFromPersist.SecurityQuestionsFromCSL.Except(questionsFromPersist.SecurityQuestionsSentToClient, new SecurityquestionEqualityComparer()).ToList();
                                        response.MPSignInNeedHelpDetails.SecurityQuestions = questionToBeSent.Take(1).ToList();
                                        questionsFromPersist.SecurityQuestionsSentToClient = response.MPSignInNeedHelpDetails.SecurityQuestions;
                                        await _sessionHelperService.SaveSession<MPPINPWSecurityQuestionsValidation>(questionsFromPersist, session.SessionId, new List<string> { session.SessionId, questionsFromPersist.ObjectName }, questionsFromPersist.ObjectName).ConfigureAwait(false);
                                        response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetNeedHelpTitleMessages(MOBMPSecurityUpdatePath.ValidateSecurityQuestions, false, session.SessionId).ConfigureAwait(false);
                                        response.NeedHelpSecurityPath = MOBMPSecurityUpdatePath.ValidateSecurityQuestions;
                                    }
                                    else
                                    {
                                        //Reordering to present the same questions across all channels
                                        await _mileagePlusCSL.ShuffleSavedSecurityQuestions(session.Token, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId, request.MileagePlusNumber).ConfigureAwait(false);
                                        response.NeedHelpSecurityPath = MOBMPSecurityUpdatePath.IncorrectSecurityQuestion;
                                        response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetNeedHelpTitleMessages(MOBMPSecurityUpdatePath.IncorrectSecurityQuestion, false, session.SessionId).ConfigureAwait(false);
                                        //lock the account
                                        await _securityQuestion.LockCustomerAccount(0, request.MileagePlusNumber, session.Token, request.LanguageCode, request.SessionID, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false);
                                        await _mileagePlusCSL.UpdateWrongAnswersFlag(true, session.Token, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId, 0, request.MileagePlusNumber).ConfigureAwait(false);
                                    }
                                    #endregion
                                }
                                else if (answeredAllCorrect)
                                {
                                    //Reordering to present the same questions across all channels
                                    await _mileagePlusCSL.ShuffleSavedSecurityQuestions(session.Token, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId, request.MileagePlusNumber).ConfigureAwait(false);
                                    response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetNeedHelpTitleMessages("MP_RESET_PWD_TITLES", session.SessionId).ConfigureAwait(false);
                                    response.NeedHelpSecurityPath = MOBMPSecurityUpdatePath.UpdatePassword;
                                    if (_configuration.GetValue<bool>("MPSignInNeedHelpFix"))
                                    {
                                        questionsFromPersist.AllSecurityQuestionsAnsweredCorrect = true;
                                        await _sessionHelperService.SaveSession<MPPINPWSecurityQuestionsValidation>(questionsFromPersist, session.SessionId, new List<string> { session.SessionId, questionsFromPersist.ObjectName }, questionsFromPersist.ObjectName).ConfigureAwait(false);
                                    }
                                }
                            }
                            #endregion
                        }
                        else if (request.SecurityUpdateType == MOBMPSecurityUpdatePath.ForgotMileagePlusNumber)
                        {
                            #region   
                            if (_configuration.GetValue<bool>("SendMultipleMpNumberEnabled")
                               && GeneralHelper.IsApplicationVersionGreaterorEqual(request.Application.Id, request.Application.Version.Major, _configuration.GetValue<string>("Android_SendMultipleMpNumber_AppVersion"), _configuration.GetValue<string>("iPhone_SendMultipleMpNumber_AppVersion")))
                            {
                                var searchResponse = _mileagePlusCSL.SearchAllMPNumbers(request, session.Token, session.SessionId).Result;
                                var email = request.MPSignInNeedHelpItems.EmailAddress;

                                if (searchResponse != null && searchResponse.Data != null && searchResponse.Data.Count == 1)
                                {
                                    var firstResult = searchResponse.Data.FirstOrDefault();
                                    var messages = await GetMPPINPWDTitleMessages(new List<string>() { "MP_NEED_HELP_MILEAGEPLUS_ACCOUNT_FOUND_REFRESH_TITLES" }, session.SessionId);
                                    var emailFormat = messages.Find(m => m.Id == "Body1").CurrentValue;

                                    email = string.Format("{0}**********{1}", email[0], email.Substring(email.IndexOf('@') - 1));
                                    messages.Find(m => m.Id == "Body1").CurrentValue = string.Format(emailFormat, email);
                                    response.MPSignInNeedHelpDetails.NeedHelpMessages = messages;
                                    GetRavenInformation(session.Token, firstResult.MPNumber, firstResult.FirstName, string.Empty, request.MPSignInNeedHelpItems.EmailAddress, request.DeviceId, request.TransactionId);
                                }
                                else if (searchResponse != null && searchResponse.Data != null && searchResponse.Data.Count > 1)
                                {
                                    if (string.IsNullOrEmpty(email))
                                    {
                                        response.SearchCriteria = new Dictionary<string, string>()
                                                {
                                                    {"firstName", request.MPSignInNeedHelpItems.NeedHelpSignInInfo.First},
                                                    {"lastName", request.MPSignInNeedHelpItems.NeedHelpSignInInfo.Last},
                                                    {"dateOfBirth", request.MPSignInNeedHelpItems.NeedHelpSignInInfo.DateOfBirth}
                                                };
                                        response.NeedHelpSecurityPath = MOBMPSecurityUpdatePath.MultipleAccount;
                                        response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetNeedHelpTitleMessages(MOBMPSecurityUpdatePath.MultipleAccount, false, request.SessionID);
                                    }
                                    else
                                    {
                                        response.ErrorScreenType = MOBMPErrorScreenType.MultipleAccount;
                                        var messages = await GetMPPINPWDTitleMessages(new List<string>() { "MP_NEED_HELP_MILEAGEPLUS_MULTIPLE_ACCOUNT_FOUND_TITLES" }, session.SessionId);
                                        var emailFormat = messages.Find(m => m.Id == "Body1").CurrentValue;

                                        var emails = searchResponse.Data.Select(x => x.PrimaryEmail.ToLower()).ToList();
                                        emails.Add(request.MPSignInNeedHelpItems.EmailAddress.ToLower());
                                        emails = emails.Distinct().ToList();

                                        var versionCheck = GeneralHelper.IsApplicationVersionGreaterorEqual(request.Application.Id, request.Application.Version.Major, _configuration.GetValue<string>("Android_SendMultipleMpNumber_AppVersion"), _configuration.GetValue<string>("iPhone_SendMultipleMpNumber_AppVersion"));
                                        var breakLine = versionCheck ? request.Application.Id == 1 ? "\n" : "<br />" : ", ";
                                        var formatedEmaills = new List<string>();

                                        foreach (var e in emails)
                                        {
                                            var emailText = string.Format("{0}**********{1}", e[0], e.Substring(e.IndexOf('@') - 1));
                                            formatedEmaills.Add(emailText);
                                        }

                                        var emailsText = string.Join(breakLine, formatedEmaills);

                                        if (versionCheck)
                                        {
                                            emailsText = breakLine + emailsText + breakLine;
                                        }

                                        messages.Find(m => m.Id == "Body1").CurrentValue = string.Format(emailFormat, emailsText);
                                        response.MPSignInNeedHelpDetails.NeedHelpMessages = messages;

                                        var firstResult = searchResponse.Data.FirstOrDefault();

                                        var dictionary = searchResponse.Data.GroupBy(it => it.PrimaryEmail.ToLower()).ToDictionary(dict => dict.Key, dict => dict.Select(item => item.MPNumber).ToList());

                                        string recommondedMpNumbers = string.Empty;
                                        foreach (var d in dictionary)
                                        {
                                            var mpNumbers = d.Value.OrderBy(x => x).ToList();
                                            var firstName = searchResponse.Data.FirstOrDefault(x => x.MPNumber == mpNumbers.First()).FirstName;

                                            recommondedMpNumbers = RecommendedMPNumbers(session.Token, d.Value, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId);
                                            SendMultiplesMpNumbersToRaven(session.Token, mpNumbers, recommondedMpNumbers, firstName, string.Empty, d.Key, request);
                                        }
                                    }
                                }
                                else
                                {
                                    var title = request.Application.Id == 1 ? "MP_NEED_HELP_MILEAGEPLUS_ACCOUNT_NOT_FOUND_REFRESH_TITLES" : "MP_NEED_HELP_MILEAGEPLUS_ACCOUNT_NOT_FOUND_ANDROID_REFRESH_TITLES";
                                    response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetMPPINPWDTitleMessages(new List<string>() { title }, request.SessionID);
                                    response.ErrorScreenType = MOBMPErrorScreenType.AccountNotFound;
                                }
                            }
                            else if (_configuration.GetValue<bool>("SearchMpNumberEnabled")
                               && GeneralHelper.IsApplicationVersionGreaterorEqual(request.Application.Id, request.Application.Version.Major, _configuration.GetValue<string>("Android_ForgotMPPasswordRefresh_AppVersion"), _configuration.GetValue<string>("iPhone_ForgotMPPasswordRefresh_AppVersion")))
                            {
                                var customerMPSearchResponse = _mileagePlusCSL.SearchMPNumber(request, session.Token, session.SessionId).Result;
                                var email = request.MPSignInNeedHelpItems.EmailAddress;

                                if (customerMPSearchResponse != null && customerMPSearchResponse.Data != null)
                                {
                                    var messages = await GetMPPINPWDTitleMessages(new List<string>() { "MP_NEED_HELP_MILEAGEPLUS_ACCOUNT_FOUND_REFRESH_TITLES" }, session.SessionId).ConfigureAwait(false);
                                    var emailFormat = messages.Find(m => m.Id == "Body1").CurrentValue;
                                    email = string.Format("{0}**********{1}", email[0], email.Substring(email.IndexOf('@') - 1));
                                    messages.Find(m => m.Id == "Body1").CurrentValue = string.Format(emailFormat, email);
                                    response.MPSignInNeedHelpDetails.NeedHelpMessages = messages;
                                    GetRavenInformation(session.Token, customerMPSearchResponse.Data?.MPNumber, customerMPSearchResponse.Data?.FirstName, string.Empty, request.MPSignInNeedHelpItems.EmailAddress, request.DeviceId, request.TransactionId);
                                }
                                else if (customerMPSearchResponse != null && customerMPSearchResponse.Errors != null && customerMPSearchResponse.Errors.Any())
                                {
                                    if (customerMPSearchResponse.Errors.Any(error => error?.MinorCode == "66006" || error?.MinorDescription == "Multiple MP Numbers found"))
                                    {
                                        if (string.IsNullOrEmpty(email))
                                        {
                                            response.SearchCriteria = new Dictionary<string, string>()
                                                {
                                                    {"firstName", request.MPSignInNeedHelpItems.NeedHelpSignInInfo.First},
                                                    {"lastName", request.MPSignInNeedHelpItems.NeedHelpSignInInfo.Last},
                                                    {"dateOfBirth", request.MPSignInNeedHelpItems.NeedHelpSignInInfo.DateOfBirth}
                                                };
                                            response.NeedHelpSecurityPath = MOBMPSecurityUpdatePath.MultipleAccount;
                                            response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetNeedHelpTitleMessages(MOBMPSecurityUpdatePath.MultipleAccount, false, session.SessionId).ConfigureAwait(false);
                                        }
                                        else
                                        {
                                            response.ErrorScreenType = MOBMPErrorScreenType.Duplicate;
                                            response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetNeedHelpTitleMessages(MOBMPSecurityUpdatePath.MultipleAccount, true, session.SessionId).ConfigureAwait(false);

                                        }
                                    }
                                    else
                                    {
                                        var title = request.Application.Id == 1 ? "MP_NEED_HELP_MILEAGEPLUS_ACCOUNT_NOT_FOUND_REFRESH_TITLES" : "MP_NEED_HELP_MILEAGEPLUS_ACCOUNT_NOT_FOUND_ANDROID_REFRESH_TITLES";
                                        response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetMPPINPWDTitleMessages(new List<string>() { title }, session.SessionId).ConfigureAwait(false);
                                        response.ErrorScreenType = MOBMPErrorScreenType.AccountNotFound;
                                    }
                                }
                            }
                            else
                            {
                                if (await _mileagePlusCSL.SearchMPAccount(request, session.Token, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false))
                                {
                                    response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetNeedHelpTitleMessages(MOBMPSecurityUpdatePath.ForgotMileagePlusNumber, false, session.SessionId).ConfigureAwait(false);
                                }
                                else
                                {
                                    response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetNeedHelpTitleMessages(MOBMPSecurityUpdatePath.ForgotMileagePlusNumber, true, session.SessionId).ConfigureAwait(false);
                                }
                            }

                            #endregion
                        }
                        else if (request.SecurityUpdateType == MOBMPSecurityUpdatePath.UpdatePassword)
                        {
                            #region
                            if (_configuration.GetValue<bool>("MPSignInNeedHelpFix"))
                            {
                                bool answeredAllQuesetionsCorrect = false;
                                var questionsFromPersist = await _sessionHelperService.GetSession<MPPINPWSecurityQuestionsValidation>(request.SessionID, new MPPINPWSecurityQuestionsValidation().ObjectName, new List<string> { request.SessionID, new MPPINPWSecurityQuestionsValidation().ObjectName }).ConfigureAwait(false);
                                if (questionsFromPersist != null)
                                {
                                    answeredAllQuesetionsCorrect = questionsFromPersist.AllSecurityQuestionsAnsweredCorrect;
                                }
                                if (answeredAllQuesetionsCorrect == false)
                                {
                                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
                                }
                            }
                            if (await _securityQuestion.MPPinPwdValidatePassowrd(session.Token, request.LanguageCode, request.MPSignInNeedHelpItems.UpdatedPassword, request.MileagePlusNumber, string.Empty, request.MPSignInNeedHelpItems.EmailAddress, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false))
                            {
                                if (await _securityQuestion.MPPinPwdUpdateCustomerPassword(session.Token, string.Empty, request.MPSignInNeedHelpItems.UpdatedPassword, 0, request.MileagePlusNumber, request.LanguageCode, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false))
                                {
                                    await _securityQuestion.UnLockCustomerAccount(session.CustomerID, request.MileagePlusNumber, session.Token, request.LanguageCode, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false);
                                    response.NeedHelpCompleteMessage = _configuration.GetValue<string>("PasswordUpdatedSuccessMessage");
                                }
                            }
                            else if (request.MPSignInNeedHelpItems.UpdatedPassword.ToLower().Contains("password") || request.MPSignInNeedHelpItems.UpdatedPassword.Contains(request.MileagePlusNumber) || (!string.IsNullOrEmpty(request.MPSignInNeedHelpItems.EmailAddress) && request.MPSignInNeedHelpItems.UpdatedPassword.Contains(request.MPSignInNeedHelpItems.EmailAddress)))
                            {
                                string exceptionMessage = "Please enter a different password. We cannot accept the password you entered.";
                                if (_configuration.GetValue<string>("NotValidMPPasswordMessage") != null)
                                {
                                    exceptionMessage = _configuration.GetValue<string>("NotValidMPPasswordMessage").ToString();
                                }
                                throw new MOBUnitedException(exceptionMessage);
                            }
                            else
                            {
                                string exceptionMessage = "Please enter a valid password.";
                                if (_configuration.GetValue<string>("MinComplaintCheckFailMessage") != null)
                                {
                                    exceptionMessage = _configuration.GetValue<string>("MinComplaintCheckFailMessage").ToString();
                                }
                                throw new MOBUnitedException(exceptionMessage);
                            }
                            #endregion
                        }
                        else if (request.SecurityUpdateType == MOBMPSecurityUpdatePath.UpdateSecurityQuestions)
                        {
                            if (GeneralHelper.IsApplicationVersionGreaterorEqual(request.Application.Id, request.Application.Version.Major,
                                        _configuration.GetValue<string>("Android_ResendActivationEmail_AppVersion"),
                                        _configuration.GetValue<string>("iPhone_ResendActivationEmail_AppVersion")))
                            {
                                CSLModels.CustomerPrimaryEmailData customerPrimaryEmailData = new CSLModels.CustomerPrimaryEmailData();
                                customerPrimaryEmailData = await _sessionHelperService.GetSession<CSLModels.CustomerPrimaryEmailData>(session.SessionId, customerPrimaryEmailData.GetType().FullName,
                                    new List<string> { session.SessionId, customerPrimaryEmailData.GetType().FullName, }).ConfigureAwait(false);
                                if (customerPrimaryEmailData != null && customerPrimaryEmailData?.Emails != null)
                                {
                                    await GetSendActivationEmailResponse(request, session, customerPrimaryEmailData).ConfigureAwait(false);
                                    response.MPSignInNeedHelpDetails.NeedHelpMessages = await GetMPPINPWDTitleMessages(new List<string>() {
                                            "MP_NEED_HELP_SEND_ACTIVATION_EMAIL_TITLES" }, session.SessionId);
                                    response.MPSignInNeedHelpDetails.NeedHelpMessages.Find(m => m.Id == "Body1").CurrentValue
                                                = FormatNeedHelpEmailMessages(response.MPSignInNeedHelpDetails.NeedHelpMessages
                                                .Find(m => m.Id == "Body1").CurrentValue, customerPrimaryEmailData);
                                    response.NeedHelpSecurityPath = MOBMPSecurityUpdatePath.UpdateSecurityQuestions;
                                    response.ErrorScreenType = MOBMPErrorScreenType.ResendActivationEmail;
                                }
                            }
                        }
                        #endregion

                        if (response.NeedHelpSecurityPath == MOBMPSecurityUpdatePath.UnableToResetOnline
                       && request.SecurityUpdateType == MOBMPSecurityUpdatePath.ForgotMPPassWord
                       && _configuration.GetValue<bool>("ForgotMPPasswordRefresh")
                       && GeneralHelper.IsApplicationVersionGreaterorEqual(request.Application.Id, request.Application.Version.Major, _configuration.GetValue<string>("Android_ForgotMPPasswordRefresh_AppVersion"), _configuration.GetValue<string>("iPhone_ForgotMPPasswordRefresh_AppVersion")))
                        {
                            response.ErrorScreenType = MOBMPErrorScreenType.UnableToReset;
                        }
                    }
                    else
                    {
                        _logger.LogInformation("MPSignInNeedHelp {request} and {VerifyMPDeviceAPPIDException}", request, "MP , DeviceID and APPID not matched");

                        throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
                    }
                }
                if (response.NeedHelpSecurityPath == MOBMPSecurityUpdatePath.UnableToResetOnline
                        && request.SecurityUpdateType == MOBMPSecurityUpdatePath.ForgotMPPassWord
                        && _configuration.GetValue<bool>("ForgotMPPasswordRefresh")
                        && GeneralHelper.IsApplicationVersionGreaterorEqual(request.Application.Id, request.Application.Version.Major, _configuration.GetValue<string>("Android_ForgotMPPasswordRefresh_AppVersion"), _configuration.GetValue<string>("iPhone_ForgotMPPasswordRefresh_AppVersion")))
                {
                    response.ErrorScreenType = MOBMPErrorScreenType.UnableToReset;
                }
            }
            catch (MOBUnitedException uaex)
            {
                #region
                if (uaex != null && !string.IsNullOrEmpty(uaex.Message.Trim()) && !uaex.Message.Trim().Contains("ORA-") && !uaex.Message.Trim().Contains("PL/SQL"))
                {
                    response.Exception = new MOBException();
                    response.Exception.Message = uaex.Message;
                }
                else
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                #endregion
            }
            catch (Exception ex)
            {
                #region
                if (!_configuration.GetValue<bool>("SurfaceErrorToClient"))
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    response.Exception = new MOBException("9999", "SessionId :" + request.SessionID + ex.Message);
                }
                #endregion
            }

            return response;
        }

        private string FormatNeedHelpEmailMessages(string messageBody, CSLModels.CustomerPrimaryEmailData primaryEmailResponse)
        {
            if (primaryEmailResponse != null && primaryEmailResponse?.Emails != null)
            {
                var primaryEmail = primaryEmailResponse.Emails.Any() ? primaryEmailResponse?.Emails.FirstOrDefault()?.Address : string.Empty;
                if (!string.IsNullOrEmpty(primaryEmail))
                {
                    primaryEmail = string.Format("{0}**********{1}", primaryEmail[0], primaryEmail.Substring(primaryEmail.IndexOf('@') - 1));
                }
                return string.Format(messageBody, primaryEmail);
            }
            return string.Empty;
        }

        private async Task<List<MOBItem>> GetActivationEmailTitleMessages(string mileagePlusNumber, Session session)
        {
            return await GetMPPINPWDTitleMessages(new List<string>() {
                                            "MP_NEED_HELP_INCORRECT_ANSWERS_RESEND_ACTIVATION_EMAIL_TITLES"
                                        }, session.SessionId);
        }
        private async Task<CSLModels.CustomerPrimaryEmailData> GetCustomerPrimaryEmail(string mileagePlusNumber, Session session)
        {
            var primaryEmailResponse = await _contactPointService.GetPrimaryEmail(session.Token, mileagePlusNumber, session.SessionId);
            var primaryEmailData = JsonConvert.DeserializeObject<CSLModels.CslResponse<CSLModels.CustomerPrimaryEmailData>>(primaryEmailResponse)?.Data;
            await _sessionHelperService.SaveSession<CSLModels.CustomerPrimaryEmailData>(primaryEmailData, session.SessionId,
                new List<string> { session.SessionId, primaryEmailData.GetType().FullName });
            return primaryEmailData;
        }

        private string RecommendedMPNumbers(string token, List<string> mpNumbers, string sessionId, int applicationId, string appVersion, string deviceId)
        {
            var response = _mileagePlusCSL.RecommendedMPNumbers(mpNumbers, token, sessionId).Result;

            return response.Data.HasRecommendation
                ? response.Data.MileagePlusRecommendations.FirstOrDefault(x => x.Recommended).MileagePlusId
                : string.Empty;
        }

        private async void SendMultiplesMpNumbersToRaven(string token, List<string> mpNumbers, string recommondedMpNumber,
            string firstName, string lastName, string email, MOBMPSignInNeedHelpRequest request)
        {
            var eventSubtype = "MultipleMpWithoutMerge";

            if (mpNumbers.Count() == 2 && string.IsNullOrEmpty(recommondedMpNumber))
            {
                eventSubtype = "WithoutRecommendation";
            }
            else if (mpNumbers.Count() == 2 && !string.IsNullOrEmpty(recommondedMpNumber))
            {
                eventSubtype = "WithRecommendation";
                mpNumbers.Remove(recommondedMpNumber);
            }

            Guid myuuid = Guid.NewGuid();
            try
            {
                MOBMPRavenRequest ravenRequest = new MOBMPRavenRequest()
                {
                    Header = new MOBMPRavenHeader
                    {
                        EventHeader = new MOBMPRavenEventHeader
                        {
                            EventID = myuuid.ToString(),
                            EventName = _configuration.GetValue<string>("RavenEventName"),
                            EventCreationSystem = _configuration.GetValue<string>("RavenEventCreationSystem"),
                            EventCreationTime = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffZ"),
                            Version = _configuration.GetValue<string>("RavenVersion"),
                            EventSubtype = eventSubtype,
                            EventExpriationTime = DateTime.Now.AddDays(15).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffZ")
                        }
                    },
                    Body = new MOBMPRavenBody
                    {
                        LanguageCode = "en",
                        ContactData = new MOBMPRavenContactData
                        {
                            Email = new MOBMPRavenEmail
                            {
                                Address = new List<MOBMPRavenAddress>()
                                {
                                   new MOBMPRavenAddress(){ Type = "EVENT", Id = email}
                                }
                            }
                        },
                        Presentation = new MOBMPRavenPresentation()
                        {
                            AdditionalDtl = new MOBMPRavenAdditionalDtl
                            {
                                ParameterEntry = new List<MOBMPRavenParameterEntry>()
                                {
                                    new MOBMPRavenParameterEntry(){ Name = "mpnumber", Value = string.Join(",", mpNumbers.OrderBy(x => x).Take(5)) },
                                    new MOBMPRavenParameterEntry(){ Name = "firstName", Value = firstName },
                                }
                            }
                        }
                    }
                };

                if (eventSubtype == "WithRecommendation" && !string.IsNullOrEmpty(recommondedMpNumber))
                {
                    ravenRequest.Body.Presentation.AdditionalDtl.ParameterEntry
                        .Add(new MOBMPRavenParameterEntry() { Name = "recommondedMpNumber", Value = recommondedMpNumber });
                }

                bool isRavenProcessSuccess = _configuration.GetValue<bool>("RavenCloudMultipleMPEmail")
                    ? await GetRavenCloudResponse(ravenRequest, request.DeviceId, request.TransactionId).ConfigureAwait(false)
                    : await GetRavenResponse(token, ravenRequest, request.DeviceId, request.TransactionId).ConfigureAwait(false);

                if (!isRavenProcessSuccess)
                {
                    _logger.LogError("MPSignInNeedHelp - Error sending the Raven Email");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("MPSignInNeedHelp - Error sending the Raven Email");
            }
        }

        private async void GetRavenInformation(string token, string MPNumber, string firstName, string lastName, string email, string deviceId, string transactionId)
        {
            try
            {
                Guid myuuid = Guid.NewGuid();

                MOBMPRavenRequest ravenRequest = new MOBMPRavenRequest()
                {
                    Header = new MOBMPRavenHeader
                    {
                        EventHeader = new MOBMPRavenEventHeader
                        {
                            EventID = myuuid.ToString(),//"b821058a-bea8-44bf-8d13-a2f321cfdec9",
                            EventName = _configuration.GetValue<string>("RavenEventName"),
                            EventCreationSystem = _configuration.GetValue<string>("RavenEventCreationSystem"),
                            EventCreationTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"),
                            Version = _configuration.GetValue<string>("RavenVersion"),
                            EventSubtype = _configuration.GetValue<string>("RavenEventSubtype"),
                            EventExpriationTime = DateTime.Now.AddDays(15).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK")

                        }
                    },
                    Body = new MOBMPRavenBody
                    {
                        LanguageCode = "en",
                        ContactData = new MOBMPRavenContactData
                        {
                            Email = new MOBMPRavenEmail
                            {
                                Address = new List<MOBMPRavenAddress>()
                       {
                           new MOBMPRavenAddress(){ Type = "EVENT", Id = email}
                       }
                            }
                        },
                        Presentation = new MOBMPRavenPresentation()
                        {
                            AdditionalDtl = new MOBMPRavenAdditionalDtl
                            {
                                ParameterEntry = new List<MOBMPRavenParameterEntry>()
                       {
                           new MOBMPRavenParameterEntry(){ Name = "mpnumber", Value =MPNumber },
                           new MOBMPRavenParameterEntry(){ Name = "firstName", Value =firstName },
                           new MOBMPRavenParameterEntry(){ Name = "lastName", Value =lastName },
                       }
                            }
                        }
                    }
                };

                bool isRavenProcessSuccess = _configuration.GetValue<bool>("RavenCloudForgotMPEmail")
                    ? await GetRavenCloudResponse(ravenRequest, deviceId, transactionId).ConfigureAwait(false)
                    : await GetRavenResponse(token, ravenRequest, deviceId, transactionId).ConfigureAwait(false);
                if (!isRavenProcessSuccess)
                {
                    _logger.LogError("MPSignInNeedHelp - Error sending the Raven Email");
                    //throw new Exception();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("MPSignInNeedHelp - Error sending the Raven Email");
                //throw new MOBUnitedException(exceptionMessage);
            }
        }

        public async Task<bool> GetRavenResponse(string token, MOBMPRavenRequest request, string deviceId, string transactionId)
        {
            bool isRavenProcessSuccess = false;
            var settingsJson = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            string jsonRequest = JsonConvert.SerializeObject(request, settingsJson);// <MOBMPRavenRequest>
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "eventname", "MYACCOUNT" },
                { "eventSubtype", request.Header.EventHeader.EventSubtype }
            };
            _logger.LogInformation("MPSignInNeedHelp - Request {request}", jsonRequest);

            string jsonResponse = await _RavenService.SendRavenEmail(token, jsonRequest, headers, deviceId, transactionId).ConfigureAwait(false);

            MOBMPRavenResponse response = JsonConvert.DeserializeObject<MOBMPRavenResponse>(jsonResponse);
            if (response != null && response.status != null && response.status.Equals("SUCCESS"))
            {
                isRavenProcessSuccess = true;
            }
            return isRavenProcessSuccess;
        }

        private async Task<bool> GetRavenCloudResponse(MOBMPRavenRequest request, string deviceId, string transactionId)
        {
            bool isRavenProcessSuccess = false;
            var settingsJson = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            string jsonRequest = JsonConvert.SerializeObject(request, settingsJson);// <MOBMPRavenRequest>
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "eventname", request.Header.EventHeader.EventName },
                { "eventSubtype", request.Header.EventHeader.EventSubtype },
                { "eventCreationSystem", request.Header.EventHeader.EventCreationSystem },
                { "eventID", request.Header.EventHeader.EventID },
                { "Content-Type", "application/json" }
            };

            string jsonResponse = await _ravenCloudService.SendRavenEmail(jsonRequest, headers, deviceId, transactionId).ConfigureAwait(false);

            MOBMPRavenResponse response = JsonConvert.DeserializeObject<MOBMPRavenResponse>(jsonResponse);
            if (response != null && response.status != null && response.status.Equals("SUCCESS"))
            {
                isRavenProcessSuccess = true;
            }
            return isRavenProcessSuccess;
        }

        public async Task<MOBTFAMPDeviceResponse> SendResetAccountEmail(MOBTFAMPDeviceRequest request)
        {

            MOBTFAMPDeviceResponse response = new MOBTFAMPDeviceResponse();
            response.RememberMEFlags = new MOBMPTFARememberMeFlags(_configuration);

            #region
            response.MileagePlusNumber = request.MileagePlusNumber;
            response.SessionID = request.SessionID;

            Session session = null;
            if (!string.IsNullOrEmpty(request.SessionID))
            {
                session = await _shoppingSessionHelper.GetShoppingSession(request.SessionID).ConfigureAwait(false);
            }
            else
            {
                _logger.LogInformation("SendResetAccountEmail - Session Not found {request}", request);
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
            }

            request.TransactionId = session.SessionId;
            request.SessionID = session.SessionId;
            response.SessionID = session.SessionId;
            response.tFAMPDeviceSecurityPath = new MOBMPSecurityUpdatePath();

            if (session.AppID == request.Application.Id && session.DeviceID.ToUpper().Trim() == request.DeviceId.ToUpper().Trim())
            {
                response.tFAMPDeviceSecurityPath = MOBMPSecurityUpdatePath.IncorrectSecurityQuestion;
                response.tFAMPDeviceMessages = await GetNeedHelpTitleMessages(request.Application.Id == 6 ? "MP_PIN_PWD_TFA_ACCOUNT_SIGNIN_TRY_2_TITLES__WINDOWS_APP" : "MP_PIN_PWD_TFA_ACCOUNT_SIGNIN_TRY_2_TITLES_ALL", session.SessionId).ConfigureAwait(false);
                //lock the account
                var mobcpprofile = await _sessionHelperService.GetSession<MOBCPProfile>(session.SessionId, new MOBCPProfile().ObjectName, new List<string> { session.SessionId, new MOBCPProfile().ObjectName }).ConfigureAwait(false);

                #region getting PrimaryEmail address
                string strEmail = string.Empty;
                if (mobcpprofile != null)
                {
                    strEmail = GetProfileEmailAddress(mobcpprofile);
                }
                #endregion

                if (!string.IsNullOrEmpty(strEmail))
                {
                    //Send email for reset account
                    await _mileagePlusTFACSL.SendResetAccountEmail(session.SessionId, session.Token, session.CustomerID, request.MileagePlusNumber, strEmail, request.LanguageCode).ConfigureAwait(false);
                    string maskedEmail = MaskEmailAddress(strEmail) + ".";
                    response.tFAMPDeviceMessages?.Add(new MOBItem() { CurrentValue = maskedEmail, Id = "Body3", SaveToPersist = false });
                    response.tFAMPDeviceSecurityPath = MOBMPSecurityUpdatePath.TFAAccountLocked;
                }
                response.SecurityUpdate = true;
            }
            else
            {
                _logger.LogInformation("SendResetAccountEmail {request} and {SendResetAccountEmailException} ", JsonConvert.SerializeObject(request), "MP , DeviceID and APPID not matched");

                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
            }
            #endregion
            return response;
        }
        private async Task<string> GetSendActivationEmailResponse(MOBMPSignInNeedHelpRequest request, Session session, CSLModels.CustomerPrimaryEmailData customerPrimaryEmailData)
        {
            string jsonRequest = GetSendActivationEmailRequest(customerPrimaryEmailData);
            string jsonResponse = await _LoyaltyAWSService.SendActivationEmail(session.Token, jsonRequest, request.DeviceId, request.TransactionId).ConfigureAwait(false);
            return customerPrimaryEmailData?.Emails?.FirstOrDefault().Address;
        }
        private string GetSendActivationEmailRequest(CSLModels.CustomerPrimaryEmailData customerPrimaryEmailData)
        {
            SendActivationEmailRequest activationEmailRequest = new SendActivationEmailRequest
            {
                CustomerId = customerPrimaryEmailData?.CustomerId,
                Email = customerPrimaryEmailData?.Emails?.FirstOrDefault()?.Address ?? string.Empty,
                LanguageCode = "EN",
                MileagePlusId = customerPrimaryEmailData?.LoyaltyId
            };

            return JsonConvert.SerializeObject(activationEmailRequest);
        }

        private string GetProfileEmailAddress(MOBCPProfile profile)
        {
            #region getting PrimaryEmail address
            string strEmail = string.Empty;
            if (string.IsNullOrEmpty(strEmail) && profile != null && profile != null &&
                profile.Travelers != null && profile.Travelers.Count > 0 &&
                profile.Travelers[0].EmailAddresses != null &&
                profile.Travelers[0].EmailAddresses.Count > 0 &&
                profile.Travelers[0].EmailAddresses.Where(p => p.IsPrimary).Count() > 0)
            {
                strEmail = profile.Travelers[0].EmailAddresses.FirstOrDefault(p => p.IsPrimary).EmailAddress;
            }

            if (string.IsNullOrEmpty(strEmail) && profile != null && profile != null &&
                profile.Travelers != null && profile.Travelers.Count > 0 &&
                profile.Travelers[0].ReservationEmailAddresses != null &&
                profile.Travelers[0].ReservationEmailAddresses.Count > 0 &&
                profile.Travelers[0].ReservationEmailAddresses.Where(p => p.IsPrimary).Count() > 0)
            {
                strEmail = profile.Travelers[0].ReservationEmailAddresses.FirstOrDefault(p => p.IsPrimary).EmailAddress;
            }
            #endregion

            return strEmail;
        }

        private string MaskEmailAddress(string strEmail)
        {
            string maskedEmail = string.Empty;
            if (!string.IsNullOrEmpty(strEmail))
            {
                string[] emailidSplit = strEmail.Split('@');
                maskedEmail = String.Concat(Enumerable.Repeat("*", emailidSplit[0].Length));
                maskedEmail = maskedEmail.Length > 2 ? emailidSplit[0].Substring(0, 1) + maskedEmail.Substring(0, maskedEmail.Length - 2) + emailidSplit[0].Substring(emailidSplit[0].Length - 1) : maskedEmail;
                maskedEmail = maskedEmail + (strEmail.Count() > 1 ? "@" + emailidSplit[1].ToString() : "");
            }

            return maskedEmail;
        }

        private async Task<List<MOBItem>> GetNeedHelpTitleMessages(string title, string sessionId)
        {
            List<MOBItem> messages = new List<MOBItem>();
            List<string> titleList = new List<string>
            {
                title
            };
            messages = await GetMPPINPWDTitleMessages(titleList, sessionId).ConfigureAwait(false);
            return messages;
        }

        private async Task<List<MOBItem>> GetNeedHelpTitleMessages(MOBMPSecurityUpdatePath securityPath, bool isErrorMsg, string sessionId)
        {
            List<MOBItem> messages = new List<MOBItem>();
            List<string> titleList = new List<string>();

            if (securityPath == MOBMPSecurityUpdatePath.ValidateSecurityQuestions)
            {
                titleList.Add("MP_NEED_HELP_VALIDATE_SECURITY_QUESTIONS_TITLES");
            }
            if (securityPath == MOBMPSecurityUpdatePath.ForgotMileagePlusNumber)
            {
                if (!isErrorMsg)
                {
                    titleList.Add("MP_NEED_HELP_MILEAGEPLUS_ACCOUNT_FOUND_TITLES");
                }
                else
                {
                    titleList.Add("MP_NEED_HELP_MILEAGEPLUS_ACCOUNT_NOT_FOUND_TITLES");
                }
            }
            if (securityPath == MOBMPSecurityUpdatePath.IncorrectUserDetails)
            {
                titleList.Add("MP_NEED_HELP_USER_DETAILS_NOT_FOUND_TITLES");
            }
            if (securityPath == MOBMPSecurityUpdatePath.IncorrectSecurityQuestion)
            {
                titleList.Add("MP_NEED_HELP_INCORRECT_ANSWERS_TITLES");
            }

            if (securityPath == MOBMPSecurityUpdatePath.AccountLocked)
            {
                titleList.Add("MP_NEED_HELP_ACCOUNT_LOCKED_TITLES");
            }
            if (securityPath == MOBMPSecurityUpdatePath.ValidateTFASecurityQuestions)
            {
                titleList.Add("MP_NEED_HELP_VALIDATE_SECURITY_QUESTIONS_TITLES");
            }
            if (securityPath == MOBMPSecurityUpdatePath.MultipleAccount)
            {
                if (!isErrorMsg)
                {
                    titleList.Add("MP_NEED_HELP_MULTIPLE_ACCOUNT_TITLES");
                }
                else
                {
                    titleList.Add("MP_NEED_HELP_DUPLICATE_ACCOUNT_TITLES");
                }
            }
            messages = await GetMPPINPWDTitleMessages(titleList, sessionId).ConfigureAwait(false);
            return messages;
        }

        private async Task<MOBJoinMileagePlusEnrollmentResponse> OneClickEnrollment(
           MOBJoinMileagePlusEnrollmentRequest request,
           Session session,
           ReservationDetail cslReservation)
        {
            MOBJoinMileagePlusEnrollmentResponse response = new MOBJoinMileagePlusEnrollmentResponse();
            CSLEnrollUCB enrollCustomerRequest = new CSLEnrollUCB();
            if (cslReservation != null)
            {
                enrollCustomerRequest = BuildOneClickEnrollmentRequest(request, cslReservation);

                string jsonRequest = JsonConvert.SerializeObject(enrollCustomerRequest);

                string jsonResponse = await _LoyaltyAWSService.OneClickEnrollment(session.Token, jsonRequest, request.SessionId).ConfigureAwait(false);

                CslResponse<LoyaltyResponse> result = JsonConvert.DeserializeObject<CslResponse<LoyaltyResponse>>(jsonResponse);
                response.SessionId = session.SessionId;
                response.Email = request.Email;
                response.IsGetPNRByRecordLocatorCall = request.IsGetPNRByRecordLocatorCall;
                response.IsGetPNRByRecordLocator = request.IsGetPNRByRecordLocator;

                response.EnrolledUserInfo = new List<MOBKVP>();
                var KeyValues = new List<MOBKVP>() {

                            new MOBKVP (){ Key="Name", Value = request.TravelerName},
                            new MOBKVP (){ Key="MileagePlus Number", Value = result?.Data?.LoyaltyId}
                           };
                response.EnrolledUserInfo = KeyValues;

                string[] strMessages = request.TravelerName != null ? request.TravelerName?.Split(' ') : new string[] { " " };

                var firstName = string.Empty;
                if (strMessages.Count() > 0)
                {
                    firstName = strMessages[0];
                }

                response.AccountConfirmationTitle = !string.IsNullOrEmpty(_configuration.GetValue<string>("AccountConfirmationTitle")) ? _configuration.GetValue<string>("AccountConfirmationTitle") : string.Empty;
                response.AccountConfirmationHeader = !string.IsNullOrEmpty(_configuration.GetValue<string>("AccountConfirmationHeader")) ? _configuration.GetValue<string>("AccountConfirmationHeader") + firstName : string.Empty;
                response.AccountConfirmationBody = !string.IsNullOrEmpty(_configuration.GetValue<string>("AccountConfirmationBody")) ? _configuration.GetValue<string>("AccountConfirmationBody") : string.Empty;
                response.CloseButtion = !string.IsNullOrEmpty(_configuration.GetValue<string>("CloseButton")) ? _configuration.GetValue<string>("CloseButton") : string.Empty;
                response.AccountCreatedText = !string.IsNullOrEmpty(_configuration.GetValue<string>("AccountCreated")) ? _configuration.GetValue<string>("AccountCreated") : string.Empty;

                response.LastName = request.LastName;
                response.RecordLocator = request.RecordLocator;
                response.TransactionId = request.TransactionId;
                response.DeviceId = request.DeviceId;
                response.IsGetPNRByRecordLocatorCall = await DeterminePNRByRecordLocalCallRequired(cslReservation, request, session, result.Data.LoyaltyId).ConfigureAwait(false);
                response.IsGetPNRByRecordLocator = response.IsGetPNRByRecordLocatorCall;

                if (_configuration.GetValue<bool>(("OneClickValidateAddressEnabled")))
                {
                    cslReservation = await _sessionHelperService.GetSession<ReservationDetail>(session.SessionId, new ReservationDetail().GetType().FullName, new List<string> { session.SessionId, new ReservationDetail().GetType().FullName }).ConfigureAwait(false);
                }

                var travelersCount = cslReservation?.Detail?.Travelers?.Where(x => x.Person.Contact?.PhoneNumbers != null && x.Person.Contact?.PhoneNumbers.Count() > 0 && x.Person?.Type != "INF" && x.LoyaltyProgramProfile?.LoyaltyProgramMemberID == null)?.ToList().Count();
                var totalTravelerInPNR = cslReservation?.Detail?.Travelers?.Count();

                if (travelersCount >= 1 && totalTravelerInPNR > 1)
                {
                    response.IsEnrollAnotherTraveler = true;
                    response.EnrollAnotherTravelerButtonText = _configuration.GetValue<string>("AccountEnrollAnotherButton");
                }
            }
            return response;
        }

        private async Task<MOBJoinMileagePlusEnrollmentResponse> OneClickUcbEnrollment(
            MOBJoinMileagePlusEnrollmentRequest request,
            Session session,
            Service.Presentation.ReservationResponseModel.ReservationDetail cslReservation)
        {
            MOBJoinMileagePlusEnrollmentResponse response = new MOBJoinMileagePlusEnrollmentResponse();

            CSLEnrollUCB enrollCustomerRequest = new CSLEnrollUCB();
            try
            {
                if (cslReservation != null)
                {
                    enrollCustomerRequest = BuildOneClickEnrollmentUcbRequest(request, cslReservation);

                    string jsonRequest = JsonConvert.SerializeObject(enrollCustomerRequest);
                    string jsonResponse = await _LoyaltyAWSService.OneClickEnrollment(session.Token, jsonRequest, request.SessionId).ConfigureAwait(false);

                    CslUcbResponse<LoyaltyUCBResponse> resultUcb = JsonConvert.DeserializeObject<CslUcbResponse<LoyaltyUCBResponse>>(jsonResponse);
                    response.SessionId = session.SessionId;
                    response.Email = request.Email;
                    response.IsGetPNRByRecordLocatorCall = request.IsGetPNRByRecordLocatorCall;
                    response.IsGetPNRByRecordLocator = request.IsGetPNRByRecordLocator;

                    response.EnrolledUserInfo = new List<MOBKVP>();
                    var KeyValues = new List<MOBKVP>() {

                                new MOBKVP (){ Key="Name", Value = request.TravelerName},
                                new MOBKVP (){ Key="MileagePlus Number", Value = resultUcb?.Data?.LoyaltyId}
                               };
                    response.EnrolledUserInfo = KeyValues;

                    string[] strMessages = request.TravelerName != null ? request.TravelerName?.Split(' ') : new string[] { " " };

                    var firstName = string.Empty;
                    if (strMessages.Count() > 0)
                    {
                        firstName = strMessages[0];
                    }

                    response.AccountConfirmationTitle = !string.IsNullOrEmpty(_configuration.GetValue<string>("AccountConfirmationTitle")) ? _configuration.GetValue<string>("AccountConfirmationTitle") : string.Empty;
                    response.AccountConfirmationHeader = !string.IsNullOrEmpty(_configuration.GetValue<string>("AccountConfirmationHeader")) ? _configuration.GetValue<string>("AccountConfirmationHeader") + firstName : string.Empty;
                    response.AccountConfirmationBody = !string.IsNullOrEmpty(_configuration.GetValue<string>("AccountConfirmationBody")) ? _configuration.GetValue<string>("AccountConfirmationBody") : string.Empty;
                    response.CloseButtion = !string.IsNullOrEmpty(_configuration.GetValue<string>("CloseButton")) ? _configuration.GetValue<string>("CloseButton") : string.Empty;
                    response.AccountCreatedText = !string.IsNullOrEmpty(_configuration.GetValue<string>("AccountCreated")) ? _configuration.GetValue<string>("AccountCreated") : string.Empty;

                    response.LastName = request.LastName;
                    response.RecordLocator = request.RecordLocator;
                    response.TransactionId = request.TransactionId;
                    response.DeviceId = request.DeviceId;
                    response.IsGetPNRByRecordLocatorCall = await DeterminePNRByRecordLocalCallRequired(cslReservation, request, session, resultUcb.Data.LoyaltyId).ConfigureAwait(false);
                    response.IsGetPNRByRecordLocator = response.IsGetPNRByRecordLocatorCall;

                    if (_configuration.GetValue<bool>("OneClickValidateAddressEnabled"))
                    {
                        cslReservation = await _sessionHelperService.GetSession<ReservationDetail>(session.SessionId, new ReservationDetail().GetType().FullName, new List<string> { session.SessionId, new ReservationDetail().GetType().FullName }).ConfigureAwait(false);
                    }

                    var travelersCount = cslReservation?.Detail?.Travelers?.Where(x => x.Person.Contact?.PhoneNumbers != null && x.Person.Contact?.PhoneNumbers.Count() > 0 && x.Person?.Type != "INF" && x.LoyaltyProgramProfile?.LoyaltyProgramMemberID == null).ToList().Count();
                    var totalTravelerInPNR = cslReservation?.Detail?.Travelers?.Count();

                    if (travelersCount >= 1 && totalTravelerInPNR > 1)
                    {
                        response.IsEnrollAnotherTraveler = true;
                        response.EnrollAnotherTravelerButtonText = _configuration.GetValue<string>("AccountEnrollAnotherButton");
                    }

                }
                return response;
            }
            catch (System.Net.WebException webex)
            {
                if (webex.Status == WebExceptionStatus.ReceiveFailure || webex.Status == WebExceptionStatus.Timeout)
                {
                    if (webex.Message.Contains("400.93") || webex.Message.Contains("duplicate account(s) found"))
                    {
                        response.Exception = new MOBException("0528", "Duplicate Enrollment");
                        return response;
                    }
                    else if (webex.Message.Contains("500.102"))
                    {
                        response.Exception = new MOBException("102", _configuration.GetValue<string>("OneClickEnrollmentTimeOutError"));
                        response.Exception.ErrMessage = "Account Created | Welcome to MileagePlus";
                        return response;
                    }
                    else if (_configuration.GetValue<bool>("ShowFullEnrollmentErrorPopUp") &&
                        GeneralHelper.IsApplicationVersionGreaterorEqual(request.Application.Id, request.Application.Version.Major, _configuration.GetValue<string>("Android_ShowFullEnrollmentErrorPopUp_AppVersion"), _configuration.GetValue<string>("IPhone_ShowFullEnrollmentErrorPopUp_AppVersion")))
                    {
                        response.Exception = new MOBException("0600", _configuration.GetValue<string>("FullEnrollmentErrorPopUpTitle"));
                        response.Exception.ErrMessage = _configuration.GetValue<string>("FullEnrollmentErrorPopUpText");
                        response.FullEnrollmentUserInfo = BuildFullEnrollmentUserInfo(enrollCustomerRequest);
                        response.AccountConfirmationTitle = null;
                        response.AccountConfirmationBody = null;
                        response.AccountConfirmationHeader = null;
                        response.AccountCreatedText = null;
                        response.EnrolledUserInfo = null;
                        return response;
                    }
                    else
                    {
                        throw webex;
                    }
                }
                else
                {
                    throw webex;
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        private List<MOBKVP> BuildFullEnrollmentUserInfo(CSLEnrollUCB enrollCustomerRequest)
        {
            var countryName = GetCountryName(enrollCustomerRequest?.Address?.CountryCode).Result;
            var gender = Enum.TryParse<MOBGenderType>(enrollCustomerRequest?.Gender, true, out MOBGenderType genderType)
                ? genderType.GetDisplayName()
                : _configuration.GetValue<string>("FullEnrollmentGender");

            var address = enrollCustomerRequest.Address;

            return new List<MOBKVP>()
            {
                new MOBKVP(){Key = "Title", Value = enrollCustomerRequest.Title},
                new MOBKVP(){Key = "FirstName", Value = ToTitleCase(enrollCustomerRequest.FirstName)},
                new MOBKVP(){Key = "MiddleName", Value =  string.Empty},
                new MOBKVP(){Key = "LastName", Value = ToTitleCase(enrollCustomerRequest.LastName)},
                new MOBKVP(){Key = "Suffix",  Value =  string.Empty},
                new MOBKVP(){Key = "DateOfBirth", Value = enrollCustomerRequest.BirthDate.ToString("MM/dd/yyyy")},
                new MOBKVP(){Key = "Gender", Value = gender},
                new MOBKVP(){Key = "StreetAddress", Value = ToTitleCase(address?.Line1)},
                new MOBKVP(){Key = "StreetAddress2", Value = string.Empty},
                new MOBKVP(){Key = "City", Value = ToTitleCase(address?.City)},
                new MOBKVP(){Key = "StateCode", Value = address?.StateCode},
                new MOBKVP(){Key = "PostalCode", Value = address?.PostalCode},
                new MOBKVP(){Key = "CountryCode", Value = address?.CountryCode},
                new MOBKVP(){Key = "CountryName", Value = countryName},
                new MOBKVP(){Key = "PhoneCountryCode", Value = enrollCustomerRequest.Phone.CountryCode},
                new MOBKVP(){Key = "PhoneNumber", Value = enrollCustomerRequest.Phone.Number},
                new MOBKVP(){Key = "Email", Value = enrollCustomerRequest.Email.Address}
            };
        }

        private string ToTitleCase(string str)
        {
            return string.IsNullOrEmpty(str)
                ? string.Empty
                : System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        private CSLEnrollUCB BuildOneClickEnrollmentUcbRequest(MOBJoinMileagePlusEnrollmentRequest request, Service.Presentation.ReservationResponseModel.ReservationDetail cslReservation)
        {
            CSLEnrollUCB enrollCustomerRequest = new CSLEnrollUCB();
            var currentTraveler = cslReservation?.Detail?.Travelers?.FirstOrDefault(x => string.Equals(x.Person?.Key, request.SharesPosition, StringComparison.OrdinalIgnoreCase) == true);

            enrollCustomerRequest.FirstName = currentTraveler?.Person?.GivenName;
            enrollCustomerRequest.LastName = currentTraveler?.Person?.Surname;
            //enrollCustomerRequest.MiddleName =currentTraveler?.Person?.MiddleName;
            //enrollCustomerRequest.Suffix = currentTraveler?.Person?.Suffix;
            enrollCustomerRequest.Gender = !string.IsNullOrEmpty(currentTraveler?.Person?.Sex)
                ? currentTraveler?.Person?.Sex?.ToUpper() == "MALE" ? "M"
                : currentTraveler?.Person?.Sex?.ToUpper() == "FEMALE" ? "F"
                : currentTraveler?.Person?.Sex
                : string.Empty;
            enrollCustomerRequest.Title = (currentTraveler?.Person?.Sex == "M") ? "Mr." : (currentTraveler?.Person?.Sex == "F") ? "Ms." : "Mx.";
            enrollCustomerRequest.BirthDate = Convert.ToDateTime(currentTraveler?.Person?.DateOfBirth);
            enrollCustomerRequest.CountryOfResidency = currentTraveler?.Person?.Contact?.PhoneNumbers?.FirstOrDefault().CountryAccessCode;
            enrollCustomerRequest.SourceCode = !string.IsNullOrEmpty(_configuration.GetValue<string>("OneClickEnrollmentEnrollmentSourceCode")) ?
                    _configuration.GetValue<string>("OneClickEnrollmentEnrollmentSourceCode") : string.Empty;

            enrollCustomerRequest.Address = new Model.OneClickEnrollment.Address();
            // enrollCustomerRequest.Address.Type = !string.IsNullOrEmpty(_configuration.GetValue<string>["OneClickEnrollmentAddressType"]) ?
            //        _configuration.GetValue<string>["OneClickEnrollmentAddressType"] : string.Empty;
            enrollCustomerRequest.Address.Line1 = currentTraveler?.Tickets?.FirstOrDefault().Payments.FirstOrDefault().BillingAddress?.AddressLines.FirstOrDefault();
            enrollCustomerRequest.Address.City = currentTraveler?.Tickets?.FirstOrDefault().Payments.FirstOrDefault().BillingAddress.City;
            enrollCustomerRequest.Address.StateCode = currentTraveler?.Tickets?.FirstOrDefault().Payments?.FirstOrDefault().BillingAddress?.StateProvince?.StateProvinceCode;
            enrollCustomerRequest.Address.PostalCode = currentTraveler?.Tickets?.FirstOrDefault().Payments?.FirstOrDefault().BillingAddress?.PostalCode;
            enrollCustomerRequest.Address.CountryCode = currentTraveler?.Person?.Contact?.PhoneNumbers?.FirstOrDefault().CountryAccessCode;
            enrollCustomerRequest.UseAddressValidation = _configuration.GetValue<bool>("UseAddressValidation");
            enrollCustomerRequest.Phone = new Model.OneClickEnrollment.Phone();
            //enrollCustomerRequest.Phone.Type = _configuration.GetValue<string>("OneClickEnrollmentPhoneType");

            enrollCustomerRequest.Phone.CountryCode = currentTraveler?.Person?.Contact?.PhoneNumbers?.FirstOrDefault().CountryAccessCode;
            enrollCustomerRequest.Phone.Number = currentTraveler?.Person?.Contact?.PhoneNumbers?.FirstOrDefault().PhoneNumber;
            if (enrollCustomerRequest.Phone.CountryCode != null && enrollCustomerRequest.Phone.Number != null)
            {
                var countryPhoneNumber = GetCountryCode(enrollCustomerRequest.Phone.CountryCode).Result;
                if (enrollCustomerRequest.Phone.Number.Length > 10 && enrollCustomerRequest.Phone.CountryCode.Equals("US"))
                {
                    enrollCustomerRequest.Phone.Number = ExcludeCountryCodeFrmPhoneNumber(enrollCustomerRequest.Phone.Number, countryPhoneNumber);
                }
            }


            enrollCustomerRequest.Email = new Model.OneClickEnrollment.Email();
            enrollCustomerRequest.Email.Address = string.IsNullOrEmpty(request.Email) ? currentTraveler?.Person?.Contact?.Emails?.FirstOrDefault().Address : request.Email;

            if (request.ConsentToReceiveMarketingEmails)
            {
                string[] CSLMarketingPreferenceCodes = _configuration.GetValue<string>("CSLMarketingPreferenceCodes").Split(',');

                if (CSLMarketingPreferenceCodes.Count() > 0)
                {
                    enrollCustomerRequest.MarketingSubscriptions = new List<string>();
                    enrollCustomerRequest.MarketingSubscriptions = CSLMarketingPreferenceCodes.ToList();
                }

                enrollCustomerRequest.IsBatchEnrollment = _configuration.GetValue<bool>("IsBatchEnrollment");
                enrollCustomerRequest.IsPartner = _configuration.GetValue<bool>("IsPartner");
                enrollCustomerRequest.ValidateAddress = _configuration.GetValue<bool>("ValidateAddress");
                enrollCustomerRequest.ApplyDuplicateCheck = _configuration.GetValue<bool>("ApplyDuplicateCheck");

            }

            return enrollCustomerRequest;
        }



        private string ExcludeCountryCodeFrmPhoneNumber(string phonenumber, string countrycode)
        {
            try
            {
                Int64 _phonenumber;
                if (!string.IsNullOrEmpty(phonenumber)) phonenumber = phonenumber.Replace(" ", "");
                if (Int64.TryParse(phonenumber, out _phonenumber))
                {
                    if (!string.IsNullOrEmpty(countrycode))
                    {
                        var phonenumbercountrycode = phonenumber.Substring(0, countrycode.Length);
                        if (string.Equals(countrycode, phonenumbercountrycode, StringComparison.OrdinalIgnoreCase))
                        {
                            return phonenumber.Remove(0, countrycode.Length);
                        }
                    }
                }
            }
            catch
            { return string.Empty; }
            return phonenumber;
        }

        private async Task<string> GetCountryCode(string countryaccesscode)
        {
            var countrycode = string.Empty;
            try
            {
                var _countries = await _utility.LoadCountries().ConfigureAwait(false);
                countrycode = _countries.FirstOrDefault<CacheCountry>(_ => _.CODE == countryaccesscode)?.ACCESSCODE;
            }
            catch { return countrycode; }

            return countrycode;
        }

        public async Task<string> GetCountryName(string countryCode)
        {
            var countryName = string.Empty;
            try
            {
                var _countries = await _utility.LoadCountries().ConfigureAwait(false);
                countryName = _countries.FirstOrDefault<CacheCountry>(_ => _.CODE == countryCode)?.NAME;
                countryName = ToTitleCase(countryName.Replace("-", " "));
            }
            catch
            {
                return countryName;
            }

            return countryName;
        }

        private CSLEnrollUCB BuildOneClickEnrollmentRequest(MOBJoinMileagePlusEnrollmentRequest request, ReservationDetail cslReservation)
        {
            CSLEnrollUCB enrollCustomerRequest = new CSLEnrollUCB();
            var currentTraveler = cslReservation?.Detail?.Travelers?.FirstOrDefault(x => string.Equals(x.Person?.Key, request.SharesPosition, StringComparison.OrdinalIgnoreCase) == true);

            enrollCustomerRequest.FirstName = currentTraveler?.Person?.GivenName;
            enrollCustomerRequest.LastName = currentTraveler?.Person?.Surname;
            //enrollCustomerRequest.MiddleName =currentTraveler?.Person?.MiddleName;
            //enrollCustomerRequest.Suffix = currentTraveler?.Person?.Suffix;
            enrollCustomerRequest.Title = (currentTraveler?.Person?.Sex == "M") ? "Mr." : (currentTraveler?.Person?.Sex == "F") ? "Ms." : "Mx.";
            enrollCustomerRequest.DateOfBirth = Convert.ToDateTime(currentTraveler?.Person?.DateOfBirth);
            enrollCustomerRequest.CountryOfResidency = currentTraveler?.Person?.Contact?.PhoneNumbers?.FirstOrDefault().CountryAccessCode;
            enrollCustomerRequest.Gender = !string.IsNullOrEmpty(_configuration.GetValue<string>("OneClickEnrollmentGender")) ?
                    _configuration.GetValue<string>("OneClickEnrollmentGender") : string.Empty;
            enrollCustomerRequest.EnrollmentSourceCode = !string.IsNullOrEmpty(_configuration.GetValue<string>("OneClickEnrollmentEnrollmentSourceCode")) ?
                    _configuration.GetValue<string>("OneClickEnrollmentEnrollmentSourceCode") : string.Empty;

            enrollCustomerRequest.Address = new Model.OneClickEnrollment.Address
            {
                // enrollCustomerRequest.Address.Type = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OneClickEnrollmentAddressType"]) ?
                //        ConfigurationManager.AppSettings["OneClickEnrollmentAddressType"] : string.Empty;
                Line1 = currentTraveler?.Tickets?.FirstOrDefault().Payments.FirstOrDefault().BillingAddress?.AddressLines.FirstOrDefault(),
                City = currentTraveler?.Tickets?.FirstOrDefault().Payments.FirstOrDefault().BillingAddress.City,
                StateCode = currentTraveler?.Tickets?.FirstOrDefault().Payments?.FirstOrDefault().BillingAddress?.StateProvince?.StateProvinceCode,
                PostalCode = currentTraveler?.Tickets?.FirstOrDefault().Payments?.FirstOrDefault().BillingAddress?.PostalCode,
                CountryCode = currentTraveler?.Person?.Contact?.PhoneNumbers?.FirstOrDefault().CountryAccessCode
            };
            enrollCustomerRequest.UseAddressValidation = _configuration.GetValue<bool>("UseAddressValidation");
            enrollCustomerRequest.Phone = new Model.OneClickEnrollment.Phone
            {
                //enrollCustomerRequest.Phone.Type = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OneClickEnrollmentPhoneType"]) ?
                //        ConfigurationManager.AppSettings["OneClickEnrollmentPhoneType"] : string.Empty;

                CountryCode = currentTraveler?.Person?.Contact?.PhoneNumbers?.FirstOrDefault().CountryAccessCode,
                Number = currentTraveler?.Person?.Contact?.PhoneNumbers?.FirstOrDefault().PhoneNumber
            };

            enrollCustomerRequest.Email = new Model.OneClickEnrollment.Email
            {
                Address = string.IsNullOrEmpty(request.Email) ? currentTraveler?.Person?.Contact?.Emails?.FirstOrDefault().Address : request.Email
            };

            if (request.ConsentToReceiveMarketingEmails)
            {
                string[] CSLMarketingPreferenceCodes = (
                    !string.IsNullOrEmpty(_configuration.GetValue<string>("CSLMarketingPreferenceCodes")) ?
                    _configuration.GetValue<string>("CSLMarketingPreferenceCodes") : string.Empty
                ).Split(',');

                List<CSLMarketingPreference> ListOfCSLMarketingPreference = new List<CSLMarketingPreference>();

                foreach (var code in CSLMarketingPreferenceCodes)
                {
                    ListOfCSLMarketingPreference.Add(new CSLMarketingPreference { Code = code });
                }

                enrollCustomerRequest.MarketingPreferences = new List<CSLMarketingPreference>();
                enrollCustomerRequest.MarketingPreferences = ListOfCSLMarketingPreference;
                if (_configuration.GetValue<bool>("EnableUCBchange"))
                {
                    enrollCustomerRequest.IsBatchEnrollment = _configuration.GetValue<bool>("IsBatchEnrollment");
                    enrollCustomerRequest.IsPartner = _configuration.GetValue<bool>("IsPartner");
                    enrollCustomerRequest.ValidateAddress = _configuration.GetValue<bool>("ValidateAddress");
                    enrollCustomerRequest.ApplyDuplicateCheck = _configuration.GetValue<bool>("ApplyDuplicateCheck");
                }
            }

            return enrollCustomerRequest;
        }

        private async Task<bool> DeterminePNRByRecordLocalCallRequired(ReservationDetail cslReservation,
           MOBJoinMileagePlusEnrollmentRequest request,
           Session session, string mileagePlusId)
        {
            var travelerInfo = cslReservation?.Detail?.Travelers?.FirstOrDefault(x => string.Equals(x.Person?.Key, request.SharesPosition, StringComparison.OrdinalIgnoreCase) == true);

            var updateMPNumber = _mPTraveler.UpdateTravelerMPId(request.DeviceId, request.AccessCode, request.RecordLocator, session.SessionId, request.TransactionId, request.LanguageCode, request.Application.Id, request.Application.Version.Major, mileagePlusId, travelerInfo?.Person?.GivenName, travelerInfo?.Person?.Surname, request.SharesPosition, session.Token);
            if (updateMPNumber != null && updateMPNumber.Exception == null)
            {
                if (_configuration.GetValue<bool>("OneClickValidateAddressEnabled"))
                {
                    travelerInfo.LoyaltyProgramProfile = new LoyaltyProgramProfile()
                    {
                        LoyaltyProgramMemberID = mileagePlusId
                    };
                    cslReservation.Detail.Travelers[cslReservation.Detail.Travelers.IndexOf(travelerInfo)] = travelerInfo;

                    await _sessionHelperService.SaveSession<ReservationDetail>(cslReservation, session.SessionId, new List<string> { session.SessionId, new ReservationDetail().GetType().FullName },
                        new ReservationDetail().GetType().FullName).ConfigureAwait(false);
                }
                return true;
            }
            return false;
        }




        private async Task<List<MOBItem>> GetMPPINPWDTitleMessages(List<string> titleList, string sessionId)
        {
            bool isTermsnConditions = false;
            StringBuilder stringBuilder = new StringBuilder();
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
            var docs = new List<United.Definition.MOBLegalDocument>();
            try
            {
                docs = await _dynamoDBUtility.GetNewLegalDocumentsForTitles(reqTitles, sessionId, true).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("GetLegalDocumentsForTitles Error {message} {@StackTrace}", ex.Message, ex.StackTrace);
            }
            List<MOBItem> messages = new List<MOBItem>();
            if (docs != null && docs.Count > 0)
            {
                foreach (United.Definition.MOBLegalDocument doc in docs)
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
    }
}
