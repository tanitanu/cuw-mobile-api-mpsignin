using System;
using System.Collections.Generic;
using System.Text;
using United.Mobile.Model.MPSignIn;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBEmpJAResponse : MOBResponse
    {

        public bool AllowImpersonation { get; set; }


        public MOBEmpJA EmpJA { get; set; }
        public MOBEmpJARequest EmpJARequest { get; set; }
        public string ImpersonateType { get; set; }
        public MOBEmpJA LoggedInJA { get; set; }
        public MOBEmpPassRiderExtended LoggedInPassRider { get; set; }
        public MOBEmployeeProfileExtended EmpProfileExtended { get; set; }
    }
}
