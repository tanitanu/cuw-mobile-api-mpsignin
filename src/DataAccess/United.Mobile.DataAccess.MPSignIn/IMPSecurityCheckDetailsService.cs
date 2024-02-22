using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface IMPSecurityCheckDetailsService
    {
        Task<string> GetMPSecurityCheckDetails(string token, string requestData, string sessionId);
        Task<string> UpdateTfaWrongAnswersFlag(string token, string requestData, string sessionId);
        Task<T> InsertMPEnrollment<T>(string token, string request, string sessionId, string path);
        Task<T> ValidateCustomerData<T>(string token, string requestData, string sessionId, string path);
    }
}
