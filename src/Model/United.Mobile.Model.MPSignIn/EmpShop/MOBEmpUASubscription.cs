using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBEmpUASubscription
    {
        public MOBEmpUASubscription()
            : base()
        {
        }

        public List<MOBItem> Items { get; set; }
    }
}
