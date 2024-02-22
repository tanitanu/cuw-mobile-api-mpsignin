using System;
using System.Collections.Generic;
using System.Text;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.MPSignIn.MPNumberToPNR
{
    public class MPSearchRequest : MOBRequest
    {
        public string SessionId { get; set; }
        public string PNR { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string EmailAddress { get; set; }
    }
}
