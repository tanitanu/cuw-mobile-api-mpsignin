using System;
using System.Collections.Generic;
using System.Text;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBPayPalPayor
    {

        public MOBAddress PayPalBillingAddress { get; set; }


        public string PayPalStatus { get; set; } // EX: Description=VERIFIED


        public string PayPalSurName { get; set; }


        public string PayPalGivenName { get; set; }


        public string PayPalContactEmailAddress { get; set; }


        public string PayPalCustomerID { get; set; }

        public string PayPalContactPhoneNumber { get; set; }
    }
}
