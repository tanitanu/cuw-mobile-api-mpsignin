using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping
{

    [Serializable]
    public class PriorityBoarding
    {
        public string ProductCode { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public MOBOfferTile PbOfferTileInfo { get; set; } = null;

        public List<MOBTypeOption> PbOfferDetails { get; set; } = null;

        public List<PBSegment> Segments { get; set; } = null; // populate for scenario PB purchased. 

        public List<MOBTypeOption> TAndC { get; set; } = null;

        public string PbDetailsConfirmationTxt { get; set; } = string.Empty; // Priority Boarding details

        public string PbAddedTravelerTxt { get; set; } = string.Empty; // Priority Boarding added for the traveler(s) below

    }
}
