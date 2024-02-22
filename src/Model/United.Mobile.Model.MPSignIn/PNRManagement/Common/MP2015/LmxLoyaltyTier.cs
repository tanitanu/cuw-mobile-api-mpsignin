using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common.MP2015
{
    [Serializable()]
    public class LmxLoyaltyTier
    {
        private string description = string.Empty;
        private string key = string.Empty;

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

        public string Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int Level { get; set; }

        public List<LmxQuote> LmxQuotes { get; set; }
    }
}
