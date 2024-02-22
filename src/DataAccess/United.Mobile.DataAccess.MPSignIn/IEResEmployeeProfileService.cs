using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface IEResEmployeeProfileService
    {
        Task<string> GetEResEmployeeProfile(string token, string requestPayload, string sessionId);
    }
}
