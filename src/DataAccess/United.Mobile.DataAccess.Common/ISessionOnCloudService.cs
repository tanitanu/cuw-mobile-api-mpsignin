using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.Model;

namespace United.Mobile.DataAccess.Common
{
    public interface ISessionOnCloudService
    {

        Task<string> GetSession(string sessionID, string objectName, List<string> vParams = null, string transactionId = "Test", bool isReadOnPrem = false);

        //Save Method
        Task<bool> SaveSessionONCloud<T>(T data, string sessionID, List<string> validateParams, string objectName, TimeSpan expiry, string transactionId = "Test");
        Task SaveSessionOnPrem(string data, string sessionID, List<string> validateParams, string objectName, TimeSpan expiry, string transactionId = "Test");
    }
}
