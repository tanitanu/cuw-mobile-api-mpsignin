using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBJoinMileagePlusEnrollmentResponse : MOBResponse
    {
        #region Private Properties   
        private string sessionId= string.Empty;


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

        //"CustomerId":164345762,"LoyaltyId":"ADG61449","AlreadyEnrolled":true,

        public string EnrollAnotherTravelerButtonText { get; set; } = string.Empty;
        public string AccountConfirmationTitle { get; set; } = string.Empty;
        public string AccountConfirmationHeader { get; set; } = string.Empty;
        public string AccountConfirmationBody { get; set; } = string.Empty;
        public string RecordLocator { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsGetPNRByRecordLocatorCall { get; set; } = false;
        public bool IsGetPNRByRecordLocator { get; set; } = false;
        public string CloseButtion { get; set; } = string.Empty;
        public string AccountCreatedText { get; set; } = string.Empty;
        public string DeviceId  { get; set; } = string.Empty;

        public List<MOBKVP> EnrolledUserInfo { get; set; } = null;
        public bool IsEnrollAnotherTraveler { get; set; } = false;

        public List<MOBKVP> FullEnrollmentUserInfo { get; set; } = null;
        #endregion
    }
}
