using Css.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Ebs.Logging.Enrichers;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.Model.Common;
using United.Mobile.Model.Common.CloudDynamoDB;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.Internal.Exception;
using United.Mobile.Model.Shopping;
using United.Utility.Enum;
using United.Utility.Helper;

namespace United.Common.Helper
{
    public class ShoppingSessionHelper : IShoppingSessionHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IDynamoDBHelperService _dynamoDBHelperService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataPowerFactory _dataPowerFactory;
        private readonly IHeaders _headers;
        private readonly ICacheLog<ShoppingSessionHelper> _logger;
        private readonly ICachingService _cachingService;
        private Session _session;
        private readonly IApplicationEnricher _requestEnricher;
        private readonly IHashPin _HashPin;


        public ShoppingSessionHelper(IConfiguration configuration
            , ISessionHelperService sessionHelperService
            , IDynamoDBHelperService dynamoDBHelperService
            , IHttpContextAccessor httpContextAccessor
            , IDataPowerFactory dataPowerFactory
            , IApplicationEnricher requestEnricher
            , IHeaders headers
            , ICacheLog<ShoppingSessionHelper> logger
            , ICachingService cachingService
            , IHashPin hashPin)
        {
            _configuration = configuration;
            _sessionHelperService = sessionHelperService;
            _dynamoDBHelperService = dynamoDBHelperService;
            _httpContextAccessor = httpContextAccessor;
            _dataPowerFactory = dataPowerFactory;
            _requestEnricher = requestEnricher;
            _headers = headers;
            _logger = logger;
            _cachingService = cachingService;
            _HashPin = hashPin;
        }

        public async Task<Session> CreateShoppingSession(int applicationId, string deviceId, string appVersion, string transactionId, string mileagPlusNumber, string employeeId, bool isBEFareDisplayAtFSR = false, bool isReshop = false, bool isAward = false, string travelType = "", string flow = "", string hashPin = "")
        {
            var session = new Session
            {
                DeviceID = deviceId,
                AppID = applicationId,
                SessionId = Guid.NewGuid().ToString().ToUpper().Replace("-", ""),
                CreationTime = DateTime.Now,
                LastSavedTime = DateTime.Now,
                MileagPlusNumber = mileagPlusNumber,
                IsBEFareDisplayAtFSR = isBEFareDisplayAtFSR,
                IsReshopChange = isReshop,
                IsAward = isAward,
                EmployeeId = employeeId
            };

            if (_configuration.GetValue<bool>("EnableCorporateLeisure"))
            {
                session.TravelType = travelType;
            }
            #region //**// LMX Flag For AppID change
            bool supressLMX = false;
            bool.TryParse(_configuration.GetValue<string>("SupressLMX"), out supressLMX); // ["SupressLMX"] = true to make all Apps Turn off. ["SupressLMX"] = false then will check for each app as below.
            if (!supressLMX && _configuration.GetValue<string>("AppIDSToSupressLMX") != null && _configuration.GetValue<string>("AppIDSToSupressLMX").ToString().Trim() != "")
            {
                string appIDS = _configuration.GetValue<string>("AppIDSToSupressLMX"); // AppIDSToSupressLMX = ~1~2~3~ or ~1~ or empty to allow lmx to all apps
                supressLMX = appIDS.Contains("~" + applicationId.ToString() + "~");
            }
            session.SupressLMXForAppID = supressLMX;
            #endregion
            var isValidToken = false;
            if (!string.IsNullOrEmpty(mileagPlusNumber))
            {
                if (flow == FlowType.BOOKING.ToString() && EnableVulnerabilityCheck("EnableVulnerabilityCheck", isReshop, applicationId, appVersion, _configuration.GetValue<string>("AndroidVulnerabilityCheckVersion"), _configuration.GetValue<string>("iPhoneVulnerabilityCheckVersion")))
                {
                    bool validAuthCall = await Authorize(session.SessionId, applicationId, appVersion, deviceId, mileagPlusNumber, hashPin).ConfigureAwait(false);
                    if (!validAuthCall) throw new MOBUnitedException(_configuration.GetValue<string>("VulnerabilityErrorMessage"));
                }
                var tupleRes = await GetMPAuthToken(mileagPlusNumber, applicationId, deviceId, appVersion, session).ConfigureAwait(false);
                session.Token = tupleRes.validAuthToken;
                session = tupleRes.shopSession;
                var refreshShopTokenIfLoggedInTokenExpInThisMinVal = _configuration.GetValue<string>("RefreshShopTokenIfLoggedInTokenExpInThisMinVal") ?? "";
                if (string.IsNullOrEmpty(refreshShopTokenIfLoggedInTokenExpInThisMinVal))
                {
                    if (!string.IsNullOrEmpty(session.Token))
                    {
                        var tupleResponse = await CheckIsCSSTokenValid(applicationId, deviceId, appVersion, transactionId, session, string.Empty).ConfigureAwait(false);
                        isValidToken = tupleResponse.isTokenValid;
                        session = tupleResponse.shopTokenSession;
                    }
                }
                else
                {
                    var tupleResponse = await isValidTokenCheckWithExpireTime(applicationId, deviceId, appVersion, transactionId, session, refreshShopTokenIfLoggedInTokenExpInThisMinVal).ConfigureAwait(false);
                    isValidToken = tupleResponse.isValidToken;
                    session = tupleResponse.session;
                }
            }

            if (!string.IsNullOrEmpty(session?.SessionId) && _headers.ContextValues != null)
            {
                _headers.ContextValues.SessionId = session.SessionId;
                _requestEnricher.Add(United.Mobile.Model.Constants.SessionId, session.SessionId);
            }

            if (isValidToken)
                return session;

            //var token = await _dPService.GetAndSaveAnonymousToken(applicationId, deviceId, _configuration, "dpTokenRequest", session).ConfigureAwait(false);
            var token = await _dataPowerFactory.GetAnonymousCSSToken(applicationId, deviceId, appVersion, session.SessionId, session).ConfigureAwait(false);

            //await _sessionHelperService.SaveSession<Session>(session, session.SessionId, new List<string> { session.SessionId, session.ObjectName }, session.ObjectName).ConfigureAwait(false);
            session.Token = token;
            _session = session;
            return session;
        }

