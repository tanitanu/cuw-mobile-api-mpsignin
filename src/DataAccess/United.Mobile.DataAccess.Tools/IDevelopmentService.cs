using System.Threading.Tasks;

namespace United.Mobile.DataAccess.Tools
{
    public interface IDevelopmentService
    {
        Task<string> GetHealthCheck(string serviceName);
        Task<string> GetVersion(string serviceName);
        Task<string> GetEnvironment(string serviceName);
        Task<string> GetAllValidateMpAppidDeviceIdByMP(string tableName, string secondaryKey, string transactionId);
        Task<bool> DynamoForceSignOut(string tableName, string secondaryKey, string transactionId);
    }
}