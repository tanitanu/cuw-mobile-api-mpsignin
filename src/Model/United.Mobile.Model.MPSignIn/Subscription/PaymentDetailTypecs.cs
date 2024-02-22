using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MPSignIn.Subscription
{
    public class PaymentDetailType : PaymentFormType
    {

        //private PaymentDetailTypePaymentAmount[] paymentAmountField;

        //private CommissionType commissionField;

        //private ArrayOfPaymentDetailTypeOfferMappingOfferMapping[] associationField;

        //private PaymentDetailTypePaymentType paymentTypeField;

        private bool paymentTypeFieldSpecified;

        private bool splitPaymentIndField;

        private bool splitPaymentIndFieldSpecified;

        private string authorizedDaysField;

        private bool primaryPaymentIndField;

        private bool primaryPaymentIndFieldSpecified;

        //public PaymentDetailTypePaymentAmount[] PaymentAmount
        //{
        //    get
        //    {
        //        return this.paymentAmountField;
        //    }
        //    set
        //    {
        //        this.paymentAmountField = value;
        //    }
        //}

        //public CommissionType Commission
        //{
        //    get
        //    {
        //        return this.commissionField;
        //    }
        //    set
        //    {
        //        this.commissionField = value;
        //    }
        //}

        //public ArrayOfPaymentDetailTypeOfferMappingOfferMapping[] Association
        //{
        //    get
        //    {
        //        return this.associationField;
        //    }
        //    set
        //    {
        //        this.associationField = value;
        //    }
        //}

        //public PaymentDetailTypePaymentType PaymentType
        //{
        //    get
        //    {
        //        return this.paymentTypeField;
        //    }
        //    set
        //    {
        //        this.paymentTypeField = value;
        //    }
        //}

        public bool PaymentTypeSpecified
        {
            get
            {
                return this.paymentTypeFieldSpecified;
            }
            set
            {
                this.paymentTypeFieldSpecified = value;
            }
        }

        public bool SplitPaymentInd
        {
            get
            {
                return this.splitPaymentIndField;
            }
            set
            {
                this.splitPaymentIndField = value;
            }
        }

        public bool SplitPaymentIndSpecified
        {
            get
            {
                return this.splitPaymentIndFieldSpecified;
            }
            set
            {
                this.splitPaymentIndFieldSpecified = value;
            }
        }

        public string AuthorizedDays
        {
            get
            {
                return this.authorizedDaysField;
            }
            set
            {
                this.authorizedDaysField = value;
            }
        }

        public bool PrimaryPaymentInd
        {
            get
            {
                return this.primaryPaymentIndField;
            }
            set
            {
                this.primaryPaymentIndField = value;
            }
        }

        public bool PrimaryPaymentIndSpecified
        {
            get
            {
                return this.primaryPaymentIndFieldSpecified;
            }
            set
            {
                this.primaryPaymentIndFieldSpecified = value;
            }
        }
    }
}
