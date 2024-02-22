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
    public class EmployeeProfileService : IEmployeeProfileService
    {
        private readonly IResilientClient _employeeProfileResilientClient;
        private readonly ICacheLog<EmployeeProfileService> _logger;
        public EmployeeProfileService([KeyFilter("employeeProfileClientKey")] IResilientClient employeeProfileResilientClient, ICacheLog<EmployeeProfileService> logger)
        {
            _employeeProfileResilientClient = employeeProfileResilientClient;
            _logger = logger;
        }

        public async Task<string> GetEmployeeProfile(string token, string request, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for GetEmployeeProfile service call", sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                };

                var path = $"/employee/{request}";
                _logger.LogInformation("GetEmployeeProfile {@cslrequest}", request);

                var serviceResponse = await _employeeProfileResilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(false);

                if (serviceResponse.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetEmployeeProfile {@Url} error {@cslresponse} ", serviceResponse.url, serviceResponse.response);
                    if (serviceResponse.statusCode != HttpStatusCode.BadRequest)
                    {
                        throw new Exception(serviceResponse.response);
                    }
                }

                _logger.LogInformation("GetEmployeeProfile {@Url} and {@cslresponse}", serviceResponse.url, serviceResponse.response);
                return serviceResponse.response;
            }
        }
    }
}