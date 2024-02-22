using Autofac.Features.AttributeFilters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;
using Microsoft.Extensions.Logging;

namespace United.Mobile.DataAccess.MPSignIn
{
    public class CustomerProfileService : ICustomerProfileService
    {
        private readonly ICacheLog<CustomerProfileService> _logger;
        private readonly IResilientClient _resilientClient;

        public CustomerProfileService(
              [KeyFilter("CustomerProfileClientKey")] IResilientClient resilientClient
            , ICacheLog<CustomerProfileService> logger)
        {
            _resilientClient = resilientClient;
            _logger = logger;
        }

        public async Task<string> Search(string token, string sessionId, string path)
        {
            using (_logger.BeginTimedOperation("Total time taken for Search service call", transationId: sessionId))
            {
                _logger.LogInformation("Search  {@cslrequest}", GeneralHelper.RemoveCarriageReturn(path));

                var headers = new Dictionary<string, string>
                     {
                          { "Authorization", token }
                     };

                var data = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(false);
                _logger.LogInformation("Search {@url}", data.url);


                if (data.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("Search {@url} error {@cslresponse}", data.url, data.response);
                    if (data.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(data.response);
                }

                _logger.LogInformation("Search {@url}, {@cslresponse}", data.url, data.response);

                return data.response;
            }
        }

        public async Task<string> SearchMPNumber(string path, string token, string sessionId)
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", token }
            };

            Tuple<string, HttpStatusCode, string> data;
            using (_logger.BeginTimedOperation("Total time taken for SearchMPNumber CSL call", transationId: sessionId))
            {
                data = await _resilientClient.GetHttpAsync(path, headers).ConfigureAwait(false);
            }

            var response = data.Item1;
            var statusCode = data.Item2;
            var url = data.Item3;

            if (statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("csl-SearchMPNumber-service {requestUrl} error {response} and {headers}", url, response, headers);
            }
            else
            {
                _logger.LogInformation("csl-SearchMPNumber-service {requestUrl}, {response} and {headers}", url, response, headers);
            }
            if ((statusCode != HttpStatusCode.OK) && (statusCode != HttpStatusCode.BadRequest))
            {
                throw new Exception(response);
            }
            return response;
        }
    }
}
