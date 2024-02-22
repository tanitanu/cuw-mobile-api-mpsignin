using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Mobile.Model.ManagRes;

namespace United.Mobile.Model.PNRManagement
{
    [Serializable()]
    public class MOBPNR
    {
        [JsonIgnore()]
        public string ObjectName { get; set; } = "United.Definition.MOBPNR";
        private string sessionId = string.Empty;
        private string recordLocator = string.Empty;
        private string dateCreated = string.Empty;
        private string description = string.Empty;
        private string ticketType = string.Empty;
        private string numberOfPassengers = string.Empty;
        private string lastTripDateDepartureDate = string.Empty;
        private string lastTripDateArrivalDate = string.Empty;
        private string checkinEligible = "N";
        private string alreadyCheckedin = "false";
        private string notValid = "false";
        private string validforCheckin = "false";
        private string pnrCanceled = "false";
        private string uaRecordLocator = string.Empty;
        private string coRecordLocator = string.Empty;
        private string pnrOwner = string.Empty;
        private string oaRecordLocatorMessageTitle = string.Empty;
        private string oaRecordLocatorMessage = string.Empty;
        private string emailAddress = string.Empty;

        ////private bool isInCabinPetInReservation; //Kept Commented If need in future about More pet information

        ////private List<PetInformation> pets; //Kept Commented If need in future about More pet information

        private string upgradeMessage = string.Empty;
        private string farelockExpirationDate = string.Empty;
        private string farelockPurchaseMessage = string.Empty;
        private string earnedMilesHeader = string.Empty;
        private string earnedMilesText = string.Empty;
        private string ineligibleToEarnCreditMessage = string.Empty;
        private string oaIneligibleToEarnCreditMessage = string.Empty;

        private string overMileageLimitMessage = ConfigurationManager.AppSettings["lmxOverMileageLimitMessage"];
        private string overMileageLimitAmount = ConfigurationManager.AppSettings["lmxOverMileageLimitAmount"];

        private string journeyType;
        private string checkInStatus = string.Empty;

        public Boolean IsJSENonChangeableFare { get; set; }
        public Boolean Is24HrFlexibleBookingPolicy { get; set; }
        public Boolean IsMilesAndMoney { get; set; }
        public Boolean IsATREEligible { get; set; }
        public Boolean IsTicketedByUA { get; set; }
        public Boolean IsSCChangeEligible { get; set; }
        public Boolean IsSCRefundEligible { get; set; }
        public Boolean IsSCBulkGroupPWC { get; set; }
        public MOBScheduleChange ScheduleChangeInfo { get; set; }
        public MOBIRROPSChange IRROPSChangeInfo { get; set; }
        public List<MOBPNRAdvisory> AdvisoryInfo { get; set; }
        public Boolean ConsolidateScheduleChangeMessage { get; set; }
        public Boolean IsCorporateBooking { get; set; }
        public string CorporateVendorName { get; set; }
        public Boolean IsPolicyExceptionAlert { get; set; }
        public Boolean IsBEChangeEligible { get; set; }
        public Boolean IsCheckinEligible { get; set; }
        public Boolean IsAgencyBooking { get; set; }
        public string AgencyName { get; set; }
        public Boolean ShouldDisplayUpgradeCabin { get; set; }
        public List<MOBItem> EarnedMilesInfo { get; set; }
        public List<MOBShuttleOffer> ShuttleOfferInformation { get; set; }
        public MOBFutureFlightCredit FutureFlightCredit { get; set; }
        public MOBUmnr Umnr { get; set; }
        public List<MOBPNRPassenger> InfantInLaps { get; set; }
        public string MealAccommodationAdvisory { get; set; } = string.Empty;
        public string MealAccommodationAdvisoryHeader { get; set; } = string.Empty;
        public Boolean IsUnaccompaniedMinor { get; set; }
        public Boolean IsETicketed { get; set; }
        public MOBInCabinPet InCabinPetInfo { get; set; }
        public Boolean IsPetAvailable { get; set; }
        public Boolean ShouldDisplayEmailReceipt { get; set; }
        public Boolean IsCanceledWithFutureFlightCredit { get; set; }
        public Boolean ShowOverride24HrFlex { get; set; }
        public Boolean ShowOverrideATREEligible { get; set; }




