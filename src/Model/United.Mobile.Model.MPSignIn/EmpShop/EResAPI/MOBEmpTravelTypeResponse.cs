using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBEmpTravelTypeResponse : MOBResponse
    {

        private string eResTransactionId = string.Empty;
        private string sessionId = string.Empty;
        private string displayEmployeeId = string.Empty;
        private int openSearchFlightDays;
        public int OpenSearchFlightDays
        {
            get
            {
                return this.openSearchFlightDays;
            }
            set
            {
                this.openSearchFlightDays = value;
            }
        }

        public string EResTransactionId
        {
            get
            {
                return this.eResTransactionId;
            }
            set
            {
                this.eResTransactionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public MOBEmpTravelType EmpTravelType { get; set; }
        public bool IsPayrollDeduct { get; set; }

        public int AdvanceBookingDays { get; set; }

        public string DisplayEmployeeId
        {
            get
            {
                return this.displayEmployeeId;
            }
            set
            {
                this.displayEmployeeId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public List<DependentInfo> DependentInfos { get; set; }

        public MOBName EmployeeName { get; set; }
        public string EmployeeDateOfBirth { get; set; }
    }
}
