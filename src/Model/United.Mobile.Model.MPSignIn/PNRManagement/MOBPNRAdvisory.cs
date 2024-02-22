using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.PNRManagement
{
    [Serializable]
    public enum AdvisoryType
    {
        [EnumMember(Value = "WARNING")] //RED
        WARNING,
        [EnumMember(Value = "INFORMATION")] //BLUE
        INFORMATION,
        [EnumMember(Value = "CAUTION")] //YELLOW
        CAUTION,
    }

    [Serializable]
    public enum ContentType
    {
        [EnumMember(Value = "SCHEDULECHANGE")]
        SCHEDULECHANGE,
        [EnumMember(Value = "POLICYEXCEPTION")]
        POLICYEXCEPTION,
        [EnumMember(Value = "INCABINPET")]
        INCABINPET,
        [EnumMember(Value = "MAX737WAIVER")]
        MAX737WAIVER,
        [EnumMember(Value = "TRAVELWAIVERALERT")]
        TRAVELWAIVERALERT,
        [EnumMember(Value = "FACECOVERING")]
        FACECOVERING,
        [EnumMember(Value = "MILESINSUFFICIENT")]
        MILESINSUFFICIENT,
        [EnumMember(Value = "MILESWELCOMEMSG")]
        MILESWELCOMEMSG,
        [EnumMember(Value = "PPOINTSINSUFFICIENT")]
        PPOINTSINSUFFICIENT,
        [EnumMember(Value = "PPOINTSWELCOMEMSG")]
        PPOINTSWELCOMEMSG,
        [EnumMember(Value = "PPOINTSPARTIALEXPIRY")]
        PPOINTSPARTIALEXPIRY,
        [EnumMember(Value = "PPOINTSFULLEXPIRY")]
        PPOINTSFULLEXPIRY,
        [EnumMember(Value = "PPOINTSUSERNOTE")]
        PPOINTSUSERNOTE,
        [EnumMember(Value = "MIXEDINSUFFICIENT")]
        MIXEDINSUFFICIENT,
        [EnumMember(Value = "SKIPWAITLIST")]
        SKIPWAITLIST,
        [EnumMember(Value = "CABINOPTIONNOTSELECTED")]
        CABINOPTIONNOTSELECTED,
        [EnumMember(Value = "CABINOPTIONNOTLOADED")]
        CABINOPTIONNOTLOADED,
        [EnumMember(Value = "RESHOPNEWTRIP")]
        RESHOPNEWTRIP,
        [EnumMember(Value = "FUTUREFLIGHTCREDIT")]
        FUTUREFLIGHTCREDIT,
        [EnumMember(Value = "FFCRRESIDUAL")]
        FFCRRESIDUAL,
        [EnumMember(Value = "TRAVELREADY")]
        TRAVELREADY,
        [EnumMember(Value = "OTFCONVERSION")]
        OTFCONVERSION,
        [EnumMember(Value = "IRROPS")]
        IRROPS,
        [EnumMember(Value = "MILESMONEY")]
        MILESMONEY,
        [EnumMember(Value = "JSENONCONVERTEABLEPNR")]
        JSENONCONVERTEABLEPNR,
    }

    [Serializable]
    public class MOBPNRAdvisory
    {
        public AdvisoryType AdvisoryType { get; set; }
        public ContentType ContentType { get; set; }
        public string Header { get; set; }
        public string SubTitle { get; set; }
        public string Body { get; set; }
        public string Footer { get; set; }
        public string Buttontext { get; set; }
        public string Buttonlink { get; set; }
        public bool IsBodyAsHtml { get; set; }
        public bool IsDefaultOpen { get; set; } = true;
        public Boolean ShouldExpand { get; set; } = true;
        public List<MOBItem> ButtonItems { get; set; }
        public List<MOBItem> SubItems { get; set; }

    }
}
