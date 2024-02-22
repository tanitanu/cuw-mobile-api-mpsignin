using System;
using System.Collections.Generic;
using System.Text;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.MPSignIn.MPNumberToPNR
{
    public class AddMPNumberToPnrRequest: MOBRequest
    {
        public string SessionId { get; set; }
        public string PNR { get; set; }
        public List<AddMPNumberToPnrTraveler> Traveler { get; set; }

    }
    public class AddMPNumberToPnrTraveler
    {
        public AddMPNumberToPnrTravelerInfo Person { get; set; }

    }
    public class AddMPNumberToPnrTravelerInfo
    {
        public string GivenName { get; set; }
        public string LastName { get; set; }
        public string MileagePlusNumber { get; set; }
        public int CurrentTierLevel { get; set; }
        public string SharesPosition { get; set; }

    }
}
