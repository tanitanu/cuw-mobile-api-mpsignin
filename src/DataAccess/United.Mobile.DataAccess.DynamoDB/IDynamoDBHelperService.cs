using System.Threading.Tasks;

namespace United.Mobile.DataAccess.DynamoDB
{
    public interface IDynamoDBHelperService
    {
        Task<bool> SaveTSAFlaggedAccount<T>(string key, T accNumber, string sessionID);
        Task<bool> IsTSAFlaggedAccount(string key, string sessionID);
        Task<T> GetEResBetaTesterItems<T>(string applicationId, string appVersion, string mileageplusNumber, string sessionId);
        Task<T> GetAuthToken<T>(string accountNumber,int applicationId, string deviceId, string sessionId);
        Task<bool> SaveMPAppIdDeviceId<T>(T data, string sessionId, string key, string secondaryKey = "001", string transactionId = "transId");
        Task<bool> IsVBQWelcomeModelDisplayed(string mileagePlusNumber, string applicationId, string deviceId, string sessionId);
        Task<bool> InsertDevicePushToken<T>(T data, string key, string sessionID);
        Task<bool> RegisterDevice<T>(T data, string key, string transactionId);
        Task<bool> RegisterDeviceHistory<T>(T data, string key, string transactionId);
        Task<bool> InsertMileagePlusAndHash<T>(T data, string key, string sessionId);
        Task<T> GetDeviceIdAppIdMPNumber<T>(string key, string transactionId);

        Task<bool> InsertMilagePlusDevice<T>(T data, string key, string sessionId);
    }
}
