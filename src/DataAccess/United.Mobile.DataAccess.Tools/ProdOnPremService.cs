using Autofac.Features.AttributeFilters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.Tools
{
    public class ProdOnPremService : IProdOnPremService
    {
        private readonly ICacheLog<ProdOnPremService> _logger;
        private readonly IResilientClient _resilientClient;

        public ProdOnPremService(ICacheLog<ProdOnPremService> logger, [KeyFilter("OnPremSQLPRODClientKey")] IResilientClient resilientClient)
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
            var requestObject = new CatalogRequest { DeviceId = deviceId, Application = new Application { Id = Int32.Parse(applicationId), Version = new Version() }, TransactionId = SessionId };
            var requestData = JsonConvert.SerializeObject(requestObject);
            string path = "/UnitedMobileDataServices/api/Catalog/GetCatalogItemsV2";
            _logger.LogInformation("GetCatalogServiceDetails Service {@RequestUrl}", path);
            var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, requestData, headers).ConfigureAwait(false);

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
            var path = "/OnPremSQLService/api/MPSignIn/MPPwdChangeForceSignOut";
            var requestDataObject = new SQLForceSignOutRequest { Data = new Data { MileagePlusNumber = mileagePlusNumber } };
            var requestData = JsonConvert.SerializeObject(requestDataObject);
            _logger.LogInformation("SQLForceSignOut {@requestData} {@transactionId}", requestData, transactionId);
            var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, requestData, headers).ConfigureAwait(false);
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
