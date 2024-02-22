using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping
{
    [Serializable]
    public class MOBSHOPShoppingProduct
    {
        private string productId = string.Empty;
        public string ProductId
        {
            get { return productId; }
            set { productId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string type = string.Empty;
        public string Type
        {
            get { return type; }
            set { type = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }


        private string subType = string.Empty;
        public string SubType
        {
            get { return subType; }
            set { subType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string longCabin = string.Empty;
        public string LongCabin
        {
            get { return longCabin; }
            set
            {
                longCabin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();

                // For backwards compatibility with releases before 17B
                // remove logic after force-upgrading all 17A and earlier clients
                if (string.IsNullOrEmpty(cabin))
                {
                    cabin = longCabin;
                }
            }
        }
        public string ReshopFees { get; set; } = string.Empty;

        public bool IsReshopCredit { get; set; }

        private CreditTypeColor reshopCreditColor;
        public CreditTypeColor ReshopCreditColor
        {
            get;
            set;
        }

        private string cabin = string.Empty;
        public string Cabin
        {
            get { return cabin; }
            set { cabin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string description = string.Empty;
        public string Description
        {
            get { return description; }
            set { description = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string price = string.Empty;
        public string Price
        {
            get { return price; }
            set { price = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string milesDisplayValue = string.Empty;
        public string MilesDisplayValue
        {
            get { return milesDisplayValue; }
            set { milesDisplayValue = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public decimal PriceAmount { get; set; } = 0;

        public decimal MilesDisplayAmount { get; set; } = 0;

        private string meal = string.Empty;
        public string Meal
        {
            get { return meal; }
            set { meal = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public MOBSHOPShoppingProductDetail ProductDetail { get; set; }

        public bool IsMixedCabin { get; set; } = false;

        public List<string> MixedCabinSegmentMessages { get; set; }

        private string awardType = string.Empty;
        public string AwardType
        {
            get { return awardType; }
            set { awardType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string allCabinButtonText = string.Empty;
        public string AllCabinButtonText
        {
            get { return allCabinButtonText; }
            set { allCabinButtonText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public bool IsSelectedCabin { get; set; } = false;

        public int MileageButton { get; set; } = -1;

        public int SeatsRemaining { get; set; } = -1;

        private string pqdText = string.Empty;
        public string PqdText
        {
            get { return pqdText; }
            set { pqdText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string pqmText = string.Empty;
        public string PqmText
        {
            get { return pqmText; }
            set { pqmText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string rdmText = string.Empty;
        public string RdmText
        {
            get { return rdmText; }
            set { rdmText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public bool ISPremierCabinSaver { get; set; } = false;

        public bool IsUADiscount { get; set; } = false;

        public bool IsELF { get; set; } = false;

        private string cabinType = string.Empty;
        public string CabinType
        {
            get { return cabinType; }
            set { cabinType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        private string priceFromText = string.Empty;
        public string PriceFromText
        {
            get { return priceFromText; }
            set { priceFromText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public bool IsIBELite { get; set; }

        public bool IsIBE { get; set; }

        private string shortProductName;

        /// <summary>
        /// Currently, only populated for IBELite product
        /// </summary>
        public string ShortProductName
        {
            get { return shortProductName; }
            set { shortProductName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string productCode;

        public string ProductCode
        {
            get { return productCode; }
            set { productCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public List<MOBStyledText> ProductBadges { get; set; }


        public string FareContentDescription { get; set; }


        public string ColumnID { get; set; }


        public string PriceApplyLabelText { get; set; }
        public string CabinDescription { get; set; }
        public string BookingCode { get; set; }


        public string StrikeThroughDisplayValue { get; set; }
    }
}
