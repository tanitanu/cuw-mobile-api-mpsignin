using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using United.Mobile.Model.Common;
using United.Mobile.Model.FormofPayment;
using United.Mobile.Model.Shopping.FormofPayment;
using United.Service.Presentation.CommonEnumModel;
using United.Utility.Enum;

namespace United.Mobile.Model.Shopping
{
    [Serializable()]
    public class MOBShoppingCart
    {
        [JsonIgnore()]
        public string ObjectName { get; set; } = "United.Definition.MOBShoppingCart";
        public bool IsHidePaymentMethod { get; set; }

        public MOBSHOPInflightContactlessPaymentEligibility InFlightContactlessPaymentEligibility { get; set; }

        public MOBPromoCodeDetails PromoCodeDetails { get; set; }

        public List<MOBMobileCMSContentMessages> ConfirmationPageAlertMessages { get; set; } = new List<MOBMobileCMSContentMessages>();

        public TripShare TripShare { get; set; }

        public List<MOBFOPCertificate> ProfileTravelerCertificates { get; set; } = new List<MOBFOPCertificate>();

        public List<MOBFOPCertificateTraveler> CertificateTravelers { get; set; }

        public string BundleCartId { get; set; }

        public string PartialCouponEligibleUrl { get; set; }

        public string PartialCouponEligibleMessage { get; set; }

        public bool IsCouponEligibleProduct { get; set; }

        public MOBCPTraveler Travelers { get; set; }

        public bool IsCouponApplied { get; set; }

        public bool DisableCouponEditOption { get; set; }

        public string CouponCode { get; set; }

        public string CouponOfferDescription { get; set; }

        public string CartId { get; set; } = string.Empty;

        public string Flow { get; set; } = string.Empty;

        [XmlArrayItem("MOBProdDetail")]
        public List<ProdDetail> Products { get; set; }

        public List<UpgradeOption> UpgradeCabinProducts { get; set; } = new List<UpgradeOption>();

        public string TotalPoints { get; set; } = string.Empty;

        public string DisplayTotalPoints { get; set; } = string.Empty;

        public string TotalPrice { get; set; } = string.Empty;

        public string DisplayTotalPrice { get; set; } = string.Empty;

        public string DisplaySubTotalPrice { get; set; } = string.Empty;

        public string DisplayTaxesAndFees { get; set; } = string.Empty;

        public string TotalMiles { get; set; }

        public MOBFormofPaymentDetails FormofPaymentDetails { get; set; }

        public List<MOBCPTraveler> SCTravelers { get; set; }

        public string PointofSale { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;

        public List<Section> AlertMessages { get; set; } = new List<Section>();

        private List<MOBMobileCMSContentMessages> termsAndConditions;

        public List<MOBMobileCMSContentMessages> TermsAndConditions
        {
            get => termsAndConditions;
            set
            {
                termsAndConditions = value;
                PopulateTermsAndConditionsForOldClient(termsAndConditions);
            }
        }
        public string PaymentTarget { get; set; }

        private void PopulateTermsAndConditionsForOldClient(List<MOBMobileCMSContentMessages> termsAndConditions)
        {
            if (termsAndConditions != null && termsAndConditions.Any() &&
                Products != null && Products.Any() &&
                Products.FirstOrDefault() != null)
            {
                Products.FirstOrDefault().TermsAndCondition = termsAndConditions.FirstOrDefault();
            }
        }
        public List<MOBSHOPTrip> Trips { get; set; } = new List<MOBSHOPTrip>();

        public List<MOBSHOPPrice> Prices { get; set; }

        public List<List<MOBSHOPTax>> Taxes { get; set; } = new List<List<MOBSHOPTax>>();

        public List<MOBItem> Captions { get; set; }

        public List<MOBItem> ELFLimitations { get; set; } = new List<MOBItem>();

        public string DisplayTotalMiles { get; set; }

        public string FlightShareMessage { get; set; } = string.Empty;

        public List<Section> PaymentAlerts { get; set; } = new List<Section>();

        public List<Section> DisplayMessage { get; set; }

        public bool IsMultipleTravelerEtcFeatureClientToggleEnabled { get; set; }

        //[XmlArray("MOBULTripInfo")]
        public ULTripInfo TripInfoForUplift { get; set; }

        public int CslWorkFlowType { get; set; }
        #region HandleCouponWhenFopIsUplift
        private void HandleCouponWhenFopIsUplift()
        {
            if (this.Flow != FlowType.VIEWRES.ToString())
            {
                return;
            }

            UpdateIsCouponEligibleProduct();
            UpdateDisableCouponEditOption();
        }

        private void UpdateDisableCouponEditOption()
        {
            // this.DisableCouponEditOption = this.FormofPaymentDetails?.FormOfPaymentType?.ToUpper() == FormofPayment.Uplift.ToString().ToUpper() && !string.IsNullOrEmpty(this.CouponCode);
        }

        private void UpdateIsCouponEligibleProduct()
        {

            //this.IsCouponEligibleProduct = this.FormofPaymentDetails?.FormOfPaymentType?.ToUpper() == FormofPayment.Uplift.ToString().ToUpper() && string.IsNullOrEmpty(this.CouponCode)
            //? false
            //: this.Products?.Any(p => ConfigurationManager.AppSettings["IsCouponEligibleProduct"]?.Split('|')?.ToList().Contains(p?.Code) ?? false) ?? false;
        }
        #endregion

        public Cart OmniCart { get; set; }

        public MOBShoppingCart()
        {
            Products = new List<ProdDetail>();
            TermsAndConditions = new List<MOBMobileCMSContentMessages>();
            Prices = new List<MOBSHOPPrice>();
            Captions = new List<MOBItem>();
            DisplayMessage = new List<Section>();
            CertificateTravelers = new List<MOBFOPCertificateTraveler>();
            SCTravelers = new List<MOBCPTraveler>();
        }
    }
    [Serializable()]
    public class ProdDetail
    {
        public List<CouponDetails> CouponDetails { get; set; }

