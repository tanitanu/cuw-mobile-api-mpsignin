using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface IPNRRetrievalService
    {
        Task<string> UpdateTravelerInfo(string token, string requestData, string path, string sessionId);
    }
}
