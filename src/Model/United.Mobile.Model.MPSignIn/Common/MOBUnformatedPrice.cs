using System;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBReservationPrice
    {
        public string CurrencyCode { get; set; }

        public string CurrencyShortCode { get; set; }
        public double Amount { get; set; }
        public string PriceTypeDescription { get; set; }
    }
}
