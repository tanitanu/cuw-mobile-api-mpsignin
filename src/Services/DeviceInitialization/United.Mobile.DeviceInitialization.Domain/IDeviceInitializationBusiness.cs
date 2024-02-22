using System.Threading.Tasks;
using United.Mobile.Model.DeviceInitialization;

namespace United.Mobile.DeviceInitialization.Domain
{
    public interface IDeviceInitializationBusiness
    {
        Task<bool> InsertPushTokenToDB(string accessCode, string transactionId, string deviceId, string apnsDeviceId, int applicationId);
        Task<DeviceResponse> RegisterDevice(string accessCode, string transactionId, string identifierForVendor, string name, string model, string localizedModel, string systemName, string systemVersion, string applicationId, string applicationVersion);
        Task<bool> LegalDocumentUpdateToCache(string key, string transactionId,bool IsForceInsert = false);
        Task<bool> FetchMPHashPin(string mpnumber, string deviceId, int applicationId, string transactionId);
    }
}
