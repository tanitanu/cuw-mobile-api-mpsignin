using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MPSignIn.MPNumberToPNR
{
    [Serializable]
    public class MOBAddMpToPnrEligibilityResponse : MOBResponse
    {
        public string SessionId { get; set; }
        public string AddMpToPnrHeader { get; set; }
        public string AddMpToPnrLinkText { get; set; }
        public string AddMpToPnrDescription { get; set; }
        public bool ShowAddMpToPnr { get; set; }
    }
}
