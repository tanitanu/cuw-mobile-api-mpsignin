using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using United.Mobile.MPSignInTool.Domain;

namespace United.Mobile.MPSignInTool.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ICatalogBusiness _catalogBusiness;
        public CatalogController(ICatalogBusiness catalogBusiness)
        {
            _catalogBusiness = catalogBusiness;
        }

        public async Task<IActionResult> Index()
        {
            var report = await _catalogBusiness.GetCatalogItems();
            return View(report);
        }
    }
}