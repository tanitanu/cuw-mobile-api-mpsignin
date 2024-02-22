using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface IMPSecurityQuestionsService
    {
        Task<string> GetMPPINPWDSecurityQuestions(string token, string requestData, string sessionId);
        Task<string> ValidateSecurityAnswer(string token, string requestData, string sessionId, string path);
        Task<string> AddDeviceAuthentication(string token, string requestData, string sessionId, string path );
    }
}
