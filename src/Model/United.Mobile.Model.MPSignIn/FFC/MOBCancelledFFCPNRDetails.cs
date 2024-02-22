using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBCancelledFFCPNRDetails
    {
        public string RecordLocator { get; set; }


        public string PNRLastName { get; set; }

        public List<MOBName> Passengers { get; set; }
    }
}
