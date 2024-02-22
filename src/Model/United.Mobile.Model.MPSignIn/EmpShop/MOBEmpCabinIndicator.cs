using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBEmpCabinIndicator
    {
        public int SeatCount { get; set; }
        public string CabinClass { get; set; } = string.Empty;
    }

}
