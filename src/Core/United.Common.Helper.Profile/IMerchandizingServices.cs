using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Common.Helper.Profile
{
    public interface IMerchandizingServices
    {
        void SetMerchandizeChannelValues(string merchChannel, ref string channelId, ref string channelName);
        Task<MOBUASubscriptions> GetUASubscriptions(string mpAccountNumber, int applicationID, string sessionID, string channelId, string channelName, string token);
        Task<(MOBClubMembership clubMemberShip, Service.Presentation.ProductResponseModel.Subscription merchOut)> GetUAClubSubscriptions(string mpAccountNumber, string sessionID, string channelId, string channelName, string token);
        MOBUASubscriptions GetUASubscriptions(string mpAccountNumber, Service.Presentation.ProductResponseModel.Subscription merchOut);        
    }
}
