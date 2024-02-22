using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Mobile.MemberSignIn.Domain
{
    public interface IMemberSignInBusiness
    {
        Task<MOBJoinMileagePlusEnrollmentResponse> OneClickEnrollment(MOBJoinMileagePlusEnrollmentRequest request);
        Task<MOBMPSignInNeedHelpResponse> MPSignInNeedHelp(MOBMPSignInNeedHelpRequest request);
        Task<MOBTFAMPDeviceResponse> SendResetAccountEmail(MOBTFAMPDeviceRequest request);
    }
}
