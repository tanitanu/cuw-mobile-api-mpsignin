using Autofac.Features.AttributeFilters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<EmployeeService> _logger;

        public EmployeeService([KeyFilter("EmployeeServiceClientKey")] IResilientClient resilientClient, ICacheLog<EmployeeService> logger)
        {
            _resilientClient = resilientClient;
            _logger = logger;
        }

        public async Task<string> GetEmployeeId(string mileageplusNumber, string transactionId, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for GetEmployeeIdy service call", sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", string.Empty },
                          {"Accept","application/json" }
                     };
                string requestData = string.Format("?mileageplusid={0}", mileageplusNumber);
                _logger.LogInformation("GetEmployeeId {@cslrequest}", requestData);

                var responseData = await _resilientClient.GetHttpAsyncWithOptions(requestData, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetEmployeeId {@url} error {@cslResponse}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("GetEmployeeId {@url} {@cslResponse}", responseData.url, responseData.response);
                return responseData.response;
            }
        }
    }
}
