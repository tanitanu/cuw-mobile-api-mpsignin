using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBPlusPoints
    {
        public string PlusPointsAvailableText { get; set; }
        public string PlusPointsAvailableValue { get; set; }
        public string PlusPointsDeductedText { get; set; }
        public string PlusPointsDeductedValue { get; set; }
        public string PlusPointsExpirationText { get; set; }
        public string PlusPointsExpirationValue { get; set; }
        public string PlusPointsUpgradesText { get; set; }
        public string PlusPointsUpgradesLink { get; set; }
        public bool IsHidePlusPointsExpiration { get; set; }
        public string PlusPointsExpirationInfo { get; set; }
        public string PlusPointsExpirationInfoHeader { get; set; }
        public string plusPointsExpirationPointsInfoSubHeader { get; set; }
        //{
        //    get { return this.plusPointsExpirationPointsInfoSubHeader; }
        //    set { this.plusPointsExpirationPointsInfoSubHeader = value; }
        //}
        public string PlusPointsExpirationInfoDateSubHeader { get; set; }
        public List<MOBKVP> ExpirationPointsAndDatesKVP { get; set; }
        public string WebShareToken { get; set; } = string.Empty;
        public string WebSessionShareUrl { get; set; } = string.Empty;
        public Boolean RedirectToDotComMyTripsWithSSOCheck { get; set; }
    }
}
