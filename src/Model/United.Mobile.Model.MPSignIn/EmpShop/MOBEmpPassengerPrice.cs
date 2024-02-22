using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBEmpPassengerPrice
    {
        private string baseFare;
        private string destination;
        private string totalFare;

        public string BaseFare 
        {
            get
            {
                return baseFare;
            }
            set
            {
                this.baseFare = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string Destination
        {
            get
            {
                return destination;
            }
            set
            {
                this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public List<string> Errors { get; set; }
        public decimal RawBaseFare { get; set; }
        public List<MOBEmpTax> Taxes { get; set; }
        public string TotalFare
        {
            get
            {
                return totalFare;
            }
            set
            {
                this.totalFare = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public decimal TotalFareRaw { get; set; }
    }
}
