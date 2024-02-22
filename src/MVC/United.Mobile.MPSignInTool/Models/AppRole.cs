using Microsoft.AspNetCore.Identity;

namespace United.Mobile.MPSignInTool.Models
{
    public class AppRole : IdentityRole
    {
        public string RoleId { get; set; }
        public string RoleName { get; set;}
    }
}