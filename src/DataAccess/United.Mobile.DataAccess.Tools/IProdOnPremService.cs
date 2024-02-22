using System.Threading.Tasks;

namespace United.Mobile.DataAccess.Tools
{
    public  interface IProdOnPremService
    {
        Task<T>GetCatalogServiceDetails<T>(string applicationId, string deviceId = "DeviceID", string SessionId = "SessionID", bool IsCloudCatalog = true);
        Task<bool> SQLForceSignOut(string mileagePlusNumber, string transactionId);
    }
}
