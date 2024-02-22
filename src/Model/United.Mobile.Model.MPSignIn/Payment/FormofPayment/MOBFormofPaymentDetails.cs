using Newtonsoft.Json;
using System;
using System.Xml.Serialization;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping.FormofPayment
{
    [Serializable()]
    public class MOBFormofPaymentDetails
    {
        public MOBFOPTravelBankDetails TravelBankDetails { get; set; }

        public FOPMoneyPlusMilesCredit MoneyPlusMilesCredit { get; set; }
        public FOPTravelFutureFlightCredit TravelFutureFlightCredit { get; set; }

        public bool IsOtherFOPRequired { get; set; }

        [XmlElement("TravelCertificate")]
        public MOBFOPTravelCertificate TravelCertificate { get; set; }


        public MOBApplePay ApplePayInfo { get; set; }
        public string EmailAddress { get; set; }

        public MOBAddress BillingAddress { get; set; }
        public MOBCPPhone Phone { get; set; }

        public string FormOfPaymentType { get; set; } = string.Empty;
        public MOBCreditCard CreditCard { get; set; }
        public MOBCreditCard SecondaryCreditCard { get; set; }
        public bool ClientCardType { get; set; } = false;
        public MOBPayPal PayPal { get; set; }
        public MOBPayPalPayor PayPalPayor { get; set; }
        public MasterpassSessionDetails MasterPassSessionDetails { get; set; }
        public MOBMasterpass masterPass { get; set; }
        public MOBEmail Email { get; set; }
        public MilesFOP MilesFOP { get; set; }

        public MOBCreditCard Uplift { get; set; }
        public MOBInternationalBilling InternationalBilling { get; set; }
        public MOBFOPTravelCreditDetails TravelCreditDetails { get; set; }//chNGE Nmw

    }
}
