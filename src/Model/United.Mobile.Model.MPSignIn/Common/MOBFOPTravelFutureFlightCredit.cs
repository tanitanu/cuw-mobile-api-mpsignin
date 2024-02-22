using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;
using United.Mobile.Model.MPSignIn;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBFOPTravelFutureFlightCredit
    {
        private double totalRedeemAmount;
        private string displayTotalRedeemAmountText;


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


        public string DisplayTotalRedeemAmountText
        {
            get
            {
                displayTotalRedeemAmountText = (TotalRedeemAmount).ToString("N2", CultureInfo.CurrentCulture);
                return displayTotalRedeemAmountText;
            }
        }

        //public List<MOBFOPFutureFlightCredit> FutureFlightCredits
        //{
        //    get { return futureFlightCredits; }
        //    set { futureFlightCredits = value; }
        //}

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

    }
}
