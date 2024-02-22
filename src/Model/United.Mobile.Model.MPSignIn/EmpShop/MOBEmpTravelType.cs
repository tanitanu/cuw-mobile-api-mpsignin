using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBEmpTravelType
    {
        private string termsAndConditions = string.Empty;
        private string verbiageDescription = string.Empty;



        public List<MOBEmpTravelTypeItem> EmpTravelTypes { get; set; }

        public bool IsTermsAndConditionsAccepted { get; set; }

        public int NumberOfPassengersInJA { get; set; }

        public string TermsAndConditions
        {
            get
            {
                return this.termsAndConditions;
            }
            set
            {
                this.termsAndConditions = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int TaxType { get; set; }

        public string VerbiageDescription
        {
            get
            {
                return this.verbiageDescription;
            }
            set
            {
                this.verbiageDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
