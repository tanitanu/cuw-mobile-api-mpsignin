using System;
using System.Collections.Generic;
using System.Text;
using United.Mobile.Model.CSLModels;

namespace United.Mobile.Model.MPSignIn.Common
{
    public class MOBCustomerMPSearchResponse
    {
        public MOBCustomerMPSearchDetail Data { get; set; }
        public List<ProfileErrorInfo> Errors { get; set; }
        public int Status { get; set; }
        public string ServerName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
    public class MOBCustomerAllMPSearchResponse
    {
        public List<MOBCustomerMPSearchDetail> Data { get; set; }
        public List<ProfileErrorInfo> Errors { get; set; }
        public int Status { get; set; }
        public string ServerName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class MOBCustomerMPSearchDetail
    {
        public string MPNumber { get; set; }
        public string PrimaryEmail { get; set; }
        public string EnrollmentDate { get; set; }
        public string FirstName { get; set; }
    }

    public class MOBMemberInfoRecommendationResponse
    {
        public MemberInfoRecommendation Data { get; set; }
    }

    public class MemberInfoRecommendation
    {
        public bool HasRecommendation { get; set; }

        public List<MileagePlusRecommendation> MileagePlusRecommendations { get; set; }
    }

    public class MileagePlusRecommendation
    {
        public string MileagePlusId { get; set; }
        public bool Recommended { get; set; }
        public string Status { get; set; }
    }
}
