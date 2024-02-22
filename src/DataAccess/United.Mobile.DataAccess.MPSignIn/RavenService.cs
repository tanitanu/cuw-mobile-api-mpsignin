using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public class RavenService: IRavenService
    {
        private readonly ICacheLog<RavenService> _logger;
        private readonly IResilientClient _resilientClient;

        public RavenService(ICacheLog<RavenService> logger, [KeyFilter("RavenClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient; ;
        }

        public async Task<string> SendRavenEmail(string token, string requestData, Dictionary<string, string> headers, string deviceId, string transactionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for Raven service call"))
            {
                headers.Add("Authorization", token);

                _logger.LogInformation("RavenService Service {@cslRequest}", requestData);

                var ravenResult = await _resilientClient.PostHttpAsyncWithOptions("/event", requestData, headers).ConfigureAwait(false);

                if (ravenResult.statusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation("Raven-service {@url} {@cslResponse} {@DeviceId} {@TransactionId}", ravenResult.url, ravenResult.response, deviceId, transactionId);
                    return ravenResult.response;
                }

                _logger.LogError("Raven-service {@url} error {@cslResponse}", ravenResult.url, ravenResult);

                throw new Exception(ravenResult.response);
            }
        }
    }
}
