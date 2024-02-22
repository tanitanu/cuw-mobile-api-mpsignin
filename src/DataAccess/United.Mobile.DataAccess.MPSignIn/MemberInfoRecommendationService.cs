using Autofac.Features.AttributeFilters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.MPSignIn
{
    public class MemberInfoRecommendationService : IMemberInfoRecommendationService
    {
        private readonly ICacheLog<CustomerProfileService> _logger;
        private readonly IResilientClient _resilientClient;

        public MemberInfoRecommendationService(
              [KeyFilter("MemberInfoRecommendationClientKey")] IResilientClient resilientClient
            , ICacheLog<CustomerProfileService> logger)
        {
            _resilientClient = resilientClient;
            _logger = logger;
        }

        public async Task<string> RecommendedMPNumbers(string path, string request, string token, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for RecommendedMPNumbers service call", transationId: sessionId))
            {
                _logger.LogInformation("RecommendedMPNumbers  {@cslrequest}", request);

                var headers = new Dictionary<string, string>
                     {
                          { "Authorization", token }
                     };

                var data = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers).ConfigureAwait(false);
                _logger.LogInformation("RecommendedMPNumbers {@url}", data.url);


                if (data.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("RecommendedMPNumbers {@url} error {@cslresponse}", data.url, data.response);
                    if (data.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(data.response);
                }

                _logger.LogInformation("RecommendedMPNumbers {@url}, {@cslresponse}", data.url, data.response);

                return data.response;
            }
        }
    }
}