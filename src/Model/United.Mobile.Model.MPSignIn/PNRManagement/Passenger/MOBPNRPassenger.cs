using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;
using United.Service.Presentation.CommonModel;

namespace United.Mobile.Model.PNRManagement
{
    [Serializable()]
    public class MOBPNRPassenger
    {
        private string knownTravelerNumber;
        private string ktnDisplaySequence;
        private string redressNumber;
        private string redressDisplaySequence;
        private string ssrDisplaySequence;
        private string sharesPosition = string.Empty;
        private string sharesGivenName = string.Empty;
        private string birthDate = string.Empty;
        private string travelerTypeCode = string.Empty;
        private string pricingPaxType = string.Empty;
        public MOBLMXTraveler EarnedMiles { get; set; }

        public MOBContact Contact { get; set; }

        public string BirthDate
        {
            get => this.birthDate;
            set => this.birthDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string TravelerTypeCode
        {
            get => this.travelerTypeCode;
            set => this.travelerTypeCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string PricingPaxType
        {
            get => this.pricingPaxType;
            set => this.pricingPaxType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string SSRDisplaySequence
        {
            get => this.ssrDisplaySequence;
            set => this.ssrDisplaySequence = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
        }

        public List<MOBTravelSpecialNeed> SelectedSpecialNeeds { get; set; }

        public string SharesGivenName
        {
            get => this.sharesGivenName;
            set => this.sharesGivenName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        /// <summary>Customer Identification from PNR service.</summary>
        /// <value>The PNR customer identifier format as "LastName/FirstName+MiddleName+Title" [RILEY/LEIGHNIEMADR].</value>
        private string pnrCustomerID;

        public string PNRCustomerID
        {
            get => this.pnrCustomerID;
            set => this.pnrCustomerID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string KnownTravelerNumber { get => this.knownTravelerNumber; set => this.knownTravelerNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }

        public string RedressNumber { get => this.redressNumber; set => this.redressNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }

        public string KTNDisplaySequence { get => this.ktnDisplaySequence; set => this.ktnDisplaySequence = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }

        public string REDRESSDisplaySequence { get => this.redressDisplaySequence; set => this.redressDisplaySequence = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }


        public MOBName PassengerName { get; set; }

        public string SHARESPosition
        {
            get => this.sharesPosition;
            set => this.sharesPosition = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public MOBCPMileagePlus MileagePlus { get; set; }

        public List<MOBRewardProgram> OaRewardPrograms { get; set; }

        public bool IsMPMember { get; set; }

        public LoyaltyProgramProfile LoyaltyProgramProfile { get; set; }

        public MOBPNREmployeeProfile EmployeeProfile { get; set; }
    }
}
