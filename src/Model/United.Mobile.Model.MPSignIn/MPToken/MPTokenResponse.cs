using United.Mobile.Model.Internal.Exception;

namespace United.Mobile.Model.MPSignIn
{
    public class MPTokenResponse
    {
        public int AccountFound { get; set; }
        public string AuthenticatedToken { get; set; }
        public MOBException Exception { get; set; }
        public long CallDuration { get; set; }
    }
}