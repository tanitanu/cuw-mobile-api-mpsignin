using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Common;
using United.Utility.Helper;
using United.Utility.Http;
using Microsoft.Extensions.Logging;

namespace United.Mobile.DataAccess.Common
{
    public class SessionOnCloudService : ISessionOnCloudService
    {
        private readonly ICacheLog<SessionOnCloudService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IResilientClient _resilientClient;
        public SessionOnCloudService([KeyFilter("sessionOnCloudConfigKey")] IResilientClient resilientClient, ICacheLog<SessionOnCloudService> logger, IConfiguration configuration)
        {
            _resilientClient = resilientClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<string> GetSession(string sessionID, string objectName, List<string> vParams = null, string transactionId = "Test", bool isReadOnPrem = false)
        {
            SessionRequest sessionRequest = new SessionRequest()
            {
                TransactionId = transactionId,
                ValidationParams = vParams,
                SessionId = sessionID,

                ObjectName = objectName
            };
            var requestData = JsonConvert.SerializeObject(sessionRequest);

            var endpoint = isReadOnPrem ? $"GetSessionV2?isReadOnPrem={isReadOnPrem}" : $"GetSession";
            _logger.LogInformation("GetSession {endpoint} - request {RequestData}", endpoint, requestData);

            var sessionReturnValue = await _resilientClient?.PostHttpAsyncWithOptions(endpoint, requestData);

            if ((sessionReturnValue.statusCode != HttpStatusCode.OK) && (sessionReturnValue.statusCode != HttpStatusCode.NotFound))
            {
                _logger.LogError("GetSession  {requestUrl}, error {response} ", sessionReturnValue.url, sessionReturnValue.response);
                if (sessionReturnValue.statusCode != HttpStatusCode.BadRequest)
                    throw new Exception();
            }
            _logger.LogInformation("GetSession {requestUrl}, {response}", sessionReturnValue.url, sessionReturnValue.response);

            return sessionReturnValue.response;
        }

        public async Task<bool> SaveSessionONCloud<T>(T data, string sessionID, List<string> validateParams, string objectName, TimeSpan expiry, string transactionId = "Test")
        {
            SaveSessionRequest saveSessionRequest = new SaveSessionRequest()
            {
                TransactionId = transactionId,
                SessionId = sessionID,
                ObjectName = objectName,
                Data = data,
                ValidationParams = validateParams,
                ExpirationOptions = new ExpirationOptions()
                {
                    AbsoluteExpiration = DateTime.UtcNow.AddMinutes(90),
                    SlidingExpiration = expiry
                }
            };

            try
            {
                var requestData = JsonConvert.SerializeObject(saveSessionRequest);
                _logger.LogInformation("SaveSessionONCloud {requestData} ", requestData);

                var savedResult = await _resilientClient.PostHttpAsyncWithOptions("SaveSessionONCloudV2", requestData).ConfigureAwait(false);

                if (savedResult.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("SaveSessionONCloud {requestUrl} error {response} ", savedResult.url, savedResult.response);
                    if (savedResult.statusCode != HttpStatusCode.BadRequest)
                        return false;
                }

                _logger.LogInformation("SaveSessionONCloud {requestUrl}, {response} ", savedResult.url, savedResult.response);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Task SaveSessionOnPrem(string data, string sessionID, List<string> validateParams, string objectName, TimeSpan expiry, string transactionId = "Test")
        {
            SaveSessionRequest saveSessionRequest = new SaveSessionRequest()
            {
                TransactionId = transactionId,
                SessionId = sessionID,
                ObjectName = objectName,
                Data = data,
                ValidationParams = validateParams,
                ExpirationOptions = new ExpirationOptions()
                {
                    AbsoluteExpiration = DateTime.UtcNow.AddMinutes(90),
                    SlidingExpiration = expiry
                }
            };

            try
            {
                var requestData = JsonConvert.SerializeObject(saveSessionRequest);
                //_logger.LogInformation("SaveSessionOnPrem {requestData} ", requestData);

                var savedResult = _resilientClient.PostHttpAsyncWithOptions("SaveSessionOnCouchbase", requestData);

                //if (savedResult.Result.statusCode != HttpStatusCode.OK)
                //{
                //    _logger.LogError("SaveSessionOnPrem {requestUrl} error {response} ", savedResult.Result.url, savedResult.Result.response);
                //    if (savedResult.Result.statusCode != HttpStatusCode.BadRequest)
                //        return false;
                //}

                //_logger.LogInformation("SaveSessionOnPrem {requestUrl}, {response} ", savedResult.Result.url, savedResult.Result.response);
            }
            catch (Exception ex)
            {
               // _logger.LogError("Exception - SaveSessionOnPrem {Exception}, {ValidateParam}, {StackTrace} ", ex.Message, validateParams, ex.StackTrace);
                throw ex;
            }
            return default;
        }
    }
}
