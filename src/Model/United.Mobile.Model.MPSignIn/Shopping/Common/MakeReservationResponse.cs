using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using United.Mobile.Model.BagCalculator;
using United.Mobile.Model.Common;
using United.Mobile.Model.MPSignIn;

namespace United.Mobile.Model.Shopping
{
    [Serializable()]
    public class MakeReservationResponse : MOBResponse
    {
        private string sessionID = string.Empty;
        private string shareFlightTitle = string.Empty;
        private string shareFlightMessage = string.Empty;
        private string warning = string.Empty;
        private string fqtvNameMismatchMessage = string.Empty;
        private string otpMessage = string.Empty;
        private string confirmationMsgForTPI;

        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBSHOPReservation Reservation { get; set; }

        public List<string> DOTBagRules { get; set; }

        public MOBDOTBaggageInfo DOTBaggageInfo { get; set; }

        public string ShareFlightTitle
        {
            get
            {
                return this.shareFlightTitle;
            }
            set
            {
                this.shareFlightTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ShareFlightMessage
        {
            get
            {
                return this.shareFlightMessage;
            }
            set
            {
                this.shareFlightMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Warning
        {
            get
            {
                return this.warning;
            }
            set
            {
                this.warning = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FQTVNameMismatchMessage
        {
            get
            {
                return this.fqtvNameMismatchMessage;
            }
            set
            {
                this.fqtvNameMismatchMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<string> Disclaimer { get; set; }

        public MOBSHOPFormOfPayment FormOfPayment { get; set; }

        public List<MOBClubDayPass> Passes { get; set; }

        public ClubPassPurchaseRequest ClubPassPurchaseRequest { get; set; }

        public string OTPMessage
        {
            get
            {
                return this.otpMessage;
            }
            set
            {
                this.otpMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBSHOPResponseStatusItem ResponseStatusItem { get; set; }

        public MOBCreditCard FormOfPaymentForTPI { get; set; }

        public string ConfirmationMsgForTPI
        {
            get
            {
                return this.confirmationMsgForTPI;
            }
            set
            {
                this.confirmationMsgForTPI = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SpecialNeedsMessage { get; set; }
    }

}
