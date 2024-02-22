using System;
using System.Collections.Generic;
using System.Text;
using United.Mobile.Model.Common.CloudDynamoDB;

namespace United.Mobile.Model.MPSignIn.HashpinVerify
{
    public class HashpinVerifyResponse : MOBResponse
    {
        public MileagePlusDetails MPDetails {  set; get; }
        public string Message { set; get; }
        public string MessageCode { set; get; }
    }
}
