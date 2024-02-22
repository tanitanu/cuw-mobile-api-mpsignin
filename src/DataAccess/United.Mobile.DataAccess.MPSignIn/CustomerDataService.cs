using Autofac.Features.AttributeFilters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;
using United.Utility.Serilog;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public class CustomerDataService : ICustomerDataService
    {
        private readonly ICacheLog<CustomerDataService> _logger;
        private readonly IResilientClient _resilientClient;

        public CustomerDataService(
              [KeyFilter("CustomerDataClientKey")] IResilientClient resilientClient
            , ICacheLog<CustomerDataService> logger)
        {
            _resilientClient = resilientClient;
            _logger = logger;
        }

        public async Task<(T response, long callDuration)> GetCustomerData<T>(string token, string sessionId, string jsonRequest)
        {
            IDisposable timer = null;
            string returnValue = string.Empty;

            using (timer = _logger.BeginTimedOperation("Total time taken for GetCustomerData service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
                string path = string.Format("/GetProfile");
                _logger.LogInformation("GetCustomerData Service {@cslRequest} {@RequestUrl}", jsonRequest, path);
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, jsonRequest, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetCustomerData Service {@RequestUrl} error {@cslResponse}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                    {
                        return default;
                    }
                }
                returnValue = responseData.response;

                _logger.LogInformation("GetCustomerData Service {@RequestUrl} {@cslResponse}", responseData.url, returnValue);
            }

            return (returnValue == null) ? default : (JsonConvert.DeserializeObject<T>(returnValue), (timer != null) ? ((TimedOperation)timer).GetElapseTime() : 0);
        }

        public async Task<T> InsertMPEnrollment<T>(string token, string request, string sessionId, string path)
        {
            using (_logger.BeginTimedOperation("Total time taken for InsertMPEnrollment service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token }
                     };
                _logger.LogInformation("InsertMPEnrollment Service {@cslRequest} {@path}", request, path);

                var enrollmentData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers).ConfigureAwait(false);

                if (enrollmentData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("InsertMPEnrollment Service {@url} error {@cslResponse}", enrollmentData.url, enrollmentData.response);
                    if (enrollmentData.statusCode != HttpStatusCode.BadRequest)
                    {
                        throw new Exception(enrollmentData.response);
                    }
                }

                _logger.LogInformation("InsertMPEnrollment Service {@url} {@cslResponse}", enrollmentData.url, enrollmentData.response);

                return JsonConvert.DeserializeObject<T>(enrollmentData.response);
            }
        }

        public async Task<T> ValidateCustomerData<T>(string token, string requestData, string sessionId, string path)
        {
            using (_logger.BeginTimedOperation("Total time taken for ValidateCustomerData service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token }
                     };
                string url = string.Format("{0}", path);
                var profileValidateData = await _resilientClient.PostHttpAsyncWithOptions(url, requestData, headers).ConfigureAwait(false);

                if (profileValidateData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("ValidateCustomerData-service {@requestUrl} error {@response} ", profileValidateData.url, profileValidateData.statusCode);
                    if (profileValidateData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(profileValidateData.response);
                }

                _logger.LogInformation("ValidateCustomerData-service {@requestUrl} ", profileValidateData.url);

                return JsonConvert.DeserializeObject<T>(profileValidateData.response);
            }
        }
    }
}
