using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBMPSecurityUpdate
    {
        public MOBMPSecurityUpdate()
            : base()
        {
        }

        public List<MOBMPSecurityUpdatePath> MPSecurityPathList { get; set; }

        public int DaysToCompleteSecurityUpdate { get; set; }

        public bool PasswordOnlyAllowed { get; set; }

        public bool UpdateLaterAllowed { get; set; }

    }
}
