using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping
{
    [Serializable()]
    public class MOBSHOPFlight
    {
        private string tripId = string.Empty;
        private string flightId = string.Empty;
        private string productId = string.Empty;
        private string airfareDisplayValue = string.Empty;
        private string cabin;

        private string connectTimeMinutes;
        private string departDate = string.Empty;
        private string departTime = string.Empty;
        private string destination = string.Empty;
        private string destinationDate = string.Empty;
        private string destinationTime = string.Empty;
        private string destinationDescription = string.Empty;
        private string destinationCountryCode = string.Empty;
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
        private string originCountryCode = string.Empty;
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

        //adding ammenities bool values - Wade 4/16/2014


        private string segID = string.Empty;

        private string flightDepartureDays = string.Empty;
        private string flightArrivalDays = string.Empty;

        private string corporateFareIndicator = string.Empty;

        //private bool isAddCollectWaived;

        private string flightHash;

        private string originStateCode = string.Empty;
        private string destinationStateCode = string.Empty;

        private string originDecodedWithCountry;
        private string destinationDecodedWithCountry;

        private string departureDateFormated;

        [XmlArrayItem("MOBSHOPSegmentInfoAlerts")]
        public List<SegmentInfoAlerts> FlightSegmentAlerts { get; set; }
        public string ReshopFees { get; set; } = string.Empty;

        public CreditTypeColor ReshopCreditColor { get; set; }

        public bool IsChangeFeeWaiver { get; set; }
        public bool IsCovidTestFlight { get; set; }
        public string DestinationDecodedWithCountry
        {
            get => this.destinationDecodedWithCountry;
            set => this.destinationDecodedWithCountry = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string OriginDecodedWithCountry
        {
            get => this.originDecodedWithCountry;
            set => this.originDecodedWithCountry = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }


        public string OriginStateCode
        {
            get => originStateCode;
            set => originStateCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string DestinationStateCode
        {
            get => destinationStateCode;
            set => destinationStateCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
        public string YaDiscount { get; set; } = string.Empty;

        public bool isAddCollectWaived { get; set; }
        //public bool isAddcollectWaived
        //{
        //    get
        //    {
        //        return this.isAddCollectWaived;
        //    }
        //    set
        //    {
        //        this.isAddCollectWaived = value;
        //    }
        //}

        public string AddCollectProductId { get; set; }

        public string CorporateFareIndicator
        {
            get => corporateFareIndicator;
            set => corporateFareIndicator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
        private string milesDisplayValue = string.Empty;
        public string MilesDisplayValue
        {
            get => milesDisplayValue;
            set => milesDisplayValue = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        private string cabinDisclaimer = string.Empty;
        public string CabinDisclaimer
        {
            get => cabinDisclaimer;
            set => cabinDisclaimer = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        private string availSeatsDisclaimer = string.Empty;
        public string AvailSeatsDisclaimer
        {
            get => availSeatsDisclaimer;
            set => availSeatsDisclaimer = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }


        private string preferredCabinName = string.Empty;
        public string PreferredCabinName
        {
            get => preferredCabinName;
            set => preferredCabinName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        private string preferredCabinMessage = string.Empty;
        public string PreferredCabinMessage
        {
            get => preferredCabinMessage;
            set => preferredCabinMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }


        private string overnightConnection = string.Empty;
        public string OvernightConnection
        {
            get => overnightConnection;
            set => overnightConnection = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        private string airportChange = string.Empty;
        public string AirportChange
        {
            get => airportChange;
            set => airportChange = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public bool ShowSeatMap { get; set; }

        public bool IsStopOver { get; set; }

        public string TripId
        {
            get => this.tripId;
            set => this.tripId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string FlightId
        {
            get => this.flightId;
            set => this.flightId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string ProductId
        {
            get => this.productId;
            set => this.productId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public decimal Airfare { get; set; }

        public string AirfareDisplayValue
        {
            get => this.airfareDisplayValue;
            set => this.airfareDisplayValue = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public int SeatsRemaining { get; set; }

        public string Cabin
        {
            get => this.cabin;
            set => this.cabin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public bool ChangeOfGauge { get; set; }

        public List<GaugeChange> GaugeChanges { get; set; }

        public List<MOBSHOPFlight> Connections { get; set; }

        public string ConnectTimeMinutes
        {
            get => this.connectTimeMinutes;
            set => this.connectTimeMinutes = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string DepartDate
        {
            get => this.departDate;
            set => this.departDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string DepartTime
        {
            get => this.departTime;
            set => this.departTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string Destination
        {
            get => this.destination;
            set => this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string DestinationDate
        {
            get => this.destinationDate;
            set => this.destinationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string DestinationTime
        {
            get => this.destinationTime;
            set => this.destinationTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string DestinationDescription
        {
            get => this.destinationDescription;
            set => this.destinationDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string DestinationCountryCode
        {
            get => this.destinationCountryCode;
            set => this.destinationCountryCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }


        public SHOPEquipmentDisclosure EquipmentDisclosures { get; set; }

        public string FareBasisCode
        {
            get => this.fareBasisCode;
            set => this.fareBasisCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string FlightNumber
        {
            get => this.flightNumber;
            set => this.flightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string GroundTime
        {
            get => this.groundTime;
            set
            {
                this.groundTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ;
            }
        }

        public string InternationalCity
        {
            get => this.internationalCity;
            set => this.internationalCity = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public bool IsCheapestAirfare { get; set; }

        public bool IsConnection { get; set; }

        public string MarketingCarrier
        {
            get => this.marketingCarrier;
            set => this.marketingCarrier = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string MarketingCarrierDescription
        {
            get => this.marketingCarrierDescription;
            set => this.marketingCarrierDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string Miles
        {
            get => this.miles;
            set => this.miles = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public SHOPOnTimePerformance OnTimePerformance { get; set; }

        public string OperatingCarrier
        {
            get => this.operatingCarrier;
            set => this.operatingCarrier = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string OperatingCarrierDescription
        {
            get => this.operatingCarrierDescription;
            set => this.operatingCarrierDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string Origin
        {
            get => this.origin;
            set => this.origin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string OriginDescription
        {
            get => this.originDescription;
            set => this.originDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string OriginCountryCode
        {
            get => this.originCountryCode;
            set => this.originCountryCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public List<SHOPReward> Rewards { get; set; }

        public List<SHOPRewardPriceSummary> RewardPriceSummaries { get; set; }

        public bool Selected { get; set; }

        public string StopDestination
        {
            get => this.stopDestination;
            set => this.stopDestination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public List<MOBSHOPFlight> StopInfos { get; set; }

        public int Stops { get; set; }

        public string TravelTime
        {
            get => this.travelTime;
            set => this.travelTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string TotalTravelTime
        {
            get => this.totalTravelTime;
            set
            {
                this.totalTravelTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ;
            }
        }

        public List<MOBSHOPMessage> Messages { get; set; }

        public string Meal
        {
            get => this.meal;
            set
            {
                this.meal = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ;
            }
        }

        [JsonProperty(PropertyName = "fpwSAir")]
        [JsonPropertyName("fpwSAir")]
        public bool FPWSAir { get; set; }

        public string ServiceClass
        {
            get => this.serviceClass;
            set
            {
                this.serviceClass = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); ;
            }
        }

        public string ServiceClassDescription
        {
            get => this.serviceClassDescription;
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
            get => this.checkInWindowText;
            set => this.checkInWindowText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string DepartureDateTime
        {
            get => this.departureDateTime;
            set => this.departureDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string ArrivalDateTime
        {
            get => this.arrivalDateTime;
            set => this.arrivalDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string DepartureDateTimeGMT
        {
            get => this.departureDateTimeGMT;
            set => this.departureDateTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string ArrivalDateTimeGMT
        {
            get => this.arrivalDateTimeGMT;
            set => this.arrivalDateTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }


        public bool IsReshopCredit { get; set; }
        public bool MatchServiceClassRequested { get; set; }

        public bool HasInSeatPower { get; set; } = false;

        public bool HasWifi { get; set; } = false;

        public bool HasDirecTV { get; set; } = false;

        public bool HasAVOnDemand { get; set; } = false;

        public bool HasBeverageService { get; set; } = false;

        public bool HasEconomyLieFlatSeating { get; set; } = false;

        public bool HasEconomyMeal { get; set; } = false;

        public bool HasFirstClassMeal { get; set; } = false;

        public bool HasFirstClassLieFlatSeating { get; set; } = false;

        public List<MOBSHOPShoppingProduct> ShoppingProducts { get; set; }

        public string SegID
        {
            get => this.segID;
            set => this.segID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        [JsonProperty(PropertyName = "segNumber")]
        [JsonPropertyName("segNumber")]
        public int SegmentNumber { get; set; }

        public string FlightDepartureDays
        {
            get => this.flightDepartureDays;
            set => this.flightDepartureDays = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string FlightArrivalDays
        {
            get => this.flightArrivalDays;
            set => this.flightArrivalDays = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        private string pqdText = string.Empty;
        public string PqdText
        {
            get => pqdText;
            set => pqdText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        private string pqmText = string.Empty;
        public string PqmText
        {
            get => pqmText;
            set => pqmText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        private string rdmText = string.Empty;
        public string RdmText
        {
            get => rdmText;
            set => rdmText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        private string yqyrMessage = string.Empty;
        public string YqyrMessage
        {
            get => yqyrMessage;
            set => yqyrMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        private string govtMessage = string.Empty;
        public string GovtMessage
        {
            get => govtMessage;
            set => govtMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public bool IsAwardSaver { get; set; }

        public string RedEyeFlightDepDate { get; set; }


        public string NextDayFlightArrDate { get; set; }


        public bool FlightDateChanged { get; set; }

        public string BookingClassAvailability { get; set; }


        public int TripIndex { get; set; }

        /// <example>
        /// "16-31|1180-UA"
        /// </example>
        /// <hint>
        /// The hash of the flight
        /// </hint>
        public string FlightHash
        {
            get => flightHash;
            set => flightHash = string.IsNullOrEmpty(value) ? "US" : value.Trim().ToUpper();
        }

        public string OperatingCarrierFlightNumber { get; set; }

        public string LineOfFlightMessage { get; set; }

        public string DepartureDateFormated
        {
            get => this.departureDateFormated;
            set => this.departureDateFormated = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public List<MOBItem> SelectedAncillaries { get; set; }

        //public List<MOBItem> SelectedAncillaries
        //{
        //    get { return selectedAncillaries; }
        //    set { selectedAncillaries = value; }
        //}

        public bool ChangeOfPlane { get; set; }

        public bool IsThroughFlight { get; set; }
    }


    [Serializable]
    public enum CreditTypeColor
    {
        [EnumMember(Value = "NONE")] //DEFAULT
        NONE,
        [EnumMember(Value = "GREEN")]
        GREEN,
    }
}
