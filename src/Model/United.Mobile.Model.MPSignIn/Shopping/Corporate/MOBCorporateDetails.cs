using System;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBCorporateDetails
    {

        private string discountCode;
        public string DiscountCode
        {
            get { return discountCode; }
            set { discountCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string LeisureDiscountCode { get; set; }//For Corporate Leisure Break from business.

        private string corporateCompanyName;
        public string CorporateCompanyName
        {
            get { return corporateCompanyName; }
            set { corporateCompanyName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string corporateTravelProvider;
        public string CorporateTravelProvider
        {
            get { return corporateTravelProvider; }
            set { corporateTravelProvider = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string fareGroupId;

        public string FareGroupId
        {
            get { return fareGroupId; }
            set { fareGroupId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string corporateBookingType;

        public string CorporateBookingType
        {
            get { return corporateBookingType; }
            set { corporateBookingType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public int NoOfTravelers { get; set; }

        private bool isPersonalized;
        public bool IsPersonalized
        {
            get { return isPersonalized; }
            set { isPersonalized = value; }
        }

        private bool isMultiPaxAllowed;
        public bool IsMultiPaxAllowed
        {
            get { return isMultiPaxAllowed; }
            set { isMultiPaxAllowed = value; }
        }
    }
}
