using Autofac.Features.AttributeFilters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Mobile.Model.Internal.Common;
using United.Utility.Helper;
using United.Utility.Http;
using Microsoft.Extensions.Logging;

namespace United.Mobile.DataAccess.Common
{
    public class DynamoDBService : IDynamoDBService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<DynamoDBService> _logger;
        public DynamoDBService([KeyFilter("DynamoDBClientKey")] IResilientClient resilientClient, ICacheLog<DynamoDBService> logger)
        {
            _resilientClient = resilientClient;
            _logger = logger;
        }

        public async Task<T> GetRecords<T>(string tableName, string transactionId, string key, string sessionId)
        {
            try
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };

                var requestDataObject = new GetDataRequest() { TransactionId = transactionId, TableName = tableName, Key = key };
                var requestData = JsonConvert.SerializeObject(requestDataObject);
                _logger.LogInformation("DynamoDB - GetRecords {@requestData}  ", requestData);
                var responseData = await _resilientClient.PostHttpAsyncWithOptions("/GetData", requestData, headers).ConfigureAwait(false);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning("DynamoDB - GetRecords {requestUrl} {@response} ", responseData.url, responseData.response);
                    if (responseData.statusCode == HttpStatusCode.NotFound)
                        return default;
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("DynamoDB - GetRecords {requestUrl} {@response}", responseData.url, responseData.response);
                return JsonConvert.DeserializeObject<T>(responseData.response);
            }
            catch (Exception ex)
            {
                _logger.LogError("DynamoDB - GetRecords {@StackTrace} ", ex.StackTrace);
            }

            return default;
        }

        public async Task<bool> SaveRecords<T>(string tableName, string transactionId, string key, T data, string sessionId)
        {
            try
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };
                var requestDataObject = new SaveDataRequest<T>() { TransactionId = transactionId, TableName = tableName, Key = key, Data = data };
                if (tableName == "cuw-mileageplusvalidation")
                {
                    double absoluteExpirationDays = 36500;
                    requestDataObject.AbsoluteExpiration = DateTime.UtcNow.AddDays(absoluteExpirationDays);
                }
                var requestData = JsonConvert.SerializeObject(requestDataObject);
                _logger.LogInformation("DynamoDB - SaveRecords {@requestData}  ", requestData);
                var responseData = await _resilientClient.PostHttpAsyncWithOptions("/SaveData", requestData, headers).ConfigureAwait(false);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning("DynamoDB - SaveRecords {requestUrl} {@response} ", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }
                _logger.LogInformation("DynamoDB - SaveRecords {@response}  ", responseData.response);
                return JsonConvert.DeserializeObject<bool>(responseData.response);

            }
            catch (Exception ex)
            {
                _logger.LogError("DynamoDB - SaveRecords {StackTrace} ", ex.StackTrace);
            }

            return default;
        }

        public async Task<bool> SaveRecords<T>(string tableName, string transactionId, string key, string secondaryKey, T data, string sessionId, double absoluteExpirationDays = 2)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };
            var requestDataObject = new SaveDataRequest<T>() { TransactionId = transactionId, TableName = tableName, Key = key, SecondaryKey = secondaryKey, Data = data, AbsoluteExpiration = DateTime.UtcNow.AddDays(absoluteExpirationDays) };
            var requestData = JsonConvert.SerializeObject(requestDataObject);
            _logger.LogInformation("DynamoDB - SaveRecords {@requestData} ", requestData);
            var responseData = await _resilientClient.PostHttpAsyncWithOptions("/SaveData", requestData, headers).ConfigureAwait(false);

            _logger.LogInformation("DynamoDB - SaveRecords {@response} ", responseData.response);
            if (responseData.statusCode != HttpStatusCode.OK && responseData.statusCode != HttpStatusCode.BadRequest)
            {
                _logger.LogError("DynamoDB - SaveRecords Error {requestUrl} {response} ", responseData.url, responseData.response);
                throw new Exception(responseData.response);
            }
            return JsonConvert.DeserializeObject<bool>(responseData.response);
        }
    }
}