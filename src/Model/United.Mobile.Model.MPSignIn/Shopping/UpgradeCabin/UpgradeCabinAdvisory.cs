using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping
{

    [Serializable]
    public enum UpgradeCabinAdvisoryType
    {
        [EnumMember(Value = "NONE")]
        NONE,
        [EnumMember(Value = "WARNING")]
        WARNING,
        [EnumMember(Value = "INFORMATION")]
        INFORMATION,
    }


    [Serializable]
    public enum UpgradeCabinContentType
    {
        [EnumMember(Value = "NONE")]
        NONE,
        [EnumMember(Value = "ELIGIBILITYSERVICEERROR")]
        ELIGIBILITYSERVICEERROR,
        [EnumMember(Value = "PPOINTSPARTIALEXPIRY")]
        PPOINTSPARTIALEXPIRY,
        [EnumMember(Value = "PPOINTSFULLEXPIRY")]
        PPOINTSFULLEXPIRY,
        [EnumMember(Value = "PPOINTSEVERGREEN")]
        PPOINTSEVERGREEN,
        [EnumMember(Value = "CABINOPTIONNOTSELECTED")]
        CABINOPTIONNOTSELECTED,
        [EnumMember(Value = "CABINOPTIONNOTLOADED")]
        CABINOPTIONNOTLOADED,
        [EnumMember(Value = "MILESDOUBLEUPGRADEDETAIL")]
        MILESDOUBLEUPGRADEDETAIL,
    }


    [Serializable]
    public class UpgradeCabinAdvisory
    {
        public UpgradeCabinAdvisoryType AdvisoryType { get; set; }
        public UpgradeCabinContentType ContentType { get; set; }
        public string Header { get; set; }
        public string Body { get; set; }
        public Boolean ShouldExpand { get; set; }
        public List<MOBItem> BodyItems { get; set; }
    }
}
