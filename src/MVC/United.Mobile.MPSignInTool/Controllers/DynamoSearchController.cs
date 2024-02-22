using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.MPSignInTool.Domain;
using United.Mobile.MPSignInTool.Models;
using United.Utility.Helper;

namespace United.Mobile.MPSignInTool.Controllers
{
    [Authorize(Roles = "ADMIN,DEV")]
    public class DynamoSearchController : Controller
    {
        private readonly ICacheLog<DynamoSearchController> _logger;
        private readonly IDynamoSearchToolBusiness _dynamoSearchToolBusiness;
        public DynamoSearchController(
            ICacheLog<DynamoSearchController> logger, 
            IDynamoSearchToolBusiness dynamoSearchToolBusiness)
        {
            _logger = logger;
            _dynamoSearchToolBusiness = dynamoSearchToolBusiness;
        }

        public IActionResult Index()
        {
            ForceSignOutModel searchModel = new ForceSignOutModel
            {
                Environments = GetEnvInfo(),
                Environment = "DevelopmentClient"
            };

            return View(searchModel);
        }

        [HttpPost]
        public async Task<IActionResult> IndexAsync(ForceSignOutModel searchInfo)
        {
            var transactionId = Guid.NewGuid().ToString();
            _logger.LogInformation("Processing DynamoSearch Request @{MileagePlusNumber} {@transactionId}", searchInfo.MileagePlusNumber, transactionId);

            searchInfo.Records = await _dynamoSearchToolBusiness.GetRecordsFromDynamo(searchInfo.MileagePlusNumber, searchInfo.Environment, transactionId).ConfigureAwait(false);
            searchInfo.Environments = GetEnvInfo();

            return View(searchInfo);
        }

        [HttpPost]
        public async Task<IActionResult> ForceSignOut(ForceSignOutModel searchInfo)
        {
            var transactionId = Guid.NewGuid().ToString();
            _logger.LogInformation("Processing ForceSignOut Request @{MileagePlusNumber} {@transactionId}", searchInfo.MileagePlusNumber, transactionId);

            var response = await _dynamoSearchToolBusiness.ForceSignOut(searchInfo.MileagePlusNumber, searchInfo.Environment, transactionId).ConfigureAwait(false);

            if (response)
            {
                _logger.LogInformation("ForceSignOut Successfull @{MileagePlusNumber} {@env} {@transactionId}", searchInfo.MileagePlusNumber, searchInfo.Environment, transactionId);
                TempData["SuccessMsg"] = $"{searchInfo.MileagePlusNumber} ForceSignOut Successfull.";
            }
            else
            {
                TempData["FailedMsg"] = $"{searchInfo.MileagePlusNumber} ForceSignOut Failed.";
            }

            return RedirectToAction("Index", "DynamoSearch");
        }

        public List<SelectListItem> GetEnvInfo() => new List<SelectListItem>
        {
            new SelectListItem {Text = "Dev", Value = "DevelopmentClient"},
            new SelectListItem {Text = "QA", Value = "QAClient"},
            new SelectListItem {Text = "Stage", Value = "StageClient"},
            new SelectListItem {Text = "Prod", Value = "ProdClient"}
        };
    }
}