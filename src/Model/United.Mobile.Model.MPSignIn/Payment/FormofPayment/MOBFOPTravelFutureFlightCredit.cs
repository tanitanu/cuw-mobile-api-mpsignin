using System;
using System.Collections.Generic;
using System.Globalization;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping.FormofPayment
{
    [Serializable()]
    public class FOPTravelFutureFlightCredit
    {
        private double totalRedeemAmount;
        private List<MOBFOPFutureFlightCredit> futureFlightCredits;

        public MOBSection GoToTripDetailDialog { get; set; }
        public List<MOBMobileCMSContentMessages> EmailConfirmationFFCMessages { get; set; }
        public List<MOBFOPFutureFlightCredit> FutureFlightCredits
        {
            get { return futureFlightCredits; }
            set { futureFlightCredits = value; }
        }


        public MOBSection FindFFCConfirmationMessage { get; set; }


        public List<MOBMobileCMSContentMessages> FindFFCMessages { get; set; }

        //private MOBSection removeAllCertificateAlertMessage;

        public List<MOBMobileCMSContentMessages> LookUpFFCMessages { get; set; }

        public List<MOBMobileCMSContentMessages> ReviewFFCMessages { get; set; }

        public List<MOBMobileCMSContentMessages> LearnmoreTermsandConditions { get; set; }


        public string DisplayTotalRedeemAmountText { get; set; }

        public string FFCButtonText { get; set; }

        public double TotalRedeemAmount
        {
            get
            {
                totalRedeemAmount = 0;
                if (futureFlightCredits != null && futureFlightCredits.Count > 0)
                {
                    foreach (var certificate in futureFlightCredits)
                    {
                        totalRedeemAmount += certificate.RedeemAmount;
                    }
                }
                return totalRedeemAmount;
            }
        }
        //public MOBSection RemoveAllCertificateAlertMessage
        //{
        //    get { return removeAllCertificateAlertMessage; }
        //    set { removeAllCertificateAlertMessage = value; }
        //}
        public double AllowedFFCAmount { get; set; }

        public double totalAllowedAncillaryAmount { get; set; }

    }
}
