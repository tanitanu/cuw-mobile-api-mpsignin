using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Tools;
using United.Mobile.MPSignInTool.Domain.Models.Catalog;

namespace United.Mobile.MPSignInTool.Domain
{
    public class CatalogBusiness : ICatalogBusiness
    {
        private readonly ILogger<CatalogBusiness> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDevelopmentOnPremService _devOnPremService;
        private readonly IQAOnPremService _qaOnPremService;
        private readonly IStageOnPremService _stageOnPremService;
        private readonly IProdOnPremService _prodOnPremService;

        public CatalogBusiness(
            ILogger<CatalogBusiness> logger,
            IConfiguration configuration,
            IDevelopmentOnPremService devOnPremService,
            IQAOnPremService qaOnPremService,
            IStageOnPremService stageOnPremService,
            IProdOnPremService prodOnPremService)
        {
            _logger = logger;
            _configuration = configuration;
            _devOnPremService = devOnPremService;
            _qaOnPremService = qaOnPremService;
            _stageOnPremService = stageOnPremService;
            _prodOnPremService = prodOnPremService;
        }

        private List<ServiceInfo> GetServiceDetails()
        {
            var listOfServices = new List<ServiceInfo>();
            var configServices = _configuration.GetSection("services").GetChildren();
            foreach (var item in configServices.ToList())
            {
                listOfServices.Add(item.Get<ServiceInfo>());
            }
            return listOfServices;
        }

        private IList<CatalogInfo> ProcessiOSCatalogItems(List<ServiceInfo> services, List<CatalogItems> catalogItems)
        {
            List<CatalogInfo> result = new List<CatalogInfo>();

            services?.ForEach(service =>
            {
                var record = new CatalogInfo();
                record.ServiceName = service?.ServiceName;
                record.CatalogID = service?.CatalogIDiOS;
                record.CatalogURLID = service?.CatalogURLiOS;

                catalogItems?.ForEach(item =>
                {
                    if (item.Id == service.CatalogIDiOS)
                        record.Status = item?.CurrentValue;
                    else if (item.Id == service.CatalogURLiOS)
                        record.URL = item?.CurrentValue;
                });
                result.Add(record);
            });

            return result;
        }

        private IList<CatalogInfo> ProcessAndroidCatalogItems(List<ServiceInfo> services, List<CatalogItems> catalogItems)
        {
            List<CatalogInfo> result = new List<CatalogInfo>();

            services?.ForEach(service =>
            {
                var record = new CatalogInfo();
                record.ServiceName = service?.ServiceName;
                record.CatalogID = service?.CatalogIDAndroid;
                record.CatalogURLID = service?.CatalogURLAndroid;

                catalogItems?.ForEach(item =>
                {
                    if (item.Id == service.CatalogIDAndroid)
                        record.Status = item?.CurrentValue;
                    else if (item.Id == service.CatalogURLAndroid)
                        record.URL = item?.CurrentValue;
                });
                result.Add(record);
            });

            return result;
        }

        private async Task<ServiceCatalogInfo> GetCatalogItemsDev(List<ServiceInfo> services)
        {
            ServiceCatalogInfo devReport = new ServiceCatalogInfo();
            devReport.Env = "Development";

            try
            {
                var iOScatalogItems = await _devOnPremService.GetCatalogServiceDetails<List<CatalogItems>>("1").ConfigureAwait(false);
                var iOSRecords = ProcessiOSCatalogItems(services, iOScatalogItems);

                var androidcatalogItems = await _devOnPremService.GetCatalogServiceDetails<List<CatalogItems>>("2").ConfigureAwait(false);
                var androidRecords = ProcessAndroidCatalogItems(services, androidcatalogItems);

                devReport.AndroidData = androidRecords;
                devReport.IosData = iOSRecords;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error GetCatalogItemsDev {msg}", ex.Message);
            }

            return devReport;
        }

