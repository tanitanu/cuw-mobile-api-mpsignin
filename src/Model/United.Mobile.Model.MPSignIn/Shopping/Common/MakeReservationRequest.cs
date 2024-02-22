using System;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping
{
    [Serializable()]
    public class MakeReservationRequest : MOBRequest
    {
        private string sessionId = string.Empty;
        private string emailAddress = string.Empty;
        //private MOBSHOPPhone phone;
        private string mileagePlusNumber = string.Empty;
        private string additionalData = string.Empty;
        private string token = string.Empty;

        public string AdditionalData
        {
            get
            {
                return this.additionalData;
            }
            set
            {
                this.additionalData = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool RemoveFareLock { get; set; }

        public bool FareLockAutoTicket { get; set; }

        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBSHOPFormOfPayment FormOfPayment { get; set; }

        public double PaymentAmount { get; set; }

        public double OTPAmount { get; set; }

        public MOBAddress Address { get; set; }

        public string EmailAddress
        {
            get
            {
                return this.emailAddress;
            }
            set
            {
                this.emailAddress = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBCPPhone Phone { get; set; }

        //public MOBSHOPPhone Phone
        //{
        //    get
        //    {
        //        return this.phone;
        //    }
        //    set
        //    {
        //        this.phone = value;
        //    }
        //}

        public string Token
        {
            get
            {
                return token;
            }
            set
            {
                this.token = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsTPI { get; set; } = false;

        public bool IsSecondaryPayment { get; set; } = false;
    }
}
