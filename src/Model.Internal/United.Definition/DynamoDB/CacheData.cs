using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Internal.Common.DynamoDB
{
    public class CacheData
    {
        public string SessionId { get; set; }
        public string DeviceId { get; set; }
		public string MPNumber { get; set; }
		public int AppId { get; set; }
		public string AppVersion { get; set; }
		public string Data { get; set; }
		public string DataDescription { get; set; }
		public DateTime LastRefreshDateTime { get; set; }
	}
}
