﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBEmpLmxQuote
    {
        private string amount;
        private string currency = string.Empty;
        private string description = string.Empty;
        private string type = string.Empty;
        private string currencySymbol = string.Empty;

        public string Amount
        {
            get
            {
                return this.amount;
            }
            set
            {
                this.amount = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Currency
        {
            get
            {
                return this.currency;
            }
            set
            {
                this.currency = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
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

        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public double DblAmount { get; set; }
        
        public string CurrencySymbol
        {
            get
            {
                return this.currencySymbol;
            }
            set
            {
                this.currencySymbol = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
