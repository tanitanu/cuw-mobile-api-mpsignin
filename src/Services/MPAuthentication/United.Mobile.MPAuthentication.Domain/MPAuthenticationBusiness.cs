using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using United.Common.Helper;
using United.Common.Helper.EmployeeReservation;
using United.Common.Helper.Profile;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.CSLSerivce;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.MPSignIn;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.Common.CloudDynamoDB;
using United.Mobile.Model.CSLModels;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.Internal.Common.DynamoDB;
using United.Mobile.Model.Internal.Exception;
using United.Mobile.Model.MPSignIn;
using United.Services.Customer.Common;
using United.Utility.Helper;


namespace United.Mobile.MPAuthentication.Domain
{
    public class MPAuthenticationBusiness : IMPAuthenticationBusiness
    {
        private readonly ICacheLog<MPAuthenticationBusiness> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IDPService _tokenService;
        private readonly IHeaders _headers;
        private readonly IDataPowerFactory _dataPowerFactory;
        private readonly IMPSecurityCheckDetailsService _mpSecurityCheckDetailsService;
        private readonly ICustomerProfile _customerProfile;
        private readonly IMileagePlus _mileagePlus;
        private readonly IMileagePlusTFACSL _mileagePlusTFACSL;
        private readonly IDynamoDBHelperService _dynamoDBHelperService;
        private readonly IEmployeeTravelTypeService _employeeProfileTravelType;
        private readonly IEResEmployeeProfileService _eResEmployeeProfileService;
        private readonly IEmployeeReservations _employeeReservations;
        private readonly IHomePageContentService _homePageContentService;
        private readonly IShoppingSessionHelper _shoppingSessionHelper;
        private readonly ISecurityQuestion _securityQuestion;
        private readonly IMerchandizingServices _merchandizingServices;
        private readonly IUCBProfile _uCBProfile;
        private readonly IUCBProfileService _uCBProfileService;
        private readonly IHashPin _HashPin;
        private readonly IDynamoDBUtility _dynamoDBUtility;
        private readonly IHashedPin _hashedPin;
        private readonly IFeatureSettings _featureSettings;
        private readonly IFeatureToggles _featureToggles;

        public MPAuthenticationBusiness(ICacheLog<MPAuthenticationBusiness> logger
            , IConfiguration configuration
            , ISessionHelperService sessionHelperService
            , IDPService tokenService
            , IHeaders headers
            , IDataPowerFactory dataPowerFactory
            , IMPSecurityCheckDetailsService mpSecurityCheckDetailsService
            , IMileagePlus mileagePlus
            , ICustomerProfile customerProfile
            , IMileagePlusTFACSL mileagePlusTFACSL
            , IDynamoDBHelperService dynamoDBHelperService
            , IEmployeeTravelTypeService employeeProfileTravelType
            , IEResEmployeeProfileService eResEmployeeProfileService
            , IHomePageContentService homePageContentService
            , IShoppingSessionHelper shoppingSessionHelper
            , IEmployeeReservations employeeReservations
            , ISecurityQuestion securityQuestion
            , IMerchandizingServices merchandizingServices
            , IUCBProfile uCBProfile
            , IUCBProfileService uCBProfileService
            , IHashPin hashPin
            , IDynamoDBUtility dynamoDBUtility
            , IHashedPin hashedPin
            , IFeatureSettings featureSettings
            , IFeatureToggles featureToggles)
        {
            _logger = logger;
            _configuration = configuration;
            _sessionHelperService = sessionHelperService;
            _tokenService = tokenService;
            _headers = headers;
            _dataPowerFactory = dataPowerFactory;
            _mpSecurityCheckDetailsService = mpSecurityCheckDetailsService;
            _customerProfile = customerProfile;
            _mileagePlus = mileagePlus;
            _mileagePlusTFACSL = mileagePlusTFACSL;
            _dynamoDBHelperService = dynamoDBHelperService;
            _employeeProfileTravelType = employeeProfileTravelType;
            _eResEmployeeProfileService = eResEmployeeProfileService;
            _homePageContentService = homePageContentService;
            _shoppingSessionHelper = shoppingSessionHelper;
            _employeeReservations = employeeReservations;
            _securityQuestion = securityQuestion;
            _merchandizingServices = merchandizingServices;
            _uCBProfile = uCBProfile;
            _uCBProfileService = uCBProfileService;
            _HashPin = hashPin;
            _dynamoDBUtility = dynamoDBUtility;
            _hashedPin = hashedPin;
            _featureSettings = featureSettings;
            _featureToggles = featureToggles;
        }

