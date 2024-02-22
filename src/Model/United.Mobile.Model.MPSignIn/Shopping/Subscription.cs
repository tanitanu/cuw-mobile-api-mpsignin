using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Common;

namespace United.Mobile.Model.Shopping
{
    public class Subscription : PersistBase, IPersist
    {
        public Subscription() { }
        public Subscription(string json, string objectType)
        {
            Json = json;
            ObjectType = objectType;
        }

        #region IPersist Members

        public string ObjectName { get; set; } = "United.Persist.Definition.Subscription.Subscription";

        #endregion


        public string MPNumber { get; set; } = "";

        public string CallDuration { get; set; } = "";

        public string RequestXml { get; set; } = "";

        public string ResponseXml { get; set; } = "";
    }
}
