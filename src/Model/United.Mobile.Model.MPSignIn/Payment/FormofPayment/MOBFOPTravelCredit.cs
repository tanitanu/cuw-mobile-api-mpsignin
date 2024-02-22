using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping.FormofPayment
{
	[Serializable()]
	public  class MOBFOPTravelCredit
    {

		public bool IsHideShowDetails { get; set; }
		public bool IsOTFFFC { get; set; }


		public int PaxId { get; set; }

		public string TravelerNameIndex { get; set; }
		public bool IsEligibleToRedeem { get; set; }
		public bool IsNameMatch { get; set; }
		public bool IsNameMatchWaiverApplied { get; set; }
		public bool IsTravelDateBeginsBeforeCertExpiry { get; set; }
		public List<MOBTypeOption> Captions { get; set; }

		public string LastName { get; set; }

		public string FirstName { get; set; }

		public List<string> EligibleTravelers { get; set; }


		public string Recipient { get; set; }


		public List<string> EligibleTravelerNameIndex { get; set; }


		public string Message { get; set; } = string.Empty;


		public MOBTravelCreditFOP TravelCreditType { get; set; }

		public string PromoCode { get; set; }

		public string CreditAmount { get; set; }

		public string RecordLocator { get; set; }

		public string ExpiryDate { get; set; }

		public bool IsRemove { get; set; } = false;

		public bool IsApplied { get; set; }

		public double NewValueAfterRedeem { get; set; }

		public double RedeemAmount { get; set; }

		public string DisplayRedeemAmount { get; set; }

		public string DisplayNewValueAfterRedeem { get; set; }

		public double CurrentValue { get; set; }

		public double InitialValue { get; set; }


		public string PinCode { get; set; }

		public string YearIssued { get; set; }

		public bool IsLookupCredit { get; set; }

		public bool IsAwardOTFEligible { get; set; }

		[Serializable]
		public enum MOBTravelCreditFOP
		{
			[EnumMember(Value = "ETC")]
			ETC,
			[EnumMember(Value = "FFC")]
			FFC,
			[EnumMember(Value = "FFCR")]
			FFCR,
		}
	}
}