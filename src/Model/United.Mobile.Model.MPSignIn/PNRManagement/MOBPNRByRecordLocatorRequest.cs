using System;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.PNRManagement
{

    [Serializable()]
    public class MOBPNRByRecordLocatorRequest : MOBRequest
    {
        private string recordLocator = string.Empty;
        private string lastName = string.Empty;
        private string mileagePlusNumber = string.Empty;
        private string sessionId = string.Empty;
        private string hashKey = string.Empty;
        private string flow = string.Empty;//
        public string RecordLocator
        {
            get => this.recordLocator;
            set => this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }

        public bool IsOTFConversion { get; set; }

        public string LastName
        {
            get => this.lastName;
            set => this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }



        public string MileagePlusNumber
        {
            get => this.mileagePlusNumber;
            set => this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }

        public string SessionId
        {
            get => this.sessionId;
            set => this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }
        public string HashKey
        {
            get => this.hashKey;
            set => this.hashKey = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }
        public string Flow
        {
            get => this.flow;
            set => this.flow = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }

        public bool IsRefreshedUserData { get; set; }
    }
}
