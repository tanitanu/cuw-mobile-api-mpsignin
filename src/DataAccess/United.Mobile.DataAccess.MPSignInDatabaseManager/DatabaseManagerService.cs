using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Mobile.Model.MPSignIn.CCE;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.MPSignInDatabaseManager
{
    public class DatabaseManagerService : IDatabaseManagerService
    {
        private readonly ICacheLog<DatabaseManagerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IResilientClient _resilientClient;

        public DatabaseManagerService(
              [KeyFilter("DatabaseManagerClientKey")] IResilientClient resilientClient
            , ICacheLog<DatabaseManagerService> logger
            , IConfiguration configuration)
        {
            _resilientClient = resilientClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<string> GetRecordsFromDynamoDB(string mileagePlusNumber, string transactionId)
        {
            IDisposable timer = null;
            string returnValue = string.Empty;

            using (timer = _logger.BeginTimedOperation("Total time taken for GetRecordsFromDynamoDB service call", transationId: transactionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept","application/json" }
                     };

                string path = string.Format("/GetRecordsBySecondaryKey");
                string tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("uatb_MileagePlusValidation_CSS") ?? "cuw-validate-mp-appid-deviceid";
                var data = new ForceSignOutDataRequest { TableName = tableName, SecondaryKey = mileagePlusNumber, TransactionId = transactionId};
                _logger.LogInformation("GetRecordsFromDynamoDB Service {@cslRequest} {@RequestUrl}", data, path);
                
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, JsonConvert.SerializeObject(data), headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetRecordsFromDynamoDB Service {@RequestUrl} error {@cslResponse}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                    {
                        return default;
                    }
                }

                _logger.LogInformation("GetRecordsFromDynamoDB Service {@RequestUrl} {@cslResponse}", responseData.url, returnValue);

                returnValue = responseData.response;
            }

            return returnValue;
        }
    }
}
