using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Shopping.PriceBreakDown
{
    [Serializable()]
    public class PriceBreakDownDetails
    {
        public PriceBreakDown2Items Trip { get; set; }

        public List<PriceBreakDown2TextItems> TaxAndFees { get; set; }

        public PriceBreakDownAddServices AdditionalServices { get; set; }

        public List<PriceBreakDown2Items> Total { get; set; }

        public List<PriceBreakDown2Items> FareLock { get; set; }

    }
}
