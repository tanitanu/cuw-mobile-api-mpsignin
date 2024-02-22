using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using United.Mobile.DataAccess.Common;
using United.Mobile.Model.Common;
using United.Mobile.Model.Common.MP2015;

namespace United.Mobile.Model.Shopping
{


    [Serializable()]
    public class MOBSHOPReservation
    {
        [JsonIgnore]
        public string ObjectName { get; set; } = "United.Definition.Shopping.MOBSHOPReservation";
        private IConfiguration _configuration;
        private ICachingService _cachingService;

        public MOBSHOPReservation(IConfiguration configuration
            , ICachingService cachingService)
        {
            _configuration = configuration;
            _cachingService = cachingService;

            rewardPrograms = new List<RewardProgram>();
            var key = _configuration.GetValue<string>("FrequestFlyerRewardProgramListStaticGUID") + "Booking2.0FrequentFlyerList";
            var frequestFlyerList = _cachingService.GetCache<string>(key, "TID1").Result;
            rewardPrograms = JsonConvert.DeserializeObject<List<RewardProgram>>(frequestFlyerList);
            if (rewardPrograms == null || rewardPrograms.Count == 0)
            {
                rewardPrograms = new List<RewardProgram>();
                ConfigurationParameter.ConfigParameter parm = ConfigurationParameter.ConfigParameter.Configuration;
                if (parm != null && parm.RewardTypes != null)
                {
                    for (int i = 0; i < parm.RewardTypes.Count; i++)
                    {
                        RewardProgram p = new RewardProgram
                        {
                            ProgramID = parm.RewardTypes[i].ProductID,
                            Type = parm.RewardTypes[i].Type,
                            Description = parm.RewardTypes[i].Description
                        };
                        rewardPrograms.Add(p);
                    }
                }
            }
            tcdAdvisoryMessages = new List<MOBItem>();
            int tCDAdvisoryMessagesCount = Convert.ToInt32(_configuration.GetValue<string>("TCDAdvisoryMessagesCount"));
            for (int i = 1; i <= tCDAdvisoryMessagesCount; i++)
            {
                MOBItem item = new MOBItem
                {
                    Id = HttpUtility.HtmlDecode(_configuration.GetValue<string>("TCDAdvisoryMessages" + i.ToString()).ToString().Split('~')[0]),
                    CurrentValue = HttpUtility.HtmlDecode(_configuration.GetValue<string>("TCDAdvisoryMessages" + i.ToString()).ToString().Split('~')[1].ToString()),
                    SaveToPersist = Convert.ToBoolean(HttpUtility.HtmlDecode(_configuration.GetValue<string>("TCDAdvisoryMessages" + i.ToString()).ToString().Split('~')[2].ToString()))
                };
                tcdAdvisoryMessages.Add(item);
            }
            elfMessagesForRTI = new List<MOBItem>();
        }
        public MOBSHOPReservation()
        {

        }
        private string sessionId = string.Empty;
        private string recordLocator = string.Empty;
        private string searchType = string.Empty;
        private string cartId = string.Empty;
        private List<MOBSHOPPrice> prices;
        private MOBCPPhone reservationPhone;
        private MOBEmail reservationEmail;
        private string warning;
        private string seatMessage = string.Empty;
        private string travelOptionMessage = string.Empty;
        private List<RewardProgram> rewardPrograms;
        private List<MOBItem> tcdAdvisoryMessages;
        private string flightShareMessage = string.Empty;
        private string ineligibleToEarnCreditMessage = string.Empty;
        private string oaIneligibleToEarnCreditMessage = string.Empty;
        private string overMileageLimitMessage = "You can earn up to 75,000 award miles per ticket. The 75,000 award miles cap may be applied to your posted flight activity in an order different than shown.";
        private string overMileageLimitAmount = "75000";

        private bool isUpgradedFromEntryLevelFare;



