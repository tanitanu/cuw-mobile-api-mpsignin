using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.MPSignInJobs.Domain.SQL2DynamoDBJobs
{
    public class SQL2DynamoDBJob
    {
        public string table { get; set; }
        public string Triggerkey { get; set; }
        public int ExpiryTimeOut { get; set; }
        public bool Enable { get; set; }
        public bool RunCloudService { get; set; } = false;
    }
}
