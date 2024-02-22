using System.Collections.Generic;
using United.Service.Presentation.PersonalizationModel;

namespace United.Mobile.Model.MPSignIn.Subscription
{
    public class CSLSubscriptionRequest
    {
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }
        public Filter Filter
        {
            get
            {
                return
                      new Filter()
                      {
                          Statuses = new List<int>() { 1 }
                      };
            }
        }
        public string LoyaltyProgramMemberID { get; set; }
        public Requester Requester { get; set; }
        public string TicketingCountryCode { get; set; }
    }
}
