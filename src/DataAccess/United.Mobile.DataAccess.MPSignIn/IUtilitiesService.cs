using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface IUtilitiesService
    {
        Task<T> ValidateMileagePlusNames<T>(string token, string requestData, string sessionId, string path);
    }
}
