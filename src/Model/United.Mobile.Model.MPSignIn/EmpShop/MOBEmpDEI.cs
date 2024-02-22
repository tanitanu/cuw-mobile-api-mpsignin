using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBEmpDEI
    {
        public List<MOBEmpCabinIndicator> CabinIndicators { get; set; }

        public string ServiceClasses { get; set; }

        public string MarketingCarrierName { get; set; }

        public string OperatingCarrierName { get; set; }

    }
}