        private async Task<ServiceCatalogInfo> GetCatalogItemsQA(List<ServiceInfo> services)
        {
            ServiceCatalogInfo qaReport = new ServiceCatalogInfo();
            qaReport.Env = "QA";
            try
            {
                var iOScatalogItems = await _qaOnPremService.GetCatalogServiceDetails<List<CatalogItems>>("1").ConfigureAwait(false);
                var iOSRecords = ProcessiOSCatalogItems(services, iOScatalogItems);

                var androidcatalogItems = await _qaOnPremService.GetCatalogServiceDetails<List<CatalogItems>>("2").ConfigureAwait(false);
                var androidRecords = ProcessAndroidCatalogItems(services, androidcatalogItems);

                qaReport.AndroidData = androidRecords;
                qaReport.IosData = iOSRecords;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error GetCatalogItemsQA {msg}", ex.Message);
            }

            return qaReport;
        }

        private async Task<ServiceCatalogInfo> GetCatalogItemsStage(List<ServiceInfo> services)
        {
            ServiceCatalogInfo stageReport = new ServiceCatalogInfo();
            stageReport.Env = "Stage";
            try
            {
                var iOScatalogItems = await _stageOnPremService.GetCatalogServiceDetails<CatalogResponse>("1").ConfigureAwait(false);
                var iOSRecords = ProcessiOSCatalogItems(services, iOScatalogItems?.Items);

                var androidcatalogItems = await _stageOnPremService.GetCatalogServiceDetails<CatalogResponse>("2").ConfigureAwait(false);
                var androidRecords = ProcessAndroidCatalogItems(services, androidcatalogItems?.Items);

                stageReport.AndroidData = androidRecords;
                stageReport.IosData = iOSRecords;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error GetCatalogItemsStage {msg}", ex.Message);
            }

            return stageReport;
        }

        private async Task<ServiceCatalogInfo> GetCatalogItemsProd(List<ServiceInfo> services)
        {
            ServiceCatalogInfo prodReport = new ServiceCatalogInfo();
            prodReport.Env = "Production";
            try
            {
                var iOScatalogItems = await _prodOnPremService.GetCatalogServiceDetails<CatalogResponse>("1").ConfigureAwait(false);
                var iOSRecords = ProcessiOSCatalogItems(services, iOScatalogItems?.Items);

                var androidcatalogItems = await _prodOnPremService.GetCatalogServiceDetails<CatalogResponse>("2").ConfigureAwait(false);
                var androidRecords = ProcessAndroidCatalogItems(services, androidcatalogItems?.Items);

                prodReport.AndroidData = androidRecords;
                prodReport.IosData = iOSRecords;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error GetCatalogItemsProd {msg}", ex.Message);
            }

            return prodReport;
        }

        public async Task<CatalogReport> GetCatalogItems()
        {
            _logger.LogInformation("GetCatalogItems Triggered.");
            CatalogReport report = new CatalogReport();
            Task aggregationTask = null;
            try
            {
                var services = GetServiceDetails();

                var task1 = GetCatalogItemsDev(services);
                var task2 = GetCatalogItemsQA(services);
                var task3 = GetCatalogItemsStage(services);
                var task4 = GetCatalogItemsProd(services);
                try
                {
                    aggregationTask = Task.WhenAll(task1, task2, task3, task4);
                    await aggregationTask;

                }
                catch (Exception ex)
                {
                    if (aggregationTask?.Exception?.InnerExceptions != null && aggregationTask.Exception.InnerExceptions.Any())
                    {
                        foreach (var innerEx in aggregationTask.Exception.InnerExceptions)
                        {
                            _logger.LogError("Error GetCatalogItems {msg}", innerEx.Message);
                        }
                    }
                }

                report.DevReport = await task1;
                report.QAReport = await task2;
                report.StageReport = await task3;
                report.ProdReport = await task4;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error GetCatalogItems {msg}", ex.Message);
            }

            return report;
        }
    }
}