using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Shopping.PriceBreakDown
{
    [Serializable()]
    public class PriceBreakDownAddServices
    {
        public List<PriceBreakDown4Items> Seats { get; set; }

        public List<PriceBreakDown3Items> PremiumAccess { get; set; }

        public List<PriceBreakDown2Items> OneTimePass { get; set; }


    }
}
