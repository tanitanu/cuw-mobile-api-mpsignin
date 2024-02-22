using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBPrefAirPreference
    {
        private string key;
        private string mealCode;
        private string mealDescription;
        private string seatSideDescription;
        private string searchPreferenceDescription;
        private string equipmentDesc;
        private string classDescription;
        private string airportCode;
        private string airportNameLong = string.Empty;
        private string airportNameShort;
        private string vendorCode;
        private string vendorDescription;
        private string languageCode;

        public long CustomerId { get; set; }

        public long ProfileId { get; set; }

        public long AirPreferenceId { get; set; }

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

        public int MealId { get; set; }

        public string MealCode
        {
            get
            {
                return this.mealCode;
            }
            set
            {
                this.mealCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string MealDescription
        {
            get
            {
                return this.mealDescription;
            }
            set
            {
                this.mealDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int SeatFrontBack { get; set; }

        public int SeatSide { get; set; }

        public string SeatSideDescription
        {
            get
            {
                return this.seatSideDescription;
            }
            set
            {
                this.seatSideDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int NumOfFlightsDisplay { get; set; }

        public int SearchPreferenceId { get; set; }

        public string SearchPreferenceDescription
        {
            get
            {
                return this.searchPreferenceDescription;
            }
            set
            {
                this.searchPreferenceDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int EquipmentId { get; set; }

        public string EquipmentCode { get; set; }

        public string EquipmentDesc
        {
            get
            {
                return this.equipmentDesc;
            }
            set
            {
                this.equipmentDesc = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int ClassId { get; set; }

        public string ClassDescription
        {
            get
            {
                return this.classDescription;
            }
            set
            {
                this.classDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

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

        public int VendorId { get; set; }

        public string VendorCode
        {
            get
            {
                return this.vendorCode;
            }
            set
            {
                this.vendorCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string VendorDescription
        {
            get
            {
                return this.vendorDescription;
            }
            set
            {
                this.vendorDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsNew { get; set; }

        public bool IsSelected { get; set; }

        public bool IsActive { get; set; }

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

        public List<MOBPrefRewardProgram> AirRewardPrograms { get; set; }

        public List<MOBPrefSpecialRequest> SpecialRequests { get; set; }

        public List<MOBPrefServiceAnimal> ServiceAnimals { get; set; }
    }
}
