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
    public class EResEmployeeProfileService : IEResEmployeeProfileService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<EResEmployeeProfileService> _logger;
        public EResEmployeeProfileService([KeyFilter("eResEmployeeProfileClientKey")] IResilientClient resilientClient, ICacheLog<EResEmployeeProfileService> logger)
        {
            _resilientClient = resilientClient;
            _logger = logger;
        }

        public async Task<string> GetEResEmployeeProfile(string token, string requestPayload, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for GetEResEmployeeProfile service call", sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token }
                     };

                string path = "/Employee/EmployeeProfile";
                _logger.LogInformation("EResEmployeeProfile {@cslRequest}", requestPayload);

                var serviceResponse = await _resilientClient.PostHttpAsyncWithOptions(path, requestPayload, headers).ConfigureAwait(false);

                if (serviceResponse.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("EResEmployeeProfile {@url} error {@cslresponse}", serviceResponse.url, serviceResponse.response);
                    if (serviceResponse.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(serviceResponse.response);
                }

                _logger.LogInformation("EResEmployeeProfile {@url}, {@cslresponse}", serviceResponse.url, serviceResponse.response);
                return serviceResponse.response;
            }
        }
    }
}
