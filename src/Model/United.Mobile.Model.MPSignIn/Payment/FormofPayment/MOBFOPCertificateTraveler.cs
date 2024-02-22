using System;

namespace United.Mobile.Model.FormofPayment
{
    [Serializable()]
    public class MOBFOPCertificateTraveler
    {
        public int PaxId { get; set; }


        public double IndividualTotalAmount { get; set; }


        public bool IsCertificateApplied { get; set; }


        public string Name { get; set; }

        public string TravelerNameIndex { get; set; }



    }
}
