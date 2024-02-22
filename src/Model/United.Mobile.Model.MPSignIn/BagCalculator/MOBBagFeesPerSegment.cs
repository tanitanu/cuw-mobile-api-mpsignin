using System;

namespace United.Mobile.Model.BagCalculator
{
    [Serializable()]
    public class MOBBagFeesPerSegment
    {
        private string flightTravelDate = string.Empty;
        private string originAirportCode = string.Empty;
        private string originAirportName = string.Empty;
        private string destinationAirportCode = string.Empty;
        private string destinationAirportName = string.Empty;
        private string firstBagFee = string.Empty;
        private string regularFirstBagFee = string.Empty;
        private string secondBagFee = string.Empty;
        private string regularSecondBagFee = string.Empty;
        private string weightPerBag = string.Empty;

        public string FlightTravelDate
        {
            get => this.flightTravelDate;
            set => this.flightTravelDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string OriginAirportCode
        {
            get => this.originAirportCode;
            set => this.originAirportCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string OriginAirportName
        {
            get => this.originAirportName;
            set => this.originAirportName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string DestinationAirportCode
        {
            get => this.destinationAirportCode;
            set => this.destinationAirportCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string DestinationAirportName
        {
            get => this.destinationAirportName;
            set => this.destinationAirportName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string FirstBagFee
        {
            get => this.firstBagFee;
            set => this.firstBagFee = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string RegularFirstBagFee
        {
            get => this.regularFirstBagFee;
            set => this.regularFirstBagFee = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string SecondBagFee
        {
            get => this.secondBagFee;
            set => this.secondBagFee = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string RegularSecondBagFee
        {
            get => this.regularSecondBagFee;
            set => this.regularSecondBagFee = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string WeightPerBag
        {
            get => this.weightPerBag;
            set => this.weightPerBag = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public bool IsFirstBagFree { get; set; } = false;

        public bool IsSecondBagFree { get; set; } = false;

        public string CabinName { get; set; }
    }
}
