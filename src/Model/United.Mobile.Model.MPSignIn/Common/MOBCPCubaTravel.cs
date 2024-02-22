using System;
using System.Collections.Generic;
using System.Text;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.MPSignIn
{
    [Serializable()]
    public class MOBCPCubaTravel
    {
        public List<MOBItem> CubaTravelTitles { get; set; }

        public List<MOBCPCubaTravelReason> TravelReasons { get; set; }



    }
}
