using Autofac.Features.AttributeFilters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public class CustomerPreferencesService : ICustomerPreferencesService
    {
        private readonly ICacheLog<CustomerPreferencesService> _logger;
        private readonly IResilientClient _resilientClient;

        public CustomerPreferencesService(ICacheLog<CustomerPreferencesService> logger, [KeyFilter("CustomerPreferencesClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }
        public async Task<T> GetCustomerPreferences<T>(string token, string mpNumber, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for GetCustomerPreferences-CSL Service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token }
                     };

                _logger.LogInformation("GetCustomerPreferences {@cslrequest}", mpNumber);
                string requestData = string.Format("/AirPreference/{0}?idType=LoyaltyID", mpNumber);


                using (_logger.BeginTimedOperation("Total time taken for GetCustomerPreferences-CSL Service call", transationId: sessionId))
                {
                    var cPreferencesData = await _resilientClient.GetHttpAsyncWithOptions(requestData, headers).ConfigureAwait(false);
                    _logger.LogInformation("GetCustomerPreferences {@url}", cPreferencesData.url);

                    if (cPreferencesData.statusCode != HttpStatusCode.OK)
                    {
                        _logger.LogError("GetCustomerPreferences {@url} error {@cslresponse}", cPreferencesData.url, cPreferencesData.response);
                        if (cPreferencesData.statusCode != HttpStatusCode.BadRequest)
                            throw new Exception(cPreferencesData.response);
                    }

                    _logger.LogInformation("GetCustomerPreferences {@url} {@cslresponse}", cPreferencesData.url, cPreferencesData.response);

                    return JsonConvert.DeserializeObject<T>(cPreferencesData.response);
                }
            }
        }
    }
}

