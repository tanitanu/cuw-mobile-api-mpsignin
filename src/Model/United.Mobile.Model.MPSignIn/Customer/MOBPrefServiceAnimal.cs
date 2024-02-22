using System;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBPrefServiceAnimal
    {
        private string key = string.Empty;

        public int AirPreferenceId { get; set; }

        public int ServiceAnimalId { get; set; }

        public string ServiceAnimalIdDesc { get; set; }

        public int ServiceAnimalTypeId { get; set; }

        public string ServiceAnimalTypeIdDesc { get; set; }

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

        public int Priority { get; set; }

        public bool IsNew { get; set; }

        public bool IsSelected { get; set; }
    }
}
