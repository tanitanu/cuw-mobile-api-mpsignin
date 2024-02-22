using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;
using United.Mobile.Model.Common.MP2015;
using MOBScheduleChange = United.Mobile.Model.ManagRes.MOBScheduleChange;

namespace United.Mobile.Model.PNRManagement
{
    [Serializable()]
    public class MOBPNRSegment : MOBSegment
    {
        private string meal = string.Empty;
        private string flightTime = string.Empty;
        private string groundTime = string.Empty;
        private string totalTravelTime = string.Empty;
        private string actualMileage = string.Empty;
        private string mileagePlusMileage = string.Empty;
        private string emp = string.Empty;
        private string totalMileagePlusMileage = string.Empty;
        private string classOfService = string.Empty;
        private string classOfServiceDescription = string.Empty;
        private string numberOfStops = string.Empty;
        private string scheduledDepartureDateTimeGMT = string.Empty;
        private string scheduledArrivalDateTimeGMT = string.Empty;
        private string codeshareFlightNumber = string.Empty;
        private string upgradeableMessageCode = string.Empty;
        private string upgradeableMessage = string.Empty;
        private string upgradeableRemark = string.Empty;
        private string complimentaryInstantUpgradeMessage = string.Empty;

        private string actionCode = string.Empty;

        private string waitedCOSDesc = string.Empty;

        private string cabinType = string.Empty;


        public bool IsCheckedIn { get; set; }
        public bool IsCheckInEligible { get; set; }
        public bool IsAllPaxCheckedIn { get; set; }
        public List<MOBScheduleChange> ScheduleChangeInfo { get; set; }
        public bool IsChangeOfGuage { get; set; }

        public bool ShowSeatMapLink { get; set; }

        public string NoProtection { get; set; }

        public MOBInCabinPet InCabinPetInfo { get; set; }

        public string SSRMeals { get; set; } = string.Empty;
        public string TicketCouponStatus { get; set; }

        public string TripNumber { get; set; }
        public int SegmentNumber { get; set; }


        public MOBPNRSegment()
            : base()
        {
        }

        public bool IsPlusPointSegment { get; set; }
        public bool HasPreviousSegmentDetails { get; set; }

        public bool IsElf { get; set; }

        public bool IsIBE { get; set; }

        public List<MOBBundle> Bundles { get; set; }

        public MOBAirline OperationoperatingCarrier { get; set; }

        public MOBAircraft Aircraft { get; set; }

        public string Meal
        {
            get
            {
                return this.meal;
            }
            set
            {
                this.meal = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FlightTime
        {
            get
            {
                return this.flightTime;
            }
            set
            {
                this.flightTime = string.IsNullOrEmpty(value) ? string.Empty : value.ToUpper();
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
                this.groundTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
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
                this.totalTravelTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ActualMileage
        {
            get
            {
                return this.actualMileage;
            }
            set
            {
                this.actualMileage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MileagePlusMileage
        {
            get
            {
                return this.mileagePlusMileage;
            }
            set
            {
                this.mileagePlusMileage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EMP
        {
            get
            {
                return this.emp;
            }
            set
            {
                this.emp = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TotalMileagePlusMileage
        {
            get
            {
                return this.totalMileagePlusMileage;
            }
            set
            {
                this.totalMileagePlusMileage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
                this.classOfService = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ClassOfServiceDescription
        {
            get
            {
                return this.classOfServiceDescription;
            }
            set
            {
                this.classOfServiceDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBPNRSeat> Seats { get; set; } = new List<MOBPNRSeat>();

        public string NumberOfStops
        {
            get
            {
                return this.numberOfStops;
            }
            set
            {
                this.numberOfStops = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBPNRSegment> Stops { get; set; } = new List<MOBPNRSegment>();

        public string ScheduledDepartureDateTimeGMT
        {
            get
            {
                return this.scheduledDepartureDateTimeGMT;
            }
            set
            {
                this.scheduledDepartureDateTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ScheduledArrivalDateTimeGMT
        {
            get
            {
                return this.scheduledArrivalDateTimeGMT;
            }
            set
            {
                this.scheduledArrivalDateTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBAirline CodeshareCarrier { get; set; } = new MOBAirline();

        public string CodeshareFlightNumber
        {
            get
            {
                return this.codeshareFlightNumber;
            }
            set
            {
                this.codeshareFlightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBSegmentResponse UpgradeVisibility { get; set; }

        public bool Upgradeable { get; set; }

        public string UpgradeableMessageCode
        {
            get
            {
                return this.upgradeableMessageCode;
            }
            set
            {
                this.upgradeableMessageCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string UpgradeableMessage
        {
            get
            {
                return this.upgradeableMessage;
            }
            set
            {
                this.upgradeableMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ComplimentaryInstantUpgradeMessage
        {
            get
            {
                return this.complimentaryInstantUpgradeMessage;
            }
            set
            {
                this.complimentaryInstantUpgradeMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool Remove { get; set; }

        public bool Waitlisted { get; set; }

        public int LowestEliteLevel { get; set; }

        public string UpgradeableRemark
        {
            get
            {
                return this.upgradeableRemark;
            }
            set
            {
                this.upgradeableRemark = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ActionCode
        {
            get
            {
                return this.actionCode;
            }
            set
            {
                this.actionCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public bool UpgradeEligible { get; set; }

        public string WaitedCOSDesc
        {
            get
            {
                return this.waitedCOSDesc;
            }
            set
            {
                this.waitedCOSDesc = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBLmxProduct> LmxProducts { get; set; }

        public string CabinType
        {
            get
            {
                return this.cabinType;
            }
            set
            {
                this.cabinType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool NonPartnerFlight { get; set; } = false;

        public string ProductCode { get; set; }

        public string FareBasisCode { get; set; }
    }
}
