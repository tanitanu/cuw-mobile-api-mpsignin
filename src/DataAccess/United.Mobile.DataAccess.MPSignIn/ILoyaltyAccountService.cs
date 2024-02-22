using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface ILoyaltyAccountService
    {
        Task<T> GetAccountProfileInfo<T>(string token, string mileagePlusNumber, string sessionId);
        Task<string> GetAccountProfile(string token, string mileagePlusNumber, string sessionId);
        Task<string> GetCurrentMembershipInfo(string mpNumber, string transactionId);
    }
}
