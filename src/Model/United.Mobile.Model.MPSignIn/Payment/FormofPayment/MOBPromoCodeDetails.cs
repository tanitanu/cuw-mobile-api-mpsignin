using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping.FormofPayment
{
    [Serializable()]
    public class MOBPromoCodeDetails
    {

        public List<MOBPromoCode> PromoCodes { get; set; }

        public bool IsDisablePromoOption { get; set; }

        public bool IsHidePromoOption { get; set; }

    }
}
