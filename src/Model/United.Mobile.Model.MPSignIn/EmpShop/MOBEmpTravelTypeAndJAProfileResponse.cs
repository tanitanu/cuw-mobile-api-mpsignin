using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBEmpTravelTypeAndJAProfileResponse : MOBResponse
    {
        public MOBEmpJAResponse MOBEmpJAResponse { get; set; }
        public MOBEmpTravelTypeResponse MOBEmpTravelTypeResponse { get; set; }
    }
}
