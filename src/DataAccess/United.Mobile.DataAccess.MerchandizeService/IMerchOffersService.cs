using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MerchandizeService
{
    public interface IMerchOffersService
    {
        Task<T> GetSubscriptions<T>(string subscriptionRequest, string token, string sessionId);
    }
}
