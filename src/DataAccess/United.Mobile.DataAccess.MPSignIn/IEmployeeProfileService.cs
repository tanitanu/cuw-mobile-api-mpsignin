
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface IEmployeeProfileService
    {
        Task<string> GetEmployeeProfile(string token, string request, string sessionId);
    }
}