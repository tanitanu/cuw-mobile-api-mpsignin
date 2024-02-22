using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface ICustomerPreferencesService
    {
        Task<T> GetCustomerPreferences<T>(string token, string mpNumber,string sessionId);
    }
}