        private bool EnableVulnerabilityCheck(string enableConfigKey, bool isReshop, int applicationId, string appVersion, string androidVersionKey, string iOSVersionKey)
        {
            return _configuration.GetValue<bool>(enableConfigKey) && !isReshop
            && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, androidVersionKey, iOSVersionKey);
        }
        private async Task<(string validAuthToken, Session shopSession)> GetMPAuthToken(string mileagPlusNumber, int applicationId, string deviceId, string appVersion, Session shopSession)
        {
            string validAuthToken = string.Empty;
            try
            {
                var MPAuthToken = await _dynamoDBHelperService.GetAuthToken<MileagePlusDetails>(mileagPlusNumber, applicationId, deviceId, shopSession.SessionId).ConfigureAwait(false);
                if (MPAuthToken != null)
                {
                    validAuthToken = (_configuration.GetValue<bool>("EnableDPToken")) ? MPAuthToken.DataPowerAccessToken : MPAuthToken.AuthenticatedToken;
                    shopSession.Token = validAuthToken;
                    shopSession.IsTokenAuthenticated = true; // if the token is cached with MP at DB means its authenticated
                    shopSession.TokenExpirationValueInSeconds = Convert.ToDouble(MPAuthToken.TokenExpiryInSeconds);
                    shopSession.TokenExpireDateTime = Convert.ToDateTime(MPAuthToken.TokenExpireDateTime);
                    //  shopSession.CustomerID = Convert.ToInt32(MPAuthToken.CustomerID);
                }

            }
            catch (Exception ex) { string msg = ex.Message; }

            return (validAuthToken, shopSession);
        }

        private async Task<(bool isValidToken, Session session)> isValidTokenCheckWithExpireTime(int applicationId, string deviceId, string appVersion, string transactionId, Session session, string refreshShopTokenIfLoggedInTokenExpInThisMinVal)
        {
            bool isValidToken = false;
            try
            {
                if (!string.IsNullOrEmpty(session.Token) && session.TokenExpireDateTime.Subtract(DateTime.Now).TotalMinutes > Convert.ToInt32(refreshShopTokenIfLoggedInTokenExpInThisMinVal))
                {
                    var tupleResponse = await CheckIsCSSTokenValid(applicationId, deviceId, appVersion, transactionId, session, string.Empty).ConfigureAwait(false);
                    isValidToken = tupleResponse.isTokenValid;
                    session = tupleResponse.shopTokenSession;
                }
            }
            catch
            {
                if (!string.IsNullOrEmpty(session.Token))
                {
                    var tupleResponse = await CheckIsCSSTokenValid(applicationId, deviceId, appVersion, transactionId, session, string.Empty).ConfigureAwait(false);
                    isValidToken = tupleResponse.isTokenValid;
                    session = tupleResponse.shopTokenSession;
                }
            }

            return (isValidToken, session);
        }

        public async Task<(bool isTokenValid, Session shopTokenSession)> CheckIsCSSTokenValid(int applicationId, string deviceId, string appVersion, string transactionId, Session shopTokenSession, string tokenToValidate)
        {
            bool isTokenValid = false;
            bool iSDPAuthentication = _configuration.GetValue<bool>("EnableDPToken");
            if (iSDPAuthentication)
            {
                #region TFS 53524 - Added log for trace United Airlines sending loopback address for iOS apps 23/08/2016 - Srinivas - Auguest 23
                _logger.LogInformation("CheckIsCSSTokenValid HostIPAddress ", GetClientIPAddress());
                #endregion
                isTokenValid = await _dataPowerFactory.CheckIsDPTokenValid(shopTokenSession.Token, shopTokenSession, transactionId).ConfigureAwait(false);
            }
            else
            {
                #region Get Aplication and Profile Ids

                string request = string.Empty, guidForLogEntries = shopTokenSession != null ? (string.IsNullOrEmpty(shopTokenSession.SessionId) == true ? transactionId : shopTokenSession.SessionId) : transactionId;

                System.Guid appID = new Guid("643e1e47-1242-4b6c-ab7e-64024e4bc84c"); // default App Id
                System.Guid profID = new Guid("114bfe84-cc04-49b6-8d28-74294f1d21fc"); // default Profile Id
                try
                {
                    string[] cSSAuthenticationTokenServiceApplicationIDs = _configuration.GetValue<string>("CSSAuthenticationTokenServiceApplicationIDs").Split('|');
                    foreach (string applicationID in cSSAuthenticationTokenServiceApplicationIDs)
                    {
                        if (Convert.ToInt32(applicationID.Split('~')[0].ToString().ToUpper().Trim()) == applicationId)
                        {
                            appID = new Guid(applicationID.Split('~')[1].ToString().Trim());
                            break;
                        }
                    }
                    string[] cSSAuthenticationTokeServicenProfileIDs = _configuration.GetValue<string>("CSSAuthenticationTokenServiceProfileIDs").Split('|');
                    foreach (string profileID in cSSAuthenticationTokeServicenProfileIDs)
                    {
                        if (Convert.ToInt32(profileID.Split('~')[0].ToString().ToUpper().Trim()) == applicationId)
                        {
                            profID = new Guid(profileID.Split('~')[1].ToString().Trim());
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);
                    _logger.LogError("GetFLIFOSecurityToken Exception {errorMessage}", exceptionWrapper);
                }
                #endregion
                #region TFS 53524 - Added log for trace United Airlines sending loopback address for iOS apps 23/08/2016 - Srinivas - Auguest 23

                _logger.LogInformation("CheckIsCSSTokenValid HostIPAddress ", GetClientIPAddress());

                #endregion
                #region
                List<LogEntry> cssTokenlogEntries = new List<LogEntry>();
                string securityUrl = _configuration.GetValue<string>("CSSAuthenticationTokenGeneratorURL");
                var client = new Css.ChannelProxy.Client(securityUrl);
                List<Metadata> metadata = new List<Metadata>();
                RequestAttributeSet set = new RequestAttributeSet();
                set.Attributes.Add("RequestAttribute_DeviceId", deviceId);
                set.Attributes.Add("RequestAttribute_MobileAppID", applicationId.ToString());

                //As per Venkat this Flag needs to check to avoid multiple ip logging for same user
                //set.Attributes.Add("RequestAttribute_ClientIP", GetClientIPAddress());
                IsClientIP(set);
                set.Attributes.Add("RequestAttribute_Browser", _configuration.GetValue<string>("RequestAttribute_Browser"));
                set.Attributes.Add("RequestAttribute_BrowserPlatform", _configuration.GetValue<string>("RequestAttribute_BrowserPlatform"));
                set.Attributes.Add("RequestAttribute_BrowserVersion", _configuration.GetValue<string>("RequestAttribute_BrowserVersion"));
                set.Attributes.Add("RequestAttribute_Url", _configuration.GetValue<string>("RequestAttribute_Url"));
                metadata.Add(new Metadata("RequestAttributeSet", set.Serialize()));

                request = "Token Generated for Mobile AppID = " + applicationId.ToString() + "|Token Service Application GUID = " + appID + "| Profile GUID = " + profID;
                _logger.LogInformation("Validate_Authenticate_CSS_Token Request", request);

                if (shopTokenSession != null)
                {
                    tokenToValidate = shopTokenSession.Token;
                }
                AcquireSessionContextCallWrapper scWrapper = client.AcquireSessionContext(appID, new Guid(tokenToValidate), true, true, true, true, metadata);
                //if (scWrapper.CallAuthenticationOperationResult == CallAuthenticationResult.Success && scWrapper.CallAuthorizationOperationResult == CallAuthorizationResult.Success && scWrapper.RequestIntegrityCheckOperationResult == RequestIntegrityCheckResult.Valid && (scWrapper.UseTokenValidationResult == UseTokenValidationResult.Valid || scWrapper.UseTokenValidationResult == UseTokenValidationResult.ValidAndExtended) && (scWrapper.SessionValidationResult == SessionValidationResult.Valid || scWrapper.SessionValidationResult == SessionValidationResult.ValidAndExtended))
                if (scWrapper.RequestIntegrityCheckOperationResult == RequestIntegrityCheckResult.Valid && (scWrapper.UseTokenValidationResult == UseTokenValidationResult.Valid || scWrapper.UseTokenValidationResult == UseTokenValidationResult.ValidAndExtended)) // As per Marwan Less check the better so as per his reply changed
                {
                    isTokenValid = true;
                    if (shopTokenSession != null)
                    {
                        shopTokenSession.IsTokenExpired = false;
                        shopTokenSession.TokenExpireDateTime = DateTime.Now.AddSeconds(shopTokenSession.TokenExpirationValueInSeconds);
                        int customerID = 0;
                        int.TryParse(scWrapper.UserName, out customerID);
                        shopTokenSession.CustomerID = customerID;
                    }
                }
                else
                {
                    isTokenValid = false;
                    if (shopTokenSession != null)
                    {
                        shopTokenSession.IsTokenExpired = true;
                    }
                }
                if (shopTokenSession != null)
                {

                    await _sessionHelperService.SaveSession<Session>(shopTokenSession, shopTokenSession.SessionId, new List<string> { shopTokenSession.SessionId, shopTokenSession.ObjectName }, shopTokenSession.ObjectName).ConfigureAwait(false);
                }
                #endregion

                _logger.LogInformation("Validate_Authenticate_CSS_Token Response", scWrapper);

            }
            return (isTokenValid, shopTokenSession);
        }

        private void IsClientIP(RequestAttributeSet set)
        {
            if (_configuration.GetValue<string>("Get_ClientIP") == null || _configuration.GetValue<bool>("Get_ClientIP") == true)
            {
                set.Attributes.Add("RequestAttribute_ClientIP", GetClientIPAddress());
            }
            else
            {
                set.Attributes.Add("RequestAttribute_ClientIP", _configuration.GetValue<string>("RequestAttribute_ClientIP"));
            }
        }

        private string GetClientIPAddress()
        {
            string clientIP = string.Empty;
            //checking network availability for getting client host ipAddress.
            if (!string.IsNullOrEmpty(_httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString()))
            {
                clientIP = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            else
            {   //Assigning empty client ip address, if there is no network
                clientIP = _configuration.GetValue<string>("RequestAttribute_ClientIP");
            }
            return clientIP;
        }


        private async Task<Session> GetBookingShoppingSessionV2(string sessionId, bool isBookingFlow)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
            }
            Session session = new Session();
            session = await _sessionHelperService.GetSession<Session>(sessionId, session.ObjectName, new List<string> { sessionId, session.ObjectName }).ConfigureAwait(false);
            if (session == null)
            {
                if (isBookingFlow)
                    throw new MOBUnitedException(_configuration.GetValue<string>("BookingSessionExpiryMessageExceptionCode"), _configuration.GetValue<string>("BookingSessionExpiryMessage"));
                else
                    throw new MOBUnitedException(_configuration.GetValue<string>("GeneralSessionExpiryMessageExceptionCode"), _configuration.GetValue<string>("GeneralSessionExpiryMessage"));
            }
            if (GetTokenExpireDateTimeUTC(session))
            {
                session.IsTokenExpired = true;
                throw new MOBUnitedException(_configuration.GetValue<string>("BookingSessionExpiryMessageExceptionCode"), _configuration.GetValue<string>("BookingSessionExpiryMessage"));
            }

            session.LastSavedTime = DateTime.Now;

            await _sessionHelperService.SaveSession<Session>(session, sessionId, new List<string>() { sessionId, session.ObjectName }, session.ObjectName).ConfigureAwait(false);

            return await Task.FromResult(session).ConfigureAwait(false);
        }

        private async Task<Session> GetBookingShoppingSession(string sessionId, bool isBookingFlow)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
            }
            Session session = new Session();
            session = await _sessionHelperService.GetSession<Session>(sessionId, session.ObjectName, new List<string> { sessionId, session.ObjectName }).ConfigureAwait(false);
            if (session == null)
            {
                if (isBookingFlow)
                    throw new MOBUnitedException(_configuration.GetValue<string>("BookingSessionExpiryMessage"));
                else
                    throw new MOBUnitedException(_configuration.GetValue<string>("GeneralSessionExpiryMessage"));
            }
            if (GetTokenExpireDateTimeUTC(session))
            {
                session.IsTokenExpired = true;
                throw new MOBUnitedException(_configuration.GetValue<string>("BookingSessionExpiryMessage"));
            }
            session.LastSavedTime = DateTime.Now;

            await _sessionHelperService.SaveSession<Session>(session, sessionId, new List<string> { sessionId, session.ObjectName }, session.ObjectName).ConfigureAwait(false);

            return session;
        }

        private bool GetTokenExpireDateTimeUTC(Session session)
        {
            var returnVal = false;
            try
            {
                DateTime localDateTime;

                var result = DateTime.TryParse(session.CreationTime.ToString(), out localDateTime);
                if (result)
                {
                    var expDatetime = localDateTime.AddSeconds(session.TokenExpirationValueInSeconds);
                    returnVal = (expDatetime.ToUniversalTime() <= DateTime.UtcNow && expDatetime != DateTime.MinValue);
                }


            }
            catch (FormatException)
            {
            }
            return returnVal;
        }

        private async Task<Session> GetShoppingSession(string sessionId, bool saveToPersist)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
            }

            Session session = new Session();
            session = await _sessionHelperService.GetSession<Session>(sessionId, session.ObjectName, new List<string> { sessionId, session.ObjectName }).ConfigureAwait(false);
            if (session == null)
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
            if (session.TokenExpireDateTime <= DateTime.Now)
            {
                session.IsTokenExpired = true;
            }

            session.LastSavedTime = DateTime.Now;

            await _sessionHelperService.SaveSession<Session>(session, sessionId, new List<string> { sessionId, session.ObjectName }, session.ObjectName).ConfigureAwait(false);
            return session;
        }

        public async Task<Session> GetShoppingSession(string sessionId)
        {
            return await GetShoppingSession(sessionId, true).ConfigureAwait(false);
        }

        private async Task<bool> Authorize(string sessionId, int applicationId, string applicationVersion, string deviceId, string mileagePlusNumber, string hash)
        {
            bool validateMPHashpinAuthorize = false;
            string validAuthToken = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(mileagePlusNumber) && !string.IsNullOrEmpty(deviceId) && !string.IsNullOrEmpty(hash))
                {

                    if (await ValidateMileagePlusRecordInCouchbase(sessionId, mileagePlusNumber, hash, deviceId, applicationId, applicationVersion).ConfigureAwait(false))
                    {
                        validateMPHashpinAuthorize = true;
                    }
                    else if (await ValidateHashPinAndGetAuthToken(mileagePlusNumber, hash, applicationId, deviceId, applicationId.ToString()).ConfigureAwait(false))
                    {
                        validateMPHashpinAuthorize = true;

                    }
                }

                return validateMPHashpinAuthorize;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("AuthorizingCustomer {@Exception}", JsonConvert.SerializeObject(ex));
                return validateMPHashpinAuthorize;
            }

        }

        private async Task<bool> ValidateMileagePlusRecordInCouchbase(string sessionId, string mileagePlusNumber, string hash, string deviceId, int applicationId, string applicationVersion)
        {
            bool validateMPHashpin = false;

            try
            {
                if (!string.IsNullOrEmpty(mileagePlusNumber) && !string.IsNullOrEmpty(deviceId))
                {
                    string mileagePlusNumberKey = GetMileagePlusAuthorizationPredictableKey(mileagePlusNumber, deviceId, applicationId);

                    var customerAuthorizationRecord = await _cachingService.GetCache<CustomerAuthorization>(mileagePlusNumberKey, _headers.ContextValues.TransactionId).ConfigureAwait(false);
                    if (customerAuthorizationRecord != null)
                    {
                        var mileagePlusAuthorizationRecord = JsonConvert.DeserializeObject<CustomerAuthorization>(customerAuthorizationRecord);

                        if (mileagePlusAuthorizationRecord != null && !string.IsNullOrEmpty(mileagePlusAuthorizationRecord.Hash) && mileagePlusAuthorizationRecord.Hash.Equals(hash))
                        {
                            validateMPHashpin = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("ValidateMPHashDeviceIDCheckInCouchbase {@Exception}", JsonConvert.SerializeObject(ex));
            }

            return validateMPHashpin;
        }

        private string GetMileagePlusAuthorizationPredictableKey(string mileagePlus, string deviceId, int applicationId)
        {
            return string.Format("MileagePlusAuthorization::{0}@{1}::{2}", mileagePlus, deviceId, applicationId);
        }

        public async Task<bool> ValidateHashPinAndGetAuthToken(string accountNumber, string hashPinCode, int applicationId, string deviceId, string appVersion)
        {
            var list = await _HashPin.ValidateHashPinAndGetAuthTokenDynamoDB(accountNumber,0, hashPinCode, applicationId, deviceId, appVersion, _headers.ContextValues.SessionId).ConfigureAwait(false);

            var ok = (list != null && !string.IsNullOrEmpty(list.HashPincode)) ? true : false;

            return ok;
        }

        public async Task<(bool returnValue, string validAuthToken)> ValidateHashPinAndGetAuthToken(string profileNumber, string hashPinCode, int applicationId, string deviceId, string appVersion, string validAuthToken, string transactionId, string sessionId)
        {
            bool rtnValue = false;
            _logger.LogInformation("ValidateHashPinAndGetAuthToken -  {@ProfileNumber} {@HashPinCode}", profileNumber, hashPinCode);
            var response = await _HashPin.ValidateHashPinAndGetAuthTokenDynamoDB(profileNumber,0, hashPinCode, applicationId, deviceId, appVersion).ConfigureAwait(false);
            if (response != null && response.IsTokenValid != null && response.HashPincode == hashPinCode)
            {
                validAuthToken = response.AuthenticatedToken?.ToString();
                if (response.IsTokenValid.ToLower() == "true")
                {
                    rtnValue = true;
                }
            }

            return (rtnValue, validAuthToken);
        }

        public async Task<United.Mobile.Model.EmployeeReservation.Session> CreateEmpShoppingSession(string sessionId, string mpNumber, string employeeId, string eResTransactionId, string eResSessionId)
        {
            United.Mobile.Model.EmployeeReservation.Session session = new United.Mobile.Model.EmployeeReservation.Session
            {
                SessionId = string.IsNullOrEmpty(sessionId) ? System.Guid.NewGuid().ToString().ToUpper().Replace("-", "") : sessionId
            };
            session.CreationTime = session.LastSavedTime = DateTime.Now;
            session.MpNumber = mpNumber;
            session.EmployeeId = employeeId;
            session.EResTransactionId = eResTransactionId;
            session.EResSessionId = eResSessionId;
            session.IsTokenExpired = false;
            await _sessionHelperService.SaveSession<United.Mobile.Model.EmployeeReservation.Session>(session, session.SessionId, new List<string> { session.SessionId, session.ObjectName }, session.ObjectName).ConfigureAwait(false);

            return session;
        }
    }
}
