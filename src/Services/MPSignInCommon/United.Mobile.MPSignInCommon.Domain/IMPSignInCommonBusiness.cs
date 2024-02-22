using System.Threading.Tasks;
using United.Mobile.Model.MPSignIn;
using United.Mobile.Model.MPSignIn.CCE;
using United.Mobile.Model.MPSignIn.HashpinVerify;

namespace United.Mobile.MPSignInCommon.Domain
{
    public interface IMPSignInCommonBusiness
    {
        Task<MPTokenResponse> GetMileagePlusAndPinDP(string mileagePlusNumber, int applicationId, string deviceId, string appVersion, string hashPinCode, string consumerName, string transactionId);
        Task<HashpinVerifyResponse> GetMPRecord(HashpinVerifyRequest request);
        Task<CCEResponse> IsAccountExist(CCERequest request);
    }
}
