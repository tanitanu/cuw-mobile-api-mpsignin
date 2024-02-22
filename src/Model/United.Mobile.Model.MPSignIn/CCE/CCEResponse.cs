using System;
using System.Collections.Generic;
using System.Text;
using United.Mobile.Model.Internal.Exception;

namespace United.Mobile.Model.MPSignIn.CCE
{
    public class CCEResponse
    {
        public CCERequest CCERequest { get; set; }
        public bool IsAccountExist { get; set; }
        public MOBException Exception { get; set; }
        public long CallDuration { get; set; }
    }
}
