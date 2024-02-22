using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Common;
using United.Services.Customer.Common;

namespace United.Common.Helper.Profile
{
    public interface IMileagePlusTFACSL
    {
        Task<bool> GetTfaWrongAnswersFlag(string sessionid, string token, int customerId, string mileagePlusNumber, bool answeredQuestionsIncorrectly, string languageCode);
        Task<bool> ValidateDevice(Session session, string appVersion, string languageCode);
        Task<SaveResponse> SendForgotPasswordEmail(string sessionid, string token, string mileagePlusNumber, string emailAddress, string languageCode);
        Task<SaveResponse> SendResetAccountEmail(string sessionid, string token, int customerId, string mileagePlusNumber, string emailAddress, string languageCode);
        Task<List<Securityquestion>> GetMPPINPWDSecurityQuestions(string token, string sessionId, int appId, string appVersion, string deviceId);
        bool SignOutSession(string sessionid, string token, int appId);
        Task<bool> ShuffleSavedSecurityQuestions(string token, string sessionId, int appId, string appVersion, string deviceId, string MileagePlusID, int customerID = 0, string loyaltyId = null);
        Task<bool> AddDeviceAuthentication(Session session, string appVersion, string languageCode);
    }
}
