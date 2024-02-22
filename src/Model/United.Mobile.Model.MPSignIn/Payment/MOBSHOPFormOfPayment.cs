using System;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBSHOPFormOfPayment
    {
        public MOBSHOPFormOfPayment()
        {
            this.formOfPayment = MOBFormofPayment.CreditCard;
        }

        private string formOfPaymentType = string.Empty;
        private string visaCheckOutCallID = string.Empty;
        private MOBFormofPayment formOfPayment;
        public MOBApplePay ApplePayInfo { get; set; }

        public string FormOfPaymentType
        {
            get
            {
                return this.formOfPaymentType;
            }
            set
            {
                this.formOfPaymentType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public MOBCreditCard CreditCard { get; set; }

        public string VISACheckOutCallID
        {
            get
            {
                return this.visaCheckOutCallID;
            }
            set
            {
                this.visaCheckOutCallID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public MOBFormofPayment FormOfPayment
        {
            get { return formOfPayment; }
            set { formOfPayment = value; }
        }

        public MOBPayPal PayPal { get; set; }

        public MOBMasterpass Masterpass { get; set; }
    }
}