        public async Task<MOBMPPINPWDValidateResponse> ValidateMPSignInV2(MOBMPPINPWDValidateRequest request)
        {
            MOBMPPINPWDValidateResponse response = new MOBMPPINPWDValidateResponse(_configuration);
            string passWord = request.PassWord;
            request.PassWord = _configuration.GetValue<string>("PassWordMask");
            string logAction = "";
            string channelId = string.Empty;
            string channelName = string.Empty;
            Session session = null;

            try
            {
                _logger.LogInformation("ValidateMPSignInV2 {@clientRequest} {@mileagePlusNumber}", JsonConvert.SerializeObject(request), request?.MileagePlusNumber);

                if (!_configuration.GetValue<bool>("DisabledMPFiveNumberPasswordFix") && !request.SignInWithTouchID && !string.IsNullOrWhiteSpace(passWord) && passWord.Length <= 5)
                {
                    string exceptionMessage = _configuration.GetValue<string>("MPValidationErrorMessage") != null ? _configuration.GetValue<string>("MPValidationErrorMessage").ToString() : "The account information you entered is incorrect.";
                    if (session == null)
                    {
                        session = new Session
                        {
                            SessionId = request.TransactionId
                        };
                    }
                    throw new MOBUnitedException(exceptionMessage);
                }
                if (_configuration.GetValue<bool>("EnabledMERCHChannels"))
                {
                    _merchandizingServices.SetMerchandizeChannelValues("MOBBE", ref channelId, ref channelName);
                }
                else
                {
                    channelId = _configuration.GetValue<string>("MerchandizeOffersServiceChannelID").Trim();// "401";  //Changed per Praveen Vemulapalli email
                    channelName = _configuration.GetValue<string>("MerchandizeOffersServiceChannelName").Trim();//"MBE";  //Changed per Praveen Vemulapalli emai
                }
                if (!CheckValidateMPSigInRequest(request, passWord))
                {
                    string exceptionMessage = _configuration.GetValue<string>("MPValidationErrorMessage") ?? "The account information you entered is incorrect.";
                    if (session == null)
                    {
                        session = new Session
                        {
                            SessionId = request.TransactionId
                        };
                    }
                    throw new MOBUnitedException(exceptionMessage);
                }

                if (request != null && !string.IsNullOrEmpty(request.MileagePlusNumber))
                {
                    request.MileagePlusNumber = request.MileagePlusNumber.Trim().ToUpper();
                }

                bool isTFAVersion = GeneralHelper.ValidateNonTFAVersion(request.Application.Id, request.Application.Version.Major, _configuration);
                bool enableEResBooking = _configuration.GetValue<bool>("EnableEResBooking");

                string employeeId = string.Empty;
                string displayEmployeeId = string.Empty;

                if (!string.IsNullOrEmpty(request.SessionID))
                {
                    session = await _shoppingSessionHelper.GetShoppingSession(request.SessionID).ConfigureAwait(false);
                }
                else
                {
                    session = await _shoppingSessionHelper.CreateShoppingSession(request.Application.Id, request.DeviceId, request.Application.Version.Major, request.TransactionId, string.Empty, string.Empty).ConfigureAwait(false); // Passing here empty string for MP Account this will force to get a new Anonymous token to validate PIN and to get profile from DB instead from Persist.
                    await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, session.SessionId).ConfigureAwait(false);
                }

                logAction = session.IsReshopChange ? "ReShop-" : "";
                request.TransactionId = session.SessionId;
                request.SessionID = session.SessionId;

                await _sessionHelperService.SaveSession<MOBMPPINPWDValidateRequest>(request, _headers.ContextValues.SessionId, new List<string> { session.SessionId, request.ObjectName }, request.ObjectName).ConfigureAwait(false);
                bool getEmployeeIdFromCSLCustomerData = _configuration.GetValue<bool>("GetEmployeeIDFromGetProfileCustomerData");
                if (!getEmployeeIdFromCSLCustomerData)
                {
                    #region GetMappedEmployeeID from Travel Svcs
                    //Adding Config Value to turn off IsEResBetaTester check - JD 10Aug2016
                    bool blnCheckEmployee = false;
                    if (enableEResBooking)
                    {
                        blnCheckEmployee = true;
                        bool blnBetaTesterOn = _configuration.GetValue<bool>("IsEResBetaTesterOn");
                        if (blnBetaTesterOn)
                        {
                            string appliationVersion = request.Application.Version.Major;
                            if (await IsEResBetaTester(request.Application.Id, appliationVersion, request.MileagePlusNumber, session.SessionId).ConfigureAwait(false))
                            {
                                blnCheckEmployee = true;
                            }
                            else
                            {
                                blnCheckEmployee = false;
                            }
                        }
                    }

                    if (blnCheckEmployee)
                    {
                        var tupleRes = await GetEmployeeId(request.TransactionId, request.MileagePlusNumber,
                                 displayEmployeeId).ConfigureAwait(false);
                        employeeId = tupleRes.employeeId;
                        displayEmployeeId = tupleRes.displayEmployeeId;
                    }
                    #endregion 
                }

                response.SessionID = session.SessionId;
                bool validHashPinCheckFailed = false;
                string authToken = string.Empty;
                bool isTokenValid = false;
                bool isYoungAdult = false;
                MPSignIn persistMPsignIn = new MPSignIn();
                if (request.SignInWithTouchID)
                {
                    bool isValidHashPinCode = false;
                    var mpResponse = await _HashPin.ValidateHashPinAndGetAuthTokenDynamoDB(request.MileagePlusNumber, request.CustomerID, request.HashValue, request.Application.Id, request.DeviceId, request.Application.Version.Major).ConfigureAwait(false);

                    if (mpResponse.IsTokenValid != null)
                    {
                        isValidHashPinCode = true;
                        authToken = mpResponse.DataPowerAccessToken;
                    }

                    if ((!string.IsNullOrEmpty(request.HashValue) && request.CustomerID != 0 && isValidHashPinCode))
                    {
                        #region
                        if (session != null)
                        {
                            session.MileagPlusNumber = request.MileagePlusNumber;
                            session.CustomerID = request.CustomerID;
                            await _sessionHelperService.SaveSession<Session>(session, _headers.ContextValues.SessionId, new List<string> { session.SessionId, session.ObjectName }, session.ObjectName).ConfigureAwait(false);
                        }

                        if (_configuration.GetValue<string>("ByPassMPByPassCheckForDpMPSignCall2_1_41") != null &&
                            _configuration.GetValue<string>("ByPassMPByPassCheckForDpMPSignCall2_1_41").ToUpper().Trim() == request.Application.Version.Major.ToUpper().Trim()
                            && string.IsNullOrEmpty(authToken))
                        {
                            authToken = session.Token; // Work Around 
                            _logger.LogInformation("ValidateMPSignIn_DPRequest WorkAroundCheckuat_DeviceTable {@request}", JsonConvert.SerializeObject(request));
                        }

                        if (!string.IsNullOrEmpty(authToken))
                        {
                            var tupleRes = await _shoppingSessionHelper.CheckIsCSSTokenValid(request.Application.Id, request.DeviceId, _headers.ContextValues.Application.Version.ToString(), _headers.ContextValues.TransactionId, session, authToken).ConfigureAwait(false);
                            isTokenValid = tupleRes.isTokenValid;
                            if (isTokenValid)
                            {
                                MOBMPAccountValidation accountValidation = new MOBMPAccountValidation();
                                response.MPSecurityUpdateDetails = new MOBMPPINPWDSecurityUpdateDetails();
                                accountValidation.MileagePlusNumber = request.MileagePlusNumber;
                                ////New code changes to avoid returning PII and Account summary information if the user need to answer se. questions,
                                await ValidateTFAMPDevice(response, request, session).ConfigureAwait(false);

                                if (response.MPSecurityUpdateDetails != null && response.MPSecurityUpdateDetails.SecurityItems != null && response.MPSecurityUpdateDetails.SecurityItems.needQuestionsCount > 0
                                    && ((response.MPSecurityUpdateDetails.SecurityItems.AllSecurityQuestions != null && response.MPSecurityUpdateDetails.SecurityItems.AllSecurityQuestions.Count > 0)
                                    || (response.MPSecurityUpdateDetails.SecurityItems.SavedSecurityQuestions != null && response.MPSecurityUpdateDetails.SecurityItems.SavedSecurityQuestions.Count > 0)))
                                {
                                    //If the user needs to answer the security questions, return the response to UI and no need of execution of method.
                                    persistMPsignIn.MPNumber = request.MileagePlusNumber;
                                    persistMPsignIn.MPHashValue = request.HashValue;
                                    persistMPsignIn.SessionId = session.SessionId;
                                    persistMPsignIn.CustomerId = request.CustomerID;
                                    persistMPsignIn.AuthToken = authToken;
                                    persistMPsignIn.TokenExpirationDateTime = session.TokenExpireDateTime;
                                    persistMPsignIn.TokenExpirationSeconds = session.TokenExpirationValueInSeconds;
                                    persistMPsignIn.IsSignInWithTouchID = true;
                                    await _sessionHelperService.SaveSession<MPSignIn>(persistMPsignIn, session.SessionId, new List<string> { session.SessionId, persistMPsignIn.ObjectName }, persistMPsignIn.ObjectName).ConfigureAwait(false);
                                    if (request.MPSignInPath == MOBMPSignInPath.RevenueBookingPath)
                                    {
                                        response.ShowContinueAsGuestButton = true;
                                    }
                                    response.AccountValidation = accountValidation;
                                    return response;
                                }

                                accountValidation.ValidPinCode = true;
                                accountValidation.HashValue = request.HashValue;
                                accountValidation.CustomerId = session != null ? session.CustomerID : request.CustomerID; // For TOuch ID Client should pass the Customer ID in the request
                                accountValidation.AuthenticatedToken = authToken;
                                if (request.SignInWithTouchID && accountValidation.CustomerId == 0)
                                {
                                    accountValidation.CustomerId = request.CustomerID;
                                }

                                response.EmployeeId = employeeId;
                                response.DisplayEmployeeId = displayEmployeeId;
                                if (response.EmpTravelTypeResponse != null)
                                {
                                    response.EmpTravelTypeResponse.DisplayEmployeeId = response.DisplayEmployeeId;
                                }

                                response.AccountValidation = accountValidation;
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        string exceptionMessage = _configuration.GetValue<string>("MPValidationErrorMessage") != null ? _configuration.GetValue<string>("MPValidationErrorMessage").ToString() : "The account information you entered is incorrect.";
                        throw new MOBUnitedException(exceptionMessage);
                    }

                }
                else if (passWord == null || passWord.Trim() == "")
                {
                    string exceptionMessage = _configuration.GetValue<string>("MPValidationErrorMessage") != null ? _configuration.GetValue<string>("MPValidationErrorMessage").ToString() : "The account information you entered is incorrect.";
                    throw new MOBUnitedException(exceptionMessage);
                }
                else
                {
                    validHashPinCheckFailed = true;
                }
                // #endregion
                if (response.AccountValidation == null || (session != null && !session.IsTokenAuthenticated))
                {
                    _logger.LogInformation("ValidateMPSignIn Test IsTokenAuthenticated {IsTokenAuthenticated}", session.IsTokenAuthenticated);

                    bool isCorporateBooking = _configuration.GetValue<bool>("CorporateConcurBooking");
                    bool isAwardCalendarMP2017 = _configuration.GetValue<bool>("NGRPAwardCalendarMP2017Switch");
                    MOBCPProfile mpSecurityUpdateDetails = null;
                    if (!request.SignInWithTouchID)
                    {
                        response.AccountValidation = await ValidateAccountV2(request.Application.Id, request.DeviceId, request.Application.Version.Major, request.TransactionId, request.MileagePlusNumber, passWord, session, validHashPinCheckFailed, false).ConfigureAwait(false);

                        request.CustomerID = (int)response.AccountValidation.CustomerId;

                        persistMPsignIn.MPNumber = request.MileagePlusNumber;
                        persistMPsignIn.MPHashValue = response.AccountValidation.HashValue;
                        persistMPsignIn.SessionId = session.SessionId;
                        persistMPsignIn.CustomerId = (int)response.AccountValidation.CustomerId;
                        persistMPsignIn.AuthToken = response.AccountValidation.AuthenticatedToken;
                        persistMPsignIn.TokenExpirationDateTime = response.AccountValidation.TokenExpireDateTime;
                        persistMPsignIn.TokenExpirationSeconds = response.AccountValidation.TokenExpirationSeconds;

                        response.AccountValidation.TokenExpirationSeconds = 0;
                        response.AccountValidation.TokenExpireDateTime = new DateTime();
                        await _sessionHelperService.SaveSession<MPSignIn>(persistMPsignIn, session.SessionId, new List<string> { session.SessionId, new MPSignIn().ObjectName }, new MPSignIn().ObjectName).ConfigureAwait(false);

                        if (_configuration.GetValue<bool>("EnableUCBPhase1_MobilePhase3Changes") && request.MileagePlusNumber.Length == 11)
                        {
                            request.MileagePlusNumber = response.AccountValidation.MileagePlusNumber;
                        }

                        if (response.AccountValidation.ValidPinCode)
                        {
                            //New code changes to avoid returning PII and Account summary information
                            mpSecurityUpdateDetails = await GetMPSecurityCheckDetails(request, session.Token, getEmployeeIdFromCSLCustomerData).ConfigureAwait(false);

                            if (_configuration.GetValue<bool>("EnabledPartnerCardsId") && mpSecurityUpdateDetails.Travelers != null)
                            {
                                response.PartnerRPCIds = mpSecurityUpdateDetails.Travelers[0].PartnerRPCIds;
                            }

                            if (mpSecurityUpdateDetails != null && mpSecurityUpdateDetails.Travelers != null
                                && mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate != null
                                && mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.PasswordOnlyAllowed
                                && !String.IsNullOrEmpty(passWord) && United.Utility.Helper.SafeConverter.IsNumeric(passWord)
                                && passWord.Trim().Length == 4)
                            {
                                response.AccountValidation.ValidPinCode = false;
                                response.AccountValidation.AuthenticatedToken = string.Empty;
                                return response;
                            }
                            //Get the security questions
                            response.MPSecurityUpdateDetails = new MOBMPPINPWDSecurityUpdateDetails();
                            await ValidateTFAMPDevice(response, request, session).ConfigureAwait(false);

                            if (response.MPSecurityUpdateDetails != null && response.MPSecurityUpdateDetails.SecurityItems != null && response.MPSecurityUpdateDetails.SecurityItems.needQuestionsCount > 0
                                && ((response.MPSecurityUpdateDetails.SecurityItems.AllSecurityQuestions != null && response.MPSecurityUpdateDetails.SecurityItems.AllSecurityQuestions.Count > 0)
                                || (response.MPSecurityUpdateDetails.SecurityItems.SavedSecurityQuestions != null && response.MPSecurityUpdateDetails.SecurityItems.SavedSecurityQuestions.Count > 0)))
                            {
                                //If the user needs to answer the security questions, return the response to UI and no need of execution of method.
                                persistMPsignIn.Profile = mpSecurityUpdateDetails;
                                await _sessionHelperService.SaveSession<MPSignIn>(persistMPsignIn, session.SessionId, new List<string> { session.SessionId, persistMPsignIn.ObjectName }, persistMPsignIn.ObjectName).ConfigureAwait(false);

                                response.AccountValidation.HashValue = string.Empty;
                                response.AccountValidation.AuthenticatedToken = string.Empty;
                                if (request.MPSignInPath == MOBMPSignInPath.RevenueBookingPath)
                                {
                                    response.ShowContinueAsGuestButton = true;
                                }
                                if (request.MPSignInPath != MOBMPSignInPath.RevenueBookingPath)
                                {
                                    PinPwdAutoSignIn(response.MPSecurityUpdateDetails);
                                    await GetSecurityUpdateDetails(mpSecurityUpdateDetails, request, response, session).ConfigureAwait(false);
                                }

                                MPPINPWDValidateResponse responsePersist = new MPPINPWDValidateResponse
                                {
                                    Response = new MOBMPSecurityUpdateResponse
                                    {
                                        MPSecurityUpdateDetails = new MOBMPPINPWDSecurityUpdateDetails()
                                    }
                                };
                                responsePersist.Response.MPSecurityUpdateDetails = response.MPSecurityUpdateDetails;
                                responsePersist.Response.SecurityUpdate = response.SecurityUpdate;
                                responsePersist.Response.SessionID = response.SessionID;
                                responsePersist.Profile = mpSecurityUpdateDetails;
                                responsePersist.SessionId = session.SessionId;

                                await _sessionHelperService.SaveSession<MPPINPWDValidateResponse>(responsePersist, responsePersist.SessionId, new List<string> { responsePersist.SessionId, responsePersist.ObjectName }, responsePersist.ObjectName).ConfigureAwait(false);

                                return response;
                            }
                            await InsertMileagePlusAndHash(request.Application.Id, request.DeviceId, request.Application.Version.Major, request.TransactionId,
                                                                            response.AccountValidation.AuthenticatedToken, request.MileagePlusNumber, string.Empty, response.AccountValidation.HashValue,
                                                                            persistMPsignIn.TokenExpirationDateTime, persistMPsignIn.TokenExpirationSeconds, response.AccountValidation.CustomerId, false, session.SessionId, request, "ValidateMPSigninV2").ConfigureAwait(false);

                            if (_configuration.GetValue<bool>("EnableYoungAdultBooking")) //_customerProfile.EnableYoungAdult())
                            {
                                if (mpSecurityUpdateDetails != null && mpSecurityUpdateDetails.Travelers != null && mpSecurityUpdateDetails.Travelers.Count > 0)
                                {
                                    if (mpSecurityUpdateDetails.Travelers.Exists(t => t.IsProfileOwner))
                                    {
                                        try
                                        {
                                            isYoungAdult = TopHelper.IsYoungAdult(mpSecurityUpdateDetails.Travelers.First(t => t.IsProfileOwner).BirthDate);
                                        }
                                        catch (Exception)
                                        {
                                            _logger.LogError("ValidateMPSignIn_IsYoungAdult {@response}", GeneralHelper.RemoveCarriageReturn(JsonConvert.SerializeObject(response)));
                                        }
                                    }
                                }
                            }

                            if (getEmployeeIdFromCSLCustomerData &&
                                mpSecurityUpdateDetails != null &&
                                mpSecurityUpdateDetails.Travelers != null &&
                                mpSecurityUpdateDetails.Travelers.Count > 0)
                            {
                                employeeId = displayEmployeeId = mpSecurityUpdateDetails.Travelers[0].EmployeeId;
                                response.KtnNumber = mpSecurityUpdateDetails.Travelers[0].KnownTravelerNumber;

                            }
                            #region Corporate Booking
                            await ValidateCorporateBooking(request, isCorporateBooking, mpSecurityUpdateDetails, response, isYoungAdult, session).ConfigureAwait(false);
                            #endregion

                            #region Customer Metrics
                            response.CustomerMetrics = AssignCustomerMetrics(isAwardCalendarMP2017, mpSecurityUpdateDetails);
                            #endregion
                        }

                        if (response.AccountValidation.ValidPinCode)
                        {
                            var requestData = new MilagePlusDevice()
                            {
                                DeviceId = request.DeviceId,
                                ApplicationId = request.Application.Id.ToString(),
                                MileagePlusNumber = response.AccountValidation.MileagePlusNumber,
                                CustomerID = response.AccountValidation.CustomerId.ToString()
                            };

                            await _dynamoDBUtility.InsertMilagePlusDevice(requestData, session.SessionId).ConfigureAwait(false);

                        }

                        response.EmployeeId = employeeId;
                        response.DisplayEmployeeId = displayEmployeeId;
                        if (response.EmpTravelTypeResponse != null)
                        {
                            response.EmpTravelTypeResponse.DisplayEmployeeId = response.DisplayEmployeeId;
                        }
                    }
                    else
                    {
                        // !session.IsTokenExpired means for the InsertUpdateMPCSSValidationInfo() paramter isAuthTokenValid true or false session.IsTokenExpired = false means AuthTokenValid = true.
                        await InsertUpdateMPCSSValidationInfo(request.MileagePlusNumber, string.Empty, string.Empty, request.DeviceId, request.Application.Id, request.Application.Version.Major, session.Token, !session.IsTokenExpired, session.TokenExpireDateTime, session.TokenExpirationValueInSeconds, true, true, request.HashValue, Convert.ToInt64(request.CustomerID)).ConfigureAwait(false);

                    }

                    if (response.AccountValidation.ValidPinCode)
                    {

                        // #region
                        request.CustomerID = (int)response.AccountValidation.CustomerId;

                        if (CheckAppIDToReturnAccountSummary(request.Application.Id)) // TO DO Need to work Adding entries at web.config to allow only for specific Application IDS. For now only return for Android
                        {
                            #region
                            response.OPAccountSummary = await _mileagePlus.GetAccountSummary(request.TransactionId, response.AccountValidation.MileagePlusNumber, request.LanguageCode, true, session.SessionId).ConfigureAwait(false);
                            response.OPAccountSummary.HashValue = response.AccountValidation.HashValue;

                            response.isUASubscriptionsAvailable = false;

                            #region GetUASubscriptions
                            if (_configuration.GetValue<bool>("EnableUASubscriptions"))
                            {
                                response.UASubscriptions = await _merchandizingServices.GetUASubscriptions(response.AccountValidation.MileagePlusNumber, 1, request.TransactionId, channelId, channelName, session.Token).ConfigureAwait(false);
                                if (response.UASubscriptions != null && response.UASubscriptions.SubscriptionTypes != null && response.UASubscriptions.SubscriptionTypes.Count > 0)
                                {
                                    response.isUASubscriptionsAvailable = true;
                                }
                            }
                            #endregion
                            #endregion


                            MOBMPAccountValidationRequest requestSLO = new MOBMPAccountValidationRequest()
                            {
                                MileagePlusNumber = response.AccountValidation.MileagePlusNumber,
                                TransactionId = request.TransactionId,
                                DeviceId = request.DeviceId,
                                Application = new MOBApplication()
                                {
                                    Id = request.Application.Id,
                                    Version = new MOBVersion()
                                    {
                                        Major = request.Application.Version.Major,
                                        Build = request.Application.Version.Major
                                    }
                                }
                            };

                            //Promotion endpoint not available
                            //response.OPAccountSummary.statusLiftBanner = await _utility.GetStatusLiftBanner(requestSLO).ConfigureAwait(false);
                        }

                        if (!_configuration.GetValue<bool>("ByPassPINPWDValidate"))
                        {

                            //  #region
                            if (CheckAppIDToReturnAccountSummary(request.Application.Id) || ((request.MPSignInPath != MOBMPSignInPath.RevenueBookingPath || isTFAVersion) && response.AccountValidation.ValidPinCode))
                            {
                                if (response.MPSecurityUpdateDetails == null)
                                {
                                    response.MPSecurityUpdateDetails = new MOBMPPINPWDSecurityUpdateDetails();
                                }

                                if (mpSecurityUpdateDetails == null)
                                {
                                    mpSecurityUpdateDetails = await GetMPSecurityCheckDetails(request, session.Token, getEmployeeIdFromCSLCustomerData).ConfigureAwait(false);//response.AccountValidation.AuthenticatedToken);
                                    if (getEmployeeIdFromCSLCustomerData &&
                                        mpSecurityUpdateDetails != null &&
                                        mpSecurityUpdateDetails.Travelers != null &&
                                        mpSecurityUpdateDetails.Travelers.Count > 0)
                                    {
                                        if (_configuration.GetValue<bool>("EnableYoungAdultBooking"))
                                        {
                                            if (mpSecurityUpdateDetails != null && mpSecurityUpdateDetails.Travelers != null && mpSecurityUpdateDetails.Travelers.Count > 0)
                                            {
                                                if (mpSecurityUpdateDetails.Travelers.Exists(t => t.IsProfileOwner))
                                                {
                                                    try
                                                    {
                                                        isYoungAdult = TopHelper.IsYoungAdult(mpSecurityUpdateDetails.Travelers.First(t => t.IsProfileOwner).BirthDate);
                                                    }
                                                    catch (Exception)
                                                    {
                                                        _logger.LogError("ValidateMPSignIn_IsYoungAdult Exception {@response}", GeneralHelper.RemoveCarriageReturn(JsonConvert.SerializeObject(response)));
                                                    }
                                                }
                                            }
                                        }

                                        employeeId = displayEmployeeId = mpSecurityUpdateDetails.Travelers[0].EmployeeId;
                                        response.KtnNumber = mpSecurityUpdateDetails.Travelers[0].KnownTravelerNumber;
                                    }
                                    #region Corporate Booking
                                    await ValidateCorporateBooking(request, isCorporateBooking, mpSecurityUpdateDetails, response, isYoungAdult, session).ConfigureAwait(false);
                                    #endregion
                                    #region Customer Metrics
                                    response.CustomerMetrics = AssignCustomerMetrics(isAwardCalendarMP2017, mpSecurityUpdateDetails);
                                    #endregion
                                }
                            }

                            //config value - "Keep_MREST_MP_EliteLevel_Expiration_Logic" was missing in all config file of MRest app
                            if (!_configuration.GetValue<bool>("Keep_MREST_MP_EliteLevel_Expiration_Logic") && mpSecurityUpdateDetails != null && mpSecurityUpdateDetails.Travelers != null && mpSecurityUpdateDetails.Travelers.Any() && mpSecurityUpdateDetails.Travelers[0].MileagePlus != null)
                            {
                                // This method is to get new changes from profile service for expiration date.
                                // New parameters in profile service PremierLevelExpirationDate, InstantElite gives info on account(Trail) and expiration date
                                #region
                                _mileagePlus.GetMPEliteLevelExpirationDateAndGenerateBarCode(response.OPAccountSummary, mpSecurityUpdateDetails.Travelers[0].MileagePlus.PremierLevelExpirationDate, mpSecurityUpdateDetails.Travelers[0].MileagePlus.InstantElite);
                                #endregion
                            }
                            if (CheckAppIDToReturnAccountSummary(request.Application.Id))// TO DO Need to work Adding entries at web.config to allow only for specific Application IDS. For now only return for Android
                            {
                                #region
                                if (mpSecurityUpdateDetails != null)
                                {
                                    foreach (MOBCPTraveler traveler in mpSecurityUpdateDetails.Travelers)
                                    {
                                        if (traveler.MileagePlus != null && traveler.MileagePlus.MileagePlusId.ToUpper().Trim().Equals(request.MileagePlusNumber.ToUpper().Trim()))
                                        {
                                            response.OPAccountSummary.IsMPAccountTSAFlagON = traveler.IsTSAFlagON;
                                            break;
                                        }
                                    }
                                }
                                #endregion
                            }

                            if ((request.MPSignInPath != MOBMPSignInPath.RevenueBookingPath || isTFAVersion) && response.AccountValidation.ValidPinCode && mpSecurityUpdateDetails != null) // ****By Pass MP Pwd , Email and Security Questions update for Revenue Path Booking.***
                            {
                                #region
                                response.MPSecurityUpdateDetails.SecurityItems = new MOBMPPINPWDSecurityItems(_configuration);

                                #region
                                if (_configuration.GetValue<string>("SampleMPSecurityUpdates") != null)
                                {
                                    #region
                                    //<add key="SampleMPSecurityUpdates" value="SU929201~primaryemail~questions~password|VW344781~verifyemail~questions~password|XS189071~questions~password|CD904335~password|FT567838"/>
                                    string[] sampleMPSecurityUpdates = _configuration.GetValue<string>("SampleMPSecurityUpdates").ToUpper().Trim().Split('|');
                                    var result = sampleMPSecurityUpdates.Select((s, i) => s).Where(s => s.Contains(request.MileagePlusNumber.ToString().ToUpper() + "~")).ToList();

                                    if (result.Count > 0)
                                    {
                                        string mPSecurityUpdates = result[0].ToString();
                                        List<string> securityUpdatesList = mPSecurityUpdates.Split('~').ToList();

                                        List<MOBMPSecurityUpdatePath> mobSecurityPathList = new List<MOBMPSecurityUpdatePath>();
                                        foreach (string securityTypeUpdate in securityUpdatesList)
                                        {
                                            if (securityTypeUpdate.ToUpper().Trim() == "verifyemail".ToUpper().Trim())
                                            {
                                                mobSecurityPathList.Add(MOBMPSecurityUpdatePath.VerifyPrimaryEmail);
                                                if (mpSecurityUpdateDetails.Travelers[0].EmailAddresses.Count == 0)
                                                {
                                                    mpSecurityUpdateDetails.Travelers[0].EmailAddresses = new List<MOBEmail>();
                                                    MOBEmail email = new MOBEmail
                                                    {
                                                        EmailAddress = "test@testing.com"
                                                    };
                                                    mpSecurityUpdateDetails.Travelers[0].EmailAddresses.Add(email);
                                                }
                                            }
                                            else if (securityTypeUpdate.ToUpper().Trim() == "primaryemail".ToUpper().Trim())
                                            {
                                                mobSecurityPathList.Add(MOBMPSecurityUpdatePath.NoPrimayEmailExist);
                                            }
                                            else if (securityTypeUpdate.ToUpper().Trim() == "password".ToUpper().Trim())
                                            {
                                                mobSecurityPathList.Add(MOBMPSecurityUpdatePath.UpdatePassword);
                                            }
                                            else if (securityTypeUpdate.ToUpper().Trim() == "questions".ToUpper().Trim())
                                            {
                                                mobSecurityPathList.Add(MOBMPSecurityUpdatePath.UpdateSecurityQuestions);
                                            }
                                            else if (securityTypeUpdate.ToUpper().Trim() == "forceSignOut".ToUpper().Trim())
                                            {
                                                if (mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate == null) // This is the scenario where if already the accoutn we trying to use is arleady had security updated ready and we try using sample data for that MP
                                                {
                                                    mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate = new MOBMPSecurityUpdate
                                                    {
                                                        UpdateLaterAllowed = true,
                                                        PasswordOnlyAllowed = false,
                                                        DaysToCompleteSecurityUpdate = 11
                                                    };
                                                }
                                                mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.UpdateLaterAllowed = false;
                                            }
                                        }
                                        if (mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate == null) // This is the scenario where if already the accoutn we trying to use is arleady had security updated ready and we try using sample data for that MP
                                        {
                                            mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate = new MOBMPSecurityUpdate
                                            {
                                                UpdateLaterAllowed = true,
                                                PasswordOnlyAllowed = false,
                                                DaysToCompleteSecurityUpdate = 11
                                            };
                                        }
                                        mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.MPSecurityPathList = mobSecurityPathList;
                                    }
                                    #endregion
                                }
                                #endregion
                                if (mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate != null && mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.MPSecurityPathList != null && mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.MPSecurityPathList != null && mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.MPSecurityPathList.Count > 0)
                                {
                                    #region
                                    foreach (MOBMPSecurityUpdatePath securityPath in mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.MPSecurityPathList)
                                    {
                                        if (securityPath == MOBMPSecurityUpdatePath.NoPrimayEmailExist || securityPath == MOBMPSecurityUpdatePath.VerifyPrimaryEmail)
                                        {
                                            response.MPSecurityUpdateDetails.MPSecurityPath = securityPath;
                                            if (securityPath == MOBMPSecurityUpdatePath.VerifyPrimaryEmail && mpSecurityUpdateDetails.Travelers[0].EmailAddresses.Count > 0)
                                            {
                                                response.MPSecurityUpdateDetails.SecurityItems.PrimaryEmailAddress = mpSecurityUpdateDetails.Travelers[0].EmailAddresses[0].EmailAddress;
                                            }
                                            break;
                                        }
                                        if (securityPath == MOBMPSecurityUpdatePath.UpdateSecurityQuestions)
                                        {
                                            response.MPSecurityUpdateDetails.MPSecurityPath = securityPath;
                                            response.MPSecurityUpdateDetails.SecurityItems.AllSecurityQuestions = await _mileagePlusTFACSL.GetMPPINPWDSecurityQuestions(session.Token, request.SessionID, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false);
                                            break;
                                        }
                                        if (securityPath == MOBMPSecurityUpdatePath.UpdatePassword)
                                        {
                                            response.MPSecurityUpdateDetails.MPSecurityPath = securityPath;
                                            break;
                                        }
                                    }
                                    #endregion
                                    #region
                                    response.SecurityUpdate = true;
                                    response.MPSecurityUpdateDetails.UpdateLaterAllowed = mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.UpdateLaterAllowed;
                                    response.MPSecurityUpdateDetails.PasswordOnlyAllowed = mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.PasswordOnlyAllowed;
                                    response.MPSecurityUpdateDetails.DaysToCompleteSecurityUpdate = mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.DaysToCompleteSecurityUpdate;
                                    if (mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.MPSecurityPathList.Count >= 1)
                                    {
                                        response.MPSecurityUpdateDetails.MPSecurityPathList = mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.MPSecurityPathList;
                                        response.MPSecurityUpdateDetails.MPSecurityPathList.Remove(response.MPSecurityUpdateDetails.MPSecurityPath);
                                        if (!mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.UpdateLaterAllowed)
                                        {
                                            response.MPSecurityUpdateDetails.MPSecurityPathList.Add(MOBMPSecurityUpdatePath.SignInBackWithNewPassWord);
                                            response.MPSecurityUpdateDetails.ForceSignOut = true;
                                        }
                                    }
                                    #endregion
                                }

                                MPPINPWDValidateResponse responsePersist = new MPPINPWDValidateResponse
                                {
                                    Response = new MOBMPSecurityUpdateResponse
                                    {
                                        MPSecurityUpdateDetails = new MOBMPPINPWDSecurityUpdateDetails()
                                    }
                                };
                                //TFS Backlog Defect #27502 - PINPWD AutoSignIn
                                PinPwdAutoSignIn(response.MPSecurityUpdateDetails);

                                responsePersist.Response.MPSecurityUpdateDetails = response.MPSecurityUpdateDetails;
                                responsePersist.Response.SecurityUpdate = response.SecurityUpdate;
                                responsePersist.Response.SessionID = response.SessionID;
                                responsePersist.Profile = mpSecurityUpdateDetails;
                                responsePersist.SessionId = session.SessionId;

                                await _sessionHelperService.SaveSession<MPPINPWDValidateResponse>(responsePersist, responsePersist.SessionId, new List<string> { responsePersist.SessionId, responsePersist.ObjectName }, responsePersist.ObjectName).ConfigureAwait(false);

                                if (response.SecurityUpdate)
                                {
                                    response.LandingPageMessages = await GetSecurityUpdateTitleMessages(MOBMPSecurityUpdatePath.None, true, false, request.MileagePlusNumber, response.MPSecurityUpdateDetails.DaysToCompleteSecurityUpdate).ConfigureAwait(false);
                                    response.MPSecurityUpdateDetails.SecurityUpdateMessages = await GetSecurityUpdateTitleMessages(response.MPSecurityUpdateDetails.MPSecurityPath, false, false, request.MileagePlusNumber, 0).ConfigureAwait(false);
                                }
                                #endregion
                            }
                            else if ((request.MPSignInPath != MOBMPSignInPath.RevenueBookingPath || isTFAVersion) && !response.AccountValidation.ValidPinCode && _configuration.GetValue<string>("SampleMPSecurityUpdates") != null)
                            {
                                #region
                                response.AccountValidation.ValidPinCode = true;
                                response.MPSecurityUpdateDetails = new MOBMPPINPWDSecurityUpdateDetails
                                {
                                    SecurityItems = new MOBMPPINPWDSecurityItems(_configuration)
                                };
                                #region
                                //<add key="SampleMPSecurityUpdates" value="SU929201~primaryemail~questions~password|VW344781~verifyemail~questions~password|XS189071~questions~password|CD904335~password|FT567838"/>
                                string[] sampleMPSecurityUpdates = _configuration.GetValue<string>("SampleMPSecurityUpdates").ToUpper().Trim().Split('|');
                                string mPSecurityUpdates = sampleMPSecurityUpdates.Select((s, i) => s).Where(s => s.Contains(request.MileagePlusNumber.ToString().ToUpper() + "~")).ToList()[0].ToString();
                                List<string> securityUpdatesList = mPSecurityUpdates.Split('~').ToList();
                                List<MOBMPSecurityUpdatePath> mobSecurityPathList = new List<MOBMPSecurityUpdatePath>();
                                foreach (string securityTypeUpdate in securityUpdatesList)
                                {
                                    if (securityTypeUpdate.ToUpper().Trim() == "verifyemail".ToUpper().Trim())
                                    {
                                        mobSecurityPathList.Add(MOBMPSecurityUpdatePath.VerifyPrimaryEmail);
                                    }
                                    else if (securityTypeUpdate.ToUpper().Trim() == "primaryemail".ToUpper().Trim())
                                    {
                                        mobSecurityPathList.Add(MOBMPSecurityUpdatePath.NoPrimayEmailExist);
                                    }
                                    else if (securityTypeUpdate.ToUpper().Trim() == "password".ToUpper().Trim())
                                    {
                                        mobSecurityPathList.Add(MOBMPSecurityUpdatePath.UpdatePassword);
                                    }
                                    else if (securityTypeUpdate.ToUpper().Trim() == "questions".ToUpper().Trim())
                                    {
                                        mobSecurityPathList.Add(MOBMPSecurityUpdatePath.UpdateSecurityQuestions);
                                    }
                                    else if (securityTypeUpdate.ToUpper().Trim() == "forceSignOut".ToUpper().Trim())
                                    {
                                        mobSecurityPathList.Add(MOBMPSecurityUpdatePath.SignInBackWithNewPassWord);
                                        response.MPSecurityUpdateDetails.UpdateLaterAllowed = false;
                                    }
                                }
                                #endregion
                                if (mobSecurityPathList != null)
                                {
                                    #region
                                    foreach (MOBMPSecurityUpdatePath securityPath in mobSecurityPathList)
                                    {
                                        if (securityPath == MOBMPSecurityUpdatePath.NoPrimayEmailExist || securityPath == MOBMPSecurityUpdatePath.VerifyPrimaryEmail)
                                        {
                                            response.MPSecurityUpdateDetails.MPSecurityPath = securityPath;
                                            response.MPSecurityUpdateDetails.SecurityItems.PrimaryEmailAddress = "test@testing.com";
                                            break;
                                        }
                                        if (securityPath == MOBMPSecurityUpdatePath.UpdateSecurityQuestions)
                                        {
                                            response.MPSecurityUpdateDetails.MPSecurityPath = securityPath;
                                            response.MPSecurityUpdateDetails.SecurityItems.AllSecurityQuestions = await _mileagePlusTFACSL.GetMPPINPWDSecurityQuestions(session.Token, request.SessionID, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false);
                                            break;
                                        }
                                        if (securityPath == MOBMPSecurityUpdatePath.UpdatePassword)
                                        {
                                            response.MPSecurityUpdateDetails.MPSecurityPath = securityPath;
                                            break;
                                        }
                                    }
                                    response.SecurityUpdate = true;
                                    response.MPSecurityUpdateDetails.UpdateLaterAllowed = true;
                                    response.MPSecurityUpdateDetails.PasswordOnlyAllowed = false;
                                    response.MPSecurityUpdateDetails.DaysToCompleteSecurityUpdate = 11;

                                    if (mobSecurityPathList.Count > 1)
                                    {
                                        response.MPSecurityUpdateDetails.MPSecurityPathList = mobSecurityPathList;
                                        response.MPSecurityUpdateDetails.MPSecurityPathList.Remove(response.MPSecurityUpdateDetails.MPSecurityPath);
                                    }
                                    #endregion
                                }
                                MPPINPWDValidateResponse responsePersist = new MPPINPWDValidateResponse
                                {
                                    Response = new MOBMPSecurityUpdateResponse
                                    {
                                        MPSecurityUpdateDetails = new MOBMPPINPWDSecurityUpdateDetails()
                                    }
                                };
                                //TFS Backlog Defect #27502 - PINPWD AutoSignIn
                                PinPwdAutoSignIn(response.MPSecurityUpdateDetails);

                                responsePersist.Response.MPSecurityUpdateDetails = response.MPSecurityUpdateDetails;
                                responsePersist.Response.SecurityUpdate = response.SecurityUpdate;
                                responsePersist.Response.SessionID = response.SessionID;
                                responsePersist.SessionId = session.SessionId;

                                await _sessionHelperService.SaveSession<MPPINPWDValidateResponse>(responsePersist, responsePersist.SessionId, new List<string> { responsePersist.SessionId, responsePersist.ObjectName }, responsePersist.ObjectName).ConfigureAwait(false);

                                response.LandingPageMessages = await GetSecurityUpdateTitleMessages(MOBMPSecurityUpdatePath.None, true, false, request.MileagePlusNumber, 0).ConfigureAwait(false);
                                response.MPSecurityUpdateDetails.SecurityUpdateMessages = await GetSecurityUpdateTitleMessages(response.MPSecurityUpdateDetails.MPSecurityPath, false, false, request.MileagePlusNumber, 0).ConfigureAwait(false);
                                #endregion
                            }

                            if (response.SecurityUpdate && !response.MPSecurityUpdateDetails.UpdateLaterAllowed)
                            {
                                #region
                                response.MPSecurityUpdateDetails.ForceSignOut = true;  // This is to force sign out when update later is disabled as its time to update the Security Data and will be forced to update data to move forward (As of now here too other than Revenue Booking)
                                #endregion
                            }
                            #region
                            //Version Check for TFA
                            if (isTFAVersion && !response.SecurityUpdate)
                            {
                                //This if condition for after PINPWD phase 1 setup done and enforced to re-enter password with RememberDevice true, then it executes
                                if (request.RememberDevice)
                                {
                                    //Reordering to present the same questions across all channels
                                    await _mileagePlusTFACSL.ShuffleSavedSecurityQuestions(session.Token, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId, request.MileagePlusNumber).ConfigureAwait(false);
                                    response.MPSecurityUpdateDetails.MPSecurityPath = MOBMPSecurityUpdatePath.None;
                                    response.SecurityUpdate = false;
                                    response.RememberMEFlags.RememberMeDeviceSwitchON = request.RememberDevice;
                                    //Saving DeviceID with CSL call, So next time it will validate true for the same device
                                    await _mileagePlusTFACSL.AddDeviceAuthentication(session, request.Application.Version.Major, request.LanguageCode).ConfigureAwait(false);

                                }
                            }

                            #endregion
                        }
                        response.EmployeeId = employeeId;
                        response.DisplayEmployeeId = displayEmployeeId;
                        if (response.EmpTravelTypeResponse != null)
                        {
                            response.EmpTravelTypeResponse.DisplayEmployeeId = response.DisplayEmployeeId;
                        }

                    }
                }

                if (response.AccountValidation != null && response.AccountValidation.ValidPinCode && !string.IsNullOrEmpty(response.AccountValidation.AuthenticatedToken) && !string.IsNullOrEmpty(response.EmployeeId) && !string.IsNullOrEmpty(response.EmployeeId.Trim()))
                {

                    #region Employee

                    {
                        var profileNumber = request.MileagePlusNumber;
                        StaticLog.Information(_logger, "Validate MP Sign In {@employeeID} | {@profileNumber} ", response.EmployeeId, profileNumber);
                    }

                    MOBEmpTravelTypeAndJAProfileResponse empTravelTypesAndJAResponse = new MOBEmpTravelTypeAndJAProfileResponse();

                    if (_configuration.GetValue<bool>("EnableMicroserviceAPIMigration"))
                    {

                        //#region Microservice API
                        United.Mobile.Model.EmployeeReservation.Session empSession = await _shoppingSessionHelper.CreateEmpShoppingSession(session.SessionId, request.MileagePlusNumber, response.EmployeeId, request.TransactionId, session.SessionId).ConfigureAwait(false);

                        string empAuthToken = await _tokenService.GetAnonymousToken(request.Application.Id, request.DeviceId, _configuration).ConfigureAwait(false);

                        empTravelTypesAndJAResponse = await GetEmployeeProfileDetailsMicroservice(response.EmployeeId, request.TransactionId, request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, request.MileagePlusNumber, request.MileagePlusNumber, empAuthToken).ConfigureAwait(false);
                        //#endregion
                    }
                    else

                    if (_configuration.GetValue<bool>("EnableEResAPIMigration"))
                    {
                        #region eRes new API
                        empTravelTypesAndJAResponse = await GetEmployeeProfileDetails(session.Token, response.EmployeeId, request.TransactionId, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false);
                        #endregion
                    }
                    else
                    {
                        MOBEmpTravelTypeAndJAProfileRequest travelTypeJARequest = new MOBEmpTravelTypeAndJAProfileRequest
                        {
                            AccessCode = request.AccessCode,
                            Application = request.Application,
                            DeviceId = request.DeviceId,
                            EmployeeID = response.EmployeeId,
                            LanguageCode = request.LanguageCode,
                            MPNumber = request.MileagePlusNumber,
                            SessionId = session.SessionId,
                            TransactionId = request.TransactionId
                        };


                        if (_configuration.GetValue<bool>("eResMigrationToggle"))
                        {
                            travelTypeJARequest.TokenId = session.Token;
                        }

                        empTravelTypesAndJAResponse = _employeeReservations.GetTravelTypesAndJAProfile(travelTypeJARequest);
                    }

                    if (_configuration.GetValue<bool>("EnableEmp20PassRidersUpdate") && _configuration.GetValue<bool>("GetEmp20PassRidersFromEResService"))
                    {
                        await _sessionHelperService.SaveSession<string>(empTravelTypesAndJAResponse.MOBEmpJAResponse.TransactionId, response.EmployeeId + request.DeviceId, new List<string>() { response.EmployeeId + request.DeviceId, ObjectNames.EresTransactionIDFullName }, ObjectNames.EresTransactionIDFullName).ConfigureAwait(false);
                    }

                    if (empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.Exception == null && empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType != null && empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes.Count > 0)
                    {
                        if (!empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.IsTermsAndConditionsAccepted)
                        {
                            empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.TermsAndConditions = _configuration.GetValue<string>("TnC");
                        }

                        string token = await _tokenService.GetAnonymousToken(request.Application.Id, request.DeviceId, _configuration).ConfigureAwait(false);
                        Service.Presentation.PersonModel.EmployeeTravelProfile empProfile = await _employeeReservations.GetEmployeeProfile(request.Application.Id, request.Application.Version.Major, request.DeviceId, response.EmployeeId, token, session.SessionId).ConfigureAwait(false);

                        if (empProfile != null && empProfile.EligibleTravelers != null && empProfile.EligibleTravelers.Count > 0)
                        {
                            if (_configuration.GetValue<bool>("EnableEResAPIMigration"))
                            {
                                foreach (var t in empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes)
                                {
                                    if (t.TravelType.Equals("E20"))
                                    {
                                        t.NumberOfTravelers = empProfile.EligibleTravelers.Count;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                foreach (var t in empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes)
                                {
                                    if (t.TravelType.Equals("E20"))
                                    {
                                        t.TravelTypeDescription = _configuration.GetValue<string>("Employee20TravelTypeDescription");
                                        t.NumberOfTravelers = empProfile.EligibleTravelers.Count;
                                        break;
                                    }
                                }
                            }
                        }

                        empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.AdvanceBookingDays = _configuration.GetValue<int>("EmpAdvanceBookingDays");

                        United.Mobile.Model.EmployeeReservation.Session empSession = await _shoppingSessionHelper.CreateEmpShoppingSession(session.SessionId, request.MileagePlusNumber, response.EmployeeId, empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EResTransactionId, empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.SessionId).ConfigureAwait(false);
                        empSession.EmployeeProfleEmail = string.Empty;

                        if (_configuration.GetValue<bool>("eResMigrationToggle"))
                        {

                            empSession.TokenId = session.Token;
                            if (!string.IsNullOrEmpty(_configuration.GetValue<string>("RefresheResTokenIfLoggedInTokenExpInThisMinVal")))
                            {
                                empSession.TokenExpireDateTime = session.TokenExpireDateTime;
                                empSession.TokenExpirationValueInSeconds = session.TokenExpirationValueInSeconds;
                            }
                        }

                        empSession.IsPayrollDeduct = empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.IsPayrollDeduct;
                        await _sessionHelperService.SaveSession<United.Mobile.Model.EmployeeReservation.Session>(empSession, empSession.SessionId, new List<string> { empSession.SessionId, empSession.ObjectName }, empSession.ObjectName).ConfigureAwait(false);
                        empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.SessionId = session.SessionId;

                        int jaPassRidersCount = 0;
                        int jaBuddiesCount = 0;
                        int totalCount = 1;
                        MOBEmpJARequest empJARequest = new MOBEmpJARequest();

                        if (empTravelTypesAndJAResponse.MOBEmpJAResponse.Exception == null)
                        {
                            MOBEmpJAResponse empJAResponse = empTravelTypesAndJAResponse.MOBEmpJAResponse;
                            empSession.EmployeeProfleEmail = empJAResponse.EmpProfileExtended.Email;
                            await _sessionHelperService.SaveSession<United.Mobile.Model.EmployeeReservation.Session>(empSession, empSession.SessionId, new List<string> { empSession.SessionId, empSession.ObjectName }, empSession.ObjectName).ConfigureAwait(false);

                            if (empJAResponse != null && empJAResponse.EmpJA != null && empJAResponse.EmpJA.EmpPassRiders != null)
                            {
                                jaPassRidersCount = empJAResponse.EmpJA.EmpPassRiders.Count();
                            }
                            if (empJAResponse != null && empJAResponse.EmpJA != null && empJAResponse.EmpJA.EmpBuddies != null)
                            {
                                jaBuddiesCount = empJAResponse.EmpJA.EmpBuddies.Count();
                            }
                            totalCount = totalCount + jaPassRidersCount + jaBuddiesCount;

                            if (empJAResponse != null && empJAResponse.EmpJA != null && empJAResponse.EmpJA.EmpJAByAirlines != null && empJAResponse.EmpJA.EmpJAByAirlines.Count > 0)
                            {
                                foreach (var empJAByAirline in empJAResponse.EmpJA.EmpJAByAirlines)
                                {
                                    if (!string.IsNullOrEmpty(empJAByAirline.BusinessPassClass) && (empJAByAirline.BusinessPassClass.Equals("PS0B") || empJAByAirline.BusinessPassClass.Equals("PS1B")))
                                    {
                                        empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.AdvanceBookingDays = Convert.ToInt32(_configuration.GetValue<string>("PS0B1BEmpAdvanceBookingDays"));
                                        break;
                                    }
                                }
                            }
                        }

                        if (!_configuration.GetValue<bool>("EnableEResAPIMigration"))
                        {
                            foreach (var t in empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes)
                            {
                                if (!t.TravelType.Equals("E20"))
                                {
                                    t.NumberOfTravelers = totalCount;
                                }
                            }
                        }

                        // MB-2846 Newer Clients Break Corporate Precedent Logic 
                        if (_configuration.GetValue<bool>("EnableCorpEmpYABooking"))
                        {
                            int appId = 1;
                            string appVersion = "";
                            if (request.Application != null)
                            {
                                if (request.Application.Id > 0)
                                {
                                    appId = request.Application.Id;
                                }
                                if (request.Application.Version != null && !string.IsNullOrEmpty(request.Application.Version.Major))
                                {
                                    appVersion = request.Application.Version.Major;
                                }
                            }
                            if (GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "CorpEmpYABookingVersion_Android", "CorpEmpYABookingVersion_iOS", "", "", true, _configuration))
                            {
                                if (response.CorporateEligibleTravelType != null && response.CorporateEligibleTravelType.CorporateTravelTypes != null && response.CorporateEligibleTravelType.CorporateTravelTypes.Count > 0)
                                {
                                    MOBEmpTravelTypeItem CorpEmp = new MOBEmpTravelTypeItem
                                    {
                                        IsAuthorizationRequired = false,
                                        IsEligible = true,
                                        NumberOfTravelers = 1,
                                        TravelTypeDescription = Convert.ToString(_configuration.GetValue<string>("CorporateUILabelDes")),
                                        TravelType = Convert.ToString(_configuration.GetValue<string>("CorporateUILabel"))
                                    };
                                    empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes.Insert(1, CorpEmp);
                                }
                            }
                        }

                        if (_configuration.GetValue<bool>("EnableYoungAdultBooking") && isYoungAdult)
                        {
                            if (empTravelTypesAndJAResponse != null && empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse != null
                                && empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType != null && empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes != null
                                && empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes.Count > 0)
                            {
                                MOBEmpTravelTypeItem YA = new MOBEmpTravelTypeItem
                                {
                                    IsAuthorizationRequired = false,
                                    IsEligible = true,
                                    NumberOfTravelers = 1
                                };
                                if (_configuration.GetValue<bool>("EnableEResAPIMigration"))
                                {
                                    YA.TravelTypeDescription = _configuration.GetValue<string>("TravelTypeYoungAdult");
                                }
                                else
                                {
                                    YA.TravelTypeDescription = _configuration.GetValue<string>("YoungAdultUILabel");
                                }
                                YA.TravelType = "YA";
                                if (_configuration.GetValue<bool>("EnableCorpEmpYABooking"))
                                {
                                    int appId = 1;
                                    string appVersion = "";
                                    if (request.Application != null)
                                    {
                                        if (request.Application.Id > 0)
                                        {
                                            appId = request.Application.Id;
                                        }
                                        if (request.Application.Version != null && !string.IsNullOrEmpty(request.Application.Version.Major))
                                        {
                                            appVersion = request.Application.Version.Major;
                                        }
                                    }
                                    if (GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "CorpEmpYABookingVersion_Android", "CorpEmpYABookingVersion_iOS", "", "", true, _configuration))
                                    {
                                        if (response.CorporateEligibleTravelType != null && response.CorporateEligibleTravelType.CorporateTravelTypes != null && response.CorporateEligibleTravelType.CorporateTravelTypes.Count > 0)
                                        {
                                            empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes.Insert(2, YA);
                                        }
                                        else
                                        {
                                            empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes.Insert(1, YA);
                                        }
                                    }
                                    else
                                    {
                                        empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes.Insert(1, YA);
                                    }
                                }
                                else
                                {
                                    empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes.Insert(1, YA);
                                }
                            }
                        }

                        empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EResTransactionId = string.Empty;
                        response.EmpTravelTypeResponse = empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse;

                        if (response.EmpTravelTypeResponse != null)
                        {
                            response.EmpTravelTypeResponse.DisplayEmployeeId = response.DisplayEmployeeId;
                        }
                        else
                        {
                            response.EmployeeId = string.Empty;
                        }

                        MPPINPWDValidateResponse responsePersist = new MPPINPWDValidateResponse();

                        responsePersist = await _sessionHelperService.GetSession<MPPINPWDValidateResponse>(response.SessionID, responsePersist.ObjectName, new List<string> { response.SessionID, responsePersist.ObjectName }).ConfigureAwait(false);

                        if (responsePersist == null)
                        {
                            responsePersist = new MPPINPWDValidateResponse();
                        }
                        responsePersist.EmpTravelTypeResponse = response.EmpTravelTypeResponse;

                        await _sessionHelperService.SaveSession<MPPINPWDValidateResponse>(responsePersist, _headers.ContextValues.SessionId, new List<string> { _headers.ContextValues.SessionId, responsePersist.ObjectName }, responsePersist.ObjectName).ConfigureAwait(false);
                    }
                    #endregion //Employee

                }
                else
                {

                    if (_configuration.GetValue<bool>("EnableYoungAdultBooking") && isYoungAdult && !EnableYoungAdultCorporate(request.Application.Id, request.Application.Version.Major))
                    {
                        if ((request.MPSignInPath != MOBMPSignInPath.CorporateChangePath) &&
                            (response.CorporateEligibleTravelType == null || !response.CorporateEligibleTravelType.CorporateCustomer))
                        {
                            if (_configuration.GetValue<bool>("EnableEResAPIMigration"))
                            {
                                List<MOBTravelType> lstTravelTypes = new List<MOBTravelType>();
                                MOBYoungAdultTravelType YA = new MOBYoungAdultTravelType();
                                MOBTravelType tType = new MOBTravelType
                                {
                                    TravelType = Convert.ToString(_configuration.GetValue<string>("TravelTypeAwarAndRevenueCode")).Trim(),
                                    TravelDescription = Convert.ToString(_configuration.GetValue<string>("TravelTypeAwarAndRevenue")).Trim()
                                };
                                lstTravelTypes.Add(tType);

                                tType = new MOBTravelType
                                {
                                    TravelType = _configuration.GetValue<string>("TravelTypeYoungAdultCode").Trim(),
                                    TravelDescription = _configuration.GetValue<string>("TravelTypeYoungAdult").Trim()
                                };
                                lstTravelTypes.Add(tType);

                                YA.IsYoungAdultTravel = true;
                                YA.YoungAdultTravelTypes = lstTravelTypes;

                                response.YoungAdultTravelType = YA;
                            }
                            else
                            {
                                MOBYoungAdultTravelType YA = new MOBYoungAdultTravelType();
                                List<MOBTravelType> lstTravelTypes = new List<MOBTravelType>();
                                string[] UILabels = _configuration.GetValue<string>("RevenueAndYABookingUILabels").Split('|');
                                foreach (string s in UILabels)
                                {
                                    MOBTravelType tType = new MOBTravelType
                                    {
                                        TravelType = s.Split('~')[0],
                                        TravelDescription = s.Split('~')[1]
                                    };
                                    lstTravelTypes.Add(tType);
                                }
                                YA.IsYoungAdultTravel = true;
                                YA.YoungAdultTravelTypes = lstTravelTypes;

                                response.YoungAdultTravelType = YA;
                            }
                        }
                    }
                }

                if (EnableYoungAdultCorporate(request.Application.Id, request.Application.Version.Major) && isYoungAdult)
                {
                    if ((request.MPSignInPath != MOBMPSignInPath.CorporateChangePath))
                    {
                        MOBYoungAdultTravelType YA = new MOBYoungAdultTravelType();
                        List<MOBTravelType> lstTravelTypes = new List<MOBTravelType>();
                        string[] UILabels = _configuration.GetValue<string>("RevenueAndYABookingUILabels").Split('|');
                        foreach (string s in UILabels)
                        {
                            MOBTravelType tType = new MOBTravelType
                            {
                                TravelType = s.Split('~')[0],
                                TravelDescription = s.Split('~')[1]
                            };
                            lstTravelTypes.Add(tType);
                        }
                        YA.IsYoungAdultTravel = true;
                        YA.YoungAdultTravelTypes = lstTravelTypes;

                        response.YoungAdultTravelType = YA;
                    }
                }

                #region Locked Account
                bool resultTfaWrongAnswersFlag = false;
                if (response.AccountValidation.AccountLocked)
                {
                    //If account already locked because of TFA validation in previous history, Then response will be with  TFAAccountLocked with required messages for the screen
                    if (isTFAVersion)
                    {
                        resultTfaWrongAnswersFlag = await _mileagePlusTFACSL.GetTfaWrongAnswersFlag(session.SessionId, session.Token, request.CustomerID, request.MileagePlusNumber, false, request.LanguageCode).ConfigureAwait(false);//**==>> Change Provider to return only bool true or false. 
                        if (resultTfaWrongAnswersFlag)
                        {
                            response.MPSecurityUpdateDetails = new MOBMPPINPWDSecurityUpdateDetails
                            {
                                SecurityUpdateMessages = await GetNeedHelpTitleMessages(request.Application.Id == 6 ? "MP_PIN_PWD_TFA_ACCOUNT_SIGNIN_TRY_1_TITLES__WINDOWS_APP" : "MP_PIN_PWD_TFA_ACCOUNT_SIGNIN_TRY_1_TITLES_ALL").ConfigureAwait(false),
                                MPSecurityPath = MOBMPSecurityUpdatePath.UnableToResetOnline
                            };
                            MOBCPProfile mobcpprofile = await _customerProfile.GeteMailIDTFAMPSecurityDetails(request, session.Token).ConfigureAwait(false);
                            if (mobcpprofile != null)
                            {
                                await _sessionHelperService.SaveSession<MOBCPProfile>(mobcpprofile, _headers.ContextValues.SessionId, new List<string> { _headers.ContextValues.SessionId, mobcpprofile.ObjectName }, mobcpprofile.ObjectName).ConfigureAwait(false);
                                response.MPSecurityUpdateDetails.MPSecurityPath = MOBMPSecurityUpdatePath.TFAAccountLocked;
                                response.SecurityUpdate = true;
                            }
                        }
                        else
                        {
                            string exceptionMessage = response.AccountValidation.Message;
                            throw new MOBUnitedException(exceptionMessage);
                        }
                    }
                    else
                    {
                        string exceptionMessage = response.AccountValidation.Message;
                        throw new MOBUnitedException(exceptionMessage);
                    }
                }
                #endregion

                if (response.AccountValidation.ClosedAccount && !resultTfaWrongAnswersFlag)
                {
                    string exceptionMessage = _configuration.GetValue<string>("ErrorContactMileagePlus") ?? "Please contact the MileagePlus Service Center for assistance with your account.";
                    throw new MOBUnitedException(exceptionMessage);
                }
                if (!response.AccountValidation.ValidPinCode && !resultTfaWrongAnswersFlag)
                {
                    string exceptionMessage = _configuration.GetValue<string>("MPValidationErrorMessage") != null ? _configuration.GetValue<string>("MPValidationErrorMessage") : "The account information you entered is incorrect.";
                    throw new MOBUnitedException(exceptionMessage);
                }
                if (request.MPSignInPath == MOBMPSignInPath.RevenueBookingPath)
                {
                    response.ShowContinueAsGuestButton = true;
                }
                #region TFS Bug 88432:Android & iOS: PINPWD2-Sorry something went wrong error when signing in with sUA MP number
                if (request != null && response != null && session != null)
                {
                    await UpdatePersistMPNumberOnlyNumeric(request, response, session).ConfigureAwait(false);
                }
                #endregion

                if (response != null && response.AccountValidation != null && !string.IsNullOrEmpty(response.AccountValidation.AuthenticatedToken))
                {
                    response.AccountValidation.AuthenticatedToken = string.Empty;
                }
                //empHashEmployeeNumber
                //empEmployeeNumberToHash
                //Hashing the employee ID for Stephen Copley's test account for his demo/video that he's making.
                //This account belongs to Gemma Egana and she is aware and agreed to let us use it for this purpose.
                if (!string.IsNullOrEmpty(_configuration.GetValue<string>("empHashEmployeeNumber")))
                {
                    bool blnHashEmployeeID = false;
                    if (_configuration.GetValue<bool>("empHashEmployeeNumber"))
                    {
                        blnHashEmployeeID = true;
                    }
                    if (blnHashEmployeeID)
                    {
                        if (!string.IsNullOrEmpty(_configuration.GetValue<string>("empEmployeeNumberToHash")))
                        {
                            string strEmployeeIDsToHash = _configuration.GetValue<string>("empEmployeeNumberToHash");
                            if (!string.IsNullOrEmpty(response.DisplayEmployeeId) && strEmployeeIDsToHash.Length > 0)
                            {
                                if (strEmployeeIDsToHash.Trim().ToUpper().Contains(response.AccountValidation.MileagePlusNumber.Trim().ToUpper()))
                                {
                                    response.DisplayEmployeeId = "U000000";
                                    response.EmpTravelTypeResponse.DisplayEmployeeId = response.DisplayEmployeeId;
                                }
                            }
                        }
                    }
                }

            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("ValidateMPSigninV2 error {@UnitedException}, {@stackTrack}", uaex.Message, uaex.StackTrace);

                if (uaex != null && !string.IsNullOrEmpty(uaex.Message.Trim()) && !uaex.Message.Trim().Contains("ORA-") && !uaex.Message.Trim().Contains("PL/SQL"))
                {
                    response.Exception = new MOBException
                    {
                        Message = uaex.Message
                    };
                }
                else
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
            }
            catch (System.Exception ex)
            {
                if (session == null || string.IsNullOrEmpty(session.SessionId))
                {
                    session = new Session
                    {
                        SessionId = request.TransactionId + "_" + request.MileagePlusNumber
                    };
                }

                MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);
                _logger.LogError("ValidateMPSigninV2 error {@errormessage} {@mileagePlusNumber}", exceptionWrapper, request?.MileagePlusNumber);

                if (!_configuration.GetValue<bool>("SurfaceErrorToClient"))
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    response.Exception = new MOBException("9999", "SessionId :" + request.SessionID + ex.Message);
                }
            }
            return response;
        }

