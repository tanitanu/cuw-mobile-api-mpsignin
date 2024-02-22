using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MPSignIn
{
    [Serializable]
    public class MOBInsertCacheDataRequest
    {
        public MOBInsertCacheDataRequest()
            : base()
        {
        }


        private string strGUID;
        private string strDeviceID;
        private string strMPNumber;
        private string strAppVersion;
        private string strCacheData;
        private string strDataDescription;

        public int IntID { get; set; }

        public string StrGUID
        {
            get
            {
                return this.strGUID;
            }
            set
            {
                this.strGUID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string StrDeviceID
        {
            get
            {
                return this.strDeviceID;
            }
            set
            {
                this.strDeviceID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string StrMPNumber
        {
            get
            {
                return this.strMPNumber;
            }
            set
            {
                this.strMPNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public int IntAppId { get; set; }

        public string StrAppVersion
        {
            get
            {
                return this.strAppVersion;
            }
            set
            {
                this.strAppVersion = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string StrCacheData
        {
            get
            {
                return this.strCacheData;
            }
            set
            {
                this.strCacheData = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string StrDataDescription
        {
            get
            {
                return this.strDataDescription;
            }
            set
            {
                this.strDataDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
