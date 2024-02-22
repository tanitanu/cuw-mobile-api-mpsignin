using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.PNRManagement
{
    [Serializable]
    public class MOBSegmentResponse
    {
        private string carrierCode = string.Empty;
        private string classOfService = string.Empty;
        private string departureDateTime = string.Empty;
        private string destination = string.Empty;
        private string origin = string.Empty;
        private string previousSegmentActionCode = string.Empty;
        private string segmentActionCode = string.Empty;
        private string upgradeRemark = string.Empty;
        private string decodedUpgradeMessage = string.Empty;
        private string upgradeMessage = string.Empty;

        public string CarrierCode
        {
            get
            {
                return this.carrierCode;
            }
            set
            {
                this.carrierCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ClassOfService
        {
            get
            {
                return this.classOfService;
            }
            set
            {
                this.classOfService = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public int FlightNumber { get; set; }

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

        public string PreviousSegmentActionCode
        {
            get
            {
                return this.previousSegmentActionCode;
            }
            set
            {
                this.previousSegmentActionCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string SegmentActionCode
        {
            get
            {
                return this.segmentActionCode;
            }
            set
            {
                this.segmentActionCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public int SegmentNumber { get; set; }

        public string UpgradeRemark
        {
            get
            {
                return this.upgradeRemark;
            }
            set
            {
                this.upgradeRemark = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DecodedUpgradeMessage
        {
            get
            {
                return this.decodedUpgradeMessage;
            }
            set
            {
                this.decodedUpgradeMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string UpgradeMessage
        {
            get
            {
                return this.upgradeMessage;
            }
            set
            {
                this.upgradeMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBMessageCode UpgradeMessageCode { get; set; }

        public List<MOBUpgradePropertyKeyValue> UpgradeProperties { get; set; }

        public MOBUpgradeEligibilityStatus UpgradeStatus { get; set; }

        public MOBUpgradeType UpgradeType { get; set; }

        public List<MOBSegmentResponse> WaitlistSegments { get; set; }
    }
}
