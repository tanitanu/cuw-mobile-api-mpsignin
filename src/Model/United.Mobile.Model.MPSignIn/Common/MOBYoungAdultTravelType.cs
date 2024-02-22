using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBYoungAdultTravelType
    {

        public List<MOBTravelType> YoungAdultTravelTypes { get; set; }

        public bool IsYoungAdultTravel { get; set; }
    }
}
