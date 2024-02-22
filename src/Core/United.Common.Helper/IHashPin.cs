using System.Threading.Tasks;
using United.Mobile.Model.Common.CloudDynamoDB;

namespace United.Common.Helper
{
    public interface IHashPin
    {
        Task<MileagePlusDetails> ValidateHashPinAndGetAuthTokenDynamoDB(string accountNumber,int customerId, string hashPinCode
            , int applicationId, string deviceId, string appVersion, string sessionid = "");

        Task<(MileagePlusDetails details, string message)> GetMPRecordUsingHash_Cache(string accountNumber, int customerId, string hashPinCode
            , int applicationId, string deviceId, string appVersion, string sessionId);
    }
}