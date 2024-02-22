using Newtonsoft.Json;
using System;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBEmail
    {
        [JsonIgnore()]
        public string ObjectName { get; set; } = "United.Definition.MOBEmail";
        private string key = string.Empty;
        private string emailAddress = string.Empty;

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

        public MOBChannel Channel { get; set; }

        public string EmailAddress
        {
            get
            {
                return this.emailAddress;
            }
            set
            {
                this.emailAddress = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsPrivate { get; set; }

        public bool IsDefault { get; set; }

        public bool IsPrimary { get; set; }

        public bool IsDayOfTravel { get; set; }
    }
}
