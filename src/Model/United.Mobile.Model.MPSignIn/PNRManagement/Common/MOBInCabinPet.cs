using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.PNRManagement
{
    [Serializable()]
    public class MOBInCabinPet
    {
        public List<MOBItem> Messages { get; set; }
        public string InCabinPetLabel { get; set; }
        public string InCabinPetRefText { get; set; }
        public string InCabinPetRefValue { get; set; }
    }
}
