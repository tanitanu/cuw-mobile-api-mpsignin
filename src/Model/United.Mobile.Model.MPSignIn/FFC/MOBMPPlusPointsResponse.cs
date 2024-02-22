using System;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBMPPlusPointsResponse : MOBResponse
    {

        public MOBMPPlusPointsResponse()
            : base()
        {
        }


        public MOBPlusPoints PluspointsDetails { get; set; }
    }
}
