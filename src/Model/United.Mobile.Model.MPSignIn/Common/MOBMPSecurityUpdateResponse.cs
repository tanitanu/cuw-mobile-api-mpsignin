using Microsoft.Extensions.Configuration;
using System;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBMPSecurityUpdateResponse : MOBResponse
    {
        private readonly IConfiguration _configuration;
        public MOBMPSecurityUpdateResponse()
            : base()
        {
        }
        private string mileagePlusNumber = string.Empty;
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public int CustomerID { get; set; }

        public MOBMPSecurityUpdateRequest Request { get; set; }

        public bool SecurityUpdate { get; set; }

        public MOBMPPINPWDSecurityUpdateDetails MPSecurityUpdateDetails { get; set; }

        private string sessionID = string.Empty;
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private string securityUpdateCompleteMessage;
        public string SecurityUpdateCompleteMessage
        {
            get
            {
                return this.securityUpdateCompleteMessage;
            }
            set
            {
                this.securityUpdateCompleteMessage = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public string HashValue { get; set; } = string.Empty;

        private MOBMPTFARememberMeFlags rememberMEFlags;
        public MOBMPTFARememberMeFlags RememberMEFlags
        {
            get
            {
                if (rememberMEFlags == null)
                {
                    rememberMEFlags = new MOBMPTFARememberMeFlags();
                }
                return this.rememberMEFlags;
            }
            set
            {
                this.rememberMEFlags = value;
            }
        }
        public bool ShowContinueAsGuestButton { get; set; }
    }
}
