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
    public class EmployeeTravelTypeService : IEmployeeTravelTypeService
    {
        private readonly IResilientClient _employeeProfileResilientClient;
        private readonly ICacheLog<EmployeeTravelTypeService> _logger;
        public EmployeeTravelTypeService([KeyFilter("EmployeeTravelTypeServiceClientKey")] IResilientClient employeeProfileResilientClient, ICacheLog<EmployeeTravelTypeService> logger)
        {
            _employeeProfileResilientClient = employeeProfileResilientClient;
            _logger = logger;
        }

        public async Task<string> GetTravelType(string token, string requestPayload, string sessionId, int ApplicationId, string AppVersion, string DeviceId, string TransactionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for GetEmployeeProfileTravelType service call", sessionId))
            {
                _logger.LogInformation("EmployeeTravelType {@Request}", requestPayload);

                Dictionary<string, string> headers = new Dictionary<string, string>
                {
                    { "X_APP_ID", ApplicationId.ToString() },
                    { "X_APP_MINOR", AppVersion },
                    { "X_APP_MAJOR", AppVersion },
                    { "X_DEVICE_ID", DeviceId },
                    { "X_LANG_CODE", "en-US" },
                    { "X_TRANSACTION_ID", TransactionId },
                    { "X_REQUEST_TIME_UTC", DateTime.UtcNow.ToString("yyyyMMdd hh:mm:ss") }
                };

                var path = $"/TravelerType";
                var serviceResponse = await _employeeProfileResilientClient.PostHttpAsyncWithOptions(path, requestPayload, headers).ConfigureAwait(false);

                if (serviceResponse.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("EmployeeTravelType {@Url} error {@response}", serviceResponse.url, serviceResponse.response);
                    if (serviceResponse.statusCode != HttpStatusCode.BadRequest)
                    {
                        throw new Exception(serviceResponse.response);
                    }
                }

                _logger.LogInformation("EmployeeTravelType {@Url}, {@response}", serviceResponse.url, serviceResponse.response);
                return serviceResponse.response;
            }
        }
    }
}
