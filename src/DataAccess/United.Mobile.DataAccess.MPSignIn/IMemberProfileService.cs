using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MPSignIn
{
    public interface IMemberProfileService
    {
        Task<T> SearchMemberInfo<T>(string token, string requestData, string sessionId);
        Task<string> ValidateMemberName(string token, string requestData, string sessionId, string path);
    }
}
