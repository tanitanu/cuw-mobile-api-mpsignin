using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
using United.Mobile.MPSignInTool.Models;
using Task = System.Threading.Tasks.Task;

namespace United.Mobile.MPSignInTool.DataAccess.CustomImplementation
{
    public class RoleStore : IRoleStore<AppRole>
    {
        private IDbHelper _helper;

        public RoleStore(IDbHelper helper)
        {
            _helper = helper;
        }

        public async Task<IdentityResult> CreateAsync(AppRole role, CancellationToken cancellationToken)
        {
            await _helper.SaveRole(role).ConfigureAwait(false);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(AppRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return IdentityResult.Success;
        }

        public void Dispose()
        {
            //nothing to dispose
        }

        public async Task<AppRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return await _helper.GetRole(roleId).ConfigureAwait(false);
        }

        public async Task<AppRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return await _helper.GetRole(normalizedRoleName).ConfigureAwait(false);
        }

        public Task<string> GetNormalizedRoleNameAsync(AppRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(AppRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(AppRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(AppRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.FromResult(0);
        }

        public Task SetRoleNameAsync(AppRole role, string roleName, CancellationToken cancellationToken)
        {
            role.RoleName = roleName;
            return Task.FromResult(0);
        }

        public Task<IdentityResult> UpdateAsync(AppRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(IdentityResult.Success);
        }
    }
}