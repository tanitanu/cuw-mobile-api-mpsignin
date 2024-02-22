using System.ComponentModel.DataAnnotations;

namespace United.Mobile.MPSignInTool.Models
{
    public class UserLogin
    {
        [Required(ErrorMessage = "UserId is required.")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}