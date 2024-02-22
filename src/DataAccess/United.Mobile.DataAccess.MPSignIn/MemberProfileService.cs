using Autofac.Features.AttributeFilters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.MPSignIn
{
    public class MemberProfileService : IMemberProfileService
    {
        private readonly ICacheLog<CustomerProfileService> _logger;
        private readonly IResilientClient _resilientClient;

        public MemberProfileService(
              [KeyFilter("MemberProfileClientKey")] IResilientClient resilientClient
            , ICacheLog<CustomerProfileService> logger)
        {
            _resilientClient = resilientClient;
            _logger = logger;
        }

        public async Task<T> SearchMemberInfo<T>(string token, string requestData, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for SeachMemberInfo service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token }
                     };

                string path = "/search";
                _logger.LogInformation("SeachMemberInfo-service {@request} {@path}", requestData, path);

                var memberInfoResponse = await _resilientClient.PostHttpAsyncWithOptions(path, requestData, headers).ConfigureAwait(false);

                if (memberInfoResponse.statusCode != HttpStatusCode.OK && memberInfoResponse.statusCode != HttpStatusCode.NotFound)
                {
                    _logger.LogError("SeachMemberInfo-service {@url} error {@cslResponse}", memberInfoResponse.url, memberInfoResponse.response);
                }
                else
                {
                    _logger.LogInformation("SeachMemberInfo-service {@url} {@cslResponse}", memberInfoResponse.url, memberInfoResponse.response);
                }

                return JsonConvert.DeserializeObject<T>(memberInfoResponse.response);

            }
        }

        public async Task<string> ValidateMemberName(string token, string requestData, string sessionId, string path)
        {
            using (_logger.BeginTimedOperation("Total time taken for ValidateMemberName service call", transationId: sessionId))
            {
                var headers = new Dictionary<string, string>
                     {
                          { "Authorization", token }
                     };

                _logger.LogInformation("ValidateMemberName-service {@request} {@path}", requestData, path);

                var httpResponse = await _resilientClient.PostHttpAsyncWithOptions(path, requestData, headers).ConfigureAwait(false);

                if (httpResponse.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("ValidateMemberName-service {@url} error {@cslResponse}", httpResponse.url, httpResponse.response);
                    if (httpResponse.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(httpResponse.response);
                }

                _logger.LogInformation("ValidateMemberName-service {@url} {@cslResponse}", httpResponse.url, httpResponse.response);

                return httpResponse.response;
            }
        }
    }

}
