using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBMPAccountValidation
    {
        private string mileagePlusNumber = string.Empty;
        private string hashValue;
        private string message;
        private string authenticatedToken;

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

        [XmlIgnore]
        public long CustomerId { get; set; }

        public bool ValidPinCode { get; set; }

        public bool DeceasedAccount { get; set; }

        public bool ClosedAccount { get; set; }

        public bool NeedToAcceptTNC { get; set; }

        public bool AccountLocked { get; set; }

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

        public string Message
        {
            get
            {
                return this.message;
            }
            set
            {
                this.message = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string AuthenticatedToken
        {
            get
            {
                return this.authenticatedToken;
            }
            set
            {
                this.authenticatedToken = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsTokenValid { get; set; }

        public DateTime TokenExpireDateTime { get; set; }

        public Double TokenExpirationSeconds { get; set; }
    }
}
