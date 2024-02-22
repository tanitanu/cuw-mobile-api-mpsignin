using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common.MP2015
{
    [Serializable()]
    public class MOBLmxProduct
    {
        private string bookingCode = string.Empty;
        private string description = string.Empty;
        private string productType = string.Empty;

        public string BookingCode
        {
            get
            {
                return this.bookingCode;
            }
            set
            {
                this.bookingCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBLmxLoyaltyTier> LmxLoyaltyTiers { get; set; }

        public string ProductType
        {
            get
            {
                return this.productType;
            }
            set
            {
                this.productType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
