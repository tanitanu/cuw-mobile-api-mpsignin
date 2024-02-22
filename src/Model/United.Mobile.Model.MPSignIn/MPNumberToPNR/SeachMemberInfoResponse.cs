using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MPSignIn.MPNumberToPNR
{
    public class SearchMemberInfoResponse
    {
        public SearchMemberInformationData Data { get; set; }
        public dynamic Link { get; set; }
        public dynamic Meta { get; set; }
    }
    public class SearchMemberInformationData
    {
        private List<MemberInformation> searchMembers;
        public int SearchCount { get; set; }
        public string ServiceName { get; set; }

        public List<MemberInformation> SearchMembers { get { return this.searchMembers; } set { this.searchMembers = value; } }
    }
    public class MemberInformation
    {

        public string LoyaltyId { get; set; }
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int CurrentTierLevel { get; set; }
        public string CurrentTierLevelDescription { get; set; }
        public int MillonMilerLevel { get; set; }
        public int StarAllianceTierLevel { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool TestAccount { get; set; }
        public string AccountStatus { get; set; }
        public string OpenClosedStatusCode { get; set; }
        public string OpenClosedStatusDescription { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string CountryOfResidence { get; set; }
        public string PostalCode { get; set; }
    }

}
