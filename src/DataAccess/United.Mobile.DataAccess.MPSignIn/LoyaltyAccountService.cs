using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
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
    public class LoyaltyAccountService : ILoyaltyAccountService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<LoyaltyAccountService> _logger;
        private readonly IConfiguration _configuration;
        public LoyaltyAccountService([KeyFilter("LoyaltyAccountClientKey")] IResilientClient resilientClient, ICacheLog<LoyaltyAccountService> logger, IConfiguration configuration)
        {
            _resilientClient = resilientClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<T> GetAccountProfileInfo<T>(string token, string mileagePlusNumber, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            string requestData = string.Format("{0}", mileagePlusNumber);
            using (_logger.BeginTimedOperation("Total time taken for GetAccountProfileInfo call", transationId: sessionId))
            {
                _logger.LogInformation("GetAccountProfileInfo Service {@cslRequest} {@mileagePlusNumber} ", requestData, mileagePlusNumber);
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(requestData, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetAccountProfileInfo Service {@url} error {@cslResponse} {@mileagePlusNumber}", responseData.url, responseData.response, mileagePlusNumber);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                    {
                        return default;
                    }
                }
                _logger.LogInformation("GetAccountProfileInfo Service {@url} {@cslResponse} {@mileagePlusNumber}", responseData.url, responseData.response, mileagePlusNumber);

                return JsonConvert.DeserializeObject<T>(responseData.response);
            }
        }

        public async Task<string> GetAccountProfile(string token, string profileNumber, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},                          
                          { "Authorization", token }
                     };
            
            string requestData = $"/{profileNumber}";

            StaticLog.Information(_logger, "GetAccountProfile Service {@cslRequest} {@mileagePlusNumber}", requestData, profileNumber);

            using (_logger.BeginTimedOperation("Total time taken for CSL service-GetAccountProfile service call", transationId: sessionId))
            {
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(requestData, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetAccountProfile Service {@url} error {@cslResponse} {@mileagePlusNumber}", responseData.url, responseData.response, profileNumber);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                    {
                        return default;
                    }
                }
                _logger.LogInformation("GetAccountProfile Service {@url} {@cslResponse} {@mileagePlusNumber}", responseData.url, responseData.response, profileNumber);

                return responseData.response;
            }
        }

        public async Task<string> GetCurrentMembershipInfo(string mpNumber, string transactionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept","application/json" }
                     };
            var path = string.Format("{0}", mpNumber);
            _logger.LogInformation("GetCurrentMembershipInfo MPNumber {@cslRequest} {@mileagePlusNumber}", GeneralHelper.RemoveCarriageReturn(mpNumber), mpNumber);
            using (_logger.BeginTimedOperation("Total time taken for GetCurrentMembershipInfo call", transationId: transactionId))
            {
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("United ClubPasses CSL Service {@url} error {@cslResponse} {@mileagePlusNumber}", responseData.url, responseData.response, mpNumber);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                    {
                        throw new Exception();
                    }
                }
                _logger.LogInformation("United ClubPasses CSL Service {@url} {@cslResponse} {@mileagePlusNumber}", responseData.url, responseData.response, mpNumber);

                return responseData.response;
            }
        }
    }
}
