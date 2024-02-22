using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping
{
    [Serializable()]
    public class MOBSHOPTripBase
    {
        private string departDate = string.Empty;
        private string shareMessage = string.Empty;

        private string origin = string.Empty;

        private string destination = string.Empty;


        private string cabin = string.Empty;
        public int Index { get; set; }
        public string Origin
        {
            get
            {
                return this.origin;
            }
            set
            {
                this.origin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        //MB-2639 add all airports flag
        [JsonPropertyName("originAllAirports")]
        public int OriginAllAirports { get; set; } = -1;

        public string Destination
        {
            get
            {
                return this.destination;
            }
            set
            {
                this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        //MB-2639 add all airports flag
        public int DestinationAllAirports { get; set; } = -1;

        public string DepartDate
        {
            get
            {
                return this.departDate;
            }
            set
            {
                this.departDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ArrivalDate { get; set; }

        public string Cabin
        {
            get
            {
                return this.cabin;
            }
            set
            {
                this.cabin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool UseFilters { get; set; } = false;

        public MOBSearchFilters SearchFiltersIn { get; set; }

        public MOBSearchFilters SearchFiltersOut { get; set; }

        public bool SearchNearbyOriginAirports { get; set; } = false;

        public bool SearchNearbyDestinationAirports { get; set; } = false;
        public string ShareMessage
        {
            get
            {
                return this.shareMessage;
            }
            set
            {
                this.shareMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }


        public MOBSHOPTripChangeType ChangeType { get; set; } = MOBSHOPTripChangeType.NoChange;
    }
    [Serializable]
    [System.Text.Json.Serialization.JsonConverter(typeof(StringEnumConverter))]
    public enum MOBSHOPTripChangeType
    {
        [EnumMember(Value = "0")]
        [Display(Name = "0")]
        ChangeFlight,
        [EnumMember(Value = "1")]
        [Display(Name = "1")]
        AddFlight,
        [EnumMember(Value = "2")]
        [Display(Name = "2")]
        DeleteFlight,
        [EnumMember(Value = "3")]
        [Display(Name = "3")]
        NoChange
    }
}
