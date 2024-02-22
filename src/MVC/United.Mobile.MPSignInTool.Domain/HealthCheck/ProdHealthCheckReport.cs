using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Tools;
using United.Mobile.MPSignInTool.Domain.Models.Catalog;
using United.Utility.Helper;

namespace United.Mobile.MPSignInTool.Domain.HealthCheck
{
    public class ProdHealthCheckReport : IProdHealthCheckReport
    {
        private readonly ICacheLog<ProdHealthCheckReport> _logger;
        private readonly IConfiguration _configuration;
        private readonly IProdService _prodService;
        private readonly IProdOnPremService _prodOnPremService;

        private List<CatalogItems> _catalogiOSItems;
        private List<CatalogItems> _catalogAndroidItems;

        public ProdHealthCheckReport(
            ICacheLog<ProdHealthCheckReport> logger,
            IConfiguration configuration,
            IProdService prodService,
            IProdOnPremService prodOnPremService
        )
        {
            _logger = logger;
            _configuration = configuration;
            _prodService = prodService;
            _prodOnPremService = prodOnPremService;

            _catalogiOSItems = new List<CatalogItems>();
            _catalogAndroidItems = new List<CatalogItems>();
        }

        public async Task SetCatalog()
        {
            var iosResult = await _prodOnPremService.GetCatalogServiceDetails<CatalogResponse>("1").ConfigureAwait(false);
            var androidResult = await _prodOnPremService.GetCatalogServiceDetails<CatalogResponse>("2").ConfigureAwait(false);

            _catalogiOSItems = iosResult.Items;
            _catalogAndroidItems = androidResult.Items;
        }

        public async Task<ServiceDetails> GetServiceDetails(ServiceDetails objServiceDetails)
        {
            var localObj = new ServiceDetails();
            localObj.ServiceName = objServiceDetails.ServiceName;
            var response = await _prodService.GetHealthCheck(objServiceDetails.ServicePath).ConfigureAwait(false);
            localObj.HealthCheck = string.IsNullOrEmpty(response) ? string.Empty : JsonConvert.DeserializeObject<string>(response);
            response = await _prodService.GetEnvironment(objServiceDetails.ServicePath).ConfigureAwait(false);
            localObj.Environment = string.IsNullOrEmpty(response) ? string.Empty : JsonConvert.DeserializeObject<string>(response);

            response = await _prodService.GetVersion(objServiceDetails.ServicePath).ConfigureAwait(false);
            localObj.Version = string.IsNullOrEmpty(response) ? string.Empty : JsonConvert.DeserializeObject<string>(response);
            localObj.IsCatalogEnabledAndroid = string.IsNullOrEmpty(objServiceDetails.CatalogIDAndroid) ? string.Empty : _catalogAndroidItems?.Find(x => (x.Id.Contains((objServiceDetails.CatalogIDAndroid))))?.CurrentValue;
            localObj.IsCatalogEnablediOS = string.IsNullOrEmpty(objServiceDetails.CatalogIDiOS) ? string.Empty : _catalogiOSItems?.Find(x => (x.Id.Contains((objServiceDetails.CatalogIDiOS))))?.CurrentValue;
            return localObj;
        }
    }
}