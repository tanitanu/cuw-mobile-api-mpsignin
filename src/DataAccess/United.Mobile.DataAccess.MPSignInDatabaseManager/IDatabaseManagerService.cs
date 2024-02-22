using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MPSignInDatabaseManager
{
    public interface IDatabaseManagerService
    {
        Task<string> GetRecordsFromDynamoDB(string mileagePlusNumber, string transactionId);
    }
}
