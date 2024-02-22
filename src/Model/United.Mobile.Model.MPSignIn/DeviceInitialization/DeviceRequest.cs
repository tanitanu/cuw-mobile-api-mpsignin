using System;
using System.Xml.Serialization;

namespace United.Mobile.Model.DeviceInitialization
{
    [Serializable]
    [XmlRoot("MOBDeviceResponse")]
    public class DeviceRequest
    {
        private string identifierForVendor = string.Empty;
        private string name = string.Empty;
        private string model = string.Empty;
        private string localizedModel = string.Empty;
        private string systemName = string.Empty;
        private string systemVersion = string.Empty;
        private string applicationId = string.Empty;
        private string applicationVersion = string.Empty;
        private string accessCode = string.Empty;
        private string transactionId = string.Empty;

        public DeviceRequest()
            : base()
        {
        }
        public string TransactionId
        {
            get
            {
                return this.transactionId;
            }
            set
            {
                this.transactionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string AccessCode
        {
            get
            {
                return this.accessCode;
            }
            set
            {
                this.accessCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IdentifierForVendor
        {
            get
            {
                return this.identifierForVendor;
            }
            set
            {
                this.identifierForVendor = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Model
        {
            get
            {
                return this.model;
            }
            set
            {
                this.model = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LocalizedModel
        {
            get
            {
                return this.localizedModel;
            }
            set
            {
                this.localizedModel = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SystemName
        {
            get
            {
                return this.systemName;
            }
            set
            {
                this.systemName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SystemVersion
        {
            get
            {
                return this.systemVersion;
            }
            set
            {
                this.systemVersion = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ApplicationId
        {
            get
            {
                return this.applicationId;
            }
            set
            {
                this.applicationId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ApplicationVersion
        {
            get
            {
                return this.applicationVersion;
            }
            set
            {
                this.applicationVersion = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}



