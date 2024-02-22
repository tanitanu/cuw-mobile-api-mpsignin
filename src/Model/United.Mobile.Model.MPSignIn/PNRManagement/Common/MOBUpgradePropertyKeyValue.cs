using System;

namespace United.Mobile.Model.PNRManagement
{
    [Serializable]
    public class MOBUpgradePropertyKeyValue
    {
        private string value = string.Empty;

        public MOBUpgradeProperty Key { get; set; }

        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
