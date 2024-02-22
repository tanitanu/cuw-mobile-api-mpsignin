using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Mobile.Model.Internal;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.Common
{
    public class DPService : IDPService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<DPService> _logger;
        private readonly ICachingService _cachingService;
        private readonly IHeaders _headers;

        public DPService([KeyFilter("dpTokenConfigKey")] IResilientClient resilientClient
            , ICacheLog<DPService> logger
            , ICachingService cachingService
            , IHeaders headers)
        {
            _resilientClient = resilientClient;
            _logger = logger;
            _cachingService = cachingService;
            _headers = headers;
        }
        public async Task<string> GetAnonymousToken(int applicationId, string deviceId, IConfiguration configuration)
        {
            return await GetAnonymousToken(applicationId, deviceId, configuration, "dpTokenRequest").ConfigureAwait(false);
        }

        public async Task<string> GetAnonymousToken(int applicationId, string deviceId, IConfiguration configuration, string configSectionKey)

        {
            string dpTokenResponse = string.Empty;
            string hr = "_" + DateTime.Now.Hour.ToString();
            string key = string.Format(configuration.GetSection("dpTokenConfig").GetValue<string>("tokenKeyFormat"), deviceId + hr, applicationId);
            _logger.LogInformation("Dp token {key}", key);
            try
            {
                var token = await _cachingService.GetCache<DPTokenResponse>(key, _headers.ContextValues?.TransactionId).ConfigureAwait(false);
                var dptoken = JsonConvert.DeserializeObject<DPTokenResponse>(token);

                if (dptoken == null)
                {
                    _logger.LogWarning("Warning while getting the DPToken from caching service: {Key}", key);
                    var dpRequest = GetDPRequest(applicationId, configuration, configSectionKey);
                    Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };
                    string requestData = JsonConvert.SerializeObject(dpRequest);
                    token = await _resilientClient.PostAsync(string.Empty, requestData, headers).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogError("Dp token errors {token} and {requestData} ", token, requestData);
                        return null;
                    }
                    dptoken = JsonConvert.DeserializeObject<DPTokenResponse>(token);
                    var expiry = TimeSpan.FromSeconds(dptoken.ExpiresIn - configuration.GetSection("dpTokenConfig").GetValue<double>("tokenExpInSec"));
                    var docSaved = await _cachingService.SaveCache<DPTokenResponse>(key, dptoken, _headers.ContextValues?.TransactionId, expiry).ConfigureAwait(false);
                }

                if (dptoken != null)
                {
                    dpTokenResponse = $"{dptoken.TokenType} {dptoken.AccessToken}";
                }
                _logger.LogInformation("Dp token {@token}", dptoken);
                return dpTokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError("Dp token errors {@error}", ex);
                return null;
            }
        }

        public DPTokenRequest GetDPRequest(int applicationId, IConfiguration configuration, string configSectionKey = "dpTokenRequest")
        {
            DPTokenRequest dpRequest = null;

            switch (applicationId)
            {
                case 1:
                    dpRequest = new DPTokenRequest
                    {
                        GrantType = configuration.GetSection(configSectionKey).GetValue<string>("grantType"),
                        ClientId = configuration.GetSection(configSectionKey).GetSection("ios").GetValue<string>("clientId"),
                        ClientSecret = configuration.GetSection(configSectionKey).GetSection("ios").GetValue<string>("clientSecret"),
                        Scope = configuration.GetSection(configSectionKey).GetSection("ios").GetValue<string>("clientScope"),
                        UserType = configuration.GetSection(configSectionKey).GetValue<string>("userType"),
                    };
                    break;

                case 2:
                    dpRequest = new DPTokenRequest
                    {
                        GrantType = configuration.GetSection(configSectionKey).GetValue<string>("grantType"),
                        ClientId = configuration.GetSection(configSectionKey).GetSection("android").GetValue<string>("clientId"),
                        ClientSecret = configuration.GetSection(configSectionKey).GetSection("android").GetValue<string>("clientSecret"),
                        Scope = configuration.GetSection(configSectionKey).GetSection("android").GetValue<string>("clientScope"),
                        UserType = configuration.GetSection(configSectionKey).GetValue<string>("userType"),
                    };
                    break;

                default:
                    break;
            }
            return dpRequest;
        }

        public async Task<DPTokenResponse> GetSSOToken(int applicationId, string mpNumber, IConfiguration configuration, string configSSOSectionKey = "dpSSOTokenOption", string dpSSOConfig = "dpSSOTokenConfig", IResilientClient resilientSSOClient = null)
        {

            DPSSOTokenRequest dpSSORequest = null;
            dpSSORequest = new DPSSOTokenRequest
            {
                GrantType = configuration.GetSection(configSSOSectionKey).GetValue<string>("ssoGrantType"),
                ClientId = (applicationId == 1) ? configuration.GetSection(configSSOSectionKey).GetValue<string>("ssoIosClientId") : configuration.GetSection(configSSOSectionKey).GetValue<string>("ssoAndroidClientId"),
                ClientSecret = (applicationId == 1) ? configuration.GetSection(configSSOSectionKey).GetValue<string>("ssoIosClientSecret") : configuration.GetSection(configSSOSectionKey).GetValue<string>("ssoAndroidClientSecret"),
                Scope = configuration.GetSection(configSSOSectionKey).GetValue<string>("ssoScope"),
                UserType = configuration.GetSection(configSSOSectionKey).GetValue<string>("ssoUserType"),
                UserName = mpNumber
            };
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };
            resilientSSOClient = resilientSSOClient ?? new ResilientClient(configuration.GetSection(dpSSOConfig).Get<ResilientClientOpitons>());
            string requestData = JsonConvert.SerializeObject(dpSSORequest);
            var dpData = string.Empty;
            Tuple<string, HttpStatusCode, string> SSOtokenResponse;

            SSOtokenResponse = await resilientSSOClient.PostHttpAsync(string.Empty, requestData, headers).ConfigureAwait(false);
            dpData = SSOtokenResponse.Item1;
            _logger.LogInformation("dp-ssoToken-service  {@RequestUrl} and {@Request}", configuration.GetSection(dpSSOConfig).GetValue<string>("baseUrl"), requestData);

            if (SSOtokenResponse.Item2 != HttpStatusCode.OK)
            {
                _logger.LogError("dp-ssoToken-service {@RequestUrl} error {@Response}", SSOtokenResponse?.Item3, dpData);
            }
            else
            {
                _logger.LogInformation("dp-ssoToken-service {@RequestUrl} {@Response}", SSOtokenResponse?.Item3, dpData);
            }

            return JsonConvert.DeserializeObject<DPTokenResponse>(dpData.ToString());
        }

    }
}
