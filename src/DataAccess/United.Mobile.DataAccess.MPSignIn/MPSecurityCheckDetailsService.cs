using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public class MPSecurityCheckDetailsService : IMPSecurityCheckDetailsService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<MPSecurityCheckDetailsService> _logger;

        public MPSecurityCheckDetailsService([KeyFilter("MPSecurityCheckDetailsClientKey")] IResilientClient resilientClient, ICacheLog<MPSecurityCheckDetailsService> logger)
        {
            _resilientClient = resilientClient;
            _logger = logger;
        }

        public async Task<string> GetMPSecurityCheckDetails(string token, string requestData, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for GetMPSecurityCheckDetails service call", sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token },
                          {"Accept","application/json" }
                     };
                string path = "/GetProfile";
                _logger.LogInformation("GetMPSecurityCheckDetails {@path}", path);
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, requestData, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetMPSecurityCheckDetails {@url} error {@cslResponse}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                    {
                        throw new Exception(responseData.response);
                    }
                }

                _logger.LogInformation("GetMPSecurityCheckDetails {@url} {@cslResponse}", responseData.url, responseData.response);
                return responseData.response;
            }
        }

        public async Task<string> UpdateTfaWrongAnswersFlag(string token, string requestData, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for UpdateTfaWrongAnswersFlag service call", sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token },
                          {"Accept","application/json" }
                     };
                string path = "/UpdateTfaWrongAnswersFlag";
                _logger.LogInformation("UpdateTfaWrongAnswersFlag {@path}", path);

                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, requestData, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("UpdateTfaWrongAnswersFlag {@url} error {@cslResponse}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                    {
                        throw new Exception(responseData.response);
                    }
                }

                _logger.LogInformation("UpdateTfaWrongAnswersFlag {@url} {@cslResponse}", responseData.url, responseData.response);
                return responseData.response;
            }
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
                    _logger.LogError("ValidateCustomerData-service {@requestUrl} error {@response}", profileValidateData.url, profileValidateData.statusCode);
                    if (profileValidateData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(profileValidateData.response);
                }

                _logger.LogInformation("ValidateCustomerData-service {@requestUrl} ", profileValidateData.url);

                return JsonConvert.DeserializeObject<T>(profileValidateData.response);
            }
        }
    }
}
