using System;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.Common
{
    public interface ICachingService
    {
        Task<string> GetCache<T>(string key, string transactionId);
        Task<bool> SaveCache<T>(string key, T data, string transactionId, TimeSpan expiry);
    }
}
