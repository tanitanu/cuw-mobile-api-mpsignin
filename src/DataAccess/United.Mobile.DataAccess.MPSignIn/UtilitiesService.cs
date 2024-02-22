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
    public class UtilitiesService : IUtilitiesService
    {
        private readonly ICacheLog<UtilitiesService> _logger;
        private readonly IResilientClient _resilientClient;

        public UtilitiesService(ICacheLog<UtilitiesService> logger, [KeyFilter("UtilitiesServiceClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<T> ValidateMileagePlusNames<T>(string token, string requestData, string sessionId, string path)
        {
            using (_logger.BeginTimedOperation("Total time taken for ValidateMileagePlusNames service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token }
                     };
                string url = string.Format("/{0}", path);
                var profileValidateData = await _resilientClient.PostHttpAsyncWithOptions(url, requestData, headers).ConfigureAwait(false);

                if (profileValidateData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("ValidateMileagePlusNames-service {@requestUrl} error {@response} ", profileValidateData.url, profileValidateData.statusCode);
                    if (profileValidateData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(profileValidateData.response);
                }

                _logger.LogInformation("ValidateMileagePlusNames-service {@requestUrl}", profileValidateData.url);

                return JsonConvert.DeserializeObject<T>(profileValidateData.response);
            }
        }
    }
}
