using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBBKReward
    {
        private string tripId;
        private string flightId;
        private string rewardId;
        private string cabin = string.Empty;
        private string crossCabinMessaging = string.Empty;
        private string fareBasis = string.Empty;
        private bool promotion;
        private string rewardCode = string.Empty;
        private string rewardType = string.Empty;
        private string status = string.Empty;

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

        public string RewardId
        {
            get
            {
                return this.rewardId;
            }
            set
            {
                this.rewardId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool Available { get; set; }

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

        public decimal ChangeFee { get; set; }

        public decimal ChangeFeeTotal { get; set; }

        public string CrossCabinMessaging
        {
            get
            {
                return this.crossCabinMessaging;
            }
            set
            {
                this.crossCabinMessaging = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FareBasis
        {
            get
            {
                return this.fareBasis;
            }
            set
            {
                this.fareBasis = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public decimal Mileage { get; set; }

        public decimal MileageCollect { get; set; }

        public decimal MileageCollectTotal { get; set; }

        public decimal MileageTotal { get; set; }

        public string RewardCode
        {
            get
            {
                return this.rewardCode;
            }
            set
            {
                this.rewardCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string RewardType
        {
            get
            {
                return this.rewardType;
            }
            set
            {
                this.rewardType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool Selected { get; set; }

        public string Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.status = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBBKTax> Taxes { get; set; }

        public decimal TaxTotal { get; set; }

        public decimal TaxAndFeeTotal { get; set; }

        public List<string> Descriptions { get; set; }
    }
}
