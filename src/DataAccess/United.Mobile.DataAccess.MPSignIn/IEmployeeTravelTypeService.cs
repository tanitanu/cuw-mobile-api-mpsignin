using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface IEmployeeTravelTypeService
    {
        Task<string> GetTravelType(string token,  string requestPayload, string sessionId, int ApplicationId, string AppVersion, string DeviceId, string TransactionId);
    }
}
