using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.Common
{
   public interface IDynamoDBService
    {
        Task<T> GetRecords<T>(string TableName, string transactionId, string key,string sessionId);
        Task<bool> SaveRecords<T>(string tableName, string transactionId, string key, T data, string sessionId);
        Task<bool> SaveRecords<T>(string tableName, string transactionId, string key, string secondaryKey, T data, string sessionId, double absoluteExpirationDays = 2);
    }
}
