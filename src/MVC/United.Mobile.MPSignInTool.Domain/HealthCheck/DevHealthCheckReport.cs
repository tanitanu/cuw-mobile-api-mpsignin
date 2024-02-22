using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Tools;
using United.Utility.Helper;

namespace United.Mobile.MPSignInTool.Domain.HealthCheck
{
    public class DevHealthCheckReport : IDevHealthCheckReport
    {
        private readonly ICacheLog<DevHealthCheckReport> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDevelopmentService _devService;
        private readonly IDevelopmentOnPremService _devOnPremService;

        private List<CatalogItems> _catalogiOSItems;
        private List<CatalogItems> _catalogAndroidItems;

        public DevHealthCheckReport(
            ICacheLog<DevHealthCheckReport> logger,
            IConfiguration configuration,
            IDevelopmentService devService,
            IDevelopmentOnPremService devOnPremService
        )
        {
            _logger = logger;
            _configuration = configuration;
            _devService = devService;
            _devOnPremService = devOnPremService;

            _catalogiOSItems = new List<CatalogItems>();
            _catalogAndroidItems = new List<CatalogItems>();
        }

        public async Task SetCatalog()
        {
            _catalogiOSItems = await _devOnPremService.GetCatalogServiceDetails<List<CatalogItems>>("1").ConfigureAwait(false);
            _catalogAndroidItems = await _devOnPremService.GetCatalogServiceDetails<List<CatalogItems>>("2").ConfigureAwait(false);
        }

        public async Task<ServiceDetails> GetServiceDetails(ServiceDetails objServiceDetails)
        {
            var localObj = new ServiceDetails();
            localObj.ServiceName = objServiceDetails.ServiceName;
            var response = await _devService.GetHealthCheck(objServiceDetails.ServicePath).ConfigureAwait(false);
            localObj.HealthCheck = string.IsNullOrEmpty(response) ? string.Empty : JsonConvert.DeserializeObject<string>(response);
            response = await _devService.GetEnvironment(objServiceDetails.ServicePath).ConfigureAwait(false);
            localObj.Environment = string.IsNullOrEmpty(response) ? string.Empty : JsonConvert.DeserializeObject<string>(response);

            response = await _devService.GetVersion(objServiceDetails.ServicePath).ConfigureAwait(false);
            localObj.Version = string.IsNullOrEmpty(response) ? string.Empty : JsonConvert.DeserializeObject<string>(response);
            localObj.IsCatalogEnabledAndroid = string.IsNullOrEmpty(objServiceDetails.CatalogIDAndroid) ? string.Empty : _catalogAndroidItems?.Find(x => (x.Id.Contains((objServiceDetails.CatalogIDAndroid))))?.CurrentValue;
            localObj.IsCatalogEnablediOS = string.IsNullOrEmpty(objServiceDetails.CatalogIDiOS) ? string.Empty : _catalogiOSItems?.Find(x => (x.Id.Contains((objServiceDetails.CatalogIDiOS))))?.CurrentValue;
           
            return localObj;
        }
    }
}