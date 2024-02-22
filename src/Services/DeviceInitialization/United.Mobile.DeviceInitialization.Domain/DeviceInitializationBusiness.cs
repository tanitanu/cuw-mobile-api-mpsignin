using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Mobile.Model.DeviceInitialization;
using United.Utility.Helper;

namespace United.Mobile.DeviceInitialization.Domain
{

    public class DeviceInitializationBusiness : IDeviceInitializationBusiness
    {
        private readonly ICacheLog<DeviceInitializationBusiness> _logger;
        private readonly IDynamoDBUtility _dynamoDBUtility;

        public DeviceInitializationBusiness(ICacheLog<DeviceInitializationBusiness> logger
            , IDynamoDBUtility dynamoDBUtility)
        {
            _logger = logger;
            _dynamoDBUtility = dynamoDBUtility;
        }

        public async Task<bool> InsertPushTokenToDB(string accessCode, string transactionId, string deviceId, string apnsDeviceId, int applicationId)
        {
            DeviceData data = new DeviceData
            {
                Deviceid = deviceId,
                PushToken = apnsDeviceId,
                ApplicationId = applicationId
            };
            _logger.LogInformation("InsertPushTokenToDB {@clientRequest}", Newtonsoft.Json.JsonConvert.SerializeObject(data));

            if (GeneralHelper.ValidateAccessCode(accessCode))
            {
                return await _dynamoDBUtility.InsertDevicePushToken(data, transactionId).ConfigureAwait(false);
            }
            return await Task.FromResult(false).ConfigureAwait(false);
        }

        public async Task<DeviceResponse> RegisterDevice(string accessCode, string transactionId, string identifierForVendor, string name, string model, string localizedModel, string systemName, string systemVersion, string applicationId, string applicationVersion)
        {
            DeviceRequest request = new DeviceRequest
            {
                AccessCode = accessCode,
                TransactionId = transactionId,
                IdentifierForVendor = identifierForVendor,
                Name = name,
                Model = model,
                LocalizedModel = localizedModel,
                SystemName = systemName,
                SystemVersion = systemVersion,
                ApplicationId = applicationId,
                ApplicationVersion = applicationVersion
            };

            _logger.LogInformation("RegisterDevice-Business {@clientRequest}", Newtonsoft.Json.JsonConvert.SerializeObject(request));

            DeviceResponse response = new DeviceResponse();
            response.DeviceID = await _dynamoDBUtility.RegisterDevice(request).ConfigureAwait(false);

            response.GUID = identifierForVendor;
            return response;
        }

        public async Task<bool> LegalDocumentUpdateToCache(string key, string transactionId, bool IsForceInsert = false)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            return await _dynamoDBUtility.SaveNewLegalDocumentsForTitles(key, transactionId, IsForceInsert).ConfigureAwait(false);
        }

        public async Task<bool> FetchMPHashPin(string mpnumber, string deviceId, int applicationId, string transactionId)
        {
            if (string.IsNullOrEmpty(mpnumber))
                return false;
            return await _dynamoDBUtility.SaveMPHashPin(mpnumber, deviceId, applicationId, transactionId).ConfigureAwait(false);
        }
    }
}
