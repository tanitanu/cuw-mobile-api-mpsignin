using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Shopping
{
    [Serializable()]
    public class UpgradeCabinTypeDesc
    {
        public string UpgradeType { get; set; }
        public string UpgradeTypeDesc { get; set; }
        public string UpgradeStatus { get; set; }
        public string Id { get; set; }
        public string AvailableSeatCount { get; set; }
        public string AvailableSeatMsg { get; set; }
        public string SegmentNumber { get; set; }
    }
}
