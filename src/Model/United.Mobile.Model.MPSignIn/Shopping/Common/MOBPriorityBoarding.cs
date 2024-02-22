using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Mobile.Model.MPRewards;
using United.Mobile.Model.Shopping;

namespace United.Mobile.Model.Common
{
    
    [Serializable]
    public class MOBPriorityBoarding
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public MOBOfferTile PbOfferTileInfo { get; set; } = null;

        public List<MOBTypeOption> PbOfferDetails { get; set; } = null;

        public List<MOBPBSegment> Segments { get; set; } = null; // populate for scenario PB purchased. 

        public List<MOBTypeOption> TAndC { get; set; } = null;

        public string PbDetailsConfirmationTxt { get; set; } = string.Empty; // Priority Boarding details

        public string PbAddedTravelerTxt { get; set; } = string.Empty; // Priority Boarding added for the traveler(s) below
    }
}
