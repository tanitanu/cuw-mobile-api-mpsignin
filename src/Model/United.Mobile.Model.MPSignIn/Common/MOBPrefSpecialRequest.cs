using System;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBPrefSpecialRequest
    {
        private string key = string.Empty;
        private string languageCode = string.Empty;
        private string specialRequestCode = string.Empty;
        private string description = string.Empty;

        public long AirPreferenceId { get; set; }

        public long SpecialRequestId { get; set; }

        public string Key
        {
            get => this.key;
            set => this.key = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }

        public string LanguageCode
        {
            get => this.languageCode;
            set => this.languageCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string SpecialRequestCode
        {
            get => this.specialRequestCode;
            set => this.specialRequestCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }

        public string Description
        {
            get => this.description;
            set => this.description = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public long Priority { get; set; }

        public bool IsNew { get; set; }

        public bool IsSelected { get; set; }
    }
}
