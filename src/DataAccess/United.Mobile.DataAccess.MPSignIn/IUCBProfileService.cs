using System.Threading.Tasks;

namespace United.Mobile.DataAccess.MPSignIn
{
    public interface IUCBProfileService
    {
        Task<string> GetAllTravellers(string token, string mileagePlusNumber, string sessionId);
        Task<string> GetProfileOwner(string token, string mileagePlusNumber, string sessionId);
        Task<string> GetCreditCardsData(string token, string mileagePlusNumber, string sessionId);
        Task<string> GetLoginSecurityUpdate(string token, string mileagePlusNumber, string sessionId);
        Task<string> GetEmail(string token, string mpNumber);

    }
}
