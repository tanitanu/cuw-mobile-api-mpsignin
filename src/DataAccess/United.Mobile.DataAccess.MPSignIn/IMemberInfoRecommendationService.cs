using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MPSignIn
{
    public interface IMemberInfoRecommendationService
    {
        Task<string> RecommendedMPNumbers(string path, string request, string token, string sessionId);
    }
}
