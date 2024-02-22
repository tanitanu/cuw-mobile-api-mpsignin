using System;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class EmpployeeTravelTypeResponse : IPersist
    {
        public string ObjectName { get; set; }
        public MOBEmpTravelTypeResponse EmpTravelTypeResponse { get; set; }
    }
}
