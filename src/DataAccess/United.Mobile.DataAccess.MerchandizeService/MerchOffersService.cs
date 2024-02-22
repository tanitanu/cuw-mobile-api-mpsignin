using Autofac.Features.AttributeFilters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.MerchandizeService
{
    public class MerchOffersService : IMerchOffersService
    {
        private readonly ICacheLog<MerchOffersService> _logger;
        private readonly IResilientClient _resilientClient;

        public MerchOffersService(
              [KeyFilter("SubscriptionsClientKey")] IResilientClient resilientClient
            , ICacheLog<MerchOffersService> logger)
        {
            _resilientClient = resilientClient;
            _logger = logger;
        }
        public async Task<T> GetSubscriptions<T>(string subscriptionRequest, string token, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for Subscriptions service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
                string path = $"/subscriptions/getsubscription";
                _logger.LogInformation("Subscriptions {@cslrequest} {@path}", subscriptionRequest, path);

                var data = await _resilientClient.PostHttpAsyncWithOptions(path, subscriptionRequest, headers).ConfigureAwait(false);

                if (data.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("Subscriptions {@url} error {@cslresponse}", data.url, data.response);
                    throw new Exception(data.response);
                }

                _logger.LogInformation("Subscriptions {@url}, {@cslresponse}", data.url, data.response);

                return string.IsNullOrEmpty(data.response) ? default : JsonConvert.DeserializeObject<T>(data.response);
            }
        }
    }
}
