using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBPhone
    {
        private string key = string.Empty;
        private string phoneNumber = string.Empty;

        public string Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public MOBChannel Channel { get; set; }

        public MOBCountry Country { get; set; }

        public bool IsPrivate { get; set; }

        public bool IsDefault { get; set; }

        public bool IsPrimary { get; set; }

        public bool IsProfileOwner { get; set; }

        public string PhoneNumber
        {
            get
            {
                return this.phoneNumber;
            }
            set
            {
                this.phoneNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string AreaNumber { get; set; } = string.Empty;

        public string PhoneNumberDisclaimer { get; set; } = "";//TODO ConfigurationManager.AppSettings["PhoneNumberDisclaimer"];

        public string ExtensionNumber { get; set; } = string.Empty;

        public bool ISThisPhoneFromProfileOwner { get; set; }
    }
}
