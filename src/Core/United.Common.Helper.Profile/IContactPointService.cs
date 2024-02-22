using System.Threading.Tasks;

namespace United.Common.Helper.Profile
{
    public interface IContactPointService
    {
        Task<string> GetPrimaryEmail(string token, string mpNumber, string transactionId);
    }
}