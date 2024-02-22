using System.Threading.Tasks;
using United.Mobile.MPSignInTool.Models;

namespace United.Mobile.MPSignInTool.DataAccess
{
    public interface IDbHelper
    {
        Task<AppUser> GetUser(string userId);
        Task<bool> SaveUser(AppUser user);
        Task<bool> AddRoleToUser(string user, string role);
        Task<AppRole> GetUserRoles(string userId);
        Task<bool> UserHasRoleAsync(string userId, string role);
        Task<AppRole> GetRole(string role);
        Task<bool> SaveRole(AppRole role);
    }
}