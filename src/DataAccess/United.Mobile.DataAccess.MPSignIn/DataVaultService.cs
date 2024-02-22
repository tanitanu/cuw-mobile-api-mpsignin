using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public class DataVaultService : IDataVaultService
    {
        private readonly ICacheLog<DataVaultService> _logger;
        private readonly IResilientClient _resilientClient;

        public DataVaultService(ICacheLog<DataVaultService> logger, [KeyFilter("DataVaultTokenClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<string> GetPersistentToken(string token, string requestData, string url, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for GetPersistentToken service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                         {"Accept", "application/json"},
                         { "Authorization", token }
                     };
                _logger.LogInformation("AccountManagement-GetPersistentToken-service {@RequestUrl}", url);
                var gPTokenData = await _resilientClient.GetHttpAsyncWithOptions(url, headers).ConfigureAwait(false);

                if (gPTokenData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("AccountManagement-GetPersistentToken-service {@RequestUrl} error {@Response}", gPTokenData.url, gPTokenData.response);
                    if (gPTokenData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(gPTokenData.response);
                }

                _logger.LogInformation("AccountManagement-GetPersistentToken-service {@RequestUrl} and {@Response}", gPTokenData.url, gPTokenData.response);

                return gPTokenData.response;
            }
        }
        public async Task<string> PersistentToken(string token, string requestData, string url, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for PersistentToken1 service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token }
                     };
                _logger.LogInformation("AccountManagement-GetPersistentToken1-service {@RequestUrl} and {@Request}", url, requestData);
                var pTokenData = await _resilientClient.PostHttpAsyncWithOptions(url, requestData, headers).ConfigureAwait(false);

                if (pTokenData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("AccountManagement-GetPersistentToken1-service {@RequestUrl} error {@Response}", pTokenData.url, pTokenData.response);
                    if (pTokenData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(pTokenData.response);
                }

                _logger.LogInformation("AccountManagement-GetPersistentToken1-service {@RequestUrl} and {@Response}", pTokenData.url, pTokenData.response);

                return pTokenData.response;
            }
        }
    }
}
