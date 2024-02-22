using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Mobile.Model.Common.CachingAndSessionModels;
using United.Utility.Helper;
using United.Utility.Http;
using Microsoft.Extensions.Logging;

namespace United.Mobile.DataAccess.Common
{
    public class CachingService : ICachingService
    {
        private readonly ICacheLog<CachingService> _logger;
        private readonly IResilientClient _resilientClient;
        private readonly IConfiguration _configuration;
        public CachingService([KeyFilter("cachingConfigKey")] IResilientClient resilientClient, ICacheLog<CachingService> logger, IConfiguration configuration)
        {
            _resilientClient = resilientClient;
            _logger = logger;
            _configuration = configuration;
        }
        
        public async Task<bool> SaveCache<T>(string key, T data, string transactionId, TimeSpan expiry)
        {
            SaveCacheRequest cachingData = new SaveCacheRequest()
            {
                Data = data,
                Key = key,
                TransactionId = transactionId,
                ExpirationOptions = new ExpirationOptions()
                {
                    AbsoluteExpiration = DateTime.UtcNow.AddMinutes(expiry.TotalMinutes),
                    SlidingExpiration = expiry
                }
            };
            string requestData = JsonConvert.SerializeObject(cachingData);
            var dpData = string.Empty;
            using (_logger.BeginTimedOperation("Total time taken for Service call Save caching", transationId: transactionId))
            {
                try
                {
                    dpData = await _resilientClient.PostAsync("Save", requestData).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error while saving the document from caching service: {Key}, {@error}", key, ex);
                }
            }
            if (!string.IsNullOrEmpty(dpData))
                return true;
            return false;
        }

        public async Task<string> GetCache<T>(string key, string transactionId)
        {
            CacheRequest cachingData = new CacheRequest()
            {
                Key = key,
                TransactionId = transactionId
            };
            string requestData = JsonConvert.SerializeObject(cachingData);

            using (_logger.BeginTimedOperation("Total time taken for Service call Get caching", transationId: transactionId))
            {
                try
                {
                    var responseD = await _resilientClient.PostHttpAsync("Get", requestData).ConfigureAwait(false);
                    var response = responseD.Item1;
                    var statusCode = responseD.Item2;
                    var url = responseD.Item3;

                    if (statusCode == HttpStatusCode.OK)
                    {
                        return response;
                    }
                    else if (statusCode == HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("Warning while getting the document from caching service: {Key}", key);
                        return string.Empty;
                    }
                    else
                    {
                        _logger.LogWarning("Warning while getting the document from caching service: {Key} and {message}", key, statusCode);
                        return string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error while Get the document from caching service: {Key}, {@error}", key, ex);
                }
            }

            return string.Empty;
        }
    }
}