using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Common.Helper
{
    public interface ICatalog
    {
        Task<List<MOBItem>> GetCatalogItems(int applicationId, string deviceId = "");
        Task<bool> IsClientCatalogEnabled(int applicationId, string[] clientCatalogIds);
    }
}