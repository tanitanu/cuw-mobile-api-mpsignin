using System.Collections.Generic;
using System.Threading.Tasks;
using United.Definition;
using United.Mobile.Model.Common.CloudDynamoDB;
using United.Mobile.Model.DeviceInitialization;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.Internal.Common.DynamoDB;

namespace United.Common.Helper
{
    public interface IDynamoDBUtility
    {
        Task<bool> IsVBQWelcomeModelDisplayed(int applicationId, string deviceid, string mileagePlusNumber, string sessionid);
        Task<bool> InsertDevicePushToken(DeviceData data,string transactionId);
        Task InsertMileagePlusAndHash(string mileagePlusNumber, string hashValue, string sessionId);
        Task<bool> IsTSAFlaggedAccount(string accountNumber, string sessionid);
        Task InsertUpdateMileagePlusAndPin(MileagePlusDetails cloudResult, bool iSDPAuthentication, string key, string sessionId, string transactionId);
        Task<int> RegisterDevice(DeviceRequest request);
        Task<List<MOBLegalDocument>> GetNewLegalDocumentsForTitles(string titles, string transactionId, bool isTermsnConditions);
        Task<bool> SaveNewLegalDocumentsForTitles(string title, string transactionId, bool IsForceInsert = false);
        Task<bool> SaveMPHashPin(string mpnumber, string deviceId, int applicationId, string transactionId);
        Task<EResBetaTester> GetEResBetaTesterItems(int applicationId, string appVersion, string mileageplusNumber, string sessionid);
        Task InsertMilagePlusDevice(MilagePlusDevice data, string sessionId);
    }
}