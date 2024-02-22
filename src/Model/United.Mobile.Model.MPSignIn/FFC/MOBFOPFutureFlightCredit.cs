using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBFOPFutureFlightCredit
    {

        [JsonIgnore]
        public bool IsEligibleToRedeem { get; set; }

        [JsonIgnore]
        public bool IsNameMatch { get; set; }

        [JsonIgnore]
        public bool IsNameMatchWaiverApplied { get; set; }

        [JsonIgnore]
        public bool IsTravelDateBeginsBeforeCertExpiry { get; set; }

        public string PromoCode { get; set; }

        public int PaxId { get; set; }

        [JsonProperty(PropertyName = "creditAmmount")]
        [JsonPropertyName("creditAmmount")]
        public string CreditAmount { get; set; }

        public string RecordLocator { get; set; }

        public string ExpiryDate { get; set; }

        public bool IsRemove { get; set; } = false;

        public bool IsCertificateApplied { get; set; }

        public int Index { get; set; }

        public double NewValueAfterRedeem { get; set; }


        public double RedeemAmount { get; set; }

        public string DisplayRedeemAmount { get; set; }

        public string DisplayNewValueAfterRedeem { get; set; }

        public double CurrentValue { get; set; }

        public double InitialValue { get; set; }


        public string PinCode { get; set; }

        public string YearIssued { get; set; }

        public string RecipientsLastName { get; set; }

        public string RecipientsFirstName { get; set; }
        public string TravelerNameIndex { get; set; }

    }
}
