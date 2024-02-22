using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.OneClickEnrollment
{
    public class LoyaltyResponse : LoyaltyUCBResponse
    {
        public bool AlreadyEnrolled { get; set; }      
    }

    public class LoyaltyUCBResponse
    {        
        public long CustomerId { get; set; }
        public string LoyaltyId { get; set; }
        public List<ResponseTime> ResponseTimes { get; set; }
        public string ServiceName { get; set; }
        public System.Net.HttpStatusCode StatusCode { get; set; }
    }
}
