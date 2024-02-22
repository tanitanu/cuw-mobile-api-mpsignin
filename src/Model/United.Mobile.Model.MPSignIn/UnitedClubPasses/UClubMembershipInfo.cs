using System;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class UClubMembershipInfo
    {
        public string ClubStatusCode { get; set; }
        public string ClubStatusDescription { get; set; }
        public string CompanionMpNumber { get; set; }
        public DateTimeOffset DiscontinueDate { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        public string LastUpdateBy { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string MemberTypeCode { get; set; }
        public string MemberTypeDescription { get; set; }
        public string MpNumber { get; set; }
        public string PrimaryOrCompanion { get; set; }
    }
}
