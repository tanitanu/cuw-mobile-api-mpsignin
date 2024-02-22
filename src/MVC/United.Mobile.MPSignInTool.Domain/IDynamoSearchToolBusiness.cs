using System.Collections.Generic;
using System.Threading.Tasks;

namespace United.Mobile.MPSignInTool.Domain
{
    public interface IDynamoSearchToolBusiness
    {
        Task<List<MPSignInDynamoRecords>> GetRecordsFromDynamo(string secondaryKey, string env, string transactionId);
        Task<bool> ForceSignOut(string secondaryKey, string env, string transactionId);
    }
}