using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using United.Mobile.Model.Common;
using United.Mobile.Model.Common.MP2015;

namespace United.Mobile.Model.Shopping
{
    [Serializable]
    public class Reservation : IPersist
    {
        #region IPersist Members

        public string ObjectName { get; set; } = "United.Persist.Definition.Shopping.Reservation";

        #endregion

        public string SessionId { get; set; }
        public string CartId { get; set; }
        public string MetaSessionId { get; set; }
        public bool IsSignedInWithMP { get; set; }
        public List<MOBSHOPTrip> Trips { get; set; }
        public List<Mobile.Model.Shopping.MOBSHOPPrice> Prices { get; set; }
        public List<Mobile.Model.Shopping.MOBSHOPTax> Taxes { get; set; }
        public List<string> TravelerKeys { get; set; }
        public SerializableDictionary<string, MOBSHOPTraveler> Travelers { get; set; }
        public SerializableDictionary<string, MOBCPTraveler> TravelersCSL { get; set; }
        public List<MOBSeatPrice> SeatPrices { get; set; }
        public List<MOBCreditCard> CreditCards { get; set; }
        public string CSLReservationJSONFormat { get; set; }
        public RegisterOfferRequest MOBSHOPRegisterOfferRequest { get; set; }
        public List<TravelOption> TravelOptions { get; set; }
        public ClubPassPurchaseRequest ClubPassPurchaseRequest { get; set; }
        public int NumberOfTravelers { get; set; }
        public bool TravelersRegistered { get; set; }
        public string SearchType = string.Empty;
        public List<MOBAddress> CreditCardsAddress { get; set; }
        public List<FareRules> FareRules { get; set; }

        public MOBCPPhone ReservationPhone { get; set; }
        public MOBEmail ReservationEmail { get; set; }

        public Boolean Override24HrFlex { get; set; }
        public string EPAMessageTitle { get; set; }
        public string EPAMessage { get; set; }

        public List<MOBLMXTraveler> LMXTravelers { get; set; }

        private string pointOfSale = string.Empty;
        public string PointOfSale
        {
            get => this.pointOfSale;
            set => this.pointOfSale = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public FareLock FareLock { get; set; }

        public bool UnregisterFareLock { get; set; }

        public bool AwardTravel { get; set; }

        public string FlightShareMessage = string.Empty;
        public string PKDispenserPublicKey = string.Empty;

        public List<LmxFlight> LMXFlights { get; set; }

        public List<MOBItem> TCDAdvisoryMessages { get; set; }

        public bool IsRefundable { get; set; }

        public bool ISInternational { get; set; }


        private string ineligibleToEarnCreditMessage = string.Empty;
        public string IneligibleToEarnCreditMessage
        {
            get => this.ineligibleToEarnCreditMessage;
            set => this.ineligibleToEarnCreditMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
        private string oaIneligibleToEarnCreditMessage = string.Empty;
        public string OaIneligibleToEarnCreditMessage
        {
            get => this.oaIneligibleToEarnCreditMessage;
            set => this.oaIneligibleToEarnCreditMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public bool ISFlexibleSegmentExist { get; set; }

        public bool GetALLSavedTravelers { get; set; }

        public bool IsELF { get; set; }

        public bool IsMetaSearch { get; set; }

        public string ElfUpgradeMessagesForMetaSearch { get; set; }

        public bool IsUpgradedFromEntryLevelFare { get; set; }


        public bool IsGetProfileCalledForProfileOwner { get; set; } = false;

        public bool IsCubaTravel { get; set; }

        public CPCubaTravel CubaTravelInfo { get; set; }

        public MOBFormofPayment FormOfPaymentType { get; set; }

        public MOBPayPal PayPal { get; set; }

        public MOBPayPalPayor PayPalPayor { get; set; }

        public List<MOBTypeOption> FOPOptions { get; set; }

        public List<MOBItem> ELFMessagesForRTI { get; set; }


        //public static implicit operator Service.Presentation.ReservationModel.Reservation(Reservation v)
        //{
        //    throw new NotImplementedException();
        //}

        public MasterpassSessionDetails MasterpassSessionDetails { get; set; }

        public MOBMasterpass Masterpass { get; set; }

        public bool IsReshopChange { get; set; }

        //[XmlElement("MOBSHOPReshopTrip")]
        [XmlArrayItem("MOBSHOPReshopTrip", typeof(ReshopTrip))]
        public List<ReshopTrip> ReshopTrips { get; set; }

        public List<ReshopTrip> ReshopUsedTrips { get; set; }

        public Reshop Reshop { get; set; }

        public bool IsSSA { get; set; }

        public List<MOBItem> SeatAssignmentMessage { get; set; }

        public United.Mobile.Model.Shopping.ReservationInfo ShopReservationInfo { get; set; }

        public int AboveGoldMembers { get; set; }

        public int GoldMembers { get; set; }

        public int SilverMembers { get; set; }

        public int NoOfFreeEplusWithSubscriptions { get; set; }

        [XmlElement("TripInsuranceFile")]
        public TripInsuranceFile TripInsuranceFile { get; set; }

        public United.Mobile.Model.Shopping.ReservationInfo2 ShopReservationInfo2 { get; set; }

        public List<Section> AlertMessages { get; set; }

        public bool IsRedirectToSecondaryPayment { get; set; } = false;

        private string recordLocator = string.Empty;
        public string RecordLocator
        {
            get => this.recordLocator;
            set => this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }

        public List<string> Messages { get; set; }

        public string CheckedbagChargebutton { get; set; }

        public bool IsBookingCommonFOPEnabled { get; set; }

        public bool IsReshopCommonFOPEnabled { get; set; }

        public bool IsPostBookingCommonFOPEnabled { get; set; }
    }

}
