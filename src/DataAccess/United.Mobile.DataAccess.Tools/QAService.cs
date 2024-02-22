using Autofac.Features.AttributeFilters;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.Tools
{
    public class QAService : IQAService
    {
        private readonly ICacheLog<QAService> _logger;
        private readonly IResilientClient _resilientClient;

        public QAService(
              [KeyFilter("QAClientKey")] IResilientClient resilientClient
            , ICacheLog<QAService> logger)
        {
            _resilientClient = resilientClient;
            _logger = logger;
        }

        public async Task<string> GetHealthCheck(string serviceName)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };
            string path = $"/{serviceName}/api/HealthCheck";
            _logger.LogInformation("GetHealthCheck Service {@RequestUrl}", path);
            var responseData = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(false);

            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("GetHealthCheck Service {@RequestUrl} error {@cslResponse}", responseData.url, responseData.response);
                return default;
            }

            _logger.LogInformation("GetHealthCheck Service {@RequestUrl} {@cslResponse}", responseData.url, responseData.response);

            return responseData.response;
        }

        public async Task<string> GetVersion(string serviceName)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };
            string path = $"/{serviceName}/api/version";
            _logger.LogInformation("GetVersion Service {@RequestUrl}", path);
            var responseData = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(false);

            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("GetVersion Service {@RequestUrl} error {@cslResponse}", responseData.url, responseData.response);
                return default;
            }

            _logger.LogInformation("GetVersion Service {@RequestUrl} {@cslResponse}", responseData.url, responseData.response);

            return responseData.response;
        }

        public async Task<string> GetEnvironment(string serviceName)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };
            string path = $"/{serviceName}/api/environment";
            _logger.LogInformation("GetEnvironment Service {@RequestUrl}", path);
            var responseData = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(false);

            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("GetEnvironment Service {@RequestUrl} error {@cslResponse}", responseData.url, responseData.response);
                return default;
            }

            _logger.LogInformation("GetEnvironment Service {@RequestUrl} {@cslResponse}", responseData.url, responseData.response);

            return responseData.response;
        }

        public async Task<string> GetAllValidateMpAppidDeviceIdByMP(string tableName, string secondaryKey, string transactionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };

            var requestDataObject = new DynamoSearchRequest { TransactionId = transactionId, TableName = tableName, SecondaryKey = secondaryKey };
            var requestData = JsonConvert.SerializeObject(requestDataObject);
            _logger.LogInformation("DynamoDB - GetAllValidateMpAppidDeviceIdByMP {@requestData} {@transactionId}", requestData, transactionId);
            var responseData = await _resilientClient.PostHttpAsyncWithOptions("/dynamodbservice/api/GetAllValidateMPAppidDeviceidByMP", requestData, headers).ConfigureAwait(false);
            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Error GetAllValidateMpAppidDeviceIdByMP {@requestUrl} {@response} {@transactionId}", responseData.url, responseData.response, transactionId);
                return string.Empty;
            }
            _logger.LogInformation("GetAllValidateMpAppidDeviceIdByMP {@response} {@transactionId}", responseData.response, transactionId);
            return responseData.response;
        }

        public async Task<bool> DynamoForceSignOut(string tableName, string secondaryKey, string transactionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };

            var requestDataObject = new ForceSignOutRequest { TransactionId = transactionId, TableName = tableName, SecondaryKey = secondaryKey };
            var requestData = JsonConvert.SerializeObject(requestDataObject);
            _logger.LogInformation("DynamoForceSignOut {@requestData} {@transactionId}", requestData, transactionId);
            var responseData = await _resilientClient.PostHttpAsyncWithOptions("/mpsignindbmanagerservice/api/ForceSignOut", requestData, headers).ConfigureAwait(false);
            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Error DynamoForceSignOut {@requestUrl} {@response} {@transactionId}", responseData.url, responseData.response, transactionId);
                return false;
            }
            _logger.LogInformation("DynamoForceSignOut {@response} {@transactionId}", responseData.response, transactionId);
            return JsonConvert.DeserializeObject<bool>(responseData.response);
        }
    }
}
