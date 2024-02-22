using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MPSignIn
{
    public interface ICustomerProfileService
    {
        Task<string> Search(string token, string sessionId, string path);
        Task<string> SearchMPNumber(string path, string token, string sessionId);
    }
}