        public void UpdateRewards(IConfiguration configuration
            , ICachingService cachingService)
        {
            _configuration = configuration;
            _cachingService = cachingService;

            rewardPrograms = new List<RewardProgram>();
            var key = _configuration.GetValue<string>("FrequestFlyerRewardProgramListStaticGUID") + "Booking2.0FrequentFlyerList";
            var frequestFlyerList = _cachingService.GetCache<string>(key, "TID1").Result;
            rewardPrograms = JsonConvert.DeserializeObject<List<RewardProgram>>(frequestFlyerList);
            if (rewardPrograms == null || rewardPrograms.Count == 0)
            {
                rewardPrograms = new List<RewardProgram>();
                ConfigurationParameter.ConfigParameter parm = ConfigurationParameter.ConfigParameter.Configuration;
                if (parm != null && parm.RewardTypes != null)
                {
                    for (int i = 0; i < parm.RewardTypes.Count; i++)
                    {
                        RewardProgram p = new RewardProgram
                        {
                            ProgramID = parm.RewardTypes[i].ProductID,
                            Type = parm.RewardTypes[i].Type,
                            Description = parm.RewardTypes[i].Description
                        };
                        rewardPrograms.Add(p);
                    }
                }
            }
            tcdAdvisoryMessages = new List<MOBItem>();
            int tCDAdvisoryMessagesCount = Convert.ToInt32(_configuration.GetValue<string>("TCDAdvisoryMessagesCount"));
            for (int i = 1; i <= tCDAdvisoryMessagesCount; i++)
            {
                MOBItem item = new MOBItem
                {
                    Id = _configuration.GetValue<string>("TCDAdvisoryMessages" + i.ToString()).ToString().Split('~')[0],
                    CurrentValue = _configuration.GetValue<string>("TCDAdvisoryMessages" + i.ToString()).ToString().Split('~')[1].ToString(),
                    SaveToPersist = Convert.ToBoolean(_configuration.GetValue<string>("TCDAdvisoryMessages" + i.ToString()).ToString().Split('~')[2].ToString())
                };
                tcdAdvisoryMessages.Add(item);
            }
        }

        public bool IsReshopChange { get; set; }

        public List<ReshopTrip> ReshopTrips { get; set; }

        public Reshop Reshop { get; set; }

        public MOBPayPal PayPal { get; set; }

        public MOBPayPalPayor PayPalPayor { get; set; }
        public MOBFormofPayment FormOfPaymentType { get; set; }

        public bool IsCubaTravel { get; set; }


        public CPCubaTravel CubaTravelInfo { get; set; }

