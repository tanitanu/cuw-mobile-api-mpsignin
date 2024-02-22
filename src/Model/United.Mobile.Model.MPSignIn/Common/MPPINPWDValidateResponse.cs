using System;
using System.Xml.Serialization;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    [XmlRoot("MPPINPWDValidateResponse")]
    public class MPPINPWDValidateResponse
    {
        #region IPersist Members

        public string ObjectName { get; set; } = "United.Persist.Definition.Profile.MPPINPWDValidateResponse";

        #endregion

        public string SessionId { get; set; }
        //public MOBMPPINPWDValidateResponse Response { get; set; }
        public MOBCPProfile Profile { get; set; }
        public MOBMPSecurityUpdateResponse Response { get; set; }
        public MOBEmpTravelTypeResponse EmpTravelTypeResponse { get; set; }

    }
}
