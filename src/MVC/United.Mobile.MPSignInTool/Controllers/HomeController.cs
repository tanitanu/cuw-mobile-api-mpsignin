using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using United.Mobile.MPSignInTool.Domain;
using United.Mobile.MPSignInTool.Models;
using United.Utility.Helper;

namespace United.Mobile.MPSignInTool.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICacheLog<HomeController> _logger;
        private readonly IMPSignInToolBusiness _mpSignInToolBusiness;

        public HomeController(
            ICacheLog<HomeController> logger,
            IMPSignInToolBusiness mPSignInToolBusiness)
        {
            _logger = logger;
            _mpSignInToolBusiness = mPSignInToolBusiness;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}