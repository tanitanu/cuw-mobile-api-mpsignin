using System;
using System.Xml.Serialization;

namespace United.Mobile.Model.Shopping
{
    [Serializable()]
    [XmlRoot("MOBSHOPFareLockProduct")]
    public class FareLockProduct
    {
        private string fareLockProductTitle = string.Empty;
        public string FareLockProductTitle
        {
            get { return fareLockProductTitle; }
            set { fareLockProductTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string fareLockProductAmountDisplayText = string.Empty;
        public string FareLockProductAmountDisplayText
        {
            get { return fareLockProductAmountDisplayText; }
            set { fareLockProductAmountDisplayText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public double FareLockProductAmount { get; set; } = 0.0;
        public string ProductCode { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        private string fareLockProductCode = string.Empty;

        public string FareLockProductCode
        {
            get { return fareLockProductCode; }
            set { fareLockProductCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

    }
}
