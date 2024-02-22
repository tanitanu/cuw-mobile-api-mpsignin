using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Internal.Common.DynamoDB
{
   public class MilagePlusDevice
    {
        public string DeviceId { get; set; }
        public string MileagePlusNumber { get; set; }
        public string ApplicationId { get; set; }
        public string CustomerID { get; set; }
    }
}
