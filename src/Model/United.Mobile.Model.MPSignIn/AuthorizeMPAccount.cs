using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MPSignIn
{
    public class AuthorizeMPAccount
    {
        public string DeviceId { get; set; }
        public string HashValue { get; set; }
        public string MpNumber { get; set; }
        public int ApplicationId { get; set; }
        public string AppVersion { get; set; }
        public string TransactionId { get; set; }
    }
}
