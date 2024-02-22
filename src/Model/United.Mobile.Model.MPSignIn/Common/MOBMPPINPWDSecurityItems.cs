using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBMPPINPWDSecurityItems
    {
        public MOBMPPINPWDSecurityItems()
        {

        }
        public MOBMPPINPWDSecurityItems(IConfiguration configuration)
            : base()
        {
            needQuestionsCount = configuration.GetValue<int>("NumberOfSecurityQuestionsNeedatPINPWDUpdate");
        }
        private string primaryEmailAddress;
        public string PrimaryEmailAddress
        {
            get
            {
                return this.primaryEmailAddress;
            }
            set
            {
                this.primaryEmailAddress = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public int needQuestionsCount { get; set; }


        [JsonPropertyName("securityQuestions")]
        [Newtonsoft.Json.JsonProperty("securityQuestions")]
        public List<Securityquestion> AllSecurityQuestions { get; set; }

        public List<Securityquestion> SavedSecurityQuestions { get; set; }

        private string updatedPassword;
        public string UpdatedPassword
        {
            get
            {
                return this.updatedPassword;
            }
            set
            {
                this.updatedPassword = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

    }
}
