using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface ICustomerDataService
    {
        Task<(T response, long callDuration)> GetCustomerData<T>(string token, string sessionId, string jsonRequest);
        Task<T> InsertMPEnrollment<T>(string token, string request, string sessionId, string path);
        Task<T> ValidateCustomerData<T>(string token, string requestData, string sessionId, string path);
    }
}
