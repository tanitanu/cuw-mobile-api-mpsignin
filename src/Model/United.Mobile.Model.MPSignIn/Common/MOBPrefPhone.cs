using System;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBPrefPhone
    {
        private string channelCode = string.Empty;
        private string channelCodeDescription = string.Empty;
        private string channelTypeCode = string.Empty;
        private string channelTypeDescription = string.Empty;
        private string key = string.Empty;
        private string countryPhoneNumber = string.Empty;
        private string countryName = string.Empty;
        private string countryCode = string.Empty;
        private string areaNumber = string.Empty;
        private string phoneNumber = string.Empty;
        private string extensionNumber = string.Empty;
        private string description = string.Empty;
        private string languageCode = string.Empty;

        public long CustomerId { get; set; }

        public string ChannelCode
        {
            get
            {
                return this.channelCode;
            }
            set
            {
                this.channelCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ChannelCodeDescription
        {
            get
            {
                return this.channelCodeDescription;
            }
            set
            {
                this.channelCodeDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int ChannelTypeSeqNum { get; set; }

        public string ChannelTypeCode
        {
            get
            {
                return this.channelTypeCode;
            }
            set
            {
                this.channelTypeCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ChannelTypeDescription
        {
            get
            {
                return this.channelTypeDescription;
            }
            set
            {
                this.channelTypeDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
                this.key = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string CountryPhoneNumber
        {
            get
            {
                return this.countryPhoneNumber;
            }
            set
            {
                this.countryPhoneNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CountryName
        {
            get
            {
                return this.countryName;
            }
            set
            {
                this.countryName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CountryCode
        {
            get
            {
                return this.countryCode;
            }
            set
            {
                this.countryCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string AreaNumber
        {
            get
            {
                return this.areaNumber;
            }
            set
            {
                this.areaNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

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

        public string ExtensionNumber
        {
            get
            {
                return this.extensionNumber;
            }
            set
            {
                this.extensionNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public string LanguageCode
        {
            get
            {
                return this.languageCode;
            }
            set
            {
                this.languageCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public bool IsPrivate { get; set; }

        public bool IsNew { get; set; }

        public bool IsDefault { get; set; }

        public bool IsPrimary { get; set; }

        public bool IsSelected { get; set; }

        public bool IsProfileOwner { get; set; }
    }
}
