using System;
using System.Xml.Serialization;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    [XmlType("MOBSHOPRewardProgram")]
    public class RewardProgram
    {
        public string Type { get; set; } = string.Empty;

        public string ProgramID { get; set; } 

        public string Description { get; set; } = string.Empty;

    }
}
