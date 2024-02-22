using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBStatusLiftBannerResponse : MOBResponse
    {
        public string CurrentLevel { get; set; }
        public string CurrentLevelName { get; set; }
        public string EligibleLevel { get; set; }
        public string EligibleLevelName { get; set; }
        public string EligibleLevelCode { get; set; }
        public string Description { get; set; }
        public string StatusLiftURL { get; set; }
        public string MileagePlusNumber { get; set; }
        public string CustID { get; set; }
        public string PromoCode { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
