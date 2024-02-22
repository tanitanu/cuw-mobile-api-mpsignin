using Microsoft.AspNetCore.Mvc;
using United.Mobile.MPSignInTool.Domain;
using United.Utility.Helper;

namespace United.Mobile.MPSignInTool.Controllers
{
    public class HealthCheckController : Controller
    {
        private readonly ICacheLog<HealthCheckController> _logger;
        private readonly IMPSignInToolBusiness _mPSignInToolBusiness;

        public HealthCheckController(
            ICacheLog<HealthCheckController> logger,
            IMPSignInToolBusiness mPSignInToolBusiness)
        {
            _logger = logger;
            _mPSignInToolBusiness = mPSignInToolBusiness;
        }

        public ViewResult Index()
        {
            var mpSignInTool = _mPSignInToolBusiness.GetAllServiceDetails().Result;
            return View(mpSignInTool);
        }

        public ViewResult MPSignInServices()
        {
            var mpSignInTool = _mPSignInToolBusiness.GetAllServiceDetails().Result;
            return View(mpSignInTool);
        }
    }
}