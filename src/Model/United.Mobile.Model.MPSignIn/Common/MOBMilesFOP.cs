using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBMilesFOP
    {
        public MOBName Name { get; set; }
        public long CustomerId { get; set; }
        public string ProfileOwnerMPAccountNumber { get; set; } = string.Empty;
        public bool HasEnoughMiles { get; set; }
        public Int32 RequiredMiles { get; set; }
        public Int32 AvailableMiles { get; set; }
        public string DisplayRequiredMiles { get; set; } = string.Empty;
        public string DisplayAvailableMiles { get; set; } = string.Empty;
        public Int32 RemainingMiles { get; set; }
        public string DisplayRemainingMiles { get; set; } = string.Empty;
    }       
}
