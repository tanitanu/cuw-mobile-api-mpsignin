using System.Collections.Generic;
using United.Mobile.Model.CSLModels;

namespace United.Definition.CSLModels.CustomerProfile
{
    public class SecurityUpdateData : Base
    {
        public SecurityUpdate SecurityUpdatedetails { get; set; }
    }

    public class SecurityUpdate
    {
        #region Properties
        public List<string> AccountItemsToUpdate { get; set; }
        public bool UpdateLaterAllowed { get; set; }
        public int DaysToCompleteSecurityUpdate { get; set; }
        public bool PasswordOnlyAllowed { get; set; }
        #endregion
    }
}


