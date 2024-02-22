using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBSeatPrice
    {
        private string origin = string.Empty;
        private string destination = string.Empty;
        private string seatMessage = string.Empty;
        private string totalPriceDisplayValue;
        private string discountedTotalPriceDisplayValue;
        private string currencyCode = string.Empty;

        public MOBPromoCode SeatPromoDetails { get; set; }

        public MOBSeatPrice()
        {
        }

        public string Origin
        {
            get => this.origin;
            set => this.origin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }

        public string Destination
        {
            get => this.destination;
            set => this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }

        public string SeatMessage
        {
            get => this.seatMessage;
            set => this.seatMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public int NumberOftravelers { get; set; }

        public decimal TotalPrice { get; set; }

        public decimal DiscountedTotalPrice { get; set; }

        public string TotalPriceDisplayValue
        {
            get => this.totalPriceDisplayValue;
            set => this.totalPriceDisplayValue = string.IsNullOrEmpty(value) ? "" : value.Trim().ToUpper();
        }

        public string DiscountedTotalPriceDisplayValue
        {
            get => this.discountedTotalPriceDisplayValue;
            set => this.discountedTotalPriceDisplayValue = string.IsNullOrEmpty(value) ? "" : value.Trim().ToUpper();
        }

        public string CurrencyCode
        {
            get => this.currencyCode;
            set => this.currencyCode = string.IsNullOrEmpty(value) ? "USD" : value.Trim().ToUpper();
        }

        public List<string> SeatNumbers { get; set; }

        public MOBItem SeatPricesHeaderandTotal { get; set; }
        public Int32 TotalMiles { get; set; }
        public string TotalMilesDisplayValue { get; set; }
        public Int32 DiscountedTotalMiles { get; set; }
        public string DiscountedTotalMilesDisplayValue { get; set; }

    }
}
