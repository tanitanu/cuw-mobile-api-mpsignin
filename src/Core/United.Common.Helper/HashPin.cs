using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.OnPremiseSQLSP;
using United.Mobile.Model.Common.CloudDynamoDB;
using United.Mobile.Model.Internal.Exception;
using United.Utility.Helper;

namespace United.Common.Helper
{
    public class HashPin : IHashPin
    {
        private readonly ICacheLog<HashPin> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDPService _dPService;
        private readonly ISQLSPService _sqlSPService;
        private readonly IDynamoDBHelperService _dynamoDBHelperService;
        private readonly IHeaders _headers;
        private readonly IDynamoDBUtility _dynamoDBUtility;
        private readonly ICachingService _cachingService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFeatureSettings _featureSettings;

        public HashPin(ICacheLog<HashPin> logger
            , IConfiguration configuration
            , IDPService dPService
            , ISQLSPService sqlSPService
            , IDynamoDBHelperService dynamoDBHelperService
            , IHeaders headers
            , IDynamoDBUtility dynamoDBUtility
            , ICachingService cachingService
            , IHttpContextAccessor httpContextAccessor
            , IFeatureSettings featureSettings
            )
        {
            _logger = logger;
            _configuration = configuration;
            _dPService = dPService;
            _sqlSPService = sqlSPService;
            _dynamoDBHelperService = dynamoDBHelperService;
            _headers = headers;
            _dynamoDBUtility = dynamoDBUtility;
            _cachingService = cachingService;
            _httpContextAccessor = httpContextAccessor;
            _featureSettings = featureSettings;
        }

        public async Task<MileagePlusDetails> ValidateHashPinAndGetAuthTokenDynamoDB(string accountNumber, int customerId, string hashPinCode
            , int applicationId, string deviceId, string appVersion, string sessionid = "")
        {
            var path = _httpContextAccessor.HttpContext.Request.Path.ToString().Split('/');
            var controller = path.ElementAtOrDefault(1);
            var action = path.ElementAtOrDefault(path.Length - 1);

            var hashpinresponse = await GetMPRecordUsingHash_Cache(accountNumber, customerId, hashPinCode, Convert.ToInt32(applicationId), deviceId, appVersion, sessionid).ConfigureAwait(false);

            _logger.LogInformation("GetMPRecord {@ServiceAction} {@message}", "MPSignIn", $"{controller}:{action}", hashpinresponse.message);

            if (hashpinresponse.details == null)
            {
                _logger.LogError("ValidateHashPinAndGetAuthToken-UnAutorized");
                throw new MOBUnitedException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            return hashpinresponse.details;
        }

        public async Task<(MileagePlusDetails details, string message)> GetMPRecordUsingHash_Cache(string accountNumber, int customerId, string hashPinCode, int applicationId, string deviceId, string appVersion, string sessionId)
        {
            if (string.IsNullOrEmpty(accountNumber))
            {
                _logger.LogWarning("GetMPRecordUsingHash_Cache MP Account is null");
                return (null, "MP Account is empty");
            }

            MileagePlusDetails record = new MileagePlusDetails();

            var key = $"{accountNumber}::{applicationId}::{deviceId}";

            var response = await _cachingService.GetCache<MileagePlusDetails>(key, _headers.ContextValues?.TransactionId);
            record = JsonConvert.DeserializeObject<MileagePlusDetails>(response);

            if (record?.HashPincode == hashPinCode)
            {
                _logger.LogInformation("GetMPRecordUsingHash-Record fetched from Cache {@RecordId}", key);

                if (string.IsNullOrEmpty(record?.DataPowerAccessToken))
                {
                    record.DataPowerAccessToken = await _dPService.GetAnonymousToken(applicationId, deviceId, _configuration).ConfigureAwait(false);
                }
                record.CustomerID = customerId != 0 ? Convert.ToString(customerId) : record.CustomerID;

                return (record, "Record found from Cache");
            }

            record = await _dynamoDBHelperService.GetAuthToken<MileagePlusDetails>(accountNumber, applicationId, deviceId, sessionId).ConfigureAwait(false);

            if (record?.HashPincode == hashPinCode)
            {
                _logger.LogInformation("GetMPRecordUsingHash-Record fetched from Dynamo {@RecordId}", key);

                if (string.IsNullOrEmpty(record?.DataPowerAccessToken))
                {
                    record.DataPowerAccessToken = await _dPService.GetAnonymousToken(applicationId, deviceId, _configuration).ConfigureAwait(false);
                }
                record.CustomerID = customerId != 0 ? Convert.ToString(customerId) : record.CustomerID;

                var expiry = TimeSpan.FromSeconds(_configuration.GetSection("dpTokenConfig").GetValue<double>("tokenExpInSec"));
                await _cachingService.SaveCache<MileagePlusDetails>(key, record, _headers.ContextValues?.TransactionId, expiry);

                return (record, "Record found from DynamoDB");
            }

            if (!_configuration.GetValue<bool>("DisableSQLSupport") && !await _featureSettings.GetFeatureSettingValue("DisableSQLGetHashPin"))
            {
                try
                {
                    appVersion = (string.IsNullOrEmpty(appVersion)) ? "1.0.0" : appVersion;
                    record = await _sqlSPService.GetMPRecord<MileagePlusDetails>(accountNumber, applicationId, deviceId, hashPinCode, appVersion, sessionId).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError("SQL Service Exception {@msg} {@stacktrace}", ex.Message, ex.StackTrace);
                }

                if (!string.IsNullOrEmpty(record?.MileagePlusNumber))
                {
                    _logger.LogInformation("GetMPRecordUsingHash-Record fetched from SQL {@RecordId}", key);

                    if (string.IsNullOrEmpty(record?.DataPowerAccessToken))
                    {
                        record.DataPowerAccessToken = await _dPService.GetAnonymousToken(applicationId, deviceId, _configuration).ConfigureAwait(false);
                    }
                    record.CustomerID = customerId != 0 ? Convert.ToString(customerId) : record.CustomerID;
                    try
                    {
                        await _dynamoDBHelperService.SaveMPAppIdDeviceId<MileagePlusDetails>(record, sessionId, key, record.MileagePlusNumber, sessionId).ConfigureAwait(false);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError("GetMPRecordUsingHash-Record SaveRecords {errormessage} ", ex.Message);
                    }
                    var expiry = TimeSpan.FromSeconds(_configuration.GetSection("dpTokenConfig").GetValue<double>("tokenExpInSec"));
                    await _cachingService.SaveCache<MileagePlusDetails>(key, record, _headers.ContextValues?.TransactionId, expiry);

                    return (record, "Record found from SQL");
                }
                return (null, "Record Not found from DynamoDB & SQL");
            }

            return (null, "Record Not found from DynamoDB");
        }
    }

}
