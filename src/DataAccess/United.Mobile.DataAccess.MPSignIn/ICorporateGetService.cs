using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MPSignIn
{
    public interface ICorporateGetService
    {
        Task<(T response, long callDuration)> GetData<T>(string token, string sessionId, string jsonRequest);
        Task<string> GetCorpFOPData(string token, string sessionId, string jsonRequest);
        Task<string> GetCorpProfileData(string token, string sessionId, string jsonRequest);
    }
}
