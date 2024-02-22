using Newtonsoft.Json;
using System;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping
{
    [Serializable()]
    public class Reshop
    {
        private string recordLocator;
        private string lastName;
        private string fsrTitle;
        private string fsrChangeFeeTxt;

        public bool IsResidualFFCRAvailable { get; set; }
        public MOBFutureFlightCredit FFCMessage { get; set; }
        public string RTIChangeCancelTxt { get; set; }


        public string FsrSubHeading { get; set; }

        public string RecordLocator
        {
            get { return this.recordLocator; }
            set { this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string FlowType { get; set; }

        public string CheckinSessionKey { get; set; }

        public string ReviewChangeBackBtnText { get; set; }

        public string AncillaryRefundFormOfPayment { get; set; }

        public string LastName
        {
            get { return this.lastName; }
            set { this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string FsrTitle
        {
            get { return this.fsrTitle; }
            set { this.fsrTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string FsrChangeFeeTxt
        {
            get
            {
                return this.fsrChangeFeeTxt;
            }
            set
            {
                this.fsrChangeFeeTxt = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        private string refundFormOfPaymentMessage;
        public string RefundFormOfPaymentMessage
        {
            get
            {
                return this.refundFormOfPaymentMessage;
            }
            set
            {
                this.refundFormOfPaymentMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        private string changeTripTitle;
        public string ChangeTripTitle
        {
            get
            {
                return this.changeTripTitle;
            }
            set
            {
                this.changeTripTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        private string feeWaiverMessage;
        public string FeeWaiverMessage
        {
            get
            {
                return this.feeWaiverMessage;
            }
            set
            {
                this.feeWaiverMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        

        public bool IsLastTripFSR { get; set; } = false;

        public bool IsRefundBillingAddressRequired { get; set; } = false;

        public MOBAddress RefundAddress { get; set; }

        public MOBAddress FFCRAddress { get; set; }

        public string ChangeFlightHeaderText { get; set; }

        public string FlightHeaderText { get; set; }

        public bool IsUsedPNR { get; set; }

        public bool IsReshopWithFutureFlightCredit { get; set; }

        public bool DisplayNonResidualCreditMessage { get; set; }

    }
}
