
using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common
{
	[Serializable()]
	public  class MOBFOPTravelCredit
    {	 

		public List<string> EligibleTravelers { get; set; }


		public string Recipient { get; set; }


		public List<string> EligibleTravelerNameIndex { get; set; }


		public string Message { get; set; } = string.Empty;


		public MOBFOPTravelCredit TravelCreditType { get; set; }

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
	}
}