using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBCorporateTravelTypeItem
    {
        private string travelType;
        private string travelTypeDescription;

        public string TravelType
        {
            get
            {
                return travelType;
            }
            set
            {
                travelType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TravelTypeDescription
        {
            get
            {
                return travelTypeDescription;
            }
            set
            {
                travelTypeDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public TravelPolicy TravelPolicy { get; set; }
    }
    [Serializable]
    public class TravelPolicy
    {

        public string TravelPolicyTitle { get; set; }

        public string TravelPolicyHeader { get; set; }

        public string TravelPolicyBody { get; set; }

        public List<MOBSection> TravelPolicyContent { get; set; }

        public string TravelPolicyFooterMessage { get; set; }

        public List<string> TravelPolicyButton { get; set; }
    }
}
