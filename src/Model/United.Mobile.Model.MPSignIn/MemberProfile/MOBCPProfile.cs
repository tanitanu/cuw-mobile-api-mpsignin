using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBCPProfile
    {
        [JsonIgnore()]
        public string ObjectName { get; set; } = "United.Definition.MOBCPProfile";
        private string airportCode = string.Empty;
        private string airportNameLong = string.Empty;
        private string airportNameShort = string.Empty;
        private string description = string.Empty;
        private string key = string.Empty;
        private string languageCode = string.Empty;
        private string profileOwnerKey = string.Empty;
        private string quickCreditCardKey = string.Empty;
        private string quickCreditCardNumber = string.Empty;
        private string quickCustomerKey = string.Empty;
        public string AirportCode
        {
            get
            {
                return this.airportCode;
            }
            set
            {
                this.airportCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string AirportNameLong
        {
            get
            {
                return this.airportNameLong;
            }
            set
            {
                this.airportNameLong = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string AirportNameShort
        {
            get
            {
                return this.airportNameShort;
            }
            set
            {
                this.airportNameShort = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public string LanguageCode
        {
            get
            {
                return this.languageCode;
            }
            set
            {
                this.languageCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int ProfileId { get; set; }

        public List<MOBCPProfileMember> ProfileMembers { get; set; }

        public int ProfileOwnerId { get; set; }

        public string ProfileOwnerKey
        {
            get
            {
                return this.profileOwnerKey;
            }
            set
            {
                this.profileOwnerKey = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string QuickCreditCardKey
        {
            get
            {
                return this.quickCreditCardKey;
            }
            set
            {
                this.quickCreditCardKey = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string QuickCreditCardNumber
        {
            get
            {
                return this.quickCreditCardNumber;
            }
            set
            {
                this.quickCreditCardNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int QuickCustomerId { get; set; }

        public string QuickCustomerKey
        {
            get
            {
                return this.quickCustomerKey;
            }
            set
            {
                this.quickCustomerKey = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBCPTraveler> Travelers { get; set; }

        public bool IsProfileOwnerTSAFlagON { get; set; }

        public List<MOBTypeOption> DisclaimerList { get; set; } = null;

        public List<MOBKVP> SavedTravelersMPList { get; set; } = null;

        public MOBCPCorporate CorporateData { get; set; }

    }
}
