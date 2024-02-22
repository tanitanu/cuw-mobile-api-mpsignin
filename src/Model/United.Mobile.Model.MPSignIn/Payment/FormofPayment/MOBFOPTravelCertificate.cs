using System;
using System.Collections.Generic;
using System.Globalization;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping.FormofPayment
{
    [Serializable()]
    public class MOBFOPTravelCertificate
    {
        private double totalRedeemAmount;
        private string displayTotalRedeemAmountText;

        public List<MOBMobileCMSContentMessages> SavedETCMessages { get; set; }

        public List<MOBMobileCMSContentMessages> ReviewETCMessages { get; set; }

        public double MaxAmountOfETCsAllowed { get; set; }

        public int MaxNumberOfETCsAllowed { get; set; }
        public double NotAllowedETCAmount { get; set; }


        public double AllowedETCAmount { get; set; }

        public List<MOBMobileCMSContentMessages> LearnmoreTermsandConditions { get; set; }


        public string DisplayTotalRedeemAmountText 
        {
            get
            {
                displayTotalRedeemAmountText = (TotalRedeemAmount).ToString("N2", CultureInfo.CurrentCulture);
                return displayTotalRedeemAmountText;
            }
        }

        public List<MOBFOPCertificate> Certificates { get; set; }

        public string CertificateButtonText { get; set; }

        public double TotalRedeemAmount
        {
            get
            {
                totalRedeemAmount = 0;
                if (Certificates != null && Certificates.Count > 0)
                {
                    foreach (var certificate in Certificates)
                    {
                        totalRedeemAmount += certificate.RedeemAmount;
                    }
                }
                return totalRedeemAmount;
            }
        }
        public Section RemoveAllCertificateAlertMessage { get; set; }
    }
}