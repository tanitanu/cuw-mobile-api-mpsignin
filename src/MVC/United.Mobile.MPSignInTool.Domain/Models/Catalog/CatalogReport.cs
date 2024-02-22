namespace United.Mobile.MPSignInTool.Domain.Models.Catalog
{
    public class CatalogReport
    {
        public ServiceCatalogInfo DevReport { get; set; }
        public ServiceCatalogInfo QAReport { get; set; }
        public ServiceCatalogInfo StageReport { get; set; }
        public ServiceCatalogInfo ProdReport { get; set; }
    }
}