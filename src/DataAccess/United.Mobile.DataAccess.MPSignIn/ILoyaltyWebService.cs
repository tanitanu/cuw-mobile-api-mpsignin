using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface ILoyaltyWebService
    {
        Task<string> GetLoyaltyData(string token, string mileagePlusNumber, string sessionId);
    }
}
