using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using United.Mobile.MPSignInTool.Domain.HealthCheck;
using United.Utility.Helper;

namespace United.Mobile.MPSignInTool.Domain
{
    public class MPSignInToolBusiness : IMPSignInToolBusiness
    {
        private readonly ICacheLog<MPSignInToolBusiness> _logger;
        private readonly IConfiguration _configuration;
        private readonly IQAHealthCheckReport _qaHealthCheckReport;
        private readonly IDevHealthCheckReport _devHealthCheckReport;
        private readonly IStageHealthCheckReport _stageHealthCheckReport;
        private readonly IProdHealthCheckReport _prodHealthCheckReport;
        private readonly List<ServiceDetails> _ListOfServiceObj;

        public MPSignInToolBusiness(
            ICacheLog<MPSignInToolBusiness> logger,
            IConfiguration configuration,
            IQAHealthCheckReport qAHealthCheckReport,
            IDevHealthCheckReport devHealthCheckReport,
            IStageHealthCheckReport stageHealthCheckReport,
            IProdHealthCheckReport prodHealthCheckReport
            )
        {
            _logger = logger;
            _configuration = configuration;
            _qaHealthCheckReport = qAHealthCheckReport;
            _devHealthCheckReport = devHealthCheckReport;
            _stageHealthCheckReport = stageHealthCheckReport;
            _prodHealthCheckReport = prodHealthCheckReport;
            _ListOfServiceObj = GetAllServices();
        }

        private List<ServiceDetails> GetAllServices()
        {
            var listOfServices = new List<ServiceDetails>();
            var configServices = _configuration.GetSection("services").GetChildren();

            foreach (var item in configServices.ToList())
            {
                listOfServices.Add(item.Get<ServiceDetails>());
            }
            return listOfServices;
        }

        public async Task<HealthCheckReport> GetAllServiceDetails()
        {
            var localServiceEnvWiseObject = new HealthCheckReport();

            localServiceEnvWiseObject.DevReport = await GetDevelopmentServiceDetails().ConfigureAwait(false);
            localServiceEnvWiseObject.QAReport = await GetQAServiceDetails().ConfigureAwait(false);
            localServiceEnvWiseObject.StageReport = await GetStageServiceDetails().ConfigureAwait(false);
            localServiceEnvWiseObject.ProdReport = await GetProdServiceDetails().ConfigureAwait(false);

            return localServiceEnvWiseObject;
        }

        public async Task<List<ServiceDetails>> GetQAServiceDetails()
        {
            var obj = new List<ServiceDetails>();
            await _qaHealthCheckReport.SetCatalog().ConfigureAwait(false);

            foreach (var objServiceDetails in _ListOfServiceObj)
            {
                var localObj = await _qaHealthCheckReport.GetServiceDetails(objServiceDetails).ConfigureAwait(false);
                obj.Add(localObj);
            }

            return obj;
        }

        public async Task<List<ServiceDetails>> GetDevelopmentServiceDetails()
        {
            var obj = new List<ServiceDetails>();
            await _devHealthCheckReport.SetCatalog().ConfigureAwait(false);

            foreach (var objServiceDetails in _ListOfServiceObj)
            {
                var localObj = await _devHealthCheckReport.GetServiceDetails(objServiceDetails).ConfigureAwait(false);
                obj.Add(localObj);
            }

            return obj;
        }

        public async Task<List<ServiceDetails>> GetStageServiceDetails()
        {
            var obj = new List<ServiceDetails>();
            await _stageHealthCheckReport.SetCatalog().ConfigureAwait(false);

            foreach (var objServiceDetails in _ListOfServiceObj)
            {
                var localObj = await _stageHealthCheckReport.GetServiceDetails(objServiceDetails).ConfigureAwait(false);
                obj.Add(localObj);
            }

            return obj;
        }

        public async Task<List<ServiceDetails>> GetProdServiceDetails()
        {
            var obj = new List<ServiceDetails>();
            await _prodHealthCheckReport.SetCatalog().ConfigureAwait(false);

            foreach (var objServiceDetails in _ListOfServiceObj)
            {
                var localObj = await _prodHealthCheckReport.GetServiceDetails(objServiceDetails).ConfigureAwait(false);
                obj.Add(localObj);
            }

            return obj;
        }

    }
}
