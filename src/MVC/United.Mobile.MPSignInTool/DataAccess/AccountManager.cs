using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.MPSignInTool.Models;
using United.Utility.Helper;

namespace United.Mobile.MPSignInTool.DataAccess
{
    public class AccountManager : IAccountManager
    {
        private readonly ICacheLog<AccountManager> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public AccountManager(
            ICacheLog<AccountManager> logger,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<bool> AddRole(AppUser user, string role)
        {
            try
            {
                var roles = _userManager.GetRolesAsync(user).Result;
                if (!roles.Contains(role))
                {
                    await _userManager.RemoveFromRoleAsync(user, roles[0]).ConfigureAwait(false);

                    var roleExist = await _roleManager.RoleExistsAsync(role).ConfigureAwait(false);
                    if (!roleExist)
                    {
                        await _roleManager.CreateAsync(new AppRole { Id = role, Name = role }).ConfigureAwait(false);

                    }
                    var res = await _userManager.AddToRoleAsync(user, role).ConfigureAwait(false);
                    return res.Succeeded;
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Error AddRole {@msg}", ex.Message);
            }
            return false;
        }

        public async Task<bool> AddUser(RegistrationForm registrationForm)
        {
            try
            {
                var roleExist = await _roleManager.RoleExistsAsync(registrationForm.UserType);
                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new AppRole { Id = registrationForm.UserType, Name = registrationForm.UserType }).ConfigureAwait(false);
                }

                var existinguser = await _userManager.FindByIdAsync(registrationForm.UserId).ConfigureAwait(false);
                if (existinguser != null)
                    return false;

                var user = new AppUser
                {
                    Id = registrationForm.UserId.ToUpper(),
                    Email = registrationForm.Email,
                    UserName = registrationForm.UserId,
                    Name = registrationForm.Name,
                    Password = registrationForm.Password,
                    Role = registrationForm.UserType
                };

                var result = await _userManager.CreateAsync(user, user.Password).ConfigureAwait(false);
                return result.Succeeded;
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Error AddUser {@msg}", ex.Message);
            }
            return false;
        }

        public async Task<IList<string>> GetRoles(AppUser user)
        {
            try
            {
                return await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Error GetRoles {@msg}", ex.Message);
            }
            return default;
        }

        public async Task<AppUser> GetUserById(string userId)
        {
            try
            {
                return await _userManager.FindByIdAsync(userId).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Error GetUserById {@msg}", ex.Message);
            }
            return default;
        }

        public async Task<bool> UpdateUser(RegistrationForm registrationForm)
        {
            try
            {
                var existingUser = await _userManager.FindByIdAsync(registrationForm.UserId).ConfigureAwait(false);
                if (existingUser == null)
                    return false;

                existingUser.Name = registrationForm.Name;
                existingUser.Email = registrationForm.Email;

                var roleExist = await _roleManager.RoleExistsAsync(registrationForm.UserType);
                if (!roleExist)
                {
                    var role = new AppRole { Id = registrationForm.UserType, Name = registrationForm.UserType };
                    await _roleManager.CreateAsync(role).ConfigureAwait(false);
                }

                existingUser.Role = registrationForm.UserType;
                
                var result = await _userManager.UpdateAsync(existingUser).ConfigureAwait(false);
                _ = await _userManager.ChangePasswordAsync(existingUser, existingUser.Password, registrationForm.Password).ConfigureAwait(false);
                
                return result.Succeeded;
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Error UpdateUser {@msg}", ex.Message);
            }
            return false;
        }
    }
}