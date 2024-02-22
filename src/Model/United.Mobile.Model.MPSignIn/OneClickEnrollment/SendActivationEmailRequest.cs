using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.OneClickEnrollment
{
    public class SendActivationEmailRequest
    {
        public string CustomerId { get; set; }
        public string MileagePlusId { get; set; }
        public string Email { get; set; }
        public string LanguageCode { get; set; }
    }
}
