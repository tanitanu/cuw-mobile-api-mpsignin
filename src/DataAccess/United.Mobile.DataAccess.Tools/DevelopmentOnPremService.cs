using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.Tools
{
    public class DevelopmentOnPremService : IDevelopmentOnPremService
    {
        private readonly ICacheLog<DevelopmentOnPremService> _logger;
        private readonly IResilientClient _resilientClient;

        public DevelopmentOnPremService(ICacheLog<DevelopmentOnPremService> logger, [KeyFilter("OnPremSQLDevelopmentClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<T> GetCatalogServiceDetails<T>(string applicationId, string deviceId = "DeviceID", string SessionId = "SessionID", bool IsCloudCatalog = true)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };
            string path = $"/Catalog/GetCatalogItems?applicationId={applicationId}&deviceId={deviceId}&sessionId={SessionId}&IsCloudCatalog={IsCloudCatalog}";
            _logger.LogInformation("GetCatalogServiceDetails Service {@RequestUrl}", path);
            var responseData = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(false);

            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("GetCatalogServiceDetails Service {@RequestUrl} error {@cslResponse}", responseData.url, responseData.response);
                return default;
            }

            _logger.LogInformation("GetCatalogServiceDetails Service {@RequestUrl} {@cslResponse}", responseData.url, responseData.response);

            return JsonConvert.DeserializeObject<T>(responseData.response);
        }
        public async Task<bool> SQLForceSignOut(string mileagePlusNumber, string transactionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };

            var requestDataObject = new SQLForceSignOutRequest { Data = new Data { MileagePlusNumber = mileagePlusNumber} };
            var requestData = JsonConvert.SerializeObject(requestDataObject);
            _logger.LogInformation("SQLForceSignOut {@requestData} {@transactionId}", requestData, transactionId);
            var responseData = await _resilientClient.PostHttpAsyncWithOptions("/MPSignIn/MPPwdChangeForceSignOut", requestData, headers).ConfigureAwait(false);
            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Error SQLForceSignOut {@requestUrl} {@response} {@transactionId}", responseData.url, responseData.response, transactionId);
                return false;
            }
            _logger.LogInformation("SQLForceSignOut {@response} {@transactionId}", responseData.response, transactionId);
            return JsonConvert.DeserializeObject<bool>(responseData.response);
        }
    }
}
