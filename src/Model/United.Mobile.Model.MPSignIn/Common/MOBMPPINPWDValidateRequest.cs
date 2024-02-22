using Newtonsoft.Json;
using System;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBMPPINPWDValidateRequest : MOBRequest
    {
        [JsonIgnore()]
        public string ObjectName { get; set; } = "United.Definition.MPPINPWD.MOBMPPINPWDValidateRequest";
        private string mileagePlusNumber = string.Empty;
        private string passWord = string.Empty;
        private string sessionID = string.Empty;
        private string hashValue = string.Empty;

        public MOBMPPINPWDValidateRequest()
            : base()
        {
        }

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

        public string PassWord
        {
            get
            {
                return this.passWord;
            }
            set
            {
                this.passWord = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

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

        public MOBMPSignInPath MPSignInPath { get; set; }

        public bool SignInWithTouchID { get; set; }

        public string HashValue
        {
            get
            {
                return this.hashValue;
            }
            set
            {
                this.hashValue = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public bool RememberDevice { get; set; }
    }
}
