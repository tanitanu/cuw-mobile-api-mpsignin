using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBUASubscription
    {
        public MOBUASubscription()
            : base()
        {
        }

        public List<MOBItem> Items { get; set; }
    }
}
