using System.Threading.Tasks;
using United.Mobile.Model.Internal.Common;

namespace United.Common.Helper
{
    public interface IShoppingSessionHelper
    {
        Task<Session> CreateShoppingSession(int applicationId, string deviceId, string appVersion, string transactionId, string mileagPlusNumber, string employeeId, bool isBEFareDisplayAtFSR = false, bool isReshop = false, bool isAward = false, string travelType = "", string flow = "", string hashPin = "");
        Task<(bool isTokenValid, Session shopTokenSession)> CheckIsCSSTokenValid(int applicationId, string deviceId, string appVersion, string transactionId, Session shopTokenSession, string tokenToValidate);
        Task<Session> GetShoppingSession(string sessionId);
        Task<Mobile.Model.EmployeeReservation.Session> CreateEmpShoppingSession(string sessionId, string mpNumber, string employeeId, string eResTransactionId, string eResSessionId);
        Task<(bool returnValue, string validAuthToken)> ValidateHashPinAndGetAuthToken(string accountNumber, string hashPinCode, int applicationId, string deviceId, string appVersion, string validAuthToken, string transactionId, string sessionId);
    }
}
