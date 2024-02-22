using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBPromoCode
    {
        public string PromoCode { get; set; }
  
        public bool IsRemove { get; set; }
   

        public string AlertMessage { get; set; }


        public bool IsSuccess { get; set; }


        public MOBMobileCMSContentMessages TermsandConditions { get; set; }
     

        public double PromoValue { get; set; }

        public string FormattedPromoDisplayValue { get; set; }

        public string PriceTypeDescription { get; set; }

        public string Product { get; set; }
    }
}
