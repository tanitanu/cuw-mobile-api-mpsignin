using System;
using System.Text.RegularExpressions;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBPayPal
    {
        public string CurrencyCode { get; set; }
        public string PayPalTokenID { get; set; }
        public string PayerID { get; set; }
        public string BillingAddressCountryCode { get; set; }
    }
    
}
