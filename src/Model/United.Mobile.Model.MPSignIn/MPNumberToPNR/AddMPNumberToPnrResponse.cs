using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MPSignIn.MPNumberToPNR
{
    public class AddMPNumberToPnrResponse : MOBResponse
    {
        public string Title { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }
        public string ViewReservationDetailsButtonText { get; set; }
        public string SessionId { get; set; }
        public Alert AlertInfo { get; set; }

    }
}
