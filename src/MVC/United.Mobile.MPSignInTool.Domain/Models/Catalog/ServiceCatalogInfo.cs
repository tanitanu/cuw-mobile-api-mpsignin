using System.Collections.Generic;

namespace United.Mobile.MPSignInTool.Domain.Models.Catalog
{
    public class ServiceCatalogInfo
    {
        public string Env { get; set; }
        public IList<CatalogInfo> IosData { get; set; }
        public IList<CatalogInfo> AndroidData { get; set; }
    }
}