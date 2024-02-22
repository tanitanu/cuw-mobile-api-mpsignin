using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.MPSignInTool.Models;

namespace United.Mobile.MPSignInTool.DataAccess
{
    public interface IAccountManager
    {
        Task<bool> AddUser(RegistrationForm registrationForm);
        Task<bool> UpdateUser(RegistrationForm registrationForm);
        Task<bool> AddRole(AppUser user,string role);
        Task<AppUser> GetUserById(string userId);
        Task<IList<string>> GetRoles(AppUser user);
    }
}