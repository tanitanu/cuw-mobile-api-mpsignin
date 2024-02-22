using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Shopping.PriceBreakDown
{
    [Serializable()]
    public class PriceBreakDownSummary
    {
        public List<PriceBreakDown2Items> Trip { get; set; }

        public List<PriceBreakDown2Items> TravelOptions { get; set; }

        public List<PriceBreakDown2Items> Total { get; set; }

        public PriceBreakDown2Items FareLock { get; set; }
    }
}
