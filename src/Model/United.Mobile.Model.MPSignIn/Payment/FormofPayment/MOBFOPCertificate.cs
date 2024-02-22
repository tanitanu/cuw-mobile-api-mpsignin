using System;
using United.Mobile.Model.FormofPayment;

namespace United.Mobile.Model.Shopping.FormofPayment
{
    [Serializable()]
    public class MOBFOPCertificate
    {
        //private string promoCode;

        //public string PromoCode
        //{
        //    get { return promoCode; }
        //    set { promoCode = value; }
        //}
        public string ExpiryDate { get; set; }
        public bool IsRemove { get; set; }

        public int EditingIndex { get; set; }


        public bool IsProfileCertificate { get; set; }


        public bool IsCertificateApplied { get; set; }


        public MOBFOPCertificateTraveler CertificateTraveler { get; set; }


        public int Index { get; set; }
        public double NewValueAfterRedeem { get; set; }


        public bool IsForAllTravelers { get; set; }

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
