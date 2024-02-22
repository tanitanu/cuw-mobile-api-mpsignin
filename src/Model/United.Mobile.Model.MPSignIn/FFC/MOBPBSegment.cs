using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.MPRewards
{
    [Serializable]
    public class MOBPBSegment
    {
        private bool isChecked; // client usage

        public string Origin { get; set; } = string.Empty;

        public string Destination { get; set; } = string.Empty;

        public string FlightDate { get; set; } = string.Empty;

        public List<MOBPBCustomer> Customers { get; set; } = null;

        public MOBPBSegmentType PbSegmentType { get; set; }

        public string SegmentId { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public int Fee { get; set; } // new change. PB will be offered based on Segment instead of based on traveler. Might change later 

        public int CustPrice { get; set; } // for build segment info as $54/traveler

        public List<string> OfferIds { get; set; } //["S10", "S11"]
    }
}


