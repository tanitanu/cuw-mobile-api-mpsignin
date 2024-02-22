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
    public class RavenCloudService : IRavenCloudService
    {
        private readonly ICacheLog<RavenCloudService> _logger;
        private readonly IResilientClient _resilientClient;

        public RavenCloudService(ICacheLog<RavenCloudService> logger, [KeyFilter("RavenCloudClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient; ;
        }

        public async Task<string> SendRavenEmail(string requestData, Dictionary<string, string> headers, string deviceId, string transactionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for Raven service call"))
            {
                _logger.LogInformation("RavenService Service {@cslRequest}", requestData);

                var ravenResult = await _resilientClient.PostHttpAsyncWithOptions("/event", requestData, headers).ConfigureAwait(false);

                if (ravenResult.statusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation("Raven-service {@url} {@cslResponse} {@DeviceId} {@TransactionId}", ravenResult.url, ravenResult.response, deviceId, transactionId);
                    return ravenResult.response;
                }

                _logger.LogError("Raven-service {@url} error {@cslResponse}", ravenResult.url, ravenResult.response, deviceId, transactionId);

                throw new Exception(ravenResult.response);
            }
        }
    }
}
