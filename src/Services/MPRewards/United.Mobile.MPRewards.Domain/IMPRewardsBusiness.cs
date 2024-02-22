using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Mobile.Model.MPRewards;
using United.Mobile.Model.PNRManagement;

namespace United.Mobile.MPRewards.Domain
{
    public interface IMPRewardsBusiness
    {
        Task<MOBCancelFFCPNRsByMPNumberResponse> GetCancelFFCPnrsByMPNumber(MOBCancelFFCPNRsByMPNumberRequest request);
        Task<MOBMPPlusPointsResponse> GetAccountPlusPointsDetails(MOBMPPlusPointsRequest request);
    }
}
