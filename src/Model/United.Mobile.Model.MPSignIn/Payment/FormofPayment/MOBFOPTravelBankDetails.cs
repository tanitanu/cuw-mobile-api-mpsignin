using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping.FormofPayment
{
    [Serializable()]
    public class MOBFOPTravelBankDetails
    {
        //ADD list of newly travelbank object as is like trvelcertificate

        public double TBAppliedByCustomer { get; set; }

        public List<MOBMobileCMSContentMessages> LearnmoreTermsandConditions { get; set; }

        public string PayorLastName { get; set; }


        public string PayorFirstName { get; set; }


        public string MPNumber { get; set; }

        public List<MOBMobileCMSContentMessages> ApplyTBContentMessage { get; set; }

        public List<MOBMobileCMSContentMessages> ReviewTBContentMessage { get; set; }


        public double TBBalance { get; set; }


        public string DisplayTBBalance { get; set; }


        public double TBApplied { get; set; }


        public string DisplaytbApplied { get; set; }


        public double RemainingBalance { get; set; }

        public string DisplayRemainingBalance { get; set; }

        private string displayAvailableBalanceAsOfDate;
        public string DisplayAvailableBalanceAsOfDate { get => displayAvailableBalanceAsOfDate; set => displayAvailableBalanceAsOfDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
    }
}
