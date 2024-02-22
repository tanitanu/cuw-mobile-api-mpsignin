using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Common;

namespace United.Mobile.DataAccess.Common
{
    public interface IDataPowerFactory
    {
        Task<string> GetAnonymousCSSToken(int applicationId, string deviceId, string appVersion, string transactionId, Session persistToken, bool SaveToPersist = true, bool SaveToDataBase = false, string mpNumber = "", string customerID = "");

        Task<bool> CheckIsDPTokenValid(string _dpAccessToken, Session shopTokenSession, string transactionId, bool SaveToPersist = true);
        Task<DPAccessTokenResponse> GetDPAuthenticatedToken(int applicationID, string deviceId, string transactionId, string appVersion, Session TokenSessionObj, string username, string password, string usertype, string anonymousToken, bool SaveToPersist = true);
    }
}
