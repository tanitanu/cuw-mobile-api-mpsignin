using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface IRavenService
    {
        Task<string> SendRavenEmail(string token, string requestData, Dictionary<string, string> headers, string deviceId, string transactionId);
    }
}
