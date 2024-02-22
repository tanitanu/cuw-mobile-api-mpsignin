using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Ebs.Security.DataPowerGateway;
using United.Ebs.Security.Models;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal;
using United.Mobile.Model.Internal.Common;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.Common
{
    public class DataPowerFactory : IDataPowerFactory
    {
        private readonly ICacheLog<DataPowerFactory> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IResilientClient _resilientClient;
        private readonly ICachingService _cachingService;

        public DataPowerFactory([KeyFilter("dpDiscoveryDocumentConfigKey")] IResilientClient resilientClient
            , ICacheLog<DataPowerFactory> logger
            , IConfiguration configuration
            , ISessionHelperService sessionHelperService
            , ICachingService cachingService
            )
        {
            _configuration = configuration;
            _sessionHelperService = sessionHelperService;
            _logger = logger;
            _resilientClient = resilientClient;
            _cachingService = cachingService;

        }

        public async Task<bool> CheckIsDPTokenValid(string _dpAccessToken, Session shopTokenSession, string transactionId, bool SaveToPersist = true)
        {
            bool _bTokenValid = false;
            DpToken res = null;
            string LogSessionID = shopTokenSession != null ? shopTokenSession.SessionId : transactionId;

            try
            {
                //Forcely returning true since DPToken call is not made--Now Bearer token is generated from PostAsync(OAuth) call.
                _bTokenValid = true;
                shopTokenSession.IsTokenExpired = false;
                //res = dpGateway.VerifyAccessToken(_dpAccessToken);
                //if (res.Active && string.IsNullOrWhiteSpace(res.Error))
                //{
                //    _bTokenValid = true;
                //    if (shopTokenSession != null && SaveToPersist)
                //    {
                //        shopTokenSession.IsTokenExpired = false;
                //        //shopTokenSession.TokenExpireDateTime = DateTime.Now.AddSeconds(shopTokenSession.TokenExpirationValueInSeconds);  ////**==>> get the expiration from here expstr=2018-03-11T01:01:12Z
                //    }
                //}

                //else if (shopTokenSession != null)
                //{
                //    shopTokenSession.IsTokenExpired = true;
                //}
                if (shopTokenSession != null)
                {
                    await _sessionHelperService.SaveSession<Session>(shopTokenSession, shopTokenSession.SessionId, new List<string> { shopTokenSession.SessionId, shopTokenSession.ObjectName }, shopTokenSession.ObjectName).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("CheckIsDPTokenValid error {@errorMessage} {@stackTrace}", ex.Message, ex.StackTrace);
            }

            _logger.LogInformation("CheckIsDPTokenValid {@Response}", res);

            return _bTokenValid;
        }

        public async Task<DPAccessTokenResponse> GetDPAuthenticatedToken(int applicationID, string deviceId, string transactionId, string appVersion, Session TokenSessionObj, string username, string password, string usertype, string anonymousToken, bool SaveToPersist = true)
        {
            DPAccessTokenResponse _DpResponse = new DPAccessTokenResponse();
            if (applicationID != 0)
            {
                try
                {
                    var DpReqObj = GetDPRequestObject(applicationID, deviceId);
                    DpToken dpTokenResponse = null;
                    string request = string.Empty;
                    // Acquire Authenticated token
                    DpReqObj.AccessToken = anonymousToken.Replace("Bearer ", "");
                    DpReqObj.UserType = usertype;
                    DpReqObj.UserName = username;
                    DpReqObj.Password = _configuration.GetValue<string>("PassWordMask");
                    DpReqObj.GrantType = "password";

                    Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };

                    _logger.LogInformation("GetDPAuthenticatedToken {Request}", JsonConvert.SerializeObject(DpReqObj));
                    DpReqObj.Password = password;
                    string requestData = JsonConvert.SerializeObject(DpReqObj);
                    var tokenResponse = await _resilientClient.PostAsync(string.Empty, requestData, headers).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(tokenResponse))
                    {
                        _logger.LogError("GetDPAuthenticatedToken {token}", tokenResponse);
                        return default;
                    }

                    dpTokenResponse = JsonConvert.DeserializeObject<DpToken>(tokenResponse);
                    if (dpTokenResponse != null && !string.IsNullOrEmpty(dpTokenResponse.Error))
                    {
                        _DpResponse.IsDPThrownErrors = true;
                        DpErrorDetail dpError = JsonConvert.DeserializeObject<DpErrorDetail>(tokenResponse);
                        _logger.LogError("GetDPAuthenticatedTokenError {token}", tokenResponse);

                        _DpResponse.ErrorCode = Convert.ToInt32(dpError?.ErrorDetail?.ErrorCode);
                        _DpResponse.ErrorDescription = dpError?.ErrorDetail?.ErrorDescription ?? dpError?.ErrorDescription;
                        _DpResponse.FailedAttempts = dpError?.ErrorDetail?.FailedAttempts ?? string.Empty;
                        if (_DpResponse.ErrorDescription.ToLower() == "resource owner failed to authenticate")
                            throw new Exception(_DpResponse.ErrorDescription);
                    }
                    else if (dpTokenResponse != null)
                    {
                        IJwtTokenHandle idTokenHandle = DecryptIDTokenForMPInfo(dpTokenResponse.IdToken);
                        _DpResponse.CustomerId = Convert.ToInt32(idTokenHandle.CustomerId);
                        _DpResponse.MileagePlusNumber = idTokenHandle.LoyaltyId;
                        _DpResponse.Expires_in = dpTokenResponse.ExpiresIn;
                        _DpResponse.AccessToken = anonymousToken;//dpTokenResponse.TokenType + " " + dpTokenResponse.AccessToken;
                        if (TokenSessionObj != null && SaveToPersist)
                        {
                            TokenSessionObj.Token = anonymousToken;//_DpResponse.AccessToken;// we can keep using Anonymous Token as if this device isnot remembered https://csmc.qa.api.united.com/8.0/security/SecureProfile/api/ValidateDevice means first time customer signin on device we will sign out (grant_type=revoke_token) and then use the same Authenticated token
                            TokenSessionObj.MileagPlusNumber = _DpResponse.MileagePlusNumber;
                            TokenSessionObj.CustomerID = _DpResponse.CustomerId;
                            TokenSessionObj.IsTokenAuthenticated = true;
                            TokenSessionObj.IsTokenExpired = false;
                            TokenSessionObj.TokenExpirationValueInSeconds = Convert.ToDouble(_DpResponse.Expires_in);
                            TokenSessionObj.TokenExpireDateTime = DateTime.Now.AddSeconds(Convert.ToDouble(_DpResponse.Expires_in));
                            await _sessionHelperService.SaveSession<Session>(TokenSessionObj, TokenSessionObj.SessionId, new List<string> { TokenSessionObj.SessionId, TokenSessionObj.ObjectName }, TokenSessionObj.ObjectName).ConfigureAwait(false);
                        }

                        var dptoken = new DPTokenResponse
                        {
                            AccessToken = dpTokenResponse.AccessToken,
                            IdToken = dpTokenResponse.IdToken,
                            ExpiresIn = dpTokenResponse.ExpiresIn,
                            TokenType = dpTokenResponse.TokenType
                        };
                        dptoken.ConsentedOn = Convert.ToInt32(dpTokenResponse.ConsentedOn);

                        string hr = "_" + DateTime.Now.Hour.ToString();
                        string key = string.Format(_configuration.GetSection("dpTokenConfig").GetValue<string>("tokenKeyFormat"), deviceId + hr, applicationID);
                        _logger.LogInformation("Dp token {key}", key);

                        var expiry = TimeSpan.FromSeconds(dpTokenResponse.ExpiresIn - _configuration.GetSection("dpTokenConfig").GetValue<double>("tokenExpInSec"));
                        var docSaved = await _cachingService.SaveCache<DPTokenResponse>(key, dptoken, transactionId, expiry).ConfigureAwait(false);
                        #region Log Response
                        try
                        {
                            if (string.IsNullOrEmpty(_configuration.GetValue<string>("LogAuthenticationDPToken")) && _configuration.GetValue<bool>("LogAuthenticationDPToken"))
                            {
                                _logger.LogInformation("GetDPAuthenticatedToken {Response}", dpTokenResponse);
                            }
                        }
                        catch { }
                        #endregion
                    }
                    else
                    {
                        _logger.LogInformation("GetDPAuthenticatedToken {Exception}", "Data Power call returned Null!");
                    }
                }
                catch (WebException wex)
                {
                    _DpResponse.IsDPThrownErrors = true;

                    try
                    {
                        DpErrorDetail dpError = JsonConvert.DeserializeObject<DpErrorDetail>(wex.Message);

                        if (dpError != null && dpError.ErrorDetail != null)
                        {
                            _DpResponse.ErrorCode = Convert.ToInt32(dpError?.ErrorDetail?.ErrorCode);
                            _DpResponse.ErrorDescription = dpError?.ErrorDetail?.ErrorDescription;
                            _DpResponse.FailedAttempts = dpError?.ErrorDetail?.FailedAttempts ?? string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        #region Exception Log

                        if (string.IsNullOrEmpty(_configuration.GetValue<string>("LogAuthenticationDPToken")) && Convert.ToBoolean(_configuration.GetValue<bool>("LogAuthenticationDPToken")))
                        {
                            _logger.LogInformation("GetDPAuthenticatedToken {Exception}", ex.Message.ToString() + " :: " + ex.StackTrace.ToString());
                        }
                        throw ex;
                        #endregion

                    }
                }
                catch (Exception ex)
                {
                    #region Exception Log

                    if (string.IsNullOrEmpty(_configuration.GetValue<string>("LogAuthenticationDPToken")) && Convert.ToBoolean(_configuration.GetValue<bool>("LogAuthenticationDPToken")))
                    {
                        _logger.LogInformation("GetDPAuthenticatedToken {Exception}", ex.Message.ToString() + " :: " + ex.StackTrace.ToString());
                    }

                    throw ex;
                    #endregion
                }
            }
            return _DpResponse;
        }

        private DpRequest GetDPRequestObject(int applicationID, string deviceId, string configSectionKey = "dpTokenRequest")
        {
            DpRequest dpRequest = null;
            switch (applicationID)
            {
                case 1:
                    dpRequest = new DpRequest
                    {
                        GrantType = _configuration.GetSection(configSectionKey).GetValue<string>("grantType"),
                        ClientId = _configuration.GetSection(configSectionKey).GetSection("ios").GetValue<string>("clientId"),
                        ClientSecret = _configuration.GetSection(configSectionKey).GetSection("ios").GetValue<string>("clientSecret"),
                        Scope = _configuration.GetSection(configSectionKey).GetSection("ios").GetValue<string>("clientScope"),
                        UserType = _configuration.GetSection(configSectionKey).GetValue<string>("userType"),
                        EndUserAgentId = _configuration.GetSection(configSectionKey).GetValue<string>("endUserAgentID"),
                        EndUserAgentIP = _configuration.GetSection(configSectionKey).GetValue<string>("endUserAgentIP")
                    };
                    break;

                case 2:
                    dpRequest = new DpRequest
                    {
                        GrantType = _configuration.GetSection(configSectionKey).GetValue<string>("grantType"),
                        ClientId = _configuration.GetSection(configSectionKey).GetSection("android").GetValue<string>("clientId"),
                        ClientSecret = _configuration.GetSection(configSectionKey).GetSection("android").GetValue<string>("clientSecret"),
                        Scope = _configuration.GetSection(configSectionKey).GetSection("android").GetValue<string>("clientScope"),
                        UserType = _configuration.GetSection(configSectionKey).GetValue<string>("userType"),
                        EndUserAgentId = _configuration.GetSection(configSectionKey).GetValue<string>("endUserAgentID"),
                        EndUserAgentIP = _configuration.GetSection(configSectionKey).GetValue<string>("endUserAgentIP")
                    };
                    break;
                default:
                    break;
            }

            return dpRequest;
        }

        private IJwtTokenHandle DecryptIDTokenForMPInfo(string idToken)
        {
            IJwtTokenHandle idTokenHandle = new JwtTokenHandle(idToken);
            return idTokenHandle;
        }

        public async Task<string> GetAnonymousCSSToken(int applicationId, string deviceId, string appVersion, string transactionId, Session persistToken, bool SaveToPersist = true, bool SaveToDataBase = false, string mpNumber = "", string customerID = "")
        {
            string token = string.Empty;
            if (applicationId != 0)
            {
                var DpReqObj = GetDPRequestObject(applicationId, deviceId);

                DpToken dpTokenResponse = null;
                List<LogEntry> _dpTokenlogEntries = new List<LogEntry>();
                string request = string.Empty;
                string applicationSessionId = "";
                try
                {
                    applicationSessionId = persistToken.SessionId;
                }
                catch { }

                try
                {
                    // Acquire anonymous token
                    Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };
                    string requestData = JsonConvert.SerializeObject(DpReqObj);
                    var response = await _resilientClient.PostAsync(string.Empty, requestData, headers).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(response))
                    {
                        _logger.LogError("GetAnonymousCSSToken errors {@response} and {@requestData} ", response, requestData);
                        return default;
                    }
                    dpTokenResponse = JsonConvert.DeserializeObject<DpToken>(response);

                    if (dpTokenResponse != null)
                    {
                        token = dpTokenResponse.TokenType + " " + dpTokenResponse.AccessToken;
                        if (SaveToPersist)
                        {
                            if (persistToken != null)// For Shopping.Session
                            {
                                persistToken.Token = token;
                                persistToken.IsTokenExpired = false;
                                persistToken.IsTokenAuthenticated = false; // As this toke in annonymous token
                                persistToken.TokenExpirationValueInSeconds = Convert.ToDouble(dpTokenResponse.ExpiresIn);
                                persistToken.TokenExpireDateTime = DateTime.Now.AddSeconds(Convert.ToDouble(dpTokenResponse.ExpiresIn));
                                await _sessionHelperService.SaveSession<Session>(persistToken, persistToken.SessionId, new List<string> { persistToken.SessionId, persistToken.ObjectName }, persistToken.ObjectName).ConfigureAwait(false);
                            }
                        }
                        if (SaveToDataBase)// Added for Promotions Hot Fix
                        {
                            // Utility.UpdateMileagePlusCSSToken(mpNumber, deviceId, applicationId, appVersion, token,
                            //     true, DateTime.Now.AddSeconds(Convert.ToDouble(dpTokenResponse.ExpiresIn)),
                            //     Convert.ToDouble(dpTokenResponse.ExpiresIn), false, Convert.ToInt64(customerID.Trim()));
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError("GetAnonymousCSSToken errors {@errorMessage} and {@stackTrace} ", ex.Message, ex.StackTrace);

                    throw ex;
                }

            }
            return await Task.FromResult<string>(token).ConfigureAwait(false);
        }
    }
}
