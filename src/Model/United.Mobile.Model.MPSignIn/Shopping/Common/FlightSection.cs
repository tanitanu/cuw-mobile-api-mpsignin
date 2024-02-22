using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Shopping
{
    [Serializable()]
    public class FlightSection
    {
        private string sectionName = string.Empty;

        public string SectionName
        {
            get
            {
                return this.sectionName;
            }
            set
            {
                this.sectionName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public decimal PriceFrom { get; set; }

        public List<MOBSHOPFlattenedFlight> FlattenedFlights { get; set; }

        public FlightSection()
        {
            FlattenedFlights = new List<MOBSHOPFlattenedFlight>();
        }
    }
}