        public async Task<MOBTFAMPDeviceResponse> ValidateTFASecurityQuestionsV2(MOBTFAMPDeviceRequest request)
        {
            MOBTFAMPDeviceResponse response = new MOBTFAMPDeviceResponse
            {
                RememberMEFlags = new MOBMPTFARememberMeFlags(_configuration)
            };
            MOBCPProfile mpSecurityUpdateDetails = new MOBCPProfile();

            if (_configuration.GetValue<bool>("encryptSession"))
            {
                var encryptKey = _configuration.GetValue<string>("encryptKey");
                request.SessionID = new AesEncryptAndDecrypt(encryptKey).Decrypt(request.SessionID);
            }

            #region
            /// 109410 - PinPWD 2: Sorry something went wrong’ error message is displayed on providing incorrect answers for two security question
            /// FIX: Saving sessionid in persist and loading, if client not passing updated session id
            /// That is happening in Android, IOS is sending updated sessionid but not android
            /// Srini - 12/082017
            if (_configuration.GetValue<bool>("BugFixToggleFor17M"))
            {
                var tempQuestionsFromPersist = await _sessionHelperService.GetSession<MPPINPWSecurityQuestionsValidation>(request.SessionID, new MPPINPWSecurityQuestionsValidation().ObjectName, new List<string> { request.SessionID, new MPPINPWSecurityQuestionsValidation().ObjectName }).ConfigureAwait(false);
                if (tempQuestionsFromPersist == null)
                {
                    MOBMPPINPWDValidateResponse mobMPPINPWDValidateResponse = new MOBMPPINPWDValidateResponse();
                    mobMPPINPWDValidateResponse = await _sessionHelperService.GetSession<MOBMPPINPWDValidateResponse>(request.SessionID, mobMPPINPWDValidateResponse.ObjectName, new List<string> { request.SessionID, mobMPPINPWDValidateResponse.ObjectName }).ConfigureAwait(false);
                    request.SessionID = mobMPPINPWDValidateResponse?.SessionID;
                }
            }

            if (request != null && !string.IsNullOrEmpty(request.MileagePlusNumber))
            {
                request.MileagePlusNumber = request.MileagePlusNumber.Trim().ToUpper();
            }
            response.MileagePlusNumber = request.MileagePlusNumber;
            response.SessionID = request.SessionID;

            Session session = null;
            if (!string.IsNullOrEmpty(request.SessionID))
            {
                session = await _shoppingSessionHelper.GetShoppingSession(request.SessionID).ConfigureAwait(false);
            }
            else
            {
                _logger.LogInformation("ValidateTFASecurityQuestionsV2 - {@tFAMPDeviceSecurityPath} Session file not found.", request.tFAMPDeviceSecurityPath.ToString());
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
            var mpSignIn = await _sessionHelperService.GetSession<MPSignIn>(request.SessionID, new MPSignIn().ObjectName, new List<string>() { request.SessionID, new MPSignIn().ObjectName }).ConfigureAwait(false);
            //TFA change
            var mobmppinpwdvalidaterequest = await _sessionHelperService.GetSession<MOBMPPINPWDValidateRequest>(request.SessionID, new MOBMPPINPWDValidateRequest().ObjectName, new List<string> { request.SessionID, new MOBMPPINPWDValidateRequest().ObjectName }).ConfigureAwait(false);

            mobmppinpwdvalidaterequest.CustomerID = mpSignIn.CustomerId;
            response.CustomerID = mpSignIn.CustomerId;

            if (mobmppinpwdvalidaterequest != null && mobmppinpwdvalidaterequest.MPSignInPath == MOBMPSignInPath.RevenueBookingPath)
            {
                response.ShowContinueAsGuestButton = true;
            }

            request.TransactionId = session.SessionId;
            request.SessionID = session.SessionId;
            response.SessionID = session.SessionId;
            response.tFAMPDeviceSecurityPath = new MOBMPSecurityUpdatePath();

            bool deviceIdAlphaNumericMPNumAppIdCheck = false, deviceIdNumericMPNumAppIdCheck = false;

            if (session.AppID == request.Application.Id && session.DeviceID.ToUpper().Trim() == request.DeviceId.ToUpper().Trim()
                && mobmppinpwdvalidaterequest.MileagePlusNumber.ToUpper().Trim() == request.MileagePlusNumber.ToUpper().Trim())
            {
                deviceIdAlphaNumericMPNumAppIdCheck = true;
            }
            else if (_configuration.GetValue<bool>("TFAMPNumericAndAlphaNumericValidation") == true)
            {
                bool isNumericMPNum = long.TryParse(mobmppinpwdvalidaterequest.MileagePlusNumber, out _);

                if (isNumericMPNum && session.AppID == request.Application.Id && session.DeviceID.ToUpper().Trim() == request.DeviceId.ToUpper().Trim()
                    && session.CustomerID == request.CustomerID)
                {
                    deviceIdNumericMPNumAppIdCheck = true;
                }
            }
            if (deviceIdAlphaNumericMPNumAppIdCheck == true || deviceIdNumericMPNumAppIdCheck == true)
            {
                #region
                session.MileagPlusNumber = mobmppinpwdvalidaterequest.MileagePlusNumber.ToUpper();
                bool answeredAllCorrect = true;

                //Getting RetryCount count from session for validating not more than 2 times submit
                var questionsFromPersist = await _sessionHelperService.GetSession<MPPINPWSecurityQuestionsValidation>(request.SessionID, new MPPINPWSecurityQuestionsValidation().ObjectName, new List<string> { request.SessionID, new MPPINPWSecurityQuestionsValidation().ObjectName }).ConfigureAwait(false);

                if (questionsFromPersist != null)
                {
                    questionsFromPersist.RetryCount = questionsFromPersist.RetryCount + 1;
                }

                //Validating answered questions with CSL call ang getting the right answered count
                int correctlyAnsweredQuestions = 0;
                foreach (var question in request.AnsweredSecurityQuestions)
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

                if (!answeredAllCorrect)
                {
                    #region
                    //if One Question Right and Another One Wrong at first time, then resending the 3'rd question
                    if (questionsFromPersist.RetryCount < 2 && correctlyAnsweredQuestions > 0)
                    {
                        var questionToBeSent = new List<Securityquestion>();
                        questionToBeSent = questionsFromPersist.SecurityQuestionsFromCSL.Except(questionsFromPersist.SecurityQuestionsSentToClient, new SecurityquestionEqualityComparer()).ToList();
                        response.SecurityQuestions = questionToBeSent.Take(1).ToList();
                        questionsFromPersist.SecurityQuestionsSentToClient = response.SecurityQuestions;

                        await _sessionHelperService.SaveSession<MPPINPWSecurityQuestionsValidation>(questionsFromPersist, _headers.ContextValues.SessionId, new List<string> { _headers.ContextValues.SessionId, questionsFromPersist.ObjectName }, questionsFromPersist.ObjectName).ConfigureAwait(false);
                        response.tFAMPDeviceMessages = await GetNeedHelpTitleMessages(request.Application.Id == 6 ? "MP_PIN_PWD_TFA_SECURITY_QUESTIONS_TRY_2_TITLES_WINDOWS_APP" : "MP_PIN_PWD_TFA_SECURITY_QUESTIONS_TRY_2_TITLES_ALL").ConfigureAwait(false);
                        response.tFAMPDeviceSecurityPath = MOBMPSecurityUpdatePath.ValidateSecurityQuestions;
                        response.SecurityUpdate = true;
                        response.RememberMEFlags.RememberMeDeviceSwitchON = request.RememberDevice;

                        if (mobmppinpwdvalidaterequest != null && mobmppinpwdvalidaterequest.MPSignInPath != MOBMPSignInPath.RevenueBookingPath)
                        {
                            MOBItem mobitem = response.tFAMPDeviceMessages.FirstOrDefault(p => p.CurrentValue.ToUpper() == "CONTINUE WITH SIGN IN");
                            if (mobitem != null)
                            {
                                mobitem.CurrentValue = "Continue";
                            }
                        }
                    }
                    else //If two questions were wrong in either first attempt or total with 2'nd attempt, Will lock the account and send an email with ResetAccount link
                    {
                        //Reordering to present the same questions across all channels
                        await _mileagePlusTFACSL.ShuffleSavedSecurityQuestions(session.Token, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId, request.MileagePlusNumber).ConfigureAwait(false);
                        response.tFAMPDeviceSecurityPath = MOBMPSecurityUpdatePath.IncorrectSecurityQuestion;
                        response.tFAMPDeviceMessages = await GetNeedHelpTitleMessages(request.Application.Id == 6 ? "MP_PIN_PWD_TFA_SECURITY_QUESTIONS_TRY_3_TITLES__WINDOWS_APP" : "MP_PIN_PWD_TFA_SECURITY_QUESTIONS_TRY_3_TITLES_ALL").ConfigureAwait(false);
                        //Removing Button from the messages as per Client request for TFA, if it is not Revenue booking
                        if (mobmppinpwdvalidaterequest != null && mobmppinpwdvalidaterequest.MPSignInPath != MOBMPSignInPath.RevenueBookingPath)
                        {
                            MOBItem mobitem = response.tFAMPDeviceMessages.FirstOrDefault(p => p.Id.ToUpper() == "BUTTON");
                            if (mobitem != null)
                            {
                                response.tFAMPDeviceMessages.Remove(mobitem);
                            }
                        }
                        //lock the account
                        await _securityQuestion.LockCustomerAccount(0, request.MileagePlusNumber, session.Token, request.LanguageCode, request.SessionID, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false);

                        var mobmppinpwdvalidateresponse = await _sessionHelperService.GetSession<MPPINPWDValidateResponse>(request.SessionID, new MPPINPWDValidateResponse().ObjectName, new List<string>() { request.SessionID, new MPPINPWDValidateResponse().ObjectName }).ConfigureAwait(false);

                        #region getting PrimaryEmail address
                        string strEmail = string.Empty;
                        if (mpSignIn.IsSignInWithTouchID == true)
                        {
                            mpSecurityUpdateDetails = await GetMPSecurityCheckDetails(mobmppinpwdvalidaterequest, session.Token, false).ConfigureAwait(false);//response.AccountValidation.AuthenticatedToken);
                            strEmail = GetProfileEmailAddress(mpSecurityUpdateDetails);
                        }
                        else if (mobmppinpwdvalidateresponse != null)
                        {
                            strEmail = GetProfileEmailAddress(mobmppinpwdvalidateresponse.Profile);
                        }
                        #endregion


                        if (!string.IsNullOrEmpty(strEmail))
                        {
                            //Send email for reset account
                            await _mileagePlusTFACSL.SendResetAccountEmail(session.SessionId, session.Token, session.CustomerID, request.MileagePlusNumber.ToUpper(), strEmail, request.LanguageCode).ConfigureAwait(false);

                            string maskedEmail = MaskEmailAddress(strEmail) + ".";
                            response.tFAMPDeviceMessages.Add(new MOBItem() { CurrentValue = maskedEmail, Id = "Body3", SaveToPersist = false });
                        }
                        await UpdateTfaWrongAnswersFlag(session.SessionId, session.Token, session.CustomerID, request.MileagePlusNumber.ToUpper(), true, request.LanguageCode).ConfigureAwait(false);
                        response.SecurityUpdate = true;
                    }

                    if (_configuration.GetValue<bool>("encryptSession") && response.SecurityUpdate)
                    {
                        var encryptKey = _configuration.GetValue<string>("encryptKey");
                        response.SessionID = new AesEncryptAndDecrypt(encryptKey).Encrypt(response.SessionID);
                    }
                    #endregion
                }
                else if (answeredAllCorrect) // If all are correct passing SecurityUpdate = false, So client will forward with there next ation
                {
                    //Reordering to present the same questions across all channels
                    await _mileagePlusTFACSL.ShuffleSavedSecurityQuestions(session.Token, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId, request.MileagePlusNumber).ConfigureAwait(false);
                    response.tFAMPDeviceSecurityPath = MOBMPSecurityUpdatePath.None;
                    response.SecurityUpdate = false;
                    response.RememberMEFlags.RememberMeDeviceSwitchON = request.RememberDevice;
                    //Saving DeviceID with CSL call, So next time it will validate true for the same device
                    if (request.RememberDevice)
                    {
                        await _mileagePlusTFACSL.AddDeviceAuthentication(session, request.Application.Version.Major, request.LanguageCode).ConfigureAwait(false);
                    }

                    response.HashValue = mpSignIn.MPHashValue;
                    //Get the Hash from cache(ValidateMPSigninV2) instead of DB
                    //Assign the AccoutSummary to the response
                    //var mpSignIn = United.Persist.FilePersist.Load<United.Persist.Definition.Profile.MPSignIn>(request.SessionID, (new United.Persist.Definition.Profile.MPSignIn()).GetType().FullName);

                    #region corporateEligibleTravelType
                    if (mpSignIn.IsSignInWithTouchID == true)
                    {
                        bool getEmployeeIdFromCSLCustomerData = _configuration.GetValue<bool>("GetEmployeeIDFromGetProfileCustomerData");
                        mpSecurityUpdateDetails = await GetMPSecurityCheckDetails(mobmppinpwdvalidaterequest, session.Token, getEmployeeIdFromCSLCustomerData).ConfigureAwait(false);//response.AccountValidation.AuthenticatedToken);
                    }
                    else
                    {
                        mpSecurityUpdateDetails = mpSignIn.Profile;
                    }
                    MOBMPProfileCorporateResponse corporateResponse = new MOBMPProfileCorporateResponse();
                    corporateResponse = await GetMPProfileCorporateDetailsV2(request, mobmppinpwdvalidaterequest, mpSecurityUpdateDetails, session).ConfigureAwait(false);
                    response.CorporateEligibleTravelType = corporateResponse.CorporateEligibleTravelType;
                    response.CustomerMetrics = corporateResponse.CustomerMetrics;
                    response.EmployeeId = corporateResponse.EmployeeId;
                    if (mpSecurityUpdateDetails != null && mpSecurityUpdateDetails.Travelers != null && mpSecurityUpdateDetails.Travelers.Any())
                    {
                        response.KtnNumber = mpSecurityUpdateDetails.Travelers[0].KnownTravelerNumber;
                    }
                    #endregion

                    if (CheckAppIDToReturnAccountSummary(request.Application.Id)) // TO DO Need to work Adding entries at web.config to allow only for specific Application IDS. For now only return for Android
                    {
                        response.OPAccountSummary = await _mileagePlus.GetAccountSummary(request.TransactionId, request.MileagePlusNumber, request.LanguageCode, true, session.SessionId).ConfigureAwait(false);
                        response.OPAccountSummary.HashValue = mpSignIn.MPHashValue;

                        #region Subscriptions
                        //*** config key - "EnableUASubscriptions" did not found in entire solution config files hence commented it. it will be removed during the code review
                        if (_configuration.GetValue<bool>("EnableUASubscriptions"))
                        {
                            MOBUASubscriptions subscriptions = new MOBUASubscriptions();
                            subscriptions = await GetUASubscriptions(request).ConfigureAwait(false);
                            response.UASubscriptions = subscriptions;
                            if (subscriptions != null && subscriptions.SubscriptionTypes.Count > 0)
                            {
                                response.IsUASubscriptionsAvailable = true;
                            }
                        }
                        #endregion
                        #region StatusLiftBanner
                        MOBMPAccountValidationRequest requestSLO = new MOBMPAccountValidationRequest()
                        {
                            MileagePlusNumber = request.MileagePlusNumber,
                            TransactionId = request.TransactionId,
                            DeviceId = request.DeviceId,
                            Application = new MOBApplication()
                            {
                                Id = request.Application.Id,
                                Version = new MOBVersion()
                                {
                                    Major = request.Application.Version.Major,
                                    Build = request.Application.Version.Major
                                }
                            }
                        };
                        //Promotion endpoint not available
                        //response.OPAccountSummary.statusLiftBanner = await _utility.GetStatusLiftBanner(requestSLO).ConfigureAwait(false);
                        #endregion
                        #region Mp Expiration date 
                        if (!_configuration.GetValue<bool>("Keep_MREST_MP_EliteLevel_Expiration_Logic") && mpSecurityUpdateDetails != null && mpSecurityUpdateDetails.Travelers != null && mpSecurityUpdateDetails.Travelers.Any() && mpSecurityUpdateDetails.Travelers[0].MileagePlus != null)
                        {
                            // This method is to get new changes from profile service for expiration date.
                            // New parameters in profile service PremierLevelExpirationDate, InstantElite gives info on account(Trail) and expiration date
                            if (!_configuration.GetValue<bool>("EnableUCBPhase1_MobilePhase3Changes"))
                            {
                                _mileagePlus.GetMPEliteLevelExpirationDateAndGenerateBarCode(response.OPAccountSummary, mpSecurityUpdateDetails.Travelers[0].MileagePlus.PremierLevelExpirationDate, mpSecurityUpdateDetails.Travelers[0].MileagePlus.InstantElite);
                            }
                        }
                        #endregion

                        #region OPAccountSummary.IsMPAccountTSAFlagON
                        if (mpSecurityUpdateDetails != null)
                        {
                            foreach (MOBCPTraveler traveler in mpSecurityUpdateDetails.Travelers)
                            {
                                if (traveler.MileagePlus != null && traveler.MileagePlus.MileagePlusId.ToUpper().Trim().Equals(request.MileagePlusNumber.ToUpper().Trim()))
                                {
                                    response.OPAccountSummary.IsMPAccountTSAFlagON = traveler.IsTSAFlagON;
                                    break;
                                }
                            }
                        }
                        #endregion
                    }

                    #region Get Employee details
                    if (!string.IsNullOrEmpty(response.EmployeeId))
                    {
                        response.EmpTravelTypeResponse = await GetEmployeeDetailsV2(request, response.EmployeeId.Trim(), corporateResponse.IsYoungAdult, response.CorporateEligibleTravelType, session).ConfigureAwait(false);

                        if (_configuration.GetValue<bool>("EnableEmp20PassRidersUpdate") && _configuration.GetValue<bool>("GetEmp20PassRidersFromEResService"))
                        {
                            if (response.EmpTravelTypeResponse?.TransactionId != null)
                            {
                                await _sessionHelperService.SaveSession<string>(response.EmpTravelTypeResponse.TransactionId, response.EmployeeId + request.DeviceId, new List<string> { response.EmployeeId + request.DeviceId, ObjectNames.EresTransactionIDFullName }, ObjectNames.EresTransactionIDFullName).ConfigureAwait(false);
                            }
                        }
                    }
                    #endregion

                    #region Young Adult
                    //MOBYoungAdultTravelType
                    if (corporateResponse.IsYoungAdult && _customerProfile.EnableYoungAdult())
                    {
                        if (mobmppinpwdvalidaterequest != null && mobmppinpwdvalidaterequest.MPSignInPath != MOBMPSignInPath.CorporateChangePath)
                        {
                            response.YoungAdultTravelType = GetYoungAdultDetails(request);
                        }
                    }
                    #endregion
                    if (mpSignIn.IsSignInWithTouchID == true)
                    {
                        await InsertUpdateMPCSSValidationDetails(request.MileagePlusNumber, request.MileagePlusNumber, request.DeviceId, request.Application.Id, request.Application.Version.Major,
                                                            mpSignIn.AuthToken, true, mpSignIn.TokenExpirationDateTime, mpSignIn.TokenExpirationSeconds, mpSignIn.MPHashValue, string.Empty, true, false, mpSignIn.CustomerId).ConfigureAwait(false);
                    }
                    else
                    {
                        await InsertMileagePlusAndHash(request.Application.Id, request.DeviceId, request.Application.Version.Major, request.TransactionId,
                                                                        mpSignIn.AuthToken, request.MileagePlusNumber, string.Empty, mpSignIn.MPHashValue,
                                                                        mpSignIn.TokenExpirationDateTime, mpSignIn.TokenExpirationSeconds, mpSignIn.CustomerId, false, session.SessionId, request, "ValidateTFASecurityQuestionsV2").ConfigureAwait(false);
                    }
                }

                #endregion
            }
            else
            {
                _logger.LogInformation("ValidateTFASecurityQuestionsV2 {@TFAMPDeviceSecurityPath} ", "ValidateTFASecurityQuestionsException - " + request.tFAMPDeviceSecurityPath.ToString());
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
            }
            #endregion
            return response;
        }

        #region "ValidateTFASecurityQuestionsV2 - Methods"
        private MOBYoungAdultTravelType GetYoungAdultDetails(MOBTFAMPDeviceRequest request)
        {
            List<MOBTravelType> lstTravelTypes = new List<MOBTravelType>();
            MOBYoungAdultTravelType youngAdult = new MOBYoungAdultTravelType();

            if (_configuration.GetValue<bool>("EnableEResAPIMigration"))
            {
                MOBTravelType tType = new MOBTravelType
                {
                    TravelType = _configuration.GetValue<string>("TravelTypeAwarAndRevenueCode").Trim(),
                    TravelDescription = _configuration.GetValue<string>("TravelTypeAwarAndRevenue").Trim()
                };
                lstTravelTypes.Add(tType);

                tType = new MOBTravelType
                {
                    TravelType = _configuration.GetValue<string>("TravelTypeYoungAdultCode").Trim(),
                    TravelDescription = _configuration.GetValue<string>("TravelTypeYoungAdult").Trim()
                };
                lstTravelTypes.Add(tType);

                youngAdult.IsYoungAdultTravel = true;
                youngAdult.YoungAdultTravelTypes = lstTravelTypes;

                return youngAdult;
            }
            else
            {
                string[] UILabels = _configuration.GetValue<string>("RevenueAndYABookingUILabels").Split('|');
                foreach (string s in UILabels)
                {
                    MOBTravelType tType = new MOBTravelType
                    {
                        TravelType = s.Split('~')[0],
                        TravelDescription = s.Split('~')[1]
                    };
                    lstTravelTypes.Add(tType);
                }
                youngAdult.IsYoungAdultTravel = true;
                youngAdult.YoungAdultTravelTypes = lstTravelTypes;

                return youngAdult;
            }
        }

        private async Task<MOBEmpTravelTypeResponse> GetEmployeeDetailsV2(MOBTFAMPDeviceRequest request, string employeeId, bool isYoungAdult, MOBCorporateTravelType corporateEligibleTravelType, Session session)
        {
            #region Employee
            StaticLog.Information(_logger, "Validate MP Sign In EmployeeID | MP Number {@employeeID} | {@profileNumber}", employeeId, request.MileagePlusNumber);
            MOBEmpTravelTypeResponse empTravelTypeResponse = new MOBEmpTravelTypeResponse();
            MOBEmpTravelTypeAndJAProfileResponse empTravelTypesAndJAResponse = new MOBEmpTravelTypeAndJAProfileResponse();

            if (_configuration.GetValue<bool>("EnableMicroserviceAPIMigration"))
            {
                #region Microservice API
                United.Mobile.Model.EmployeeReservation.Session empSession = await _shoppingSessionHelper.CreateEmpShoppingSession(session.SessionId, request.MileagePlusNumber, employeeId, request.TransactionId, session.SessionId).ConfigureAwait(false);

                empTravelTypesAndJAResponse = await GetEmployeeProfileDetailsMicroservice(employeeId, request.TransactionId, request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, request.MileagePlusNumber, request.MileagePlusNumber, string.Empty).ConfigureAwait(false);
                #endregion
            }
            else
            if (_configuration.GetValue<bool>("EnableEResAPIMigration"))
            {
                #region eRes new API

                empTravelTypesAndJAResponse = await GetEmployeeProfileDetails(session.Token, employeeId, request.TransactionId, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false);
                #endregion
            }

            else
            {
                var travelTypeJARequest = new MOBEmpTravelTypeAndJAProfileRequest
                {
                    AccessCode = request.AccessCode,
                    Application = request.Application,
                    DeviceId = request.DeviceId,
                    EmployeeID = employeeId,
                    LanguageCode = request.LanguageCode,
                    MPNumber = request.MileagePlusNumber,
                    SessionId = session.SessionId,
                    TransactionId = request.TransactionId
                };
                if (_configuration.GetValue<bool>("eResMigrationToggle"))
                {
                    travelTypeJARequest.TokenId = session.Token;
                }

                empTravelTypesAndJAResponse = _employeeReservations.GetTravelTypesAndJAProfile(travelTypeJARequest);

            }

            if (empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.Exception == null && empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType != null && empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes.Count > 0)
            {
                if (!empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.IsTermsAndConditionsAccepted)
                {
                    empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.TermsAndConditions = _configuration.GetValue<string>("TnC");
                }

                Service.Presentation.PersonModel.EmployeeTravelProfile empProfile = await _employeeReservations.GetEmployeeProfile(request.Application.Id, request.Application.Version.Major, request.DeviceId, employeeId, session.Token, session.SessionId).ConfigureAwait(false);

                if (empProfile != null && empProfile.EligibleTravelers != null && empProfile.EligibleTravelers.Count > 0)
                {
                    foreach (var t in empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes)
                    {
                        if (t.TravelType.Equals("E20"))
                        {
                            t.TravelTypeDescription = _configuration.GetValue<string>("Employee20TravelTypeDescription");
                            t.NumberOfTravelers = empProfile.EligibleTravelers.Count;
                            break;
                        }
                    }
                }

                empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.AdvanceBookingDays = _configuration.GetValue<int>("EmpAdvanceBookingDays");

                United.Mobile.Model.EmployeeReservation.Session empSession = await _shoppingSessionHelper.CreateEmpShoppingSession(session.SessionId, request.MileagePlusNumber, employeeId, empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EResTransactionId, empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.SessionId).ConfigureAwait(false);
                empSession.EmployeeProfleEmail = string.Empty;

                if (_configuration.GetValue<bool>("eResMigrationToggle"))
                {
                    empSession.TokenId = session.Token;
                    if (_configuration.GetValue<string>("RefresheResTokenIfLoggedInTokenExpInThisMinVal") != null)
                    {
                        empSession.TokenExpireDateTime = session.TokenExpireDateTime;
                        empSession.TokenExpirationValueInSeconds = session.TokenExpirationValueInSeconds;
                    }
                }

                empSession.IsPayrollDeduct = empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.IsPayrollDeduct;
                await _sessionHelperService.SaveSession<United.Mobile.Model.EmployeeReservation.Session>(empSession, empSession.SessionId, new List<string> { empSession.SessionId, empSession.ObjectName }, empSession.ObjectName).ConfigureAwait(false);
                empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.SessionId = session.SessionId;

                int jaPassRidersCount = 0;
                int jaBuddiesCount = 0;
                int totalCount = 1;
                MOBEmpJARequest empJARequest = new MOBEmpJARequest();
                try
                {
                    if (empTravelTypesAndJAResponse.MOBEmpJAResponse.Exception == null)
                    {
                        MOBEmpJAResponse empJAResponse = empTravelTypesAndJAResponse.MOBEmpJAResponse;
                        empSession.EmployeeProfleEmail = empJAResponse.EmpProfileExtended.Email;
                        await _sessionHelperService.SaveSession<United.Mobile.Model.EmployeeReservation.Session>(empSession, empSession.SessionId, new List<string> { empSession.SessionId, empSession.ObjectName }, empSession.ObjectName).ConfigureAwait(false);

                        if (empJAResponse != null && empJAResponse.EmpJA != null && empJAResponse.EmpJA.EmpPassRiders != null)
                        {
                            jaPassRidersCount = empJAResponse.EmpJA.EmpPassRiders.Count();
                        }
                        if (empJAResponse != null && empJAResponse.EmpJA != null && empJAResponse.EmpJA.EmpBuddies != null)
                        {
                            jaBuddiesCount = empJAResponse.EmpJA.EmpBuddies.Count();
                        }
                        totalCount = totalCount + jaPassRidersCount + jaBuddiesCount;

                        if (empJAResponse != null && empJAResponse.EmpJA != null && empJAResponse.EmpJA.EmpJAByAirlines != null && empJAResponse.EmpJA.EmpJAByAirlines.Count > 0)
                        {
                            foreach (var empJAByAirline in empJAResponse.EmpJA.EmpJAByAirlines)
                            {
                                if (!string.IsNullOrEmpty(empJAByAirline.BusinessPassClass) && (empJAByAirline.BusinessPassClass.Equals("PS0B") || empJAByAirline.BusinessPassClass.Equals("PS1B")))
                                {
                                    empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.AdvanceBookingDays = _configuration.GetValue<int>("PS0B1BEmpAdvanceBookingDays");
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (System.Exception ex) { Console.Write(ex.Message); }
                if (!_configuration.GetValue<bool>("EnableEResAPIMigration"))
                {
                    foreach (var t in empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes)
                    {
                        if (!t.TravelType.Equals("E20"))
                        {
                            t.NumberOfTravelers = totalCount;
                        }
                    }
                }

                // MB-2846 Newer Clients Break Corporate Precedent Logic 
                if (_configuration.GetValue<bool>("EnableCorpEmpYABooking"))
                {
                    int appId = 1;
                    string appVersion = "";
                    if (request.Application != null)
                    {
                        if (request.Application.Id > 0)
                        {
                            appId = request.Application.Id;
                        }
                        if (request.Application.Version != null && !string.IsNullOrEmpty(request.Application.Version.Major))
                        {
                            appVersion = request.Application.Version.Major;
                        }
                    }
                    if (GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "CorpEmpYABookingVersion_Android", "CorpEmpYABookingVersion_iOS", "", "", true, _configuration))
                    {
                        if (corporateEligibleTravelType != null && corporateEligibleTravelType.CorporateTravelTypes != null && corporateEligibleTravelType.CorporateTravelTypes.Count > 0)
                        {
                            MOBEmpTravelTypeItem CorpEmp = new MOBEmpTravelTypeItem
                            {
                                IsAuthorizationRequired = false,
                                IsEligible = true,
                                NumberOfTravelers = 1,
                                TravelTypeDescription = _configuration.GetValue<string>("CorporateUILabelDes"),
                                TravelType = _configuration.GetValue<string>("CorporateUILabel")
                            };
                            empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes.Insert(1, CorpEmp);
                        }
                    }
                }

                if (_customerProfile.EnableYoungAdult() && isYoungAdult)
                {
                    if (empTravelTypesAndJAResponse != null && empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse != null
                        && empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType != null && empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes != null
                        && empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes.Count > 0)
                    {
                        MOBEmpTravelTypeItem YA = new MOBEmpTravelTypeItem
                        {
                            IsAuthorizationRequired = false,
                            IsEligible = true,
                            NumberOfTravelers = 1,
                            TravelTypeDescription = _configuration.GetValue<string>("YoungAdultUILabel"),
                            TravelType = "YA"
                        };
                        if (_configuration.GetValue<bool>("EnableCorpEmpYABooking"))
                        {
                            int appId = 1;
                            string appVersion = "";
                            if (request.Application != null)
                            {
                                if (request.Application.Id > 0)
                                {
                                    appId = request.Application.Id;
                                }
                                if (request.Application.Version != null && !string.IsNullOrEmpty(request.Application.Version.Major))
                                {
                                    appVersion = request.Application.Version.Major;
                                }
                            }
                            if (GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "CorpEmpYABookingVersion_Android", "CorpEmpYABookingVersion_iOS", "", "", true, _configuration))
                            {
                                if (corporateEligibleTravelType != null && corporateEligibleTravelType.CorporateTravelTypes != null && corporateEligibleTravelType.CorporateTravelTypes.Count > 0)
                                {
                                    empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes.Insert(2, YA);
                                }
                                else
                                {
                                    empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes.Insert(1, YA);
                                }
                            }
                            else
                            {
                                empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes.Insert(1, YA);
                            }
                        }
                        else
                        {
                            empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType.EmpTravelTypes.Insert(1, YA);
                        }
                    }
                }

                empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EResTransactionId = string.Empty;
                empTravelTypeResponse = empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse;
                if (empTravelTypeResponse != null)
                {
                    empTravelTypeResponse.DisplayEmployeeId = employeeId;

                    if (_configuration.GetValue<bool>("EnableEmp20PassRidersUpdate") && _configuration.GetValue<bool>("GetEmp20PassRidersFromEResService"))
                    {
                        empTravelTypeResponse.TransactionId = empTravelTypesAndJAResponse?.MOBEmpJAResponse?.TransactionId;
                    }
                }

                EmpployeeTravelTypeResponse responsePersist = new EmpployeeTravelTypeResponse();
                responsePersist = await _sessionHelperService.GetSession<EmpployeeTravelTypeResponse>(request.SessionID, responsePersist.ObjectName, new List<string>() { request.SessionID, responsePersist.ObjectName }).ConfigureAwait(false);
                if (responsePersist == null)
                {
                    responsePersist = new EmpployeeTravelTypeResponse();
                }
                responsePersist.EmpTravelTypeResponse = empTravelTypeResponse;
                await _sessionHelperService.SaveSession<EmpployeeTravelTypeResponse>(responsePersist, session.SessionId, new List<string> { session.SessionId, responsePersist.ObjectName }, responsePersist.ObjectName).ConfigureAwait(false);
            }
            #endregion //Employee
            return empTravelTypeResponse;
        }

        private async Task<MOBMPProfileCorporateResponse> GetMPProfileCorporateDetailsV2(MOBTFAMPDeviceRequest request, MOBMPPINPWDValidateRequest pinpwdRequest, MOBCPProfile mpSecurityUpdateDetails, Session session)
        {
            bool getEmployeeIdFromCSLCustomerData = _configuration.GetValue<bool>("GetEmployeeIDFromGetProfileCustomerData");
            //MOBCPProfile mpSecurityUpdateDetails = null;
            string employeeId = string.Empty;
            MOBMPProfileCorporateResponse response = new MOBMPProfileCorporateResponse();
            //Can we avoid this call and get it from persist
            //mpSecurityUpdateDetails = profileCSL.GetMPSecurityCheckDetails(pinpwdRequest, session.Token, getEmployeeIdFromCSLCustomerData);
            //response.mpSecurityUpdateDetails = mpSecurityUpdateDetails;
            bool isYoungAdult = false;
            if (_customerProfile.EnableYoungAdult())
            {
                if (mpSecurityUpdateDetails != null && mpSecurityUpdateDetails.Travelers != null && mpSecurityUpdateDetails.Travelers.Count > 0)
                {
                    if (mpSecurityUpdateDetails.Travelers.Exists(t => t.IsProfileOwner))
                    {
                        isYoungAdult = TopHelper.IsYoungAdult(mpSecurityUpdateDetails.Travelers.First(t => t.IsProfileOwner).BirthDate);
                    }
                }
            }

            if (getEmployeeIdFromCSLCustomerData &&
                mpSecurityUpdateDetails != null &&
                mpSecurityUpdateDetails.Travelers != null &&
                mpSecurityUpdateDetails.Travelers.Count > 0)
            {
                response.EmployeeId = mpSecurityUpdateDetails.Travelers[0].EmployeeId;
            }
            response.IsYoungAdult = isYoungAdult;
            #region Corporate Booking
            bool isCorporateBooking = _configuration.GetValue<bool>("CorporateConcurBooking");
            bool isAwardCalendarMP2017 = _configuration.GetValue<bool>("NGRPAwardCalendarMP2017Switch");
            MOBMPPINPWDValidateResponse pinpwdResponse = new MOBMPPINPWDValidateResponse();

            await ValidateCorporateBooking(pinpwdRequest, isCorporateBooking, mpSecurityUpdateDetails, pinpwdResponse, isYoungAdult, session).ConfigureAwait(false);

            response.CorporateEligibleTravelType = pinpwdResponse.CorporateEligibleTravelType;
            response.Exception = pinpwdResponse.Exception;
            response.ResponseStatusItem = pinpwdResponse.ResponseStatusItem;
            #endregion
            #region Customer Metrics
            response.CustomerMetrics = AssignCustomerMetrics(isAwardCalendarMP2017, mpSecurityUpdateDetails);
            #endregion

            return response;
        }
        private async Task<SaveResponse> UpdateTfaWrongAnswersFlag(string sessionid, string token, int customerId, string mileagePlusNumber, bool answeredQuestionsIncorrectly, string languageCode)
        {
            if (string.IsNullOrEmpty(mileagePlusNumber))
            {
                throw new MOBUnitedException("MPNumber request cannot be null.");
            }
            United.Services.Customer.Common.UpdateWrongAnswersFlagRequest updatewronganswersflagrequest = new UpdateWrongAnswersFlagRequest
            {
                AnsweredQuestionsIncorrectly = answeredQuestionsIncorrectly,
                CustomerId = customerId,
                LoyaltyId = mileagePlusNumber,
                LangCode = languageCode,
                DataSetting = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices")
            };

            string jsonRequest = JsonConvert.SerializeObject(updatewronganswersflagrequest);

            string jsonResponse = await _mpSecurityCheckDetailsService.UpdateTfaWrongAnswersFlag(token, jsonRequest, sessionid).ConfigureAwait(false);

            SaveResponse response = null;
            if (!string.IsNullOrEmpty(jsonResponse))
            {
                response = JsonConvert.DeserializeObject<SaveResponse>(jsonResponse);

                if (response != null && (response.Errors == null || response.Errors.Count() == 0))
                {
                    _logger.LogInformation("UpdateTfaWrongAnswersFlag {Response} {@mileagePlusNumber}", response, mileagePlusNumber);
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

                    string exceptionmessage = string.IsNullOrEmpty(errorMessage) ? errorMessage : "Unable to Update TFA Wrong Answers Flag.";

                    _logger.LogInformation("UpdateTfaWrongAnswersFlag {@Exception} {@mileagePlusNumber}", GeneralHelper.RemoveCarriageReturn(exceptionmessage), mileagePlusNumber);
                    throw new MOBUnitedException(exceptionmessage);
                }
            }

            return response;
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
        #endregion
        private async Task<MOBEmpTravelTypeAndJAProfileResponse> GetEmployeeProfileDetails(string token, string employeeId, string TransactionId, int ApplicationId, string AppVersion, string DeviceId)
        {
            MOBEmpTravelTypeAndJAProfileResponse empTravelTypesAndJAResponse = new MOBEmpTravelTypeAndJAProfileResponse();

            MOBEmpTravelType empTravelType = new MOBEmpTravelType();
            BookingTypesResponse bookingTypes = new BookingTypesResponse();
            string encEmployeeId = new AesEncryptAndDecrypt().Encrypt(employeeId);

            var jsonResponse = await GetEResEmployeeProfile(encEmployeeId, token, TransactionId, ApplicationId, AppVersion, DeviceId).ConfigureAwait(false);
            MOBEmployeeProfileResponse emplProfile = JsonConvert.DeserializeObject<MOBEmployeeProfileResponse>(jsonResponse);

            var homePageContentResponse = await GetHomePageContent(emplProfile.TransactionID, token, encEmployeeId, TransactionId, ApplicationId, AppVersion, DeviceId).ConfigureAwait(false);
            MOBEmployeeReservationHomePageContentResponse homePageContent = JsonConvert.DeserializeObject<MOBEmployeeReservationHomePageContentResponse>(homePageContentResponse);

            empTravelTypesAndJAResponse.TransactionId = emplProfile.TransactionID;
            List<MOBEmpTravelTypeItem> empTravelTypeObjs = new List<MOBEmpTravelTypeItem>();

            if (homePageContent != null && homePageContent.TravelType != null)
            {
                int NumberOfPassengersInJA = emplProfile.EmployeeJA.PassRiders.Count() + 1;  // Pass riders + employee

                MOBEmpTravelTypeItem empTravelTypeObj = new MOBEmpTravelTypeItem
                {
                    Advisory = "",
                    IsAuthorizationRequired = false,
                    IsEligible = true,
                    NumberOfTravelers = NumberOfPassengersInJA,
                    TravelType = _configuration.GetValue<string>("TravelTypeAwarAndRevenueCode") ?? string.Empty,
                    TravelTypeDescription = _configuration.GetValue<string>("TravelTypeAwarAndRevenue") ?? string.Empty
                };

                empTravelTypeObj.Advisory = "";
                empTravelTypeObjs.Add(empTravelTypeObj);


                foreach (var tType in homePageContent.TravelType)
                {
                    //NRPS
                    if (tType.TravelCode == "B")
                    {
                        empTravelTypeObj = new MOBEmpTravelTypeItem
                        {
                            Advisory = "",
                            IsAuthorizationRequired = false,
                            IsEligible = true,
                            NumberOfTravelers = 1,
                            TravelType = tType.TravelCode
                        };

                        if (NumberOfPassengersInJA > 1)
                        {
                            empTravelTypeObj.NumberOfTravelers = NumberOfPassengersInJA;
                        }

                        if (tType.TravelDescription.ToLower().Trim().Contains("authorization"))
                        {
                            empTravelTypeObj.IsAuthorizationRequired = true;
                            empTravelTypeObj.TravelTypeDescription = _configuration.GetValue<string>("TravelTypeNRPSAuth") ?? string.Empty;
                        }
                        else
                        {
                            empTravelTypeObj.IsAuthorizationRequired = false;
                            empTravelTypeObj.TravelTypeDescription = _configuration.GetValue<string>("TravelTypeNRPS") ?? string.Empty;
                        }

                        empTravelTypeObj.Advisory = Convert.ToString(HttpUtility.HtmlDecode(_configuration.GetValue<string>("eResNRPSAdvisoryMessage")))
                            .Replace("\\r", "\r")
                            .Replace("\\n", "\n");


                        // As per new eRes flow and Jira ticket MB - 5668 & MB-5852, NRPS should appear before NRSA
                        if (empTravelTypeObjs.FindIndex(i => i.TravelType == "P") >= 0)
                        {
                            empTravelTypeObjs.Insert(empTravelTypeObjs.FindIndex(i => i.TravelType == "P"), empTravelTypeObj);
                        }
                        else
                        {
                            empTravelTypeObjs.Add(empTravelTypeObj);
                        }
                    }
                    //NRSA
                    else if (tType.TravelCode == "P")
                    {
                        empTravelTypeObj = new MOBEmpTravelTypeItem
                        {
                            Advisory = "",
                            IsAuthorizationRequired = false,
                            IsEligible = true,
                            NumberOfTravelers = 1,
                            TravelType = tType.TravelCode
                        };

                        if (NumberOfPassengersInJA > 1)
                        {
                            empTravelTypeObj.NumberOfTravelers = NumberOfPassengersInJA;
                        }
                        empTravelTypeObj.TravelTypeDescription = _configuration.GetValue<string>("TravelTypeNRSA") ?? string.Empty;
                        empTravelTypeObjs.Add(empTravelTypeObj);
                    }
                    else if (_configuration.GetValue<bool>("Enable_eRes_EmergencyDeviationTraining_TravelTypes") &&
                        GeneralHelper.IsApplicationVersionGreaterorEqual(ApplicationId, AppVersion, _configuration.GetValue<string>("eRes_EmergencyDeviationTraining_TravelTypes_Supported_AppVersion_Android"), _configuration.GetValue<string>("eRes_EmergencyDeviationTraining_TravelTypes_Supported_AppVersion_iOS")))
                    {
                        //Training || deviation || Emergency 
                        if (tType.TravelCode == TravelType.T.ToString() || tType.TravelCode == TravelType.D.ToString() || tType.TravelCode == TravelType.E.ToString())
                        {
                            empTravelTypeObj = new MOBEmpTravelTypeItem
                            {
                                Advisory = "",
                                IsAuthorizationRequired = false,
                                IsEligible = true,
                                NumberOfTravelers = 1,
                                TravelType = tType.TravelCode
                            };

                            empTravelTypeObj.Advisory = Convert.ToString(HttpUtility.HtmlDecode(_configuration.GetValue<string>("eResNRPSAdvisoryMessage")))
                                    .Replace("\\r", "\r")
                                    .Replace("\\n", "\n");

                            if (NumberOfPassengersInJA > 1)
                            {
                                empTravelTypeObj.NumberOfTravelers = NumberOfPassengersInJA;
                            }

                            if (tType.TravelCode == TravelType.T.ToString())
                            {
                                empTravelTypeObj.TravelTypeDescription = _configuration.GetValue<string>("TravelTypeNRPS-TrainingAuth") ?? string.Empty;
                            }
                            else if (tType.TravelCode == TravelType.D.ToString())
                            {
                                empTravelTypeObj.TravelTypeDescription = _configuration.GetValue<string>("TravelTypeNRPS-DeviationAuth") ?? string.Empty;
                            }
                            else if (tType.TravelCode == TravelType.E.ToString())
                            {
                                empTravelTypeObj.TravelTypeDescription = _configuration.GetValue<string>("TravelTypeNRPS-Emergency") ?? string.Empty;
                            }

                            empTravelTypeObjs.Add(empTravelTypeObj);
                        }
                    }
                }

                if (emplProfile.EmployeeJA.Employee.UADiscountIndicator)
                {
                    empTravelTypeObj = new MOBEmpTravelTypeItem
                    {
                        Advisory = "",
                        IsAuthorizationRequired = false,
                        IsEligible = true,
                        NumberOfTravelers = NumberOfPassengersInJA,

                        TravelType = "E20",
                        TravelTypeDescription = _configuration.GetValue<string>("TravelTypeE20") ?? string.Empty
                    };
                    empTravelTypeObj.Advisory = "";
                    empTravelTypeObjs.Add(empTravelTypeObj);
                }

                if (_configuration.GetValue<bool>("EnableTnCAdvanceBookingDays"))
                {
                    var BusinessPassClassList = emplProfile.EmployeeJA.Airlines.Where(X => X.BusinessPassClass.Equals("PS1B")).ToList();
                    bool iSPS1B = (BusinessPassClassList == null) ? false : Convert.ToBoolean(BusinessPassClassList?.Count());
                    UpdateTnCAdvanceBookingDays(ref empTravelTypeObjs, homePageContent.BaseAlert, iSPS1B,
                                                           GeneralHelper.IsApplicationVersionGreaterorEqual(ApplicationId, AppVersion,
                                                                _configuration.GetValue<string>("eResShowNRPSNewAdvisoryMessageAndroidversion"),
                                                                _configuration.GetValue<string>("eResShowNRPSNewAdvisoryMessageiOSversion")));
                }

                empTravelType.EmpTravelTypes = empTravelTypeObjs;
                empTravelType.IsTermsAndConditionsAccepted = emplProfile.EmployeeJA.isTNCAccepted;
                empTravelType.NumberOfPassengersInJA = NumberOfPassengersInJA;
                MOBEmpTravelTypeResponse mobEmpTravelTypeResponse = new MOBEmpTravelTypeResponse
                {
                    EResTransactionId = emplProfile.TransactionID
                };
                empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse = mobEmpTravelTypeResponse;
                empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmpTravelType = empTravelType;
                if (_configuration.GetValue<bool>("eResTermsConditionsEnabled"))
                {
                    empTravelType.TaxType = emplProfile.EmployeeJA.TaxVerbiage.TaxType;
                    empTravelType.VerbiageDescription = emplProfile.EmployeeJA.TaxVerbiage.VerbiageDescription;
                }
            }
            else
            {
                throw new MOBUnitedException("BookingTypes are Empty");
            }
            empTravelTypesAndJAResponse.MOBEmpJAResponse = LoadEmployeeJA_V2(emplProfile);

            empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.DependentInfos = emplProfile.EmployeeJA.PassRiders
                        .Select(e => new DependentInfo
                        {
                            DependentId = e.DependantID,
                            DateOfBirth = e.BirthDate.ToString("MM/dd/yyyy"),
                            Age = e.Age,
                            Relationship = new MOBEmpRelationship
                            {
                                Relationship = e.Relationship.Relationship,
                                RelationshipDescription = e.Relationship.RelationshipDescription,
                                RelationshipSubType = e.Relationship.RelationshipSubType,
                                RelationshipSubTypeDescription = e.Relationship.RelationshipSubTypeDescription
                            },
                            DependentName = new MOBName
                            {
                                First = e.FirstName,
                                Last = e.LastName,
                                Middle = e.MiddleName,
                                Suffix = e.NameSuffix
                            }
                        }).ToList();

            empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmployeeName = new MOBName
            {
                First = emplProfile.EmployeeJA.Employee.FirstName,
                Last = emplProfile.EmployeeJA.Employee.LastName,
                Middle = emplProfile.EmployeeJA.Employee.MiddleName,
                Suffix = emplProfile.EmployeeJA.Employee.NameSuffix
            };
            empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.EmployeeDateOfBirth = emplProfile.EmployeeJA.Employee.BirthDate;
            if (emplProfile.EmployeeJA != null && emplProfile.EmployeeJA.Airlines != null && emplProfile.EmployeeJA.Airlines.Count > 0)
            {
                foreach (var airline in emplProfile.EmployeeJA.Airlines)
                {
                    if (airline.PaymentDetails != null &&
                        !string.IsNullOrEmpty(airline.PaymentDetails.PayrollDeduct) &&
                        (airline.PaymentDetails.PayrollDeduct.Trim().ToUpper() == "Y" ||
                        airline.PaymentDetails.PayrollDeduct.Trim().ToUpper() == "YES"))
                    {
                        empTravelTypesAndJAResponse.MOBEmpTravelTypeResponse.IsPayrollDeduct = true;
                    }
                }
            }

            return empTravelTypesAndJAResponse;
        }

        private MOBEmployeeReservationHomePageContentRequest GetHomePageContentRequest(string encEmployeeId, string transactionId)
        {
            MOBEmployeeReservationHomePageContentRequest request = new MOBEmployeeReservationHomePageContentRequest
            {
                EmployeeId = encEmployeeId,
                IsAgentToolLogOn = false,
                IsPassRiderLoggedIn = false,
                TransactionId = transactionId
            };
            return request;
        }

        private void UpdateTnCAdvanceBookingDays(ref List<MOBEmpTravelTypeItem> empTravelTypeObjs, List<BaseAlert> eResAlerts, bool IsPS1B, bool AdvisoryMsgNewApps)
        {

            bool isEnableeResAlerts = _configuration.GetValue<bool>("EnableeResAlerts");

            for (int iCnt = 0; iCnt < empTravelTypeObjs.Count; iCnt++)
            {
                empTravelTypeObjs[iCnt].AdvanceBookingDays = _configuration.GetValue<int>("PS0B1BEmpAdvanceBookingDays");
                if (empTravelTypeObjs[iCnt].TravelType == "B")
                {
                    if (!IsPS1B)
                    {
                        empTravelTypeObjs[iCnt].AdvanceBookingDays = _configuration.GetValue<int>("EmpAdvanceBookingDays");
                    }

                    var Alert = eResAlerts.Where<BaseAlert>(x => x.AlertName == "BusinessAlert").ToList();
                    if (AdvisoryMsgNewApps && isEnableeResAlerts && Alert != null && Alert.Count > 0)
                    {
                        empTravelTypeObjs[iCnt].Advisory = Alert[0].AlertDescription;
                    }
                }
                //NRSA
                else if (empTravelTypeObjs[iCnt].TravelType == "P")
                {
                    empTravelTypeObjs[iCnt].AdvanceBookingDays = _configuration.GetValue<int>("EmpNRSAAdvanceBookingDays");
                }
                else if (empTravelTypeObjs[iCnt].TravelType == TravelType.T.ToString())
                {
                    if (!IsPS1B)
                    {
                        empTravelTypeObjs[iCnt].AdvanceBookingDays = _configuration.GetValue<int>("EmpAdvanceBookingDays");
                    }

                    var Alert = eResAlerts.Where<BaseAlert>(x => x.AlertName == "TrainingAlert").ToList();
                    if (!AdvisoryMsgNewApps)
                    {
                        empTravelTypeObjs[iCnt].Advisory = _configuration.GetValue<string>("eResNRPSAdvisoryMessageTraining")
                                        .Replace("\\r", "\r")
                                        .Replace("\\n", "\n");
                    }
                    else
                    {
                        empTravelTypeObjs[iCnt].Advisory = (isEnableeResAlerts && Alert != null && Alert.Count > 0) ?
                                                 Alert[0].AlertDescription :
                                                  empTravelTypeObjs[iCnt].Advisory.Replace("\\r", "\r").Replace("\\n", "\n");
                    }
                }
                else if (empTravelTypeObjs[iCnt].TravelType == TravelType.D.ToString())
                {
                    if (!IsPS1B)
                    {
                        empTravelTypeObjs[iCnt].AdvanceBookingDays = _configuration.GetValue<int>("EmpAdvanceBookingDays");
                    }

                    var Alert = eResAlerts.Where<BaseAlert>(x => x.AlertName == "DeviationAlert").ToList();
                    if (!AdvisoryMsgNewApps)
                    {
                        empTravelTypeObjs[iCnt].Advisory = _configuration.GetValue<string>("eResNRPSAdvisoryMessageDeviation")
                                        .Replace("\\r", "\r")
                                        .Replace("\\n", "\n");
                    }
                    else
                    {
                        empTravelTypeObjs[iCnt].Advisory = (isEnableeResAlerts && Alert != null && Alert.Count > 0) ?
                                                 Alert[0].AlertDescription :
                                                 empTravelTypeObjs[iCnt].Advisory.Replace("\\r", "\r").Replace("\\n", "\n");
                    }
                }
                else if (empTravelTypeObjs[iCnt].TravelType == TravelType.E.ToString())
                {
                    empTravelTypeObjs[iCnt].AdvanceBookingDays = _configuration.GetValue<int>("EmpAdvanceBookingDays");
                    var Alert = eResAlerts.Where<BaseAlert>(x => x.AlertName == "EmergencyAlert").ToList();

                    if (!AdvisoryMsgNewApps)
                    {
                        empTravelTypeObjs[iCnt].Advisory = _configuration.GetValue<string>("eResNRPSAdvisoryMessageEmergency")
                                        .Replace("\\r", "\r")
                                        .Replace("\\n", "\n");
                    }
                    else
                    {
                        empTravelTypeObjs[iCnt].Advisory = (isEnableeResAlerts && Alert != null && Alert.Count > 0) ?
                                                     Alert[0].AlertDescription :
                                                     empTravelTypeObjs[iCnt].Advisory.Replace("\\r", "\r").Replace("\\n", "\n");
                    }
                }
            }
        }

        private async Task<string> GetHomePageContent(string transactionId, string token, string encEmployeeId, string TransactionId, int ApplicationId, string AppVersion, string DeviceId)
        {
            MOBEmployeeReservationHomePageContentResponse response = new MOBEmployeeReservationHomePageContentResponse();
            MOBEmployeeReservationHomePageContentRequest request = GetHomePageContentRequest(encEmployeeId, transactionId);
            string jsonRequest = JsonConvert.SerializeObject(request);

            _logger.LogInformation("ValidateMPSignIn - GetHomePageContent {@request}", GeneralHelper.RemoveCarriageReturn(jsonRequest));

            string jsonResponse = await _homePageContentService.GetHomePageContents(token, jsonRequest, string.Empty).ConfigureAwait(false);
            return jsonResponse;
        }

        private async Task<string> GetEResEmployeeProfile(string encEmployeeId, string token, string TransactionId, int ApplicationId, string AppVersion, string DeviceId)
        {
            MOBEmployeeProfileRequest request = GetEmployeeProfileRequest(encEmployeeId);

            string jsonRequest = JsonConvert.SerializeObject(request);

            string authToken = await _tokenService.GetAnonymousToken(ApplicationId, DeviceId, _configuration).ConfigureAwait(false);
            var jsonResponse = await _eResEmployeeProfileService.GetEResEmployeeProfile(authToken, jsonRequest, TransactionId).ConfigureAwait(false);
            MOBEmployeeProfileResponse response = JsonConvert.DeserializeObject<MOBEmployeeProfileResponse>(jsonResponse);

            return jsonResponse;
        }

        private MOBEmployeeProfileRequest GetEmployeeProfileRequest(string encEmployeeId)
        {
            MOBEmployeeProfileRequest request = new MOBEmployeeProfileRequest
            {
                EmployeeId = encEmployeeId,
                IsLogOn = true,
                IsPassRiderLoggedIn = false,
                PassRiderLoggedInID = string.Empty,
                PassRiderLoggedInUser = string.Empty
            };
            return request;
        }

        private async Task<MOBEmpTravelTypeAndJAProfileResponse> GetEmployeeProfileDetailsMicroservice(string employeeId, string TransactionId, int ApplicationId, string AppVersion, string DeviceId, string sessionID, string PassRiderLoggedInID, string PassRiderLoggedInUser, string empAuthToken)
        {
            var jsonResponse = await GetEmployeeProfileTravelType(employeeId, TransactionId, ApplicationId, AppVersion, DeviceId, sessionID, PassRiderLoggedInID, PassRiderLoggedInUser, empAuthToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var emplProfile = JsonConvert.DeserializeObject<MOBMicroserviceBaseResponse<MOBEmployeeProfileTravelTypeResponse>>(jsonResponse);

                if (emplProfile?.Errors.Count == 0)
                {
                    MOBEmpTravelTypeAndJAProfileResponse empTravelTypesAndJAResponse = new MOBEmpTravelTypeAndJAProfileResponse
                    {
                        MOBEmpJAResponse = LoadEmployeeJA_V2(emplProfile.Data.EmployeeJAResponse),
                        MOBEmpTravelTypeResponse = emplProfile.Data.TravelTypeResponse
                    };
                    return empTravelTypesAndJAResponse;
                }
                else
                {
                    string errMessage = string.Empty;
                    foreach (KeyValuePair<string, ICollection<string>> obj in emplProfile.Errors)
                    {
                        errMessage = obj.Key + ": ";
                        foreach (string errorVal in obj.Value)
                        {
                            errMessage += errorVal + ";";
                        }
                    }


                    _logger.LogWarning("ValidateMPSignIn-Microservice-TravelerType {@UnitedException}", errMessage);

                }
            }

            throw new MOBUnitedException("BookingTypes are Empty");
        }

        private MOBEmpJAResponse LoadEmployeeJA_V2(MOBEmployeeProfileResponse emplProfile)
        {
            MOBEmpJA empJA = new MOBEmpJA();
            MOBEmpJAResponse empJAResponse = new MOBEmpJAResponse();
            MOBEmpPassRiderExtended empPassRiderExtended = new MOBEmpPassRiderExtended();
            MOBEmployeeProfileExtended empProfileExtended = new MOBEmployeeProfileExtended();

            List<MOBEmpBuddy> empBuddies = new List<MOBEmpBuddy>();
            List<MOBEmpPassRider> empPassRiders = new List<MOBEmpPassRider>();
            List<MOBEmpPassRider> empSuspendedPassRiders = new List<MOBEmpPassRider>();
            List<MOBEmpJAByAirline> empJAByAirlines = new List<MOBEmpJAByAirline>();

            List<MOBEmpBuddy> empLoggedInBuddies = new List<MOBEmpBuddy>();
            List<MOBEmpPassRider> empLoggedInPassRiders = new List<MOBEmpPassRider>();
            List<MOBEmpPassRider> empLoggedInSuspendedPassRiders = new List<MOBEmpPassRider>();
            List<MOBEmpJAByAirline> empLoggedInJAByAirlines = new List<MOBEmpJAByAirline>();

            empJAResponse.EmpJA = empJA;

            if (emplProfile != null && emplProfile.EmployeeJA != null)
            {
                #region Buddies
                if (emplProfile.EmployeeJA.Buddies != null && emplProfile.EmployeeJA.Buddies.Count > 0)
                {
                    empJAResponse.EmpJA.EmpBuddies = emplProfile.EmployeeJA.Buddies
                        .Select(b => new MOBEmpBuddy
                        {
                            BirthDate = b.BirthDate,
                            Email = b.DayOfEmail,
                            Gender = b.Gender,
                            Name = new MOBEmpName
                            {
                                First = b.FirstName,
                                Middle = b.MiddleName,
                                Last = b.LastName,
                                Suffix = b.NameSuffix,
                                Title = ""
                            },
                            OwnerCarrier = b.OwnerCarrier,
                            Phone = b.DayOfPhone,
                            Redress = b.Redress,
                            empRelationship = new MOBEmpRelationship
                            {
                                Relationship = b.Relationship.Relationship,
                                RelationshipDescription = b.Relationship.RelationshipDescription,
                                RelationshipSubType = b.Relationship.RelationshipSubType,
                                RelationshipSubTypeDescription = b.Relationship.RelationshipSubTypeDescription
                            }

                        }).ToList();
                }
                #endregion

                #region PassRiders
                if (emplProfile.EmployeeJA.PassRiders != null && emplProfile.EmployeeJA.PassRiders.Count > 0)
                {
                    empJAResponse.EmpJA.EmpPassRiders = emplProfile.EmployeeJA.PassRiders
                        .Select(e => new MOBEmpPassRider
                        {
                            Age = e.Age,
                            BirthDate = Convert.ToString(e.BirthDate),
                            DependantID = e.DependantID,
                            FirstBookingBuckets = e.FirstBookingBuckets,
                            Gender = e.Gender,
                            MustUseCurrentYearPasses = e.MustUseCurrentYearPasses
                        }).ToList();
                }
                #endregion

                #region Suspended PassRiders
                if (emplProfile.EmployeeJA.SuspendedPassRiders != null && emplProfile.EmployeeJA.SuspendedPassRiders.Count > 0)
                {
                    empJAResponse.EmpJA.EmpSuspendedPassRiders = emplProfile.EmployeeJA.SuspendedPassRiders
                        .Select(spr => new MOBEmpPassRider
                        {
                            Age = spr.Age,
                            BirthDate = Convert.ToString(spr.BirthDate),
                            DependantID = spr.DependantID,
                            FirstBookingBuckets = spr.FirstBookingBuckets,
                            Gender = spr.Gender,
                            MustUseCurrentYearPasses = spr.MustUseCurrentYearPasses,
                            PrimaryFriend = spr.PrimaryFriend,
                            UnaccompaniedFirst = spr.UnaccompaniedFirst
                        }).ToList();
                }
                #endregion

                #region Airlines
                if (emplProfile.EmployeeJA.Airlines != null && emplProfile.EmployeeJA.Airlines.Count > 0)
                {
                    empJAResponse.EmpJA.EmpJAByAirlines = emplProfile.EmployeeJA.Airlines
                        .Select(m => new MOBEmpJAByAirline
                        {
                            AirlineCode = m.AirlineCode,
                            AirlineDescription = m.AirlineDescription,
                            BoardDate = m.BoardDate,
                            BuddyPassClass = m.BuddyPassClass,
                            BusinessPassClass = m.BusinessPassClass,
                            CanBookFirstOnBusiness = m.CanBookFirstOnBusiness,
                            DeviationPassClass = "", // airline.DeviationPassClass;
                            Display = true, // airline.Display;
                            ETicketIndicator = m.ETicketIndicator,
                            ExtendedFamilyPassClass = m.ExtendedFamilyPassClass,
                            FamilyPassClass = m.FamilyPassClass,
                            FamilyVacationPassClass = m.FamilyVacationPassClass,
                            FeeWaivedCoach = m.IsFeeWaivedCoach,
                            FeeWaivedFirst = m.IsFeeWaivedFirst,
                            JumpSeatPassClass = m.JumpSeatPassClass,
                            PaymentIndicator = m.PaymentIndicator,
                            PersonalPassClass = m.PersonalPassClass,
                            ScheduleEngineCode = m.ScheduleEngineCode,
                            Seniority = "", // airline.Seniority;
                            SeniorityDate = m.SeniorityDate,
                            SuspendEndDate = m.SuspendEndDate,
                            SuspendStartDate = m.SuspendStartDate,
                            TrainingPassClass = "", // airline.TrainingPassClass;
                            VacationPassClass = m.VacationPassClass,
                        }).ToList();
                }
                #endregion

                #region EmployeeProfileExtended
                if (emplProfile.EmployeeJA.Employee != null && emplProfile.EmployeeJA.Employee.DayOfContactInformation != null)
                {

                    empProfileExtended.Email = emplProfile.EmployeeJA.Employee.DayOfContactInformation.Email;
                    empProfileExtended.HomePhone = emplProfile.EmployeeJA.Employee.DayOfContactInformation.PhoneNumber;
                    empProfileExtended.FaxNumber = "";
                    empProfileExtended.WorkPhone = "";

                    empJAResponse.EmpProfileExtended = empProfileExtended;
                }
                #endregion
                empJAResponse.AllowImpersonation = emplProfile.AllowImpersonation;
                empJAResponse.ImpersonateType = emplProfile.ImpersonateType;
                empJAResponse.LanguageCode = "";
                empJAResponse.MachineName = "";
                empJAResponse.TransactionId = emplProfile.TransactionID;

            }

            return empJAResponse;
        }

        private async Task<string> GetEmployeeProfileTravelType(string EmployeeId, string TransactionId, int ApplicationId, string AppVersion, string DeviceId, string sessionID, string PassRiderLoggedInID, string PassRiderLoggedInUser, string empAuthToken)
        {
            MOBEmployeeProfileTravelTypeRequest request = new MOBEmployeeProfileTravelTypeRequest
            {
                EmployeeId = EmployeeId,
                IsLogOn = true,
                IsPassRiderLoggedIn = false,
                PassRiderLoggedInID = PassRiderLoggedInID,
                PassRiderLoggedInUser = PassRiderLoggedInUser,
                SessionId = sessionID,
                TransactionId = TransactionId
            };

            MOBMicroserviceBaseRequest<MOBEmployeeProfileTravelTypeRequest> requestFormat = new MOBMicroserviceBaseRequest<MOBEmployeeProfileTravelTypeRequest>()
            {
                Data = request
            };
            string jsonRequest = JsonConvert.SerializeObject(requestFormat);
            return await _employeeProfileTravelType.GetTravelType(empAuthToken, jsonRequest, sessionID, ApplicationId, AppVersion, DeviceId, TransactionId).ConfigureAwait(false);
        }

        private async Task InsertMileagePlusAndHash(int applicationId, string deviceId, string appVersion, string transactionId, string token, string mileagePlusNumber, string pinCode, string hashValue, DateTime expirationDateTime, double expirationSeconds, long customerId, bool isTouchID, string sessionId, MOBRequest request, string logAction, bool isTokenAnonymous = false, bool isAuthTokenValid = true)
        {
            await _dynamoDBUtility.InsertMileagePlusAndHash(mileagePlusNumber, hashValue, sessionId);

            bool iSDPAuthentication = _configuration.GetValue<bool>("EnableDPToken");
            string SPname = string.Empty;
            /// CSS Token length is 36 and Data Power Access Token length is more than 1500 to 1700 chars  

            var cloudResult = OnCloudSaveData(sessionId, mileagePlusNumber, hashValue, applicationId.ToString(), appVersion, deviceId, transactionId, token, pinCode, true, expirationDateTime.ToString("yyyy-MM-dd HH:mm:ss.FFF"), expirationSeconds.ToString(), false, false, customerId.ToString());

            string Key = string.Format("{0}::{1}::{2}", mileagePlusNumber, request.Application.Id.ToString(), request.DeviceId);
            await _dynamoDBUtility.InsertUpdateMileagePlusAndPin(cloudResult, iSDPAuthentication, Key, sessionId, transactionId).ConfigureAwait(false);
        }

        private async Task<bool> InsertUpdateMPCSSValidationDetails(string mpNumber, string mpUserName, string deviceID, int appID, string appVersion, string authToken, bool isAuthTokenValid, DateTime authTokenExpirationDateTime, double tokenExpireInSeconds, string hashValue, string pinCode = "", bool isTouchID = false, bool isTokenAnonymous = false, long customerID = 0, int retryCount = 1)
        {
            bool ok = false;
            bool iSDPAuthentication = _configuration.GetValue<bool>("EnableDPToken");
            string SPname = string.Empty;
            /// CSS Token length is 36 and Data Power Access Token length is more than 1500 to 1700 chars

            try
            {
                var cloudResult = OnCloudSaveData(_headers.ContextValues.SessionId, mpNumber, hashValue, appID.ToString(), appVersion, deviceID, _headers.ContextValues.TransactionId, authToken, pinCode, true, authTokenExpirationDateTime.ToString("yyyy-MM-dd HH:mm:ss.FFF"), tokenExpireInSeconds.ToString(), isTouchID, false, customerID.ToString());
                string Key = string.Format("{0}::{1}::{2}", mpNumber, appID.ToString(), deviceID);
                await _dynamoDBUtility.InsertUpdateMileagePlusAndPin(cloudResult, iSDPAuthentication, Key, _headers.ContextValues.SessionId, _headers.ContextValues.TransactionId).ConfigureAwait(false);

                ok = true;
            }
            catch (Exception ex)
            {
                int configRetryCount = _configuration.GetValue<int>("ValidateMpSignInRetryCount");
                if (retryCount <= configRetryCount)
                {
                    return await InsertUpdateMPCSSValidationInfo(mpNumber, mpUserName, pinCode, deviceID, appID, appVersion, authToken, isAuthTokenValid, authTokenExpirationDateTime, tokenExpireInSeconds, isTouchID, isTokenAnonymous, hashValue, customerID, ++retryCount).ConfigureAwait(false);
                }
                else
                {
                    throw ex;
                }
            }
            return ok;
        }

        private async Task<bool> InsertUpdateMPCSSValidationInfo(string mpNumber, string mpUserName, string pinCode, string deviceID, int appID, string appVersion, string authToken, bool isAuthTokenValid, DateTime authTokenExpirationDateTime, double tokenExpireInSeconds, bool isTouchID = false, bool isTokenAnonymous = false, string hashValue = "", long customerID = 0, int retryCount = 1)
        {
            bool ok = false;
            string hashedPinCode = string.Empty;
            bool iSDPAuthentication = _configuration.GetValue<bool>("EnableDPToken");
            string SPname = string.Empty;
            if (_configuration.GetValue<bool>("EnableNewHashLogic"))
            {
                hashedPinCode = hashValue;
            }
            else
            {
                if (!isTouchID && !string.IsNullOrEmpty(pinCode))
                {
                    hashedPinCode = CreateHash(pinCode);
                }
                else if (isTouchID)
                {
                    hashedPinCode = hashValue;
                }
            }
            /// CSS Token length is 36 and Data Power Access Token length is more than 1500 to 1700 chars
            try
            {
                var cloudResult = OnCloudSaveData(_headers.ContextValues.SessionId, mpNumber, hashValue, appID.ToString(), appVersion, deviceID, _headers.ContextValues.TransactionId, authToken, pinCode, true, authTokenExpirationDateTime.ToString("yyyy-MM-dd HH:mm:ss.FFF"), tokenExpireInSeconds.ToString(), isTouchID, false, customerID.ToString());
                string Key = string.Format("{0}::{1}::{2}", mpNumber, appID.ToString(), deviceID);
                await _dynamoDBUtility.InsertUpdateMileagePlusAndPin(cloudResult, iSDPAuthentication, Key, _headers.ContextValues.SessionId, _headers.ContextValues.TransactionId).ConfigureAwait(false);

                ok = true;
            }
            catch (Exception ex)
            {
                int configRetryCount = _configuration.GetValue<int>("ValidateMpSignInRetryCount");
                if (retryCount <= configRetryCount)
                {
                    return await InsertUpdateMPCSSValidationInfo(mpNumber, mpUserName, pinCode, deviceID, appID, appVersion, authToken, isAuthTokenValid, authTokenExpirationDateTime, tokenExpireInSeconds, isTouchID, isTokenAnonymous, hashValue, customerID, ++retryCount).ConfigureAwait(false);
                }
                else
                {
                    throw ex;
                }
            }
            return ok;
        }

        private string CreateHash(string pin)
        {
            var hash = _hashedPin.HashPinGeneration(pin);
            var verifyHash = _hashedPin.VerifyHashedPin(hash, pin);

            if (verifyHash == PasswordVerificationResult.Success)
                return hash;

            return default;
        }

        private async Task<MOBMPPINPWDValidateRequest> UpdatePersistMPNumberOnlyNumeric(MOBMPPINPWDValidateRequest request, MOBMPPINPWDValidateResponse response, Session session)
        {
            var mobmppinpwdvalidaterequest = new MOBMPPINPWDValidateRequest();
            mobmppinpwdvalidaterequest = await _sessionHelperService.GetSession<MOBMPPINPWDValidateRequest>(request.SessionID, mobmppinpwdvalidaterequest.ObjectName, new List<string> { request.SessionID, mobmppinpwdvalidaterequest.ObjectName }).ConfigureAwait(false);
            try
            {
                if (Regex.IsMatch(request.MileagePlusNumber, "^[0-9]*$"))
                {
                    if (mobmppinpwdvalidaterequest != null && mobmppinpwdvalidaterequest.MileagePlusNumber != "")
                    {
                        mobmppinpwdvalidaterequest.MileagePlusNumber = response.AccountValidation.MileagePlusNumber.ToUpper().Trim();
                        await _sessionHelperService.SaveSession<MOBMPPINPWDValidateRequest>(mobmppinpwdvalidaterequest, _headers.ContextValues.SessionId, new List<string> { _headers.ContextValues.SessionId, mobmppinpwdvalidaterequest.ObjectName }, mobmppinpwdvalidaterequest.ObjectName).ConfigureAwait(false);
                    }
                }
            }
            catch { }

            return mobmppinpwdvalidaterequest;
        }

        private async Task<List<MOBItem>> GetNeedHelpTitleMessages(string title)
        {
            List<MOBItem> messages = new List<MOBItem>();
            List<string> titleList = new List<string>
            {
                title
            };
            messages = await _securityQuestion.GetMPPINPWDTitleMessagesForMPAuth(titleList).ConfigureAwait(false);
            return messages;
        }

        private bool EnableYoungAdultCorporate(int appId, string appVersion, bool isReshop = false)
        {
            return _configuration.GetValue<bool>("EnableYoungAdultBooking") && _configuration.GetValue<bool>("EnableYoungAdultCorporateFix") && !isReshop
                 && GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidYoungAdultCorporateFixVersion", "iPhoneYoungAdultCorporateFixVersion", "", "", true, _configuration);
        }

        private async Task<List<MOBItem>> GetSecurityUpdateTitleMessages(MOBMPSecurityUpdatePath securityPath, bool landingPage, bool pwdUpdateSucess, string mileagPlusNumber, int daysToCompleteSecurityQuestions)
        {
            List<MOBItem> messages = new List<MOBItem>();
            #region
            List<string> titleList = new List<string>();
            if (securityPath == MOBMPSecurityUpdatePath.NoPrimayEmailExist)
            {
                titleList.Add("MP_PIN_PWD_INSERT_PRIMARY_EMAIL_ADDRESS_TITLES");
            }
            if (securityPath == MOBMPSecurityUpdatePath.VerifyPrimaryEmail)
            {
                titleList.Add("MP_PIN_PWD_VERIFY_PRIMARY_EMAIL_ADDRESS_TITLES");
            }
            if (securityPath == MOBMPSecurityUpdatePath.UpdateSecurityQuestions)
            {
                titleList.Add("MP_PIN_PWD_SELECT_SECURITY_QUESTIONS_TITLES");
            }
            if (securityPath == MOBMPSecurityUpdatePath.UpdatePassword)
            {
                titleList.Add("MP_PIN_PWD_CREATE_NEW_PASSWORD_TITLES");
            }
            if (landingPage)
            {
                titleList.Add("MP_PIN_PWD_LANDING_PAGE_TITLES");
            }
            if (securityPath == MOBMPSecurityUpdatePath.SignInBackWithNewPassWord)
            {
                titleList.Add("MP_PIN_PWD_UPDATE_SUCCESS_RE_ENTER_TITLES");
            }

            #endregion
            messages = await _securityQuestion.GetMPPINPWDTitleMessagesForMPAuth(titleList).ConfigureAwait(false);
            if (securityPath == MOBMPSecurityUpdatePath.SignInBackWithNewPassWord)
            {
                //Loop the messages and goto id=Body2 and update current values as Current Value + ":" + ****** Last 3 digits of MP for reference Document page 37 screen shot 4.5.8

                foreach (var message in messages)
                {
                    //## kirti - implemented the masking the MPiD with ***
                    if (message.Id.Trim().Equals("Body2", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //get last 3 letter of the MPID
                        string subMp = mileagPlusNumber.Substring(mileagPlusNumber.Length - 3, 3);

                        //mask mpid with number of *** 
                        string maskedMpId = String.Concat(Enumerable.Repeat("*", mileagPlusNumber.Length)); //mileagPlusNumber.Replace(mileagPlusNumber,"********");

                        string finalMpIdMasked = string.Format("{0}{1}", maskedMpId.Substring(0, maskedMpId.Length - 3), subMp);

                        message.CurrentValue = string.Format("{0}: {1}", message.CurrentValue, finalMpIdMasked);

                    }
                }

            }
            //
            if (landingPage)
            {
                foreach (var msg in messages)
                {
                    if (msg.Id.Trim().Equals("Body", StringComparison.InvariantCultureIgnoreCase))
                    {
                        msg.CurrentValue = string.Format(msg.CurrentValue, daysToCompleteSecurityQuestions < 0 ? 0 : daysToCompleteSecurityQuestions);
                    }
                }
            }
            return messages;
        }

        private bool CheckAppIDToReturnAccountSummary(int applicationID)
        {
            #region
            string[] appIDsToReturnAccountSummaryWithVaildateCall = _configuration.GetValue<string>("AppIDSToReturnAccountSumaryAtValidateMPSignIn").Split('|');
            foreach (string appID in appIDsToReturnAccountSummaryWithVaildateCall)
            {
                if (Convert.ToInt32(appID) == applicationID)
                {
                    return true;
                }
            }
            return false;
            #endregion
        }

        private MOBCPCustomerMetrics AssignCustomerMetrics(bool isAwardCalendarMP2017, MOBCPProfile mpSecurityUpdateDetails)
        {
            if (!isAwardCalendarMP2017 || mpSecurityUpdateDetails == null)
            {
                return null;
            }

            var customerMetrics = new MOBCPCustomerMetrics();

            if (mpSecurityUpdateDetails.Travelers != null && mpSecurityUpdateDetails.Travelers.Count > 0 && mpSecurityUpdateDetails.Travelers[0] != null && mpSecurityUpdateDetails.Travelers[0].CustomerMetrics != null && mpSecurityUpdateDetails.Travelers[0].CustomerMetrics.PTCCode != null)
            {
                customerMetrics.PTCCode = mpSecurityUpdateDetails.Travelers[0].CustomerMetrics.PTCCode;
            }
            return customerMetrics;
        }

        private async Task ValidateCorporateBooking(MOBMPPINPWDValidateRequest request, bool isCorporateBooking,
           MOBCPProfile mpSecurityUpdateDetails, MOBMPPINPWDValidateResponse response, bool isYoungAdult = false, Session session = null)
        {
            if (!isCorporateBooking || mpSecurityUpdateDetails == null)
            {
                return;
            }

            response.CorporateEligibleTravelType = null;
            if (mpSecurityUpdateDetails.CorporateData != null && mpSecurityUpdateDetails.CorporateData.IsValid)
            {
                TravelPolicy corporatePolicy = null;
                try
                {
                    if (request != null && request.Application != null && GeneralHelper.IsEnableU4BCorporateBooking(request.Application.Id, request.Application.Version?.Major, _configuration))
                    {
                        corporatePolicy = await _uCBProfile.GetCorporateTravelPolicy(request, session, mpSecurityUpdateDetails.CorporateData).ConfigureAwait(false);
                    }
                }
                catch (Exception)
                {

                }
                _logger.LogInformation("ValidateMPSignIn - ValidateCorporateBooking-corporatePolicy {@corporatePolicy}", JsonConvert.SerializeObject(corporatePolicy));

                bool isEnableSuppressingCompanyNameForBusiness = _uCBProfile.IsEnableSuppressingCompanyNameForBusiness(request.Application.Id, request.Application.Version.Major);
                bool isEnableMultiPaxForU4B = await _featureToggles.IsEnableU4BForMultipax(request.Application.Id, request.Application.Version.Major).ConfigureAwait(false);
                response.CorporateEligibleTravelType = CorporateTravelTypes(mpSecurityUpdateDetails.CorporateData, isYoungAdult, corporatePolicy, isEnableSuppressingCompanyNameForBusiness, isEnableMultiPaxForU4B);
            }
            else if (request.MPSignInPath == MOBMPSignInPath.CorporateBookingPath)
            {
                response.Exception = new MOBException
                {
                    Code = "3333",  //Specific code other than 9999 is sent to client to display alert
                    Message =
                    _configuration.GetValue<string>("CorporateBookingNotEligibleMessage")
                };

                _logger.LogInformation("ValidateMPSignIn - ValidateCorporateBooking {@UnitedException}", GeneralHelper.RemoveCarriageReturn(response.Exception.Code + "|" + response.Exception.Message));
            }
            else if (request.MPSignInPath == MOBMPSignInPath.CorporateChangePath)
            {
                response.Exception = new MOBException
                {
                    Code = "3333",  //Specific code other than 9999 is sent to client to display alert
                    Message =
                    _configuration.GetValue<string>("CorporateBookingNotEligibleMessage")
                };
                response.ResponseStatusItem = new MOBSHOPResponseStatusItem
                {
                    Status = MOBSHOPResponseStatus.ReshopUnableToChange,
                    StatusMessages = await _securityQuestion.GetMPPINPWDTitleMessagesForMPAuth(new List<string> { "CORPORATE_INELIGIBILITY_CHANGE" }).ConfigureAwait(false)
                };
            }
        }

        private MOBCorporateTravelType CorporateTravelTypes(MOBCPCorporate corporateData, bool isYoungAdult = false, TravelPolicy CorporateTravelPolicy = null, bool isEnableSuppressingCompanyNameForBusiness = false, bool isEnableMultiPaxForU4B = false)
        {
            MOBCorporateTravelType travelTypeResponse = new MOBCorporateTravelType();
            try
            {
                travelTypeResponse.CorporateCustomer = true;
                travelTypeResponse.CorporateDetails = new MOBCorporateDetails();
                travelTypeResponse.CorporateCustomerBEAllowed = true;
                if (!string.IsNullOrEmpty(corporateData.FareGroupId))
                {
                    travelTypeResponse.CorporateDetails.FareGroupId = corporateData.FareGroupId;
                    if (corporateData.FareGroupId.ToUpper() == "XBE")
                    {
                        travelTypeResponse.CorporateCustomerBEAllowed = false; //XBE Exclude BE fares dont show toggle on the Booking Main
                    }
                }

                if (!string.IsNullOrEmpty(corporateData.CompanyName))
                {
                    travelTypeResponse.CorporateDetails.CorporateCompanyName = corporateData.CompanyName;
                }
                if (!string.IsNullOrEmpty(corporateData.VendorName))
                {
                    travelTypeResponse.CorporateDetails.CorporateTravelProvider = corporateData.VendorName;
                }
                if (!string.IsNullOrEmpty(corporateData.DiscountCode))
                {
                    travelTypeResponse.CorporateDetails.DiscountCode = corporateData.DiscountCode;
                }
                if (_configuration.GetValue<bool>("EnableCorporateLeisure") && !string.IsNullOrEmpty(corporateData.LeisureDiscountCode))//Corporate Leisure(Break from Business)
                {
                    travelTypeResponse.CorporateDetails.LeisureDiscountCode = corporateData.LeisureDiscountCode;
                }
                if (_configuration.GetValue<bool>("EnableIsArranger"))
                {
                    if (!string.IsNullOrEmpty(corporateData.CorporateBookingType))
                    {
                        travelTypeResponse.CorporateDetails.CorporateBookingType = corporateData.CorporateBookingType;
                    }
                    travelTypeResponse.CorporateDetails.NoOfTravelers = corporateData.NoOfTravelers;
                }
                if (isEnableSuppressingCompanyNameForBusiness)
                {
                    travelTypeResponse.CorporateDetails.IsPersonalized = corporateData.IsPersonalized;
                }
                if (isEnableMultiPaxForU4B)
                {
                    travelTypeResponse.CorporateDetails.IsMultiPaxAllowed = corporateData.IsMultiPaxAllowed;
                }

                List<MOBCorporateTravelTypeItem> corporateTravelTypeItems = new List<MOBCorporateTravelTypeItem>();
                string[] corporateUiLabels;
                if (_configuration.GetValue<bool>("EnableEResAPIMigration"))
                {

                    MOBCorporateTravelTypeItem corporateTravelTypeItem = new MOBCorporateTravelTypeItem
                    {
                        TravelType = _configuration.GetValue<string>("TravelTypeAwarAndRevenueCode"),
                        TravelTypeDescription = _configuration.GetValue<string>("TravelTypeAwarAndRevenue")
                    };
                    corporateTravelTypeItems.Add(corporateTravelTypeItem);

                    if (!string.IsNullOrEmpty(corporateData.DiscountCode))
                    {
                        corporateTravelTypeItem = new MOBCorporateTravelTypeItem
                        {
                            TravelType = "CB",
                            TravelTypeDescription = _configuration.GetValue<bool>("UpdateTravelTypeNameForBusiness") ? _configuration.GetValue<string>("TravelTypeNameForBusiness") : string.Format(_configuration.GetValue<string>("TravelTypeCorporate"), corporateData.CompanyName)
                        };
                        if (CorporateTravelPolicy != null && CorporateTravelPolicy.TravelPolicyContent != null && CorporateTravelPolicy.TravelPolicyContent.Count > 0)
                            corporateTravelTypeItem.TravelPolicy = CorporateTravelPolicy;
                        corporateTravelTypeItems.Add(corporateTravelTypeItem);
                    }
                    if (_configuration.GetValue<bool>("EnableCorporateLeisure") && !string.IsNullOrEmpty(corporateData.LeisureDiscountCode))
                    {
                        corporateTravelTypeItem = new MOBCorporateTravelTypeItem
                        {
                            TravelType = TravelType.CLB.ToString(),
                            TravelTypeDescription = _configuration.GetValue<string>("TravelTypeCorporateLeisure")
                        };
                        corporateTravelTypeItems.Add(corporateTravelTypeItem);
                    }

                    if (_customerProfile.EnableYoungAdult() && isYoungAdult)
                    {
                        corporateTravelTypeItem = new MOBCorporateTravelTypeItem
                        {
                            TravelType = "YA",
                            TravelTypeDescription = _configuration.GetValue<string>("TravelTypeYoungAdult")
                        };
                        corporateTravelTypeItems.Add(corporateTravelTypeItem);
                    }
                }
                else
                {
                    if (_customerProfile.EnableYoungAdult() && isYoungAdult)
                    {
                        corporateUiLabels = _configuration.GetValue<string>("CorporateAndYABookingUILabels").Split('|');
                    }
                    else
                    {
                        corporateUiLabels = _configuration.GetValue<string>("CorporateBookingUILabels").Split('|');
                    }
                    foreach (string uiLabel in corporateUiLabels)
                    {
                        MOBCorporateTravelTypeItem corporateTravelTypeItem = new MOBCorporateTravelTypeItem
                        {
                            TravelType =
                            uiLabel.Split('~')[0].ToUpper().Trim(),
                            TravelTypeDescription = uiLabel.Split('~')[1].ToString().Trim()
                        };
                        corporateTravelTypeItems.Add(corporateTravelTypeItem);
                    }
                }
                travelTypeResponse.CorporateTravelTypes = corporateTravelTypeItems;
            }
            catch { }

            return travelTypeResponse;
        }

        private async Task GetSecurityUpdateDetails(MOBCPProfile mpSecurityUpdateDetails, MOBMPPINPWDValidateRequest request, MOBMPPINPWDValidateResponse response, Session session)
        {
            if (mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate != null && mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.MPSecurityPathList != null && mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.MPSecurityPathList != null && mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.MPSecurityPathList.Count > 0)
            {
                #region
                foreach (MOBMPSecurityUpdatePath securityPath in mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.MPSecurityPathList)
                {
                    if (securityPath == MOBMPSecurityUpdatePath.NoPrimayEmailExist || securityPath == MOBMPSecurityUpdatePath.VerifyPrimaryEmail)
                    {
                        response.MPSecurityUpdateDetails.MPSecurityPath = securityPath;
                        if (securityPath == MOBMPSecurityUpdatePath.VerifyPrimaryEmail && mpSecurityUpdateDetails.Travelers[0].EmailAddresses.Count > 0)
                        {
                            response.MPSecurityUpdateDetails.SecurityItems.PrimaryEmailAddress = mpSecurityUpdateDetails.Travelers[0].EmailAddresses[0].EmailAddress;
                        }
                        break;
                    }
                    if (securityPath == MOBMPSecurityUpdatePath.UpdateSecurityQuestions)
                    {
                        response.MPSecurityUpdateDetails.MPSecurityPath = securityPath;
                        response.MPSecurityUpdateDetails.SecurityItems.AllSecurityQuestions = await _mileagePlusTFACSL.GetMPPINPWDSecurityQuestions(session.Token, request.SessionID, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false);
                        break;
                    }
                    if (securityPath == MOBMPSecurityUpdatePath.UpdatePassword)
                    {
                        response.MPSecurityUpdateDetails.MPSecurityPath = securityPath;
                        break;
                    }
                }
                #endregion
                #region
                response.SecurityUpdate = true;
                response.MPSecurityUpdateDetails.UpdateLaterAllowed = mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.UpdateLaterAllowed;
                response.MPSecurityUpdateDetails.PasswordOnlyAllowed = mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.PasswordOnlyAllowed;
                response.MPSecurityUpdateDetails.DaysToCompleteSecurityUpdate = mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.DaysToCompleteSecurityUpdate;
                if (mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.MPSecurityPathList.Count >= 1)
                {
                    response.MPSecurityUpdateDetails.MPSecurityPathList = mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.MPSecurityPathList;
                    response.MPSecurityUpdateDetails.MPSecurityPathList.Remove(response.MPSecurityUpdateDetails.MPSecurityPath);
                    if (!mpSecurityUpdateDetails.Travelers[0].MPSecurityUpdate.UpdateLaterAllowed)
                    {
                        response.MPSecurityUpdateDetails.MPSecurityPathList.Add(MOBMPSecurityUpdatePath.SignInBackWithNewPassWord);
                        response.MPSecurityUpdateDetails.ForceSignOut = true;
                    }
                }
                #endregion
            }
        }

        private void PinPwdAutoSignIn(MOBMPPINPWDSecurityUpdateDetails securityUpdateDetails)
        {
            if (!securityUpdateDetails.UpdateLaterAllowed && _configuration.GetValue<bool>("IsPINPWDAutoSignInON"))
            {
                securityUpdateDetails.IsPinPwdAutoSignIn = true;
            }
        }
        private async Task ValidateTFAMPDevice(MOBMPPINPWDValidateResponse response, MOBMPPINPWDValidateRequest request, Session session)
        {
            response.RememberMEFlags = new MOBMPTFARememberMeFlags(_configuration);
            if (string.IsNullOrEmpty(session.MileagPlusNumber) && !string.IsNullOrEmpty(request.MileagePlusNumber))
            {
                session.MileagPlusNumber = request.MileagePlusNumber;
            }

            bool validatedevicecallwrapper = await _mileagePlusTFACSL.ValidateDevice(session, request.Application.Version.Major, request.LanguageCode).ConfigureAwait(false);//**==>> Change Provider to return only bool true or false.
            if (!validatedevicecallwrapper)
            {
                #region
                //**==>>Need to sign out and return as invalid pin code 
                response.SecurityUpdate = true;
                _mileagePlusTFACSL.SignOutSession(session.SessionId, session.Token, request.Application.Id);

                var securityQuestionsFromCSL = await _securityQuestion.GetMPPinPwdSavedSecurityQuestions(session.Token, 0, request.MileagePlusNumber, session.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId).ConfigureAwait(false);
                if (securityQuestionsFromCSL == null || securityQuestionsFromCSL.Count == 0)
                {
                    response.MPSecurityUpdateDetails.SecurityUpdateMessages = await GetNeedHelpTitleMessages(MOBMPSecurityUpdatePath.IncorrectUserDetails, false).ConfigureAwait(false);
                    response.MPSecurityUpdateDetails.MPSecurityPath = MOBMPSecurityUpdatePath.UnableToResetOnline;
                    response.AccountValidation.ValidPinCode = false; // This flag will return handled exception accout informaton not correct message so this scenario is covered
                }
                else
                {
                    int numberOfQuestionsToBeSent = _configuration.GetValue<int>("NumberOfQuestionsToBeSentToClinet");
                    if (securityQuestionsFromCSL.Count >= numberOfQuestionsToBeSent)
                    {
                        response.MPSecurityUpdateDetails.SecurityItems = new MOBMPPINPWDSecurityItems(_configuration)
                        {
                            AllSecurityQuestions = securityQuestionsFromCSL.Take(numberOfQuestionsToBeSent).ToList() // ALM 27489 PINPWD: Same security questions order is not displayed in mobile when compared with .COM
                        };
                        var persistSecurityQuestionObject = new MPPINPWSecurityQuestionsValidation
                        {
                            RetryCount = 0,
                            SecurityQuestionsFromCSL = securityQuestionsFromCSL,
                            SecurityQuestionsSentToClient = response.MPSecurityUpdateDetails.SecurityItems.AllSecurityQuestions
                        };

                        await _sessionHelperService.SaveSession<MPPINPWSecurityQuestionsValidation>(persistSecurityQuestionObject, session.SessionId, new List<string> { session.SessionId, persistSecurityQuestionObject.ObjectName }, persistSecurityQuestionObject.ObjectName).ConfigureAwait(false);
                        response.MPSecurityUpdateDetails.SecurityUpdateMessages = await GetNeedHelpTitleMessages(request.Application.Id == 6 ? "MP_PIN_PWD_TFA_SECURITY_QUESTIONS_TRY_1_TITLES_WINDOWS_APP" : "MP_PIN_PWD_TFA_SECURITY_QUESTIONS_TRY_1_TITLES_ALL").ConfigureAwait(false);
                        response.MPSecurityUpdateDetails.MPSecurityPath = MOBMPSecurityUpdatePath.ValidateTFASecurityQuestions;
                        if (request.MPSignInPath != MOBMPSignInPath.RevenueBookingPath)
                        {
                            MOBItem mobitem = response.MPSecurityUpdateDetails.SecurityUpdateMessages.FirstOrDefault(p => p.CurrentValue.ToUpper() == "CONTINUE WITH SIGN IN");
                            if (mobitem != null)
                            {
                                mobitem.CurrentValue = "Continue";
                            }
                        }

                        if(_configuration.GetValue<bool>("encryptSession") && response.SecurityUpdate)
                        {
                            var encryptKey = _configuration.GetValue<string>("encryptKey");
                            response.SessionID = new AesEncryptAndDecrypt(encryptKey).Encrypt(response.SessionID);
                        }
                    }
                    else
                    {
                        response.MPSecurityUpdateDetails.SecurityUpdateMessages = await GetNeedHelpTitleMessages(MOBMPSecurityUpdatePath.IncorrectUserDetails, false).ConfigureAwait(false);
                        response.MPSecurityUpdateDetails.MPSecurityPath = MOBMPSecurityUpdatePath.UnableToResetOnline;
                    }
                }
                #endregion
            }
        }
        private async Task<List<MOBItem>> GetNeedHelpTitleMessages(MOBMPSecurityUpdatePath securityPath, bool isErrorMsg)
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
            messages = await _securityQuestion.GetMPPINPWDTitleMessagesForMPAuth(titleList).ConfigureAwait(false);
            return messages;
        }
        private async Task<MOBCPProfile> GetMPSecurityCheckDetails(MOBMPPINPWDValidateRequest request, string token, bool getEmployeeIdFromCSLCustomerData = false)
        {
            //#region - UCB changes
            if (_configuration.GetValue<bool>("EnableUCBPhase1_MobilePhase3Changes"))
            {
                return await GetMPSecurityCheckDetailsV2(request, token).ConfigureAwait(false);
            }

            if (request == null)
            {
                throw new MOBUnitedException("Profile request cannot be null.");
            }
            MOBCPProfile mpSecurityCheckDetails = null;

            United.Services.Customer.Common.ProfileRequest profileRequest = GetPINPWDProfileRequest(request, getEmployeeIdFromCSLCustomerData);
            string jsonRequest = DataContextJsonSerializer.Serialize<United.Services.Customer.Common.ProfileRequest>(profileRequest);
            string jsonResponse = await _mpSecurityCheckDetailsService.GetMPSecurityCheckDetails(token, jsonRequest, request.TransactionId).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                United.Services.Customer.Common.ProfileResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<United.Services.Customer.Common.ProfileResponse>(jsonResponse);

                if (response != null && response.Status.Equals(United.Services.Customer.Common.Constants.StatusType.Success) && response.Profiles != null)
                {
                    MOBCPProfileRequest mobProfileRequest = null;

                    List<MOBCPProfile> profiles = await _customerProfile.PopulateProfiles(Guid.NewGuid().ToString().ToUpper().Replace("-", ""), request.MileagePlusNumber, request.CustomerID, response.Profiles, mobProfileRequest, true, application: request.Application).ConfigureAwait(false);
                    mpSecurityCheckDetails = profiles[0];
                    if (response.Profiles != null && response.Profiles[0].Travelers != null && response.Profiles[0].Travelers[0] != null && response.Profiles[0].Travelers[0].IsProfileOwner && response.Profiles[0].Travelers[0].SecurityUpdate != null)
                    {
                        mpSecurityCheckDetails.Travelers[0].MPSecurityUpdate = GetMPOwnerSecurityDetails(response.Profiles[0].Travelers[0].SecurityUpdate);
                    }
                    if (getEmployeeIdFromCSLCustomerData &&
                        response != null &&
                        response.Profiles != null &&
                        response.Profiles.Count > 0 &&
                        response.Profiles[0].Travelers != null &&
                        response.Profiles[0].Travelers.Count > 0 &&
                        response.Profiles[0].Travelers[0].EmployeeId != null)
                    {
                        mpSecurityCheckDetails.Travelers[0].EmployeeId = response.Profiles[0].Travelers[0].EmployeeId;
                    }

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
            _logger.LogInformation("GetMileagPlusPWDUpdateDetails -Client Response for get profile  client Response {@response}", mpSecurityCheckDetails);
            return mpSecurityCheckDetails;
        }

        private United.Services.Customer.Common.ProfileRequest GetPINPWDProfileRequest(MOBMPPINPWDValidateRequest mobPINPWDProfileRequest, bool getEmployeeIdFromCSLCustomerData = false)
        {
            United.Services.Customer.Common.ProfileRequest request = new United.Services.Customer.Common.ProfileRequest
            {
                DataSetting = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices"),
                CustomerId = mobPINPWDProfileRequest.CustomerID
            };
            List<string> requestStringList = new List<string>
            {
                "SecurityCheck", // This option means return Security Check Details (PWD, email or security quesitons update)
                "EmailAddresses"
            };
            if (getEmployeeIdFromCSLCustomerData)
            {
                requestStringList.Add("EmployeeLinkage");
            }
            if (_configuration.GetValue<bool>("NGRPAwardCalendarMP2017Switch"))
            {
                requestStringList.Add("CustomerMetrics");
            }
            if (!_configuration.GetValue<bool>("Keep_MREST_MP_EliteLevel_Expiration_Logic"))
            {
                requestStringList.Add("PremierLevelExpirationDate");
            }
            request.DataToLoad = requestStringList;
            request.MemberCustomerIdsToLoad = new List<int>
            {
                mobPINPWDProfileRequest.CustomerID
            };
            request.LangCode = "en-US";
            return request;
        }

        private async Task<MOBMPAccountValidation> ValidateAccountV2(int applicationId, string deviceId, string appVersion, string transactionId, string mileagePlusNumber, string pinCode, Session shopTokenSession, bool validAuthTokenHashPinCheckFailed, bool insertHashInToDB)
        {
            MOBMPAccountValidation accountValidation = new MOBMPAccountValidation();
            var tupleRes = await AuthenticateCSSTokenV2(applicationId, deviceId, appVersion, transactionId, shopTokenSession, mileagePlusNumber, pinCode, accountValidation, validAuthTokenHashPinCheckFailed, insertHashInToDB).ConfigureAwait(false);
            accountValidation = tupleRes.accountValidation;
            return accountValidation;
        }

        private async Task<(bool tokenAuthenticated, MOBMPAccountValidation accountValidation)> AuthenticateCSSTokenV2(int applicationId, string deviceId, string appVersion, string transactionId, Session shopTokenSession, string mpAccount, string pinCode, MOBMPAccountValidation accountValidation, bool validAuthTokenHashPinCheckFailed = false, bool insertHashInToDB = false)
        {
            bool tokenAuthenticated = false;

            // #region
            string anonymousToken = string.Empty;
            string mpUserName = string.Empty;
            string guidForLogEntries = shopTokenSession != null ? (string.IsNullOrEmpty(shopTokenSession.SessionId) == true ? transactionId : shopTokenSession.SessionId) : transactionId;
            DPAccessTokenResponse _DpAuthResponse = null;
            if (shopTokenSession == null && !validAuthTokenHashPinCheckFailed)
            {
                anonymousToken = await _tokenService.GetAnonymousToken(_headers.ContextValues.Application.Id, _headers.ContextValues.DeviceId, _configuration).ConfigureAwait(false);
            }
            else if (shopTokenSession != null && !shopTokenSession.IsTokenAuthenticated)
            {
                anonymousToken = shopTokenSession.Token;
            }
            bool enableCustomerCiam = await _featureSettings.GetFeatureSettingValue("EnableCustomerCiamUserType");
            string authenticateUserType = enableCustomerCiam ? _configuration.GetValue<string>("CustomerCiamUserType") 
                : _configuration.GetValue<string>("CustomerUserType");
            //string authenticateUserType = await _featureSettings.GetFeatureSettingStringValue("AuthenticatedUserType");
            //if (string.IsNullOrEmpty(authenticateUserType))
            //{
            //    authenticateUserType = _configuration.GetValue<string>("AuthenticatedUserType");
            //}
            if (!string.IsNullOrEmpty(anonymousToken))
            {
                //_DpAuthResponse = await _dataPowerFactory.GetDPAuthenticatedToken(applicationId, deviceId, transactionId, appVersion, shopTokenSession, mpAccount, pinCode, _configuration.GetValue<string>("authenticatedUserType"), anonymousToken).ConfigureAwait(false);
                _DpAuthResponse = await _dataPowerFactory.GetDPAuthenticatedToken(applicationId, deviceId, transactionId, appVersion, shopTokenSession, mpAccount, pinCode, authenticateUserType, anonymousToken).ConfigureAwait(false);
            }
            else
            {
                _DpAuthResponse = await _dataPowerFactory.GetDPAuthenticatedToken(applicationId, deviceId, transactionId, appVersion, shopTokenSession, mpAccount, pinCode, authenticateUserType, null, false).ConfigureAwait(false);
            }
            if (_DpAuthResponse != null && !_DpAuthResponse.IsDPThrownErrors)
            {
                tokenAuthenticated = true;
                //bool is4DigitPin = System.Text.RegularExpressions.Regex.IsMatch(pinCode, @"^\d{4}$"); // Regex.Match(l, @"\d{16}")

                if (mpAccount.Trim().ToUpper() != _DpAuthResponse?.MileagePlusNumber?.Trim().ToUpper())
                {
                    mpUserName = mpAccount;
                }

                accountValidation.ValidPinCode = true;

                accountValidation.MileagePlusNumber = _DpAuthResponse.MileagePlusNumber;

                if (_configuration.GetValue<bool>("EnableNewHashLogic"))
                {
                    accountValidation.HashValue = CreatePasswordDeviceHash(pinCode, deviceId);
                }
                else
                {
                    accountValidation.HashValue = CreateHash(pinCode);
                }

                //***********  "insertHashInToDB" is always false in the callee method hence below block will be ignored  **************

                if (insertHashInToDB)
                {
                    if (_configuration.GetValue<bool>("EnableNewHashLogic"))
                    {
                        await InsertUpdateOnePassValidationInfo(transactionId, accountValidation.MileagePlusNumber, pinCode, false, accountValidation.HashValue).ConfigureAwait(false);
                        await InsertUpdateMPCSSValidationInfo(_DpAuthResponse.MileagePlusNumber, mpUserName, pinCode, deviceId, applicationId, appVersion, anonymousToken.Trim(), true, DateTime.Now.AddSeconds(Convert.ToDouble(_DpAuthResponse.Expires_in)), Convert.ToDouble(_DpAuthResponse.Expires_in), false, false, accountValidation.HashValue, _DpAuthResponse.CustomerId).ConfigureAwait(false);
                    }
                    else
                    {
                        await InsertUpdateOnePassValidationInfo(transactionId, accountValidation.MileagePlusNumber, pinCode).ConfigureAwait(false);
                        await InsertUpdateMPCSSValidationInfo(_DpAuthResponse.MileagePlusNumber, mpUserName, pinCode, deviceId, applicationId, appVersion, anonymousToken.Trim(), true, DateTime.Now.AddSeconds(Convert.ToDouble(_DpAuthResponse.Expires_in)), Convert.ToDouble(_DpAuthResponse.Expires_in), false, false, String.Empty, _DpAuthResponse.CustomerId).ConfigureAwait(false);
                    }
                }

                accountValidation.CustomerId = _DpAuthResponse.CustomerId;
                accountValidation.AuthenticatedToken = _DpAuthResponse.AccessToken;
                accountValidation.TokenExpireDateTime = DateTime.Now.AddSeconds(Convert.ToDouble(_DpAuthResponse.Expires_in));
                accountValidation.TokenExpirationSeconds = Convert.ToDouble(_DpAuthResponse.Expires_in);
            }

            if (_DpAuthResponse != null && _DpAuthResponse.IsDPThrownErrors)
            {
                tokenAuthenticated = false;
                if (_DpAuthResponse != null)
                {
                    #region
                    string error = _DpAuthResponse.ErrorDescription;
                    if (error?.ToUpper().Trim() == "UserID not found".ToUpper().Trim() || error?.ToUpper().Trim() == "Password incorrect".ToUpper().Trim())
                    {
                        error = _configuration.GetValue<string>("MPValidationErrorMessage") != null ? _configuration.GetValue<string>("MPValidationErrorMessage").ToString() : "The account information you entered is incorrect.";
                    }
                    int errorCode = _DpAuthResponse.ErrorCode;
                    //int failedAttempts = Int32.Parse(_DpAuthResponse.FailedAttempts);

                    if (errorCode == -20)
                    {
                        //accountValidation.Message = "Your password is incorrect."; //**//
                        accountValidation.Message = _configuration.GetValue<string>("MPValidationErrorMessage") != null ? _configuration.GetValue<string>("MPValidationErrorMessage").ToString() : "The account information you entered is incorrect.";
                    }
                    else if (errorCode == -10)
                    {
                        //accountValidation.Message = "UserId not found."; //**//
                        accountValidation.Message = _configuration.GetValue<string>("MPValidationErrorMessage") != null ? _configuration.GetValue<string>("MPValidationErrorMessage").ToString() : "The account information you entered is incorrect.";
                    }
                    else if (errorCode == -30 || errorCode == -40)
                    {
                        accountValidation.AccountLocked = true;
                        accountValidation.Message = !string.IsNullOrEmpty(_configuration.GetValue<string>("MPAccountLockedErrorMessage"))
                            ? _configuration.GetValue<string>("MPAccountLockedErrorMessage").ToString()
                            : "You've met the maximum number of login attempts. Reset your password to access your account.";
                    }
                    else if (errorCode == -200)
                    {
                        accountValidation.DeceasedAccount = true;
                        accountValidation.Message = _configuration.GetValue<string>("MPValidationErrorMessage") != null ? _configuration.GetValue<string>("MPValidationErrorMessage").ToString() : "The account information you entered is incorrect.";
                    }
                    else if (errorCode == -110 || errorCode == -120)
                    {
                        accountValidation.ClosedAccount = true;
                        accountValidation.Message = _configuration.GetValue<string>("MPValidationErrorMessage") != null ? _configuration.GetValue<string>("MPValidationErrorMessage").ToString() : "The account information you entered is incorrect.";
                    }
                    else if (errorCode < 0 || !string.IsNullOrEmpty(error))
                    {
                        accountValidation.Message = error;
                        accountValidation.Message = _configuration.GetValue<string>("MPValidationErrorMessage") != null ? _configuration.GetValue<string>("MPValidationErrorMessage").ToString() : "The account information you entered is incorrect.";
                    }
                    #endregion
                }
                accountValidation.MileagePlusNumber = mpAccount;
            }

            //#endregion

            _logger.LogInformation("Authenticate_DP_Token {@response} {@mileagePlusNumber}", GeneralHelper.RemoveCarriageReturn(JsonConvert.SerializeObject(_DpAuthResponse)), mpAccount);
            return (tokenAuthenticated, accountValidation);
        }

        private string CreatePasswordDeviceHash(string password, string deviceId)
        {
            string salt = CreateSalt();
            string finalString = "";

            if (_configuration.GetValue<string>("NewHashSaltPhraseLength") == null)
            {
                finalString = password + deviceId + System.DateTime.Now.ToString("MMddyyyyHHmmss");
            }
            else
            {
                finalString = password;
            }

            //need to verify
            //return FormsAuthentication.HashPasswordForStoringInConfigFile(String.Concat(finalString, salt), "SHA1");
            return CreateHash(finalString);
            //return string.Empty;
        }
        private string CreateSalt()
        {
            if (_configuration.GetValue<string>("NewHashSaltPhraseLength") != null)
            {
                int hashSaltPhraseLength = _configuration.GetValue<int>("NewHashSaltPhraseLength");
                //Generate a cryptographic random number.
                System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
                byte[] buff = new byte[hashSaltPhraseLength];
                rng.GetBytes(buff);
                // Return a Base64 string representation of the random number.
                return Convert.ToBase64String(buff);
            }
            else
            {
                return _configuration.GetValue<string>("HashSaltPhrase");
            }
        }

        private async Task<(string employeeId, string displayEmployeeId)> GetEmployeeId(string transactionId, string mileagePlusNumber, string displayEmployeeId)
        {
            string employeeId = string.Empty;

            if (!string.IsNullOrEmpty(transactionId) && !string.IsNullOrEmpty(mileagePlusNumber))
            {
                var tupleRes = await _mileagePlus.GetEmployeeId(mileagePlusNumber, transactionId, _headers.ContextValues.SessionId, displayEmployeeId).ConfigureAwait(false);
                string response = tupleRes.employeeId;
                displayEmployeeId = tupleRes.displayEmployeeId;
                if (response != null)
                {
                    employeeId = response;
                }
            }

            return (employeeId, displayEmployeeId);
        }
        private async Task<bool> IsEResBetaTester(int applicatinId, string applicationVersion, string mileageplusNumber, string sessionId)
        {
            var eResBetaTesterItems = await _dynamoDBUtility.GetEResBetaTesterItems(applicatinId, applicationVersion, mileageplusNumber, sessionId).ConfigureAwait(false);

            return (eResBetaTesterItems?.MileagePlusNumber.ToUpper() == "ALL" || eResBetaTesterItems?.MileagePlusNumber == mileageplusNumber);

        }

        private bool CheckValidateMPSigInRequest(MOBMPPINPWDValidateRequest request, string passWord)
        {
            if ((!request.SignInWithTouchID && string.IsNullOrEmpty(passWord) || string.IsNullOrEmpty(request.MileagePlusNumber)))
            {
                return false;
            }
            if (request.SignInWithTouchID && string.IsNullOrEmpty(request.HashValue))
            {
                return false;
            }
            return true;
        }

        private MOBMPSecurityUpdate GetMPOwnerSecurityDetails(Services.Customer.Common.SecurityUpdate securityUpdate)
        {
            #region
            MOBMPSecurityUpdate mobSecurityUpdate = new MOBMPSecurityUpdate();
            List<MOBMPSecurityUpdatePath> mobSecurityPathList = new List<MOBMPSecurityUpdatePath>();
            foreach (string securityTypeUpdate in securityUpdate.AccountItemsToUpdate)
            {
                if (securityTypeUpdate.ToUpper().Trim() == "verifyemail".ToUpper().Trim())
                {
                    mobSecurityPathList.Add(MOBMPSecurityUpdatePath.VerifyPrimaryEmail);
                }
                else if (securityTypeUpdate.ToUpper().Trim() == "primaryemail".ToUpper().Trim())
                {
                    mobSecurityPathList.Add(MOBMPSecurityUpdatePath.NoPrimayEmailExist);
                }
                else if (securityTypeUpdate.ToUpper().Trim() == "password".ToUpper().Trim())
                {
                    mobSecurityPathList.Add(MOBMPSecurityUpdatePath.UpdatePassword);
                }
                else if (securityTypeUpdate.ToUpper().Trim() == "questions".ToUpper().Trim())
                {
                    mobSecurityPathList.Add(MOBMPSecurityUpdatePath.UpdateSecurityQuestions);
                }
            }
            mobSecurityUpdate.MPSecurityPathList = mobSecurityPathList;
            mobSecurityUpdate.DaysToCompleteSecurityUpdate = securityUpdate.DaysToCompleteSecurityUpdate;
            mobSecurityUpdate.PasswordOnlyAllowed = securityUpdate.PasswordOnlyAllowed;
            mobSecurityUpdate.UpdateLaterAllowed = securityUpdate.UpdateLaterAllowed;
            return mobSecurityUpdate;
            //throw new NotImplementedException();
            #endregion
        }

        public async Task<MOBUASubscriptions> GetUASubscriptions(MOBTFAMPDeviceRequest request)
        {
            #region GetUASubscriptions
            MOBUASubscriptions response = new MOBUASubscriptions();
            string channelId = string.Empty;
            string channelName = string.Empty;
            if (_configuration.GetValue<bool>("EnabledMERCHChannels"))
            {
                string merchChannel = "MOBBE";
                _merchandizingServices.SetMerchandizeChannelValues(merchChannel, ref channelId, ref channelName);
            }
            else
            {
                channelId = _configuration.GetValue<string>("MerchandizeOffersServiceChannelID");// "401";  //Changed per Praveen Vemulapalli email
                channelId = _configuration.GetValue<string>("MerchandizeOffersServiceChannelName");//"MBE";  //Changed per Praveen Vemulapalli email
            }
            var token = await _tokenService.GetAnonymousToken(request.Application.Id, request.DeviceId, _configuration).ConfigureAwait(false);
            response = await _merchandizingServices.GetUASubscriptions(request.MileagePlusNumber, 1, request.TransactionId, channelId, channelName, token).ConfigureAwait(false);

            return response;
            #endregion
        }

        #region
        private MileagePlusDetails OnCloudSaveData(string sessionId, string mpNumber, string HashValue, string applicationId, string appVersion
            , string deviceId, string transactionId, string authenticatedToken, string pinCode, bool isAuthTokenValid, string tokenExpirationDateTime,
           string tokenExpirationSeconds, bool isTouchID, bool isTokenAnonymous, string CustomerId)
        {

            return new MileagePlusDetails()
            {
                MileagePlusNumber = mpNumber,
                MPUserName = mpNumber,
                HashPincode = HashValue,
                PinCode = _configuration.GetValue<bool>("LogMPPinCodeOnlyForTestingAtStage") ? pinCode : string.Empty,
                ApplicationID = Convert.ToString(applicationId),
                AppVersion = appVersion,
                DeviceID = deviceId,
                IsTokenValid = Convert.ToString(isAuthTokenValid),
                TokenExpireDateTime = tokenExpirationDateTime.ToString(),
                TokenExpiryInSeconds = tokenExpirationSeconds.ToString(),
                IsTouchIDSignIn = Convert.ToString(isTouchID),
                IsTokenAnonymous = Convert.ToString(isTokenAnonymous),
                CustomerID = CustomerId.ToString(),
                DataPowerAccessToken = authenticatedToken,
                UpdateDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.FFF")
            };

        }

        /// </summary>
        public async Task<bool> OnePassValidationSaveDataInCloud(string sessionId, string mpNumber, string hashValue, string pinCode)
        {
            try
            {
                string Key = mpNumber;

                OnePassDynamoDBRequest requestData = new OnePassDynamoDBRequest
                {
                    MileagePlusNumber = mpNumber,
                    PinCode = hashValue,
                    UnhashedPinCode = string.Empty,
                    PINPWDSecurityPwdUpdate = false
                };

                bool jsonResponse = await _dynamoDBHelperService.SaveMPAppIdDeviceId<OnePassDynamoDBRequest>(requestData, sessionId, Key).ConfigureAwait(false);

                return jsonResponse;
            }
            catch (MOBUnitedException wEx)
            {
                _logger.LogError("OnePassValidationSaveDataInCloud {@exception},{@mpNumber} ", JsonConvert.SerializeObject(wEx), mpNumber);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("OnePassValidationSaveDataInCloud {@exception},{@mpNumber} ", JsonConvert.SerializeObject(ex), mpNumber);
                return false;
            }
        }

        #endregion

        #region UCB Changes
        public async Task<MOBCPProfile> GetMPSecurityCheckDetailsV2(MOBMPPINPWDValidateRequest request, string token, bool isGetEMailIDTFAMPSecurityDetails = false)
        {
            if (request == null)
            {
                throw new MOBUnitedException("Profile request cannot be null.");
            }
            MOBCPProfile mpSecurityCheckDetails = null;
            var profileRequest = new MOBCPProfileRequest()
            {
                MileagePlusNumber = request.MileagePlusNumber,
                Token = token,
                CustomerId = request.CustomerID,
                DeviceId = request.DeviceId,
                Application = request.Application,
                SessionId = Guid.NewGuid().ToString().ToUpper().Replace("-", "")
            };
            var profiles = _uCBProfile.GetProfileV2(profileRequest, true).Result;

            if (profiles != null && profiles.Count() > 0)
            {
                mpSecurityCheckDetails = profiles[0];
                if (!isGetEMailIDTFAMPSecurityDetails)
                {
                    mpSecurityCheckDetails.Travelers[0].MPSecurityUpdate = await GetMPOwnerSecurityDetailsV2(profileRequest).ConfigureAwait(false);
                }
            }
            else
            {
                throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            _logger.LogInformation("GetMPSecurityCheckDetailsV2 -Client Response for get profile {@applicaionId} {@version} {@deviceId} {@response} {@mileagePlusNumber}", request.Application.Id, request.Application.Version.Major, request.DeviceId, JsonConvert.SerializeObject(mpSecurityCheckDetails), request?.MileagePlusNumber);
            return mpSecurityCheckDetails;
        }
        private async Task<MOBMPSecurityUpdate> GetMPOwnerSecurityDetailsV2(MOBCPProfileRequest request)
        {
            string jsonResponse = await MakeHTTPGetAndLogIt(request.SessionId, request.Token, request.MileagePlusNumber).ConfigureAwait(false);
            MOBMPSecurityUpdate mobSecurityUpdate = new MOBMPSecurityUpdate();
            List<MOBMPSecurityUpdatePath> mobSecurityPathList = new List<MOBMPSecurityUpdatePath>();
            if (!string.IsNullOrEmpty(jsonResponse))
            {
                var loginProfileResponse = JsonConvert.DeserializeObject<CslResponse<Definition.CSLModels.CustomerProfile.SecurityUpdateData>>(jsonResponse);

                if (loginProfileResponse?.Data != null)
                {
                    var securityUpdate = loginProfileResponse.Data.SecurityUpdatedetails;
                    #region
                    foreach (string securityTypeUpdate in securityUpdate.AccountItemsToUpdate)
                    {
                        if (securityTypeUpdate.ToUpper().Trim() == "verifyemail".ToUpper().Trim())
                        {
                            mobSecurityPathList.Add(MOBMPSecurityUpdatePath.VerifyPrimaryEmail);
                        }
                        else if (securityTypeUpdate.ToUpper().Trim() == "primaryemail".ToUpper().Trim())
                        {
                            mobSecurityPathList.Add(MOBMPSecurityUpdatePath.NoPrimayEmailExist);
                        }
                        else if (securityTypeUpdate.ToUpper().Trim() == "password".ToUpper().Trim())
                        {
                            mobSecurityPathList.Add(MOBMPSecurityUpdatePath.UpdatePassword);
                        }
                        else if (securityTypeUpdate.ToUpper().Trim() == "questions".ToUpper().Trim())
                        {
                            mobSecurityPathList.Add(MOBMPSecurityUpdatePath.UpdateSecurityQuestions);
                        }
                    }
                    mobSecurityUpdate.MPSecurityPathList = mobSecurityPathList;
                    mobSecurityUpdate.DaysToCompleteSecurityUpdate = securityUpdate.DaysToCompleteSecurityUpdate;
                    mobSecurityUpdate.PasswordOnlyAllowed = securityUpdate.PasswordOnlyAllowed;
                    mobSecurityUpdate.UpdateLaterAllowed = securityUpdate.UpdateLaterAllowed;
                    return mobSecurityUpdate;
                    #endregion
                }
            }
            else
            {
                throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
            return mobSecurityUpdate;
        }

        private async Task<string> MakeHTTPGetAndLogIt(string sessionId, string token, string mileagePlusNumber)
        {
            return await _uCBProfileService.GetLoginSecurityUpdate(token, mileagePlusNumber, sessionId).ConfigureAwait(false);
        }
        #endregion

        //#endregion


        public async Task<bool> InsertUpdateOnePassValidationInfo(string transactionId, string mpNumber, string pinCode, bool itsHashValue = false, string hashValue = "")
        {
            bool ok = false;
            string hashedPinCode = pinCode; // CreatePasswordHash(pinCode);
            if (System.Configuration.ConfigurationManager.AppSettings["EnableNewHashLogic"] != null && System.Configuration.ConfigurationManager.AppSettings["EnableNewHashLogic"].ToString() == "true")
                hashedPinCode = hashValue;
            else
            {
                hashedPinCode = pinCode; // CreatePasswordHash(pinCode);
                if (!itsHashValue)
                {
                    hashedPinCode = CreateHash(pinCode);
                }
            }
            try
            {
                await _dynamoDBUtility.InsertMileagePlusAndHash(mpNumber, hashValue, transactionId).ConfigureAwait(false);
                ok = true;

                #region
                //Database database = DatabaseFactory.CreateDatabase("ConnectionString - iPhone");
                //DbCommand dbCommand = (DbCommand)database.GetStoredProcCommand("uasp_Insert_Update_MileagePlusAndPin");
                //database.AddInParameter(dbCommand, "@MileagePlusNumber", DbType.String, mpNumber);
                //database.AddInParameter(dbCommand, "@PinCode", DbType.String, hashedPinCode);
                //database.ExecuteNonQuery(dbCommand);
                #endregion

            }
            catch (System.Exception ex)
            {
                ok = false;
            }
            return ok;
        }

    }

}
