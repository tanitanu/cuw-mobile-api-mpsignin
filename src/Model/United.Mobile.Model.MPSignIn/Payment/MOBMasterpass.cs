using System;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBMasterpass
    {
        private string oauthToken;
        private string oauthVerifier;
        private string mpstatus;
        private string cslSessionId = string.Empty;

        public string OauthToken
        {
            get { return oauthToken; }
            set { oauthToken = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string Oauth_verifier
        {
            get { return oauthVerifier; }
            set { oauthVerifier = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string CheckoutId { get; set; }

        public string CheckoutResourceURL { get; set; }

        public string Mpstatus
        {
            get { return mpstatus; }
            set
            {
                mpstatus = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string CslSessionId
        {
            get { return cslSessionId; }
            set { this.cslSessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

    }
}
