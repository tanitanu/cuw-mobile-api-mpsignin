using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface IUnitedClubMembershipService
    {
        Task<string> GetCurrentMembershipInfoV2(string mPNumber, string Token);
    }
}
