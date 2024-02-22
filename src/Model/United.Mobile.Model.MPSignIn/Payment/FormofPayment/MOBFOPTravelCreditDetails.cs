using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping.FormofPayment
{
    [Serializable()]
    public class MOBFOPTravelCreditDetails
    {

        private double totalRedeemAmount;
        // private MOBSection findETCConfirmationMessage;

        public string NameWaiverMatchMessage { get; set; }

        public List<MOBMobileCMSContentMessages> LookUpMessages { get; set; }


        public List<MOBMobileCMSContentMessages> AlertMessages { get; set; }

        public List<MOBMobileCMSContentMessages> ReviewMessages { get; set; }

        //public MOBSection FindETCConfirmationMessage
        //{
        //    get { return findETCConfirmationMessage; }
        //    set { findETCConfirmationMessage = value; }
        //}
        public List<MOBMobileCMSContentMessages> ReviewTravelCreditMessages { get; set; }
        public string TravelCreditSummary { get; set; }
        private List<MOBFOPTravelCredit> travelCredits;
        public List<MOBFOPTravelCredit> TravelCredits
        {
            get { return travelCredits; }
            set { travelCredits = value; }
        }

        public double TotalRedeemAmount
        {
            get
            {
                totalRedeemAmount = 0;
                if (travelCredits != null && travelCredits.Count > 0)
                {
                    foreach (var travelCredit in travelCredits)
                    {
                        if (travelCredit.IsApplied)
                            totalRedeemAmount += travelCredit.RedeemAmount;
                    }
                }
                return totalRedeemAmount;
            }
        }
    }
}
