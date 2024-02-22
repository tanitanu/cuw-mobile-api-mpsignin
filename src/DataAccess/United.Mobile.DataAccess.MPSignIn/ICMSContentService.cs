using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MPSignIn
{
    public interface ICMSContentService
    {
        Task<T> GetMessages<T>(string token, string sessionId, string jsonRequest);
    }
}
