using System;
using System.Collections.Generic;
using System.Text;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBMPSecurityUpdateRequest : MOBRequest
    {

        private string sessionID = string.Empty;
        private string mileagePlusNumber = string.Empty;
        private string hashValue = string.Empty;

        public MOBMPSecurityUpdateRequest()
            : base()
        {
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

        public string HashValue
        {
            get
            {
                return this.hashValue;
            }
            set
            {
                this.hashValue = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public MOBMPSecurityUpdatePath SecurityUpdateType { get; set; }

        public MOBMPPINPWDSecurityItems SecurityItemsToUpdate { get; set; }

        public bool RememberDevice { get; set; }
    }
}
