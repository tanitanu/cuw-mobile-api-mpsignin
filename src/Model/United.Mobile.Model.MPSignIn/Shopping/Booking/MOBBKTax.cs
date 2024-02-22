using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBBKTax
    {
        private string currencyCode = string.Empty;
        private string taxCode = string.Empty;
        private string taxCodeDescription = string.Empty;

        public decimal Amount { get; set; }

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

        public decimal NewAmount { get; set; }

        public string TaxCode
        {
            get
            {
                return this.taxCode;
            }
            set
            {
                this.taxCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TaxCodeDescription
        {
            get
            {
                return this.taxCodeDescription;
            }
            set
            {
                this.taxCodeDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
