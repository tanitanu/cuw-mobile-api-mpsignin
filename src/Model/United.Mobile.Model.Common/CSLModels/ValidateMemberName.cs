using System;
using System.Collections.Generic;

namespace United.Mobile.Model.CSLModels
{
    public class ValidateMemberNameRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string Suffix { get; set; } = string.Empty;
        public string AirlineCode { set; get; } = string.Empty;
    }

    public class ValidateMemberNameData
    {
        public string LoyaltyId { get; set; }
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MatchResult { set; get; }
        public int MPTierLevel { set; get; }
        public string MPTierLevelDescription { set; get; }
        public int StarAllianceTierLevel { set; get; }
        public string StarAllianceTierLevelDescription { set; get; }
        public bool TestAccount { set; get; }
        public string AccountType { set; get; }
        public string AccountStatus { set; get; }
        public string AirlineCode { set; get; }
        public string MessageCode { set; get; }
        public string MessageDescription { set; get; }
        public string Suffix { set; get; }
        public string Title { set; get; }
        public string ServiceName { set; get; }
    }

    public class ValidateMemberNameResponse
    {
        public ValidateMemberNameData Data { set; get; }
        public List<Error> Errors { get; set; }
    }
}
