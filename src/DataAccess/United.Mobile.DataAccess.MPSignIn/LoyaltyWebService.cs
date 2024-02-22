using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public class LoyaltyWebService : ILoyaltyWebService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<LoyaltyWebService> _logger;
        private readonly IConfiguration _configuration;

        public LoyaltyWebService(
              [KeyFilter("LoyaltyWebClientKey")] IResilientClient resilientClient
            , ICacheLog<LoyaltyWebService> logger
            , IConfiguration configuration)
        {
            _resilientClient = resilientClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<string> GetLoyaltyData(string token, string mileagePlusNumber, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for GetLoyaltyData service call", sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/xml"},
                          { "Authorization", token }
                     };
                string path = string.Format("/{0}?elite_year={1}", mileagePlusNumber, DateTime.Today.Year);
                _logger.LogInformation("GetLoyaltyData {@path}", GeneralHelper.RemoveCarriageReturn(path));
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetLoyaltyWeb {@url} error {@cslResponse}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }
               
                _logger.LogInformation("GetLoyaltyWeb {@url} {@cslResponse}", responseData.url, responseData.response);
                return responseData.response;
            }
        }
    }
}
