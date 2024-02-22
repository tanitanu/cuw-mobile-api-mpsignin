using System;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MPSignIn 
    {
        public string ObjectName { get; set; } = "United.Persist.Definition.Profile.MPSignIn";

        public string SessionId { get; set; }
        public string MPNumber { get; set; }
        public string MPHashValue { get; set; }
        public int CustomerId { get; set; }
        public MOBCPProfile Profile { get; set; }
        public string AuthToken { get; set; }
        public DateTime TokenExpirationDateTime { get; set; }
        public Double TokenExpirationSeconds { get; set; }
        public bool IsSignInWithTouchID { get; set; }
    }
}
