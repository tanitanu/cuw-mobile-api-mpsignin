using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Shopping
{
    [Serializable()]
    public class MOBSHOPFlattenedFlight
    {

        private string tripId = string.Empty;
        private string flightId = string.Empty;
        private string productId = string.Empty;
        private string tripDays = string.Empty;
        private string cabinMessage = string.Empty;
        private string flightHash = string.Empty;

        public string TripId
        {
            get
            {
                return this.tripId;
            }
            set
            {
                this.tripId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FlightId
        {
            get
            {
                return this.flightId;
            }
            set
            {
                this.flightId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CabinMessage
        {
            get
            {
                return this.cabinMessage;
            }
            set
            {
                this.cabinMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ProductId
        {
            get
            {
                return this.productId;
            }
            set
            {
                this.productId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBSHOPFlight> Flights { get; set; }

        public string TripDays
        {
            get
            {
                return this.tripDays;
            }
            set
            {
                this.tripDays = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsUADiscount { get; set; }

        public bool isAddCollectWaived { get; set; }

        public string AddCollectProductId { get; set; }

        /// <example>
        /// "16-31|1180-UA"
        /// </example>
        /// <hint>
        /// The flight's hash of the flight. If a flight has connection, this is the flight hash of the first segment 
        /// </hint>
        public string FlightHash
        {
            get
            {
                return flightHash;
            }
            set
            {
                flightHash = string.IsNullOrEmpty(value) ? "US" : value.Trim().ToUpper();
            }
        }

        private string airPortChange = string.Empty;

        [JsonProperty(PropertyName = "airportChange")]
        public string AirportChange
        {
            get { return this.airPortChange; }
            set
            {
                this.airPortChange = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        
        /// <summary>
        /// True if the product is IBELite amd is available
        /// </summary>
        public bool IsIBELite { get; set; }

        public bool IsIBE { get; set; }

        public bool IsElf { get; set; }
        public bool IsChangeFeeWaiver { get; set; }
        public bool IsCovidTestFlight { get; set; }
        public List<string> FlightLabelTextList { get; set; }

        public List<MOBStyledText> FlightBadges { get; set; }

        public string MsgFlightCarrier { get; set; }

    }
}
