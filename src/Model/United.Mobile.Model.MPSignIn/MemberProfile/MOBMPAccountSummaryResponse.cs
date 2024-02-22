using System;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBMPAccountSummaryResponse : MOBResponse
    {
        //private bool isUASubscriptionsAvailable;

        public MOBMPAccountSummaryResponse()
            : base()
        {
        }


        public MOBMPAccountSummary OPAccountSummary { get; set; }

        public MOBProfile Profile { get; set; }

        public bool isUASubscriptionsAvailable { get; set; }
        //{
        //    get
        //    {
        //        return this.isUASubscriptionsAvailable;
        //    }
        //    set
        //    {
        //        this.isUASubscriptionsAvailable = value;
        //    }
        //}

        public MOBUASubscriptions UASubscriptions { get; set; }
    }
}
