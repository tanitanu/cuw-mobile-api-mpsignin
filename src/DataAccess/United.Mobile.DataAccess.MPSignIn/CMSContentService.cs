using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;
using United.Utility.Serilog;

namespace United.Mobile.DataAccess.MPSignIn
{
    public class CMSContentService : ICMSContentService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<CMSContentService> _logger;
        public CMSContentService(
              [KeyFilter("CMSContentDataClientKey")] IResilientClient resilientClient
            , ICacheLog<CMSContentService> logger)
        {
            _resilientClient = resilientClient;
            _logger = logger;
        }

        public async Task<T> GetMessages<T>(string token, string sessionId, string jsonRequest)
        {
            IDisposable timer = null;
            string returnValue = string.Empty;

            using (timer = _logger.BeginTimedOperation("Total time taken for GetCMSContentMessages service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
                string path = string.Format("/message");
                _logger.LogInformation("GetCMSContentMessages Service {@cslRequest} {@RequestUrl}", jsonRequest, path);
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, jsonRequest, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetCMSContentMessages Service {@RequestUrl} error {@cslResponse}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                    {
                        return default;
                    }
                }
                returnValue = responseData.response;

                _logger.LogInformation("GetCMSContentMessages Service {@RequestUrl} {@cslResponse}", responseData.url, returnValue);
            }

            return JsonConvert.DeserializeObject<T>(returnValue);
        }
    }
}
