using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.MPSignIn.MPNumberToPNR;

namespace United.Mobile.MemberSignIn.Domain
{
    public interface IMPNumberToPnrBussiness
    {
        Task<MPSearchResponse> SearchMPNumber(MPSearchRequest request);
        Task<MOBAddMpToPnrEligibilityResponse> AddMpToPnrEligibilityCheck(MPSearchRequest request);
        Task<AddMPNumberToPnrResponse> AddMPNumberToPnr(AddMPNumberToPnrRequest request);
    }
}
