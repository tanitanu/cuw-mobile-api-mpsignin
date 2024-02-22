using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface IDataVaultService
    {
        Task<string> GetPersistentToken(string token, string requestData,string url, string sessionId);
        Task<string> PersistentToken(string token, string requestData, string url, string sessionId);
    }
}
