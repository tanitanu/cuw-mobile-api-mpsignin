using System;

namespace United.Mobile.Model.Shopping.FormofPayment
{
    [Serializable()]
    public class MOBCPBillingCountry
    {

        public string Id { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public bool IsStateRequired { get; set; }
        public bool IsZipRequired { get; set; }
    }
}
