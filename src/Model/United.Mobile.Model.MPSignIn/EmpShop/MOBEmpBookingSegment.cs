using System;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBEmpBookingSegment
    {
        //89|IAH|EWR|0600|1025|10/15/2010|0205|F'
        //A  B   C   D    E    F          G    H
        //A = Flight Number
        //B = Origin
        //C = Destination
        //D = Depart time
        //E = Arrival Time
        //F = Departure Date
        //G = travel time in minutes
        //H = First Class bucket
        private string fln = string.Empty;
        private string departTime = string.Empty;
        private string arrivalTime = string.Empty;
        private string departDate = string.Empty;
        private string arrivalDate = string.Empty;
        private string firstClassBucket = string.Empty;
        private string marketingCarrier = string.Empty;
        private string operatingCarrier = string.Empty;
        private string cvd;

        
        public string FlightNumber
        {
            get { return this.fln; }
            set { this.fln = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public MOBEmpAirport Origin { get; set; }

        public MOBEmpAirport Destination { get; set; }

        public string DepartTime
        {
            get { return this.departTime; }
            set { this.departTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string ArrivalTime
        {
            get { return this.arrivalTime; }
            set { this.arrivalTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string ArrivalDate
        {
            get { return this.arrivalDate; }
            set { this.arrivalDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string DepartDate
        {
            get { return this.departDate; }
            set { this.departDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public MOBEmpTravelTime TravelTime { get; set; }

        public string FirstClassBucket
        {
            get { return this.firstClassBucket; }
            set { this.firstClassBucket = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string MarketingCarrier
        {
            get { return this.marketingCarrier; }
            set { this.marketingCarrier = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string OperatingCarrier
        {
            get { return this.operatingCarrier; }
            set { this.operatingCarrier = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public int SegmentIndex { get; set; }

        public bool IsBusinessFirstEligible { get; set; }

        public bool IsBusinessCoachEligible { get; set; }

        public MOBEmpDEI DEI { get; set; }

        public MOBTypeOption Cabin { get; set; }

        public string CurrentViewDate
        {
            get { return this.cvd; }
            set { this.cvd = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public int CabinCount { get; set; }

        public MOBEmpPSCost PSCost { get; set; }
        public MOBEmpPBTType Capacity { get; set; }
    }
}


