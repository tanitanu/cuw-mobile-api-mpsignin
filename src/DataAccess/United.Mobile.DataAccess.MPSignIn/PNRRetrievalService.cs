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
    public class PNRRetrievalService : IPNRRetrievalService
    {
        private readonly ICacheLog<PNRRetrievalService> _logger;
        private readonly IResilientClient _resilientClient;
        private readonly IConfiguration _configuration;

        public PNRRetrievalService([KeyFilter("PNRRetrievalClientKey")] IResilientClient resilientClient, ICacheLog<PNRRetrievalService> logger
            , IConfiguration configuration)
        {
            _logger = logger;
            _resilientClient = resilientClient;
            _configuration = configuration;
        }

        public async Task<string> UpdateTravelerInfo(string token, string requestData, string path, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token }
                     };
            _logger.LogInformation("UpdateTravelerInfo-service {@path} {@cslrequest}", GeneralHelper.RemoveCarriageReturn(path), GeneralHelper.RemoveCarriageReturn(requestData));
            var pnrData = await _resilientClient.PostHttpAsyncWithOptions(path, requestData, headers).ConfigureAwait(false);

            if (pnrData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("UpdateTravelerInfo-service {@url} error {@cslresponse}", pnrData.url, pnrData.response);
                if (pnrData.statusCode != HttpStatusCode.BadRequest)
                    throw new Exception(pnrData.response);
            }

            _logger.LogInformation("UpdateTravelerInfo-service {@url} {@cslresponse}", pnrData.url, pnrData.response);
            return pnrData.response;
        }

    }
}