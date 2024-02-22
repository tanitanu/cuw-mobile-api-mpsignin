using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.MPSignInTool.DataAccess;
using United.Mobile.MPSignInTool.Models;

namespace United.Mobile.MPSignInTool.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class UserRegistrationController : Controller
    {
        private readonly IAccountManager _accountManager;
        public UserRegistrationController(IAccountManager accountManager)
        {
            _accountManager = accountManager;
        }
        public IActionResult Index()
        {
            RegistrationForm registration = new RegistrationForm
            {
                UserTypes = GetAccountType(),
                RequestType = "Search"
            };
            return View(registration);
        }

        [HttpPost]
        public IActionResult Index(RegistrationForm user)
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Search(RegistrationForm registrationForm)
        {
            var user = await _accountManager.GetUserById(registrationForm.UserId).ConfigureAwait(false);

            if(user != null)
            {
                var role = _accountManager.GetRoles(user).Result[0];
                
                registrationForm.UserType = role;
                registrationForm.Email = user.Email;
                registrationForm.Name = user.Name;
                registrationForm.RequestType = "Update";
            } 
            else
            {
                registrationForm.RequestType = "Add";
            }

            registrationForm.UserTypes = GetAccountType();

            return View("Index", registrationForm);
        }

        [HttpPost]
        public async Task<IActionResult> Add(RegistrationForm registrationForm)
        {
            if (!ModelState.IsValid)
            {
                registrationForm.RequestType = "Add";
                registrationForm.UserTypes = GetAccountType();
                return View("Index", registrationForm);
            }

            var existingUser = await _accountManager.GetUserById(registrationForm.UserId).ConfigureAwait(false);

            if(existingUser == null)
            {
                await _accountManager.AddUser(registrationForm).ConfigureAwait(false);
                TempData["SuccessMessage"] = $"{registrationForm.UserId} Successfully Added.";
            } 
            else
            {
                TempData["SuccessMessage"] = $"{registrationForm.UserId} Already Exist.";
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Update(RegistrationForm registrationForm)
        {
            if (!ModelState.IsValid)
            {
                registrationForm.RequestType = "Update";
                registrationForm.UserTypes = GetAccountType();
                return View("Index", registrationForm);
            }

            var existingUser = _accountManager.GetUserById(registrationForm.UserId);

            if(existingUser == null)
            {
                TempData["SuccessMessage"] = $"{registrationForm.UserId} Doesn't Exist, Register First.";
            } 
            else
            {
                await _accountManager.UpdateUser(registrationForm).ConfigureAwait(false);
                TempData["SuccessMessage"] = $"{registrationForm.UserId} Successfully Updated.";
            }
            
            return RedirectToAction("Index");
        }

        public List<SelectListItem> GetAccountType() 
            => new List<SelectListItem>
        {
            new SelectListItem {Text = "User", Value = "USER"},
            new SelectListItem {Text = "Dev", Value = "DEV"},
            new SelectListItem {Text = "Admin", Value = "ADMIN"}
        };
    }
}
