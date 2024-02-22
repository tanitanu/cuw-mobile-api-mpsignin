using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Mobile.MPAuthentication.Domain
{
    public interface IMPAuthenticationBusiness
    {
        Task<MOBMPPINPWDValidateResponse> ValidateMPSignInV2(MOBMPPINPWDValidateRequest request);
        Task<MOBTFAMPDeviceResponse> ValidateTFASecurityQuestionsV2(MOBTFAMPDeviceRequest request);
    }
}
