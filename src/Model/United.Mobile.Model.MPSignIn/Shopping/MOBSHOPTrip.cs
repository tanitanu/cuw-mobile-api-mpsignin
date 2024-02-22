using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Shopping
{
    [Serializable()]
    public class MOBSHOPTrip : MOBSHOPTripBase
    {
        private string tripId = string.Empty;
        private string originDecoded = string.Empty;
        private string destinationDecoded = string.Empty;
        private string yqyrMessage = string.Empty;
        private string originDecodedWithCountry;
        private string destinationDecodedWithCountry;

        public bool DisableEplus { get; set; }

        public string DestinationDecodedWithCountry
        {
            get
            {
                return this.destinationDecodedWithCountry;
            }
            set
            {
                this.destinationDecodedWithCountry = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OriginDecodedWithCountry
        {
            get
            {
                return this.originDecodedWithCountry;
            }
            set
            {
                this.originDecodedWithCountry = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }


        public string YqyrMessage
        {
            get { return yqyrMessage; }
            set { yqyrMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

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

        public string OriginDecoded
        {
            get
            {
                return this.originDecoded;
            }
            set
            {
                this.originDecoded = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DestinationDecoded
        {
            get
            {
                return this.destinationDecoded;
            }
            set
            {
                this.destinationDecoded = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool ShowOriginDestinationForFlights { get; set; } = false;

        public List<MOBSHOPFlattenedFlight> FlattenedFlights { get; set; }

        public List<FlightSection> FlightSections { get; set; }

        public int FlightCount { get; set; }

        public int TotalFlightCount { get; set; }

        public int LastTripIndexRequested { get; set; }

        public List<MOBSHOPShoppingProduct> Columns { get; set; }

        private string callDurationText = string.Empty;

        public string CallDurationText
        {
            get { return callDurationText; }
            set { callDurationText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public int PageCount { get; set; }

        //private string flightDateChangeMessage = ConfigurationManager.AppSettings["FlightDateChangeMessage"].ToString();
        public string FlightDateChangeMessage { get; set; } = "Please note this flight involves a date change";


        public bool TripHasNonStopflightsOnly { get; set; }
        public MOBSHOPTrip()
        {
            FlattenedFlights = new List<MOBSHOPFlattenedFlight>();
        }
    }
}