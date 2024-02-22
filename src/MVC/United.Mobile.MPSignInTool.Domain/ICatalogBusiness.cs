using System.Threading.Tasks;
using United.Mobile.MPSignInTool.Domain.Models.Catalog;

namespace United.Mobile.MPSignInTool.Domain
{
    public interface ICatalogBusiness
    {
        Task<CatalogReport> GetCatalogItems();
    }
}