        public bool IsInflightMealsOfferEligible { get; set; }


        public bool IsEnableEditTraveler { get; set; }

        public Boolean IsShuttleOfferEligible { get; set; }

        public string JourneyType
        {
            get { return journeyType; }
            set { journeyType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public List<MOBItem> ELFLimitations { get; set; }

        public bool IsElf { get; set; } = false;
        public List<MOBLMXTraveler> LMXTravelers { get; set; }

        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string RecordLocator
        {
            get
            {
                return this.recordLocator;
            }
            set
            {
                this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string UARecordLocator
        {
            get
            {
                return this.uaRecordLocator;
            }
            set
            {
                this.uaRecordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string CORecordLocator
        {
            get
            {
                return this.coRecordLocator;
            }
            set
            {
                this.coRecordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string PNROwner
        {
            get
            {
                return this.pnrOwner;
            }
            set
            {
                this.pnrOwner = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }


        public List<MOBOARecordLocator> OARecordLocators { get; set; } = new List<MOBOARecordLocator>();

        public string OARecordLocatorMessageTitle
        {
            get
            {
                return this.oaRecordLocatorMessageTitle;
            }
            set
            {
                this.oaRecordLocatorMessageTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OARecordLocatorMessage
        {
            get
            {
                return this.oaRecordLocatorMessage;
            }
            set
            {
                this.oaRecordLocatorMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DateCreated
        {
            get
            {
                return this.dateCreated;
            }
            set
            {
                this.dateCreated = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsActive { get; set; }

        public string TicketType
        {
            get
            {
                return this.ticketType;
            }
            set
            {
                this.ticketType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string NumberOfPassengers
        {
            get
            {
                return this.numberOfPassengers;
            }
            set
            {
                this.numberOfPassengers = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public List<MOBTrip> Trips { get; set; }

        public List<MOBPNRPassenger> Passengers { get; set; }

        public List<MOBPNRSegment> Segments { get; set; }

        public string CheckinEligible
        {
            get
            {
                return this.checkinEligible;
            }
            set
            {
                this.checkinEligible = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string LastTripDateDepartureDate
        {
            get
            {
                return this.lastTripDateDepartureDate;
            }
            set
            {
                this.lastTripDateDepartureDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LastTripDateArrivalDate
        {
            get
            {
                return this.lastTripDateArrivalDate;
            }
            set
            {
                this.lastTripDateArrivalDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string AlreadyCheckedin
        {
            get
            {
                return this.alreadyCheckedin;
            }
            set
            {
                this.alreadyCheckedin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string NotValid
        {
            get
            {
                return this.notValid;
            }
            set
            {
                this.notValid = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ValidforCheckin
        {
            get
            {
                return this.validforCheckin;
            }
            set
            {
                this.validforCheckin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PNRCanceled
        {
            get
            {
                return this.pnrCanceled;
            }
            set
            {
                this.pnrCanceled = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBSeatOffer SeatOffer { get; set; }

        public bool IsEligibleToSeatChange { get; set; } = false;

        ////public bool IsInCabinPetInReservation
        ////{
        ////    get
        ////    {
        ////        return this.isInCabinPetInReservation;
        ////    }
        ////    set
        ////    {
        ////        this.isInCabinPetInReservation = value;
        ////    }
        ////}

        ////public List<PetInformation> Pets
        ////{
        ////    get
        ////    {
        ////        return this.pets;
        ////    }
        ////    set
        ////    {
        ////        pets = value;
        ////    }
        ////}

        public List<string> PetRecordLocators { get; set; }

        public string EmailAddress
        {
            get
            {
                return this.emailAddress;
            }
            set
            {
                this.emailAddress = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public MOBBundleInfo BundleInfo { get; set; }

        public string FarelockExpirationDate
        {
            get
            {
                return this.farelockExpirationDate;
            }
            set
            {
                this.farelockExpirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FarelockPurchaseMessage
        {
            get
            {
                return this.farelockPurchaseMessage;
            }
            set
            {
                this.farelockPurchaseMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EarnedMilesHeader
        {
            get
            {
                return this.earnedMilesHeader;
            }
            set
            {
                this.earnedMilesHeader = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EarnedMilesText
        {
            get
            {
                return this.earnedMilesText;
            }
            set
            {
                this.earnedMilesText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IneligibleToEarnCreditMessage
        {
            get
            {
                return this.ineligibleToEarnCreditMessage;
            }
            set
            {
                this.ineligibleToEarnCreditMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OaIneligibleToEarnCreditMessage
        {
            get
            {
                return this.oaIneligibleToEarnCreditMessage;
            }
            set
            {
                this.oaIneligibleToEarnCreditMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool AwardTravel { get; set; }

        public bool PsSaTravel { get; set; }

        public bool SupressLMX { get; set; } = false;

        public string OverMileageLimitMessage
        {
            get
            {
                return this.overMileageLimitMessage;
            }
            set
            {
                this.overMileageLimitMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OverMileageLimitAmount
        {
            get
            {
                return this.overMileageLimitAmount;
            }
            set
            {
                this.overMileageLimitAmount = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        private string syncedWithConcur = string.Empty;
        public string SyncedWithConcur
        {
            get
            {
                return this.syncedWithConcur;
            }
            set
            {
                this.syncedWithConcur = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TripType { get; set; } = string.Empty;

        public bool IsTPIIncluded { get; set; } = false;

        public bool IsFareLockOrNRSA { get; set; } = false;

        public bool HasCheckedBags { get; set; }


        public List<MOBItem> URLItems { get; set; } = new List<MOBItem>();



        public List<MOBItem> EditTravelerInfo { get; set; }

        public string EncryptPNR { get; set; }

        public string EncryptLastName { get; set; }

        public bool IsGroup { get; set; } = false;

        public bool IsBulk { get; set; } = false;
        public bool IsIBELite { get; set; }

        public bool IsIBE { get; set; }

        public bool IsCBE { get; set; }

        public bool HasScheduleChanged { get; set; }
        public List<MOBItem> StatusMessageItems { get; set; }


        public string ProductCode { get; set; }
        public string ProductCategory { get; set; }

        public string MarketType { get; set; }

        public string CheckInStatus
        {
            get
            {
                return this.checkInStatus;
            }
            set
            {
                this.checkInStatus = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool GetCheckInStatusFromCSLPNRRetrivalService { get; set; } // This Flag to check the CheckInEligibility Check already done at CSL PNR Retrival Service (as first phase the CSL PNR Team implemented at CSL PNR Retrival by RecordLocattor second phase they do at GetPNRByMP)

        public bool IrrOps { get; set; }

        public bool IrrOpsViewed { get; set; }

        private string fareLockMessage = string.Empty;
        private string fareLockPurchaseButton = string.Empty;
        private string fareLockPriceButton = string.Empty;

        public string FareLockMessage
        {
            get
            {
                return this.fareLockMessage;
            }
            set
            {
                this.fareLockMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FareLockPurchaseButton
        {
            get
            {
                return this.fareLockPurchaseButton;
            }
            set
            {
                this.fareLockPurchaseButton = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string FareLockPriceButton
        {
            get
            {
                return this.fareLockPriceButton;
            }
            set
            {
                this.fareLockPriceButton = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string AssociateMPId { get; set; } = "false";

        public string AssociateMPIdSharesPosition { get; set; } = string.Empty;
        public string AssociateMPIdSharesGivenName { get; set; } = string.Empty;
        public string AssociateMPIdMessage { get; set; } = string.Empty;

        public List<MOBReservationPrice> Prices { get; set; }

        public MOBOneClickEnrollmentEligibility OneClickEnrollmentEligibility { get; set; }

    }
}
