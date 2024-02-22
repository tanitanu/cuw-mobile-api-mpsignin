using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MPSignIn.Subscription
{
    public class PaymentFormType
    {

        private object itemField;

        private string rPHField;

        //private PaymentFormTypePaymentTransactionTypeCode paymentTransactionTypeCodeField;

        private bool paymentTransactionTypeCodeFieldSpecified;

        private string remarkField;
       
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

     
        public string RPH
        {
            get
            {
                return this.rPHField;
            }
            set
            {
                this.rPHField = value;
            }
        }

        //public PaymentFormTypePaymentTransactionTypeCode PaymentTransactionTypeCode
        //{
        //    get
        //    {
        //        return this.paymentTransactionTypeCodeField;
        //    }
        //    set
        //    {
        //        this.paymentTransactionTypeCodeField = value;
        //    }
        //}

        public bool PaymentTransactionTypeCodeSpecified
        {
            get
            {
                return this.paymentTransactionTypeCodeFieldSpecified;
            }
            set
            {
                this.paymentTransactionTypeCodeFieldSpecified = value;
            }
        }

        public string Remark
        {
            get
            {
                return this.remarkField;
            }
            set
            {
                this.remarkField = value;
            }
        }
    }
}
