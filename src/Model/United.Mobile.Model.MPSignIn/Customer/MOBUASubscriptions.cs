﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBUASubscriptions
    {

        public string MPAccountNumber { get; set; } = string.Empty;

        public List<MOBUASubscription> SubscriptionTypes { get; set; }

        public MOBUASubscriptions()
        {
        }
    }
}
