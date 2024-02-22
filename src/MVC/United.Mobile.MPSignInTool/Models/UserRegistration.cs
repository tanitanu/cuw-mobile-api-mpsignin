using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace United.Mobile.MPSignInTool.Models
{
    public class RegistrationForm
    {
        [Required(ErrorMessage ="UserId is required.")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
        
        [Required(ErrorMessage = "UserType is required.")]
        public string UserType { get; set; }

        public List<SelectListItem> UserTypes { get; set; }

        public string RequestType { get; set; }
    }
}
