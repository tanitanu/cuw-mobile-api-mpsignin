using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBBKFlight
    {
        private string tripId = string.Empty;
        private string flightId = string.Empty;
        private string cabin;
        private string connectTimeMinutes;
        private string departDate = string.Empty;
        private string departTime = string.Empty;
        private string destination = string.Empty;
        private string destinationDate = string.Empty;
        private string destinationTime = string.Empty;
        private string destinationDescription = string.Empty;
        private string fareBasisCode = string.Empty;
        private string flightNumber = string.Empty;
        private string groundTime;
        private string internationalCity = string.Empty;
        private string marketingCarrier = string.Empty;
        private string marketingCarrierDescription = string.Empty;
        private string miles = string.Empty;
        private string operatingCarrier = string.Empty;
        private string operatingCarrierDescription = string.Empty;
        private string origin = string.Empty;
        private string originDescription = string.Empty;
        private string stopDestination = string.Empty;
        private string travelTime;
        private string totalTravelTime;
        private string meal;
        private string serviceClass = string.Empty;
        private string serviceClassDescription = string.Empty;

        
        private string checkInWindowText = string.Empty;
        private string departureDateTime = string.Empty;
        private string arrivalDateTime = string.Empty;
        private string departureDateTimeGMT = string.Empty;
        private string arrivalDateTimeGMT = string.Empty;

        public string ContinueButtonText { get; set; }


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

        public decimal Airfare { get; set; }

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

        public bool ChangeOfGauge { get; set; }

        public List<MOBBKGaugeChange> GaugeChanges { get; set; }

        public List<MOBBKFlight> Connections { get; set; }

        public string ConnectTimeMinutes
        {
            get
            {
                return this.connectTimeMinutes;
            }
            set
            {
                this.connectTimeMinutes = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

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

        public string DepartTime
        {
            get
            {
                return this.departTime;
            }
            set
            {
                this.departTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Destination
        {
            get
            {
                return this.destination;
            }
            set
            {
                this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DestinationDate
        {
            get
            {
                return this.destinationDate;
            }
            set
            {
                this.destinationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DestinationTime
        {
            get
            {
                return this.destinationTime;
            }
            set
            {
                this.destinationTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DestinationDescription
        {
            get
            {
                return this.destinationDescription;
            }
            set
            {
                this.destinationDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBBKEquipmentDisclosure EquipmentDisclosures { get; set; }

        public string FareBasisCode
        {
            get
            {
                return this.fareBasisCode;
            }
            set
            {
                this.fareBasisCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FlightNumber
        {
            get
            {
                return this.flightNumber;
            }
            set
            {
                this.flightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string GroundTime
        {
            get
            {
                return this.groundTime;
            }
            set
            {
                this.groundTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ;
            }
        }

        public string InternationalCity
        {
            get
            {
                return this.internationalCity;
            }
            set
            {
                this.internationalCity = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsCheapestAirfare { get; set; }

        public bool IsConnection { get; set; }

        public string MarketingCarrier
        {
            get
            {
                return this.marketingCarrier;
            }
            set
            {
                this.marketingCarrier = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MarketingCarrierDescription
        {
            get
            {
                return this.marketingCarrierDescription;
            }
            set
            {
                this.marketingCarrierDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Miles
        {
            get
            {
                return this.miles;
            }
            set
            {
                this.miles = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBBKOnTimePerformance OnTimePerformance { get; set; }

        public string OperatingCarrier
        {
            get
            {
                return this.operatingCarrier;
            }
            set
            {
                this.operatingCarrier = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OperatingCarrierDescription
        {
            get
            {
                return this.operatingCarrierDescription;
            }
            set
            {
                this.operatingCarrierDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Origin
        {
            get
            {
                return this.origin;
            }
            set
            {
                this.origin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OriginDescription
        {
            get
            {
                return this.originDescription;
            }
            set
            {
                this.originDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBBKReward> Rewards { get; set; }

        public List<MOBBKRewardPriceSummary> RewardPriceSummaries { get; set; }

        public bool Selected { get; set; }

        public string StopDestination
        {
            get
            {
                return this.stopDestination;
            }
            set
            {
                this.stopDestination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBBKFlight> StopInfos { get; set; }

        public int Stops { get; set; }

        public string TravelTime
        {
            get
            {
                return this.travelTime;
            }
            set
            {
                this.travelTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TotalTravelTime
        {
            get
            {
                return this.totalTravelTime;
            }
            set
            {
                this.totalTravelTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ;
            }
        }

        public List<MOBBKMessage> Messages { get; set; }

        public string Meal  
        {
            get
            {
                return this.meal;
            }
            set
            {
                this.meal = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ;
            }
        }

        public bool FPWSAir { get; set; }

        public string ServiceClass
        {
            get
            {
                return this.serviceClass;
            }
            set
            {
                this.serviceClass = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ;
            }
        }

        public string ServiceClassDescription
        {
            get
            {
                return this.serviceClassDescription;
            }
            set
            {
                this.serviceClassDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ;
            }
        }

        public string EPAMessageTitle { get; set; } = string.Empty;
        public string EPAMessage { get; set; } = string.Empty;

        public bool ShowEPAMessage { get; set; } = false;

        public bool IsCheckInWindow { get; set; } = false;

        public string CheckInWindowText
        {
            get
            {
                return this.checkInWindowText;
            }
            set
            {
                this.checkInWindowText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DepartureDateTime
        {
            get
            {
                return this.departureDateTime;
            }
            set
            {
                this.departureDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ArrivalDateTime
        {
            get
            {
                return this.arrivalDateTime;
            }
            set
            {
                this.arrivalDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DepartureDateTimeGMT
        {
            get
            {
                return this.departureDateTimeGMT;
            }
            set
            {
                this.departureDateTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ArrivalDateTimeGMT
        {
            get
            {
                return this.arrivalDateTimeGMT;
            }
            set
            {
                this.arrivalDateTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool MatchServiceClassRequested { get; set; }


        public string ProductCode { get; set; }

        public bool IsIBE { get; set; }
    }
}
