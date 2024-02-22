using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Shopping
{

    [Serializable()]
    public class UpgradeOption
    {
        public string UpgradeType { get; set; }
        public string UpgradeStatus { get; set; }
        public string CabinUpgradeTypeDesc { get; set; }
        public string AvailableSeatCount { get; set; }
        public string TripRefId { get; set; }
        public string SegmentRefId { get; set; }
        public string Id { get; set; }
        public string DoubleUpgradeTooltip { get; set; }
        public List< UpgradeCabinTypeDesc> UpgradeCabinTypes { get; set; }
        public List< UpgradePriceOption> PriceOption { get; set; }
        public Boolean AllowSelection { get; set; }
        public List< MOBSHOPTax> Taxes { get; set; }
        public List< UpgradeCabinAdvisory> Messages { get; set; }

    }
}
