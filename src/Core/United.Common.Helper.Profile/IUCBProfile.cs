using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Common;

namespace United.Common.Helper.Profile
{
    public  interface IUCBProfile
    {
        Task<List<MOBCPProfile>> GetProfileV2(MOBCPProfileRequest request, bool getMPSecurityDetails = false);
        Task<TravelPolicy> GetCorporateTravelPolicy(MOBMPPINPWDValidateRequest request, Session session, MOBCPCorporate corporateData);
        bool IsEnableSuppressingCompanyNameForBusiness(int applicationId, string appVersion);
    }
}
