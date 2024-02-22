using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Tools;
using United.Mobile.MPSignInTool.Domain.Models.Catalog;
using United.Utility.Helper;

namespace United.Mobile.MPSignInTool.Domain.HealthCheck
{
    public class StageHealthCheckReport : IStageHealthCheckReport
    {
        private readonly ICacheLog<StageHealthCheckReport> _logger;
        private readonly IConfiguration _configuration;
        private readonly IStageService _stageService;
        private readonly IStageOnPremService _stageOnPremService;

        private List<CatalogItems> _catalogiOSItems;
        private List<CatalogItems> _catalogAndroidItems;

        public StageHealthCheckReport(
            ICacheLog<StageHealthCheckReport> logger,
            IConfiguration configuration,
            IStageService stageService,
            IStageOnPremService stageOnPremService
        )
        {
            _logger = logger;
            _configuration = configuration;
            _stageService = stageService;
            _stageOnPremService = stageOnPremService;

            _catalogiOSItems = new List<CatalogItems>();
            _catalogAndroidItems = new List<CatalogItems>();
        }

        public async Task SetCatalog()
        {
            var iosResult = await _stageOnPremService.GetCatalogServiceDetails<CatalogResponse>("1").ConfigureAwait(false);
            var androidResult = await _stageOnPremService.GetCatalogServiceDetails<CatalogResponse>("2").ConfigureAwait(false);

            _catalogiOSItems = iosResult.Items;
            _catalogAndroidItems = androidResult.Items;
        }

        public async Task<ServiceDetails> GetServiceDetails(ServiceDetails objServiceDetails)
        {
            var localObj = new ServiceDetails();
            localObj.ServiceName = objServiceDetails.ServiceName;
            var response = await _stageService.GetHealthCheck(objServiceDetails.ServicePath).ConfigureAwait(false);
            localObj.HealthCheck = string.IsNullOrEmpty(response) ? string.Empty : JsonConvert.DeserializeObject<string>(response);
            response = await _stageService.GetEnvironment(objServiceDetails.ServicePath).ConfigureAwait(false);
            localObj.Environment = string.IsNullOrEmpty(response) ? string.Empty : JsonConvert.DeserializeObject<string>(response);

            response = await _stageService.GetVersion(objServiceDetails.ServicePath).ConfigureAwait(false);
            localObj.Version = string.IsNullOrEmpty(response) ? string.Empty : JsonConvert.DeserializeObject<string>(response);
            localObj.IsCatalogEnabledAndroid = string.IsNullOrEmpty(objServiceDetails.CatalogIDAndroid) ? string.Empty : _catalogAndroidItems?.Find(x => (x.Id.Contains((objServiceDetails.CatalogIDAndroid))))?.CurrentValue;
            localObj.IsCatalogEnablediOS = string.IsNullOrEmpty(objServiceDetails.CatalogIDiOS) ? string.Empty : _catalogiOSItems?.Find(x => (x.Id.Contains((objServiceDetails.CatalogIDiOS))))?.CurrentValue;
            return localObj;
        }
    }
}