        public string OverMileageLimitMessage
        {
            get => this.overMileageLimitMessage;
            set => this.overMileageLimitMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string OverMileageLimitAmount
        {
            get => this.overMileageLimitAmount;
            set => this.overMileageLimitAmount = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public List<MOBLMXTraveler> lmxtravelers { get; set; }

        public bool AwardTravel { get; set; } = false;

        private string pointOfSale = string.Empty;
        private string pkDispenserPublicKey = string.Empty;

        private bool isRefundable;
        public bool IsRefundable
        {
            get => this.isRefundable;
            set => this.isRefundable = value;
        }

        public bool ISInternational { get; set; }

        public bool ISFlexibleSegmentExist { get; set; }

        public string PointOfSale
        {
            get => this.pointOfSale;
            set => this.pointOfSale = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string SeatMessage
        {
            get => this.seatMessage;
            set => this.seatMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string TravelOptionMessage
        {
            get => this.travelOptionMessage;
            set => this.travelOptionMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public List<TravelOption> TravelOptions { get; set; }

        public ClubPassPurchaseRequest ClubPassPurchaseRequest { get; set; }

        public string SessionId
        {
            get => this.sessionId;
            set => this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }

        public string RecordLocator
        {
            get => this.recordLocator;
            set => this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }

        public string SearchType
        {
            get => this.searchType;
            set => this.searchType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }

        public string CartId
        {
            get => this.cartId;
            set => this.cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }

        public bool IsSignedInWithMP { get; set; }

        public List<MOBSHOPTrip> Trips { get; set; }

        public List<MOBSHOPPrice> Prices
        {
            get => this.prices;
            set
            {
                this.prices = value;
                UpdateWithUpliftPrice();
            }
        }
        private void UpdateWithUpliftPrice()
        {
            if (_configuration == null)
            {
                return;
            }

            if (!_configuration.GetValue<bool>("EnableUpliftPayment"))
            {
                return;
            }

            if (Prices == null || !Prices.Any())
            {
                return;
            }

            var upliftTotalPrice = Prices.FirstOrDefault(p => "TOTALPRICEFORUPLIFT".Equals(p.DisplayType, StringComparison.CurrentCultureIgnoreCase));
            if (upliftTotalPrice != null)
            {
                Prices.Remove(upliftTotalPrice);
            }

            Prices.Add(GetTotalPriceForUplift());
        }

        private MOBSHOPPrice GetTotalPriceForUplift()
        {
            var totalPrice = Prices.FirstOrDefault(p => "GRAND TOTAL".Equals(p.DisplayType, StringComparison.CurrentCultureIgnoreCase))?.Value ?? 0;
            if (totalPrice == 0)
            {
                totalPrice = Prices.FirstOrDefault(p => "TOTAL".Equals(p.DisplayType, StringComparison.CurrentCultureIgnoreCase))?.Value ?? 0;
            }
            var tripInsurancePrice = Prices.FirstOrDefault(p => "TRAVEL INSURANCE".Equals(p.DisplayType, StringComparison.CurrentCultureIgnoreCase))?.Value ?? 0;

            var totalPriceForUplift = totalPrice - tripInsurancePrice;
            return new MOBSHOPPrice
            {
                CurrencyCode = "USD",
                DisplayType = "TOTALPRICEFORUPLIFT",
                DisplayValue = string.Format("{0:#,0.00}", totalPriceForUplift),
                Value = totalPriceForUplift,
                FormattedDisplayValue = totalPriceForUplift.ToString("C2", new CultureInfo("en-us"))
            };
        }

        public List<MOBSHOPTax> Taxes { get; set; }

        public int NumberOfTravelers { get; set; }

        public List<MOBSHOPTraveler> Travelers { get; set; }

        public List<MOBCPTraveler> TravelersCSL { get; set; }

        public List<MOBSeatPrice> SeatPrices { get; set; }
        private List<MOBCreditCard> creditCards = new List<MOBCreditCard>();
        public List<MOBCreditCard> CreditCards
        {
            get => creditCards;
            set
            {
                if (value != null)
                {
                    creditCards = value;
                }
            }
        }

        public MOBCPPhone ReservationPhone
        {
            get => this.reservationPhone;
            set
            {
                if (value != null)
                {
                    this.reservationPhone = value;
                }
            }
        }

        public MOBEmail ReservationEmail
        {
            get => this.reservationEmail;
            set
            {
                if (value != null)
                {
                    this.reservationEmail = value;
                }
            }
        }
        //Commented in ReservationToShoppingCart_DataMigration
        //[JsonIgnore()]
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public MOBEmail ReservationEmail2
        {
            get => this.reservationEmail;
            set => this.reservationEmail = value;
        }

        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public List<MOBCreditCard> CreditCards2
        {
            get => creditCards;
            set => creditCards = value;
        }

        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public MOBCPPhone ReservationPhone2
        {
            get => this.reservationPhone;
            set => this.reservationPhone = value;
        }

        public string Warning
        {
            get => warning;
            set => warning = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public bool TravelersRegistered { get; set; }

        //62681-Added the tag to avoid duplicate values in Reward programs-11/21/2016-Alekhya
        [XmlIgnore()]
        public List<RewardProgram> RewardPrograms
        {
            get => this.rewardPrograms;
            set => this.rewardPrograms = value;
        }

        public List<MOBAddress> CreditCardsAddress { get; set; } = new List<MOBAddress>();
        public List<string> Messages { get; set; }

        public List<FareRules> FareRules { get; set; }

        public bool UnregisterFareLock { get; set; } = true;

        public FareLock FareLock { get; set; }

        public List<MOBItem> TCDAdvisoryMessages
        {
            get
            {
                if (_configuration != null)
                {
                    if (Convert.ToBoolean(_configuration.GetValue<string>("FixTCDAdvisoryMessageForIBE") ?? "false"))
                    {
                        if (this.IsELF || this.shopReservationInfo2 != null && (this.shopReservationInfo2.IsIBELite || this.ShopReservationInfo2.IsIBE || this.shopReservationInfo2.IsNonRefundableNonChangable))
                        {
                            MOBItem mobItem = this.tcdAdvisoryMessages.Find(p => p.Id == "PurchaseTnC");
                            if (mobItem != null && this.shopReservationInfo2.IsNonRefundableNonChangable)
                            {
                                string purchaseTnC = _configuration.GetValue<string>("TCDAdvisoryMessagesForNonRefundableNonChangable");
                                if (purchaseTnC != null)
                                {
                                    mobItem.CurrentValue = purchaseTnC;
                                }
                            }
                            else if (mobItem != null && _configuration.GetValue<string>("TCDAdvisoryMessagesForELF") != null)
                            {
                                string purchaseTnC = GetTCDAdMesgForBE();
                                if (purchaseTnC != null)
                                {
                                    mobItem.CurrentValue = purchaseTnC;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (this.IsELF || this.shopReservationInfo2 != null && this.shopReservationInfo2.IsIBELite)
                        {
                            MOBItem mobItem = this.tcdAdvisoryMessages.Find(p => p.Id == "PurchaseTnC");
                            if (mobItem != null && _configuration.GetValue<string>("TCDAdvisoryMessagesForELF") != null)
                            {
                                var purchaseTnC = this.IsELF
                                                    ? _configuration.GetValue<string>("TCDAdvisoryMessagesForELF")
                                                    : _configuration.GetValue<string>("TCDAdvisoryMessagesForIBELite");
                                if (purchaseTnC != null)
                                {
                                    mobItem.CurrentValue = purchaseTnC;
                                }
                            }
                        }
                    }
                }
                return this.tcdAdvisoryMessages;
            }
            set => this.tcdAdvisoryMessages = value;
        }

        private string GetTCDAdMesgForBE()
        {
            return !string.IsNullOrEmpty(_configuration.GetValue<string>("AddMissingTnCForBE")) && Convert.ToBoolean(_configuration.GetValue<string>("AddMissingTnCForBE")) && this.isRefundable
                    ? _configuration.GetValue<string>("TCDAdvisoryMessagesForBERefundable")
                    : this.IsELF
                        ? _configuration.GetValue<string>("TCDAdvisoryMessagesForELF")
                        : (this.ShopReservationInfo2.IsIBELite)
                            ? _configuration.GetValue<string>("TCDAdvisoryMessagesForIBELite")
                            : _configuration.GetValue<string>("TCDAdvisoryMessagesForIBE");
        }

        public string FlightShareMessage
        {
            get => this.flightShareMessage;
            set => this.flightShareMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
        public string PKDispenserPublicKey
        {
            get => this.pkDispenserPublicKey;
            set => this.pkDispenserPublicKey = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public List<LmxFlight> LMXFlights { get; set; }

        public string IneligibleToEarnCreditMessage
        {
            get => this.ineligibleToEarnCreditMessage;
            set => this.ineligibleToEarnCreditMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string OaIneligibleToEarnCreditMessage
        {
            get => this.oaIneligibleToEarnCreditMessage;
            set => this.oaIneligibleToEarnCreditMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public bool GetALLSavedTravelers { get; set; } = true;

        private TripPriceBreakDown shopPriceBreakDown;
        public TripPriceBreakDown ShopPriceBreakDown
        {
            get => this.shopPriceBreakDown;
            set => this.shopPriceBreakDown = value;

        }

        public SHOPPriceBreakDown PriceBreakDown { get; set; }


        public bool IsEmp20 { get; set; }

        public bool IsELF { get; set; }

        public bool IsMetaSearch { get; set; }

        public bool IsUpgradedFromEntryLevelFare
        {
            get => this.isUpgradedFromEntryLevelFare;
            set
            {
                this.isUpgradedFromEntryLevelFare = value;

                if (_configuration != null)
                {
                    if (!Convert.ToBoolean(_configuration.GetValue<string>("ByPassSetUpUpgradedFromELFMessages") ?? "false"))
                    {
                        SetUpUpgradedFromELFMessages();
                    }
                }
            }
        }
        private List<MOBItem> elfMessagesForRTI = new List<MOBItem>();
        [XmlIgnore()]
        public List<MOBItem> ELFMessagesForRTI
        {
            get => this.elfMessagesForRTI;
            set => this.elfMessagesForRTI = value;
        }

        [XmlIgnore()]
        public List<ItemWithIconName> ELFMessagesForVendorQuery { get; set; } = new List<ItemWithIconName>();


        public List<MOBTypeOption> FOPOptions { get; set; }

        private void SetUpUpgradedFromELFMessages()
        {
            if (!IsUpgradedFromEntryLevelFare)
            {
                return;
            }

            ELFMessagesForRTI = new List<MOBItem> {
                new MOBItem {
                    Id = "UpgradedFromElfTitle",
                    CurrentValue = _configuration.GetValue<string>("UpgradedFromElfTitle")
    },
                new MOBItem {
                    Id = "UpgradedFromElfText",
                    CurrentValue = _configuration.GetValue<string>("UpgradedFromElfText")
}
            };
        }

        public MasterpassSessionDetails MasterpassSessionDetails { get; set; }

        public MOBMasterpass Masterpass { get; set; }


        public string ElfUpgradeMessagesForMetaSearch { get; set; }

        public string MetaSessionId { get; set; }


        public bool IsSSA { get; set; }


        public List<MOBItem> SeatAssignmentMessage { get; set; }

        public United.Mobile.Model.Shopping.ReservationInfo ShopReservationInfo { get; set; }

        private ReservationInfo2 shopReservationInfo2;
        public ReservationInfo2 ShopReservationInfo2
        {
            get => shopReservationInfo2;
            set => shopReservationInfo2 = value;
        }
        public MOBTPIInfo TripInsuranceInfo { get; set; }

        [XmlElement("TripInsuranceInfoBookingPath")]
        public MOBTPIInfoInBookingPath TripInsuranceInfoBookingPath { get; set; }

        public List<Section> AlertMessages { get; set; }


        public bool IsRedirectToSecondaryPayment { get; set; } = false;

        public string CheckedbagChargebutton { get; set; }

        public bool IsBookingCommonFOPEnabled { get; set; }

        public bool IsReshopCommonFOPEnabled { get; set; }
        public bool IsPostBookingCommonFOPEnabled { get; set; }
        public MOBOnScreenAlert OnScreenAlert { get; set; }
    }
}
