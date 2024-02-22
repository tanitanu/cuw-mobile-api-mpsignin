using System.Collections.Generic;

namespace United.Mobile.MPSignInTool.Domain
{
    public class ServiceDetails
    {
        public string ServicePath { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string HealthCheck { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public string IsCatalogEnablediOS { get; set; } = string.Empty;
        public string IsCatalogEnabledAndroid { get; set; } = string.Empty;
        public string CatalogIDAndroid { get; set; } = string.Empty;
        public string CatalogIDiOS { get; set; } = string.Empty;
    }

    public class HealthCheckReport
    {
        public List<ServiceDetails> DevReport { get; set; }
        public List<ServiceDetails> QAReport { get; set; }
        public List<ServiceDetails> StageReport { get; set; }
        public List<ServiceDetails> ProdReport { get; set; }
    }

    public class CatalogItems
    {
        public string Id { get; set; }
        public string CurrentValue { get; set; }
        public string SaveToPersist { get; set; }
    }
}