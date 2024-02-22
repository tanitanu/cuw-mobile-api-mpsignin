using System;
using System.Xml.Serialization;

namespace United.Mobile.Model.DeviceInitialization
{
    [Serializable]
    [XmlRoot("MOBDeviceResponse")]
    public class DeviceResponse : MOBResponse
    {
        private string guid = string.Empty;

        public DeviceResponse()
            : base()
        {
        }

        public string GUID
        {
            get
            {
                return this.guid;
            }
            set
            {
                this.guid = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int DeviceID { get; set; }

    }
}
