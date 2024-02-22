using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common.MP2015
{
    [Serializable()]
    public class LmxFlight
    {
        private string flightNumber = string.Empty;
        private string scheduledDepartureDateTime = string.Empty;

        public MOBAirline MarketingCarrier { get; set; }

        public string FlightNumber
        {
            get
            {
                return this.flightNumber;
            }
            set
            {
                this.flightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBAirport Departure { get; set; }

        public MOBAirport Arrival { get; set; }

        public string ScheduledDepartureDateTime
        {
            get
            {
                return this.scheduledDepartureDateTime;
            }
            set
            {
                this.scheduledDepartureDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBLmxProduct> Products { get; set; }

        public bool NonPartnerFlight { get; set; }
    }
}