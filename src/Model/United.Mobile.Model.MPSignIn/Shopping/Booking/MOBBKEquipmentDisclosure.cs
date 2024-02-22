using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBBKEquipmentDisclosure
    {
        private string equipmentDescription = string.Empty;
        private string equipmentType = string.Empty;

        public string EquipmentDescription
        {
            get
            {
                return this.equipmentDescription;
            }
            set
            {
                this.equipmentDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EquipmentType
        {
            get
            {
                return this.equipmentType;
            }
            set
            {
                this.equipmentType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsSingleCabin { get; set; }

        public bool NoBoardingAssistance { get; set; }

        public bool NonJetEquipment { get; set; }

        public bool WheelchairsNotAllowed { get; set; }
    }
}
