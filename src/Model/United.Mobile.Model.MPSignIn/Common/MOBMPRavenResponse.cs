using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MPSignIn.Common
{
    [Serializable()]
    public class MOBMPRavenResponse
    {
        public string status { get; set; }
        public string uuid { get; set; }
        public string exception { get; set; }
    }
}
