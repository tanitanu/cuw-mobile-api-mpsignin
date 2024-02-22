using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Mobile.Model.MPSignIn;

namespace United.Mobile.MPAccountProfile.Domain
{
    public interface IMPAccountProfileBusiness
    {
        Task<MOBContactUsResponse> GetContactUsDetails(MOBContactUsRequest request);
        Task<MOBCustomerPreferencesResponse> RetrieveCustomerPreferences(MOBCustomerPreferencesRequest request);
        Task<MOBMPAccountSummaryResponse> GetAccountSummaryWithMemberCardPremierActivity(MOBMPAccountValidationRequest request);
    }
}
