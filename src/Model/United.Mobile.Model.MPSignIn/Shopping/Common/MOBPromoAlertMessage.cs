using System;
using System.Collections.Generic;
using System.Text;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping
{
    public class MOBPromoAlertMessage
    {
        public string PromoCode { get; set; }

        public bool IsSignInRequired { get; set; }


        public MOBSection AlertMessages { get; set; }

        public string TermsandConditions { get; set; }

        public string CouponInlineAlertMessage { get; set; }
    }
}
