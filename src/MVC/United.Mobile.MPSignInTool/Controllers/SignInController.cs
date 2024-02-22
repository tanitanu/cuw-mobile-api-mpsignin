using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using United.Mobile.MPSignInTool.DataAccess;
using United.Mobile.MPSignInTool.Models;

namespace United.Mobile.MPSignInTool.Controllers
{
    public class SignInController : Controller
    {
        private readonly IAccountManager _accountManager;
        private readonly SignInManager<AppUser> _signInManager;
        public SignInController(
            IAccountManager accountManager, 
            SignInManager<AppUser> signInManager)
        {
            _accountManager = accountManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("Home/Index/");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> IndexAsync(UserLogin login)
        {
            if (!ModelState.IsValid)
                return View(login);

            var user = await _accountManager.GetUserById(login.UserId.ToUpper()).ConfigureAwait(false);
            if (user != null)
            {
                var signInResult = await _signInManager.PasswordSignInAsync(user, login.Password, false, false).ConfigureAwait(false);

                if (signInResult.Succeeded)
                {
                    return RedirectToAction("Index", "DynamoSearch");
                }
            }
            return View(login);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync().ConfigureAwait(false);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }
    }
}