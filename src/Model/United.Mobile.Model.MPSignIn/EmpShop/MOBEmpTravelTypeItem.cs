using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBEmpTravelTypeItem
    {
        private string travelType;
        private string travelTypeDescription;
        private string advisory;


        public bool IsEligible { get; set; }

        public bool IsAuthorizationRequired { get; set; }

        public int NumberOfTravelers { get; set; }

        public string TravelType
        {
            get
            {
                return travelType;
            }
            set
            {
                travelType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TravelTypeDescription
        {
            get
            {
                return travelTypeDescription;
            }
            set
            {
                travelTypeDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Advisory
        {
            get
            {
                return advisory;
            }
            set
            {
                advisory = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public int AdvanceBookingDays { get; set; }

    }
}
