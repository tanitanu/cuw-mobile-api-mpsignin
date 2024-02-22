using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Shopping
{    
    [Serializable]
    public class PBCustomer
    {
        public string CustId { get; set; } = string.Empty; // custId from Merch response

        public string TagId { get; set; } = string.Empty; // 1-1, first 1 represents for segment ID, second 1 represents for customer ID

        public string CustName { get; set; } = string.Empty; // cust name

        public double Fee { get; set; } // from Merch

        public bool AlreadyPurchased { get; set; } = false;

        public bool Selected { get; set; } = false; // client side usage 

        public string Message { get; set; } = string.Empty; // segment ineligible reason

        public string FormattedFee { get; set; } = string.Empty; // formatted amount with dollar sign. Will be rounded in offer tile and 2 decimal in payment page
    }
}
