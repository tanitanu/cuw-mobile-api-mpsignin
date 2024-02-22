using Autofac.Features.AttributeFilters;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.OnPremiseSQLSP
{
    public class SQL2DynamoMigrationService : ISQL2DynamoMigrationService
    {
        private readonly ICacheLog<SQL2DynamoMigrationService> _logger;
        private readonly IResilientClient _resilientClient;

        public SQL2DynamoMigrationService(ICacheLog<SQL2DynamoMigrationService> logger, [KeyFilter("SQL2DynamoMigrationClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }
        public async Task uatb_Device(string sessionID = "sessionID")
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                         {
                              {"Accept", "application/json"}
                         };
            string requestUrl = $"/Update/CuwDevice";
            using (_logger.BeginTimedOperation("Total time taken for uatb_Device call", transationId: "sessionID"))
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(requestUrl, string.Empty, headers).ConfigureAwait(false);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("uatb_Device {@requestUrl} error {@response}", responseData.url, responseData.response);
                }
                else
                {
                    _logger.LogInformation("uatb_Device {@requestUrl} info {@response}", responseData.url, responseData.response);
                }
            }
        }

        public async Task uatb_Device_History()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                         {
                              {"Accept", "application/json"}
                         };
            string requestUrl = $"/Update/CuwDeviceHistory";
            using (_logger.BeginTimedOperation("Total time taken for uatb_Device call", transationId: "sessionID"))
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(requestUrl, string.Empty, headers).ConfigureAwait(false);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("uatb_Device_History {@requestUrl} error {@response}", responseData.url, responseData.response);
                }
                else
                {
                    _logger.LogInformation("uatb_Device_History {@requestUrl} info {@response}", responseData.url, responseData.response);
                }
            }
        }

        public async Task uatb_DevicePushToken()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                         {
                              {"Accept", "application/json"}
                         };
            string requestUrl = $"/Update/CuwDeviceHistory";
            using (_logger.BeginTimedOperation("Total time taken for uatb_Device call", transationId: "sessionID"))
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(requestUrl, string.Empty, headers).ConfigureAwait(false);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("uatb_Device_History {@requestUrl} error {@response}", responseData.url, responseData.response);
                }
                else
                {
                    _logger.LogInformation("uatb_Device_History {@requestUrl} info {@response}", responseData.url, responseData.response);
                }
            }
        }

        public async Task uatb_MileagePlusValidation_CSS()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                         {
                              {"Accept", "application/json"}
                         };
            string requestUrl = $"/Update/CuwMileageplusvalidationCSS";
            using (_logger.BeginTimedOperation("Total time taken for uatb_Device call", transationId: "sessionID"))
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(requestUrl, string.Empty, headers).ConfigureAwait(false);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("uatb_MileagePlusValidation_CSS {@requestUrl} error {@response}", responseData.url, responseData.response);
                }
                else
                {
                    _logger.LogInformation("uatb_MileagePlusValidation_CSS {@requestUrl} info {@response}", responseData.url, responseData.response);
                }
            }
        }

        public async Task uatb_MileagePlusValidation()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                         {
                              {"Accept", "application/json"}
                         };
            string requestUrl = $"/Update/CuwMileageplusvalidation";
            using (_logger.BeginTimedOperation("Total time taken for uatb_Device call", transationId: "sessionID"))
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(requestUrl, string.Empty, headers).ConfigureAwait(false);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("uatb_MileagePlusValidation {@requestUrl} error {@response}", responseData.url, responseData.response);
                }
                else
                {
                    _logger.LogInformation("uatb_MileagePlusValidation {@requestUrl} info {@response}", responseData.url, responseData.response);
                }
            }
        }

        public async Task uatb_IsVBQWMDisplayed()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                         {
                              {"Accept", "application/json"}
                         };
            string requestUrl = $"/Update/CuwMileageplusvalidation";
            using (_logger.BeginTimedOperation("Total time taken for uatb_IsVBQWMDisplayed call", transationId: "sessionID"))
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(requestUrl, string.Empty, headers).ConfigureAwait(false);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("uatb_IsVBQWMDisplayed {@requestUrl} error {@response}", responseData.url, responseData.response);
                }
                else
                {
                    _logger.LogInformation("uatb_IsVBQWMDisplayed {@requestUrl} info {@response}", responseData.url, responseData.response);
                }
            }
        }

        public async Task uatb_EResBetaTester()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                         {
                              {"Accept", "application/json"}
                         };
            string requestUrl = $"/Update/CuwMileageplusvalidation";
            using (_logger.BeginTimedOperation("Total time taken for uatb_IsVBQWMDisplayed call", transationId: "sessionID"))
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(requestUrl, string.Empty, headers).ConfigureAwait(false);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("uatb_IsVBQWMDisplayed {@requestUrl} error {@response}", responseData.url, responseData.response);
                }
                else
                {
                    _logger.LogInformation("uatb_IsVBQWMDisplayed {@requestUrl} info {@response}", responseData.url, responseData.response);
                }
            }

        }

    }
}
