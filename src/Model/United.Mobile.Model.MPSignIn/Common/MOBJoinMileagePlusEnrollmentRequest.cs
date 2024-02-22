using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBJoinMileagePlusEnrollmentRequest : MOBRequest
    {
        #region Private Properties
        private string sessionId = string.Empty;
        private string flow = string.Empty;
        private string travelerName = string.Empty;
       


        #endregion

        #region Public Properties
        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public string Email { get; set; } = string.Empty;

        public bool ConsentToReceiveMarketingEmails { get; set; } = false;

        public string SharesPosition { get; set; } = string.Empty;
        public string RecordLocator { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Flow
        {
            get
            {
                return this.flow;
            }
            set
            {
                this.flow = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public bool IsGetPNRByRecordLocatorCall { get; set; } = false;

        public bool IsGetPNRByRecordLocator { get; set; } = false;

        public string TravelerName
        {
            get
            {
                return this.travelerName;
            }
            set
            {
                this.travelerName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }     

        #endregion
    }
}
