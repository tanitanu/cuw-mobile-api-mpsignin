using Autofac.Features.AttributeFilters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;
using United.Utility.Serilog;

namespace United.Mobile.DataAccess.MPSignIn
{
    public class CorporateGetService : ICorporateGetService
    {
        private readonly ICacheLog<CorporateGetService> _logger;
        private readonly IResilientClient _resilientClient;

        public CorporateGetService(
              [KeyFilter("CorporateDataClientKey")] IResilientClient resilientClient
            , ICacheLog<CorporateGetService> logger)
        {
            _resilientClient = resilientClient;
            _logger = logger;
        }

        public async Task<(T response, long callDuration)> GetData<T>(string token, string sessionId, string jsonRequest)
        {
            IDisposable timer = null;
            string returnValue = string.Empty;

            using (timer = _logger.BeginTimedOperation("Total time taken for GetCorporateData service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
                string path = string.Format("/CustomerProfile/CorpPolicy");
                _logger.LogInformation("GetCorporateData Service {@cslRequest} {@RequestUrl}", jsonRequest, path);
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, jsonRequest, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetCorporateData Service {@RequestUrl} error {@cslResponse}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                    {
                        return default;
                    }
                }
                returnValue = responseData.response;

                _logger.LogInformation("GetCorporateData Service {@RequestUrl} {@cslResponse}", responseData.url, returnValue);
            }

            return (returnValue == null) ? default : (JsonConvert.DeserializeObject<T>(returnValue), (timer != null) ? ((TimedOperation)timer).GetElapseTime() : 0);
        }

        public async Task<string> GetCorpFOPData(string token, string sessionId, string jsonRequest)
        {
            IDisposable timer = null;
            string returnValue = string.Empty;
            using (timer = _logger.BeginTimedOperation("Total time taken for GetCorpFOPData service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
                string path = string.Format("/CustomerProfile/CorpFOP");
                _logger.LogInformation("GetCorpFOPData Service {@cslRequest} {@Url}", jsonRequest, path);
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, jsonRequest, headers).ConfigureAwait(false);
                returnValue = responseData.response;

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetCorporateData Service {@Url} error {@cslResponse}", responseData.url, returnValue);
                }
                else
                {
                    _logger.LogInformation("GetCorpFOPData Service {@Url} {@cslResponse}", responseData.url, returnValue);
                }

                return returnValue;
            }
        }

        public async Task<string> GetCorpProfileData(string token, string sessionId, string jsonRequest)
        {
            IDisposable timer = null;
            string returnValue = string.Empty;
            string callTime4Tuning = string.Empty;

            Stopwatch cslCallDurationstopwatch1;
            cslCallDurationstopwatch1 = new Stopwatch();
            cslCallDurationstopwatch1.Reset();
            cslCallDurationstopwatch1.Start();

            using (timer = _logger.BeginTimedOperation("Total time taken for GetCorpProfileData service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
                string path = string.Format("/CustomerProfile/CorpProfile");
                _logger.LogInformation("GetCorpProfileData Service {@cslRequest} {@Url}", jsonRequest, path);
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, jsonRequest, headers).ConfigureAwait(false);
                returnValue = responseData.response;

                if (responseData.statusCode == HttpStatusCode.ServiceUnavailable || responseData.statusCode == HttpStatusCode.GatewayTimeout)
                {
                    _logger.LogError("GetCorpProfileData Service {@Url} error {@cslResponse}", responseData.url, returnValue);
                    return default;
                }
                else if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetCorpProfileData Service {@Url} error {@cslResponse}", responseData.url, returnValue);
                }
                else
                {
                    _logger.LogInformation("GetCorpProfileData Service {@Url} {@cslResponse}", responseData.url, returnValue);
                }
            }

            if (cslCallDurationstopwatch1.IsRunning)
            {
                cslCallDurationstopwatch1.Stop();
            }

            callTime4Tuning = (cslCallDurationstopwatch1.ElapsedMilliseconds / (double)1000).ToString();
            _logger.LogInformation("GetCorpProfileData Service call duration {@duration}", callTime4Tuning + "seconds");
            
            return returnValue;
        }
    }
}