        public string ProdDescription { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string ProdTotalPrice { get; set; } = string.Empty;

        public string ProdDisplayTotalPrice { get; set; } = string.Empty;
        public string ProdOtherPrice { get; set; } = string.Empty;

        public string ProdDisplayOtherPrice { get; set; } = string.Empty;

        public string ProdDisplaySubTotal { get; set; } = string.Empty;

        public string ProdDisplayTaxesAndFees { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "termAndCondition")]
        public MOBMobileCMSContentMessages TermsAndCondition { get; set; }

        [XmlArrayItem("MOBProductSegmentDetail")]
        public List<ProductSegmentDetail> Segments { get; set; }

        public Int32 ProdTotalMiles { get; set; }
        public string ProdDisplayTotalMiles { get; set; }

        public Int32 ProdTotalPoints { get; set; }

        public string ProdDisplayTotalPoints { get; set; }
        public List<MOBTypeOption> LineItems { get; set; }
        public string ProdOriginalPrice { get; set; }
        public ProdDetail()
        {
            CouponDetails = new List<CouponDetails>();
            LineItems = new List<MOBTypeOption>();
            Segments = new List<ProductSegmentDetail>();
        }

    }

    [Serializable()]
    public class ProductSegmentDetail
    {
        public string SegmentInfo { get; set; }

        public string ProductId { get; set; } = string.Empty;

        public string TripId { get; set; } = string.Empty;

        public string SegmentId { get; set; } = string.Empty;
        public List<string> ProductIds { get; set; }

        [XmlArrayItem("MOBProductSubSegmentDetail")]
        public List<ProductSubSegmentDetail> SubSegmentDetails { get; set; }
        public ProductSegmentDetail()
        {
            SubSegmentDetails = new List<ProductSubSegmentDetail>();
        }
    }
    [Serializable()]
    public class ProductSubSegmentDetail
    {
        //Required for strike off price to identify the flight segments uniquely

        public string Passenger { get; set; } = string.Empty;

        public string Price { get; set; } = string.Empty;

        public string DisplayPrice { get; set; } = string.Empty;

        public string SegmentDescription { get; set; } = string.Empty;

        public bool IsPurchaseFailure { get; set; }

        public string StrikeOffPrice { get; set; } = string.Empty;

        public string DisplayStrikeOffPrice { get; set; } = string.Empty;

        public string SeatCode { get; set; } = string.Empty;

        public string FlightNumber { get; set; } = string.Empty;

        public DateTime DepartureTime { get; set; }

        public DateTime ArrivalTime { get; set; }

        public Int32 Miles { get; set; }

        public string DisplayMiles { get; set; }

        public Int32 StrikeOffMiles { get; set; }

        public string DisplayStrikeOffMiles { get; set; } = string.Empty;


        public string OrginalPrice { get; set; }

        public MOBPromoCode PromoDetails { get; set; }

        public string ProductDescription { get; set; }

        public List<string> ProdDetailDescription { get; set; }


        public string DisplayOriginalPrice { get; set; }

        public string SegmentInfo { get; set; }


        public List<MOBPaxDetails> PaxDetails { get; set; }


    }
    [Serializable()]
    public class SCProductContext
    {
        public string Description { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
    [Serializable()]
    public class CouponDetails
    {
        public string Description { get; set; } = string.Empty;

        public string Product { get; set; } = string.Empty;

        public string PromoCode { get; set; } = string.Empty;

        public string IsCouponEligible { get; set; } = string.Empty;

        public CouponDiscountType DiscountType { get; set; }

    }
    [Serializable()]
    public class Cart
    {
        public int CartItemsCount { get; set; }

        public MOBItem PayLaterPrice { get; set; }


        public string CostBreakdownFareHeader { get; set; } = "Fare";
        public MOBItem TotalPrice { get; set; }


        public string NavigateToScreen { get; set; }

        public bool IsCallRegisterOffers { get; set; }

        public List<MOBOmniCartRepricingInfo> OmniCartPricingInfos { get; set; }

        public int NavigateToSeatmapSegmentNumber { get; set; }
        public bool IsUpliftEligible { get; set; }

        public List<MOBSection> FOPDetails { get; set; }
        public MOBSection AdditionalMileDetail { get; set; }
    }

    [Serializable()]
    public class MOBOmniCartRepricingInfo
    {
        public string Product { get; set; }

        public MOBSection RepriceAlertMessage { get; set; }

        public List<MOBOmniCartRepricingSegmentInfo> Segments { get; set; }


    }

    [Serializable()]
    public class MOBOmniCartRepricingSegmentInfo
    {

        public int SegmentNumber { get; set; }

    }

    [Serializable()]
    public class MOBCart
    {

        public int CartItemsCount { get; set; }

        public MOBItem PayLaterPrice { get; set; }


        public string CostBreakdownFareHeader { get; set; } = "Fare";


        public MOBItem TotalPrice { get; set; }

        public string NavigateToScreen { get; set; }
        public bool IsCallRegisterOffers { get; set; }

        public List<MOBOmniCartRepricingInfo> OmniCartPricingInfos { get; set; }

        public int NavigateToSeatmapSegmentNumber { get; set; }
    }

    [Serializable()]
    public class MOBPaxDetails
    {

        public string FullName { get; set; }

        public string Seat { get; set; }

        public string Key { get; set; }
    }
}

