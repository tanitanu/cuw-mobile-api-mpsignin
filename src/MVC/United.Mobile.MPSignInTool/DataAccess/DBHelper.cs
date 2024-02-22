using System;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Common;
using United.Mobile.MPSignInTool.Models;
using United.Utility.Helper;

namespace United.Mobile.MPSignInTool.DataAccess
{
    public class DbHelper : IDbHelper
    {
        private readonly IDynamoDBService _dynamoDBService;
        private readonly ICacheLog<DbHelper> _logger;

        public DbHelper(IDynamoDBService dynamoDBService, ICacheLog<DbHelper> logger)
        {
            _logger = logger;
            _dynamoDBService = dynamoDBService;
        }

        public async Task<bool> AddRoleToUser(string user, string role)
        {
            try
            {
                var roleInfo = await GetRole(role).ConfigureAwait(false);
                if (roleInfo != null)
                {
                    var existingUser = await GetUser(user);
                    existingUser.Role = role;
                    return await SaveUser(existingUser).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in AddRoleToUser {@msg}", ex.Message);
            }

            return false;
        }

        public async Task<AppRole> GetRole(string roleName)
        {
            try
            {
                var existingRole = await _dynamoDBService.GetRecords<DynamoRoleModel>("cuw-mpsignintoolroles", Guid.NewGuid().ToString(), roleName.ToUpper(), "sessionid").ConfigureAwait(false);
                if (existingRole != null)
                {
                    return new AppRole
                    {
                        RoleId = existingRole.Name,
                        RoleName = existingRole.Name,
                        NormalizedName = existingRole.Name.ToUpper(),
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetRole {@msg}", ex.Message);
            }

            return default;
        }

        public async Task<AppUser> GetUser(string userId)
        {
            try
            {
                var existingUser = await _dynamoDBService.GetRecords<DynamoUserModel>("cuw-mpsignintoolusers", Guid.NewGuid().ToString(), userId.ToUpper(), "sessionid").ConfigureAwait(false);
                if (existingUser != null)
                {
                    return new AppUser { Id = existingUser.UserId, Email = existingUser.Email, UserName = existingUser.UserId, Name = existingUser.Name, Password = existingUser.Password, PasswordHash = existingUser.PasswordHash, Role = existingUser.Role };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetUser {@msg}", ex.Message);
            }

            return default;
        }

        public async Task<AppRole> GetUserRoles(string userId)
        {
            try
            {
                var existingUser = await GetUser(userId).ConfigureAwait(false);
                if (existingUser != null)
                {
                    var role = await GetRole(existingUser.Role).ConfigureAwait(false);
                    return role;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in GetUserRoles {@msg}", ex.Message);
            }

            return default;
        }

        public async Task<bool> SaveRole(AppRole role)
        {
            try
            {
                DynamoRoleModel newRole = new DynamoRoleModel
                {
                    Name = role.Name
                };
                return await _dynamoDBService.SaveRecords<DynamoRoleModel>("cuw-mpsignintoolroles", Guid.NewGuid().ToString(), role.Id.ToUpper(), newRole, "sessionkey").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in SaveRole {@msg}", ex.Message);
            }
            return false;
        }

        public async Task<bool> SaveUser(AppUser user)
        {
            try
            {
                var newUser = new DynamoUserModel 
                { 
                    UserId = user.Id.ToUpper(), 
                    Name = user.Name, 
                    Email = user.Email,
                    Role = user.Role, 
                    Password = user.Password, 
                    PasswordHash = user.PasswordHash 
                };

                return await _dynamoDBService.SaveRecords<DynamoUserModel>("cuw-mpsignintoolusers", Guid.NewGuid().ToString(), user.Id.ToUpper(), newUser, "sessionkey").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in SaveUser {@msg}", ex.Message);
            }
            return false;
        }

        public async Task<bool> UserHasRoleAsync(string userId, string role)
        {
            try
            {
                var existingUser = await GetUser(userId).ConfigureAwait(false);

                if (existingUser != null)
                {
                    if (existingUser.Role == role)
                        return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in UserHasRoleAsync {@msg}", ex.Message);
            }

            return false;
        }
    }
}