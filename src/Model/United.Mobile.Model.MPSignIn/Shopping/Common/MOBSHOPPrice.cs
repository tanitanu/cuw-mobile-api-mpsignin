using System;
using System.Globalization;
using United.Mobile.Model.Common;
using United.Utility.Helper;

namespace United.Mobile.Model.Shopping
{
    [Serializable()]
    public class MOBSHOPPrice
    {
        //add new double property amount
        private string priceIndex = string.Empty;
        private string currencyCode = string.Empty;
        private string priceType = string.Empty;
        private string displayType = string.Empty;
        private string displayValue = string.Empty;
        private string totalBaseFare = string.Empty;
        private string totalOtherTaxes = string.Empty;
        private string formattedDisplayValue = string.Empty;
        // private string status = string.Empty;
        private string productId = string.Empty;

        public string StrikeThroughDescription { get; set; } = string.Empty;

        public string StrikeThroughDisplayValue { get; set; } = string.Empty;

        public string PaxTypeCode { get; set; }
        public bool Waived { get; set; }

        public string Status { get; set; } = string.Empty;
        //{
        //    get { return status; }
        //    set { status = value; }
        //}


        public string PriceIndex
        {
            get
            {
                return this.priceIndex;
            }
            set
            {
                this.priceIndex = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string CurrencyCode
        {
            get
            {
                return this.currencyCode;
            }
            set
            {
                this.currencyCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string PriceType
        {
            get
            {
                return this.priceType;
            }
            set
            {
                this.priceType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string DisplayType
        {
            get
            {
                return this.displayType;
            }
            set
            {
                this.displayType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string DisplayValue
        {
            get
            {
                return this.displayValue;
            }
            set
            {
                this.displayValue = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string TotalBaseFare
        {
            get
            {
                return this.totalBaseFare;
            }
            set
            {
                this.totalBaseFare = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string TotalOtherTaxes
        {
            get
            {
                return this.totalOtherTaxes;
            }
            set
            {
                this.totalOtherTaxes = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }


        public string FormattedDisplayValue
        {
            get
            {
                return this.formattedDisplayValue;
            }
            set
            {
                this.formattedDisplayValue = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }


        public double Value { get; set; }

        public string PriceTypeDescription { get; set; }
        public string BilledSeperateText { get; set; }

        public string ProductId
        {
            get { return productId; }
            set { this.productId = string.IsNullOrEmpty(value) ? string.Empty : value; }
        }

        public MOBPromoCode PromoDetails { get; set; }


        public string PaxTypeDescription { get; set; }

    }
}
