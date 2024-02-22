using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface IEmployeeService
    {
        Task<string> GetEmployeeId(string mileageplusNumber, string transactionId, string sessionId);
    }
}
