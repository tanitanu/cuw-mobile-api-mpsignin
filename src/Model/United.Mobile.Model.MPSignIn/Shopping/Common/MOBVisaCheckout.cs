using System;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping
{
    [Serializable]
    public class MOBVisaCheckout
    {

        public string VisaCheckOutCallID { get; set; }

        public MOBCreditCard VisaCheckoutCard { get; set; }
    }
}
