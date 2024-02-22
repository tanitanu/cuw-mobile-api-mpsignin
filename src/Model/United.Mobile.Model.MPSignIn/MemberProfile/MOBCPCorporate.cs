using System;
using System.Collections.Generic;
using System.Text;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBCPCorporate
    {

        private string companyName = string.Empty;
        private string discountCode = string.Empty;
        private string leisureDiscountCode = string.Empty;
        private string fareGroupId = string.Empty;
        private string vendorName = string.Empty;
        private bool isPersonalized;
        private bool isMultiPaxAllowed;

        public int NoOfTravelers { get; set; }

        public string CorporateBookingType { get; set; }

        public string CompanyName
        {
            get
            {
                return companyName;
            }
            set
            {
                companyName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string DiscountCode
        {
            get
            {
                return discountCode;
            }
            set
            {
                discountCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LeisureDiscountCode
        {
            get
            {
                return leisureDiscountCode;
            }
            set
            {
                leisureDiscountCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<ErrorInfo> Errors { get; set; } = null;

        public string FareGroupId
        {
            get
            {
                return fareGroupId;
            }
            set
            {
                fareGroupId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsValid { get; set; }

        public long VendorId { get; set; }

        public string VendorName
        {
            get
            {
                return vendorName;
            }
            set
            {
                vendorName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsPersonalized
        {
            get { return isPersonalized; }
            set { isPersonalized = value; }
        }

        public bool IsMultiPaxAllowed
        {
            get { return isMultiPaxAllowed; }
            set { isMultiPaxAllowed = value; }
        }
    }
}
