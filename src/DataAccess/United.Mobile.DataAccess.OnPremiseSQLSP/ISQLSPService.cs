using System.Collections.Generic;
using System.Threading.Tasks;
using United.Definition;
using United.Mobile.Model.DeviceInitialization;

namespace United.Mobile.DataAccess.OnPremiseSQLSP
{
    public interface ISQLSPService
    {
        Task<T> RegisterDevice<T>(DeviceRequest requestData, string transId);

        Task<bool> InsertDevicePushToken(string deviceId, int applicationId, string pushToken, string transactionid);

        Task<List<MOBLegalDocument>> GetNewLegalDocumentsForTitles(string titles, string transactionId, bool isTermsnConditions);

        Task<T> ValidateHashPin<T>(string accountNumber, string hashPinCode, int applicationId, string deviceId, string appVersion, string transactionId, string sessionId);

        Task<bool> InsertMileagePlusAndHash(string mileagePlusNumber, string hashValue, string sessionId);

        Task<bool> InsertUpdateMileagePlusAndPinDP(string requestData, bool iSDPAuthentication, string sessionId);

        Task<bool> IsTSAFlaggedAccount(string accountNumber, string sessionid);

        Task<bool> IsEIsEResBetaTester(int applicationId, string applicationVersion, string mileagePlusNumber, string sessionid);

        Task<bool> IsVBQWMDisplayed(int applicationId, string deviceid, string mileagePlusNumber, string sessionid);
        Task<string> GetMPHashPin(string mpnumber, string deviceId, int applicationId, string transactionId);
        Task<bool> InsertMilagePlusDevice(string deviceId, string applicationId, string mpNumber, string customerID, string sessionid);
        Task<T> GetAuthToken<T>(string accountNumber, string hashPinCode, int applicationId, string deviceId, string appVersion, string transactionId);
        Task<T> GetMPRecord<T>(string accountNumber, int applicationId, string deviceId, string hashPinCode, string appVersion, string sessionId);
        Task<T> IsAccountExist<T>(string clientId, string profileNumber, string transactionId);
    }
}
