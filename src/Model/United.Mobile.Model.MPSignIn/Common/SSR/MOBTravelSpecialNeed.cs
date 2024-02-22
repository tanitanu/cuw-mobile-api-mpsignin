using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBTravelSpecialNeed
    {
        private string value;
        private string displayDescription;
        private string type;
        private string code;
        private string displaySequence;
        private string registerServiceDescription;
        private string subOptionHeader;

        public string DisplaySequence
        {
            get { return this.displaySequence; }
            set { this.displaySequence = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }

        public string Value
        {
            get { return value; }
            set { this.value = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }

        public string Code
        {
            get { return code; }
            set { code = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }

        public string DisplayDescription
        {
            get { return displayDescription; }
            set { displayDescription = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }

        public string RegisterServiceDescription
        {
            get { return registerServiceDescription; }
            set { registerServiceDescription = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }

        /// <summary>
        /// optional
        /// </summary>
        public string Type
        {
            get { return type; }
            set { type = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }

        public List<MOBItem> Messages { get; set; }

        public List<MOBTravelSpecialNeed> SubOptions { get; set; }

        public string SubOptionHeader
        {
            get { return subOptionHeader; }
            set { subOptionHeader = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }

        public bool IsDisabled { get; set; }
    }
}
