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
    public class HomePageContentService : IHomePageContentService
    {
        private readonly IResilientClient _homePageContentResilientClient;
        private readonly ICacheLog<HomePageContentService> _logger;
        public HomePageContentService([KeyFilter("homePageContentClientKey")] IResilientClient homePageContentResilientClient, ICacheLog<HomePageContentService> logger)
        {
            _homePageContentResilientClient = homePageContentResilientClient;
            _logger = logger;
        }
        public async Task<string> GetHomePageContents(string token, string requestData, string sessionId)
        {

            using (_logger.BeginTimedOperation("Total time taken for GetHomePageContents service call", sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token }
                     };
                _logger.LogInformation("GetHomePageContents-service {@cslrequest}", requestData);
                var path = $"/GetHomePageContent";
                var serviceResponse = await _homePageContentResilientClient.PostHttpAsyncWithOptions(path, requestData, headers).ConfigureAwait(false);

                if (serviceResponse.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetHomePageContents-service {@url} error {@cslresponse}", serviceResponse.url, serviceResponse.response);
                    if (serviceResponse.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(serviceResponse.response);
                }

                _logger.LogInformation("GetHomePageContents-service {@url} and {@cslresponse}", serviceResponse.url, serviceResponse.response);
                return serviceResponse.response;
            }
        }
    }
}
