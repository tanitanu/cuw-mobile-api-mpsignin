using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace United.Mobile.Model.MPSignIn.CCE
{
    public class CCERequest
    {
        public string ClientId { get; set; }
        public string TransactionId { get; set; }
        [Required]
        public string MileagePlusNumber { get; set; }
    }
}
