using System;

namespace United.Mobile.Model.Shopping
{
    [Serializable()]
    public class UpgradePriceOption
    {
        public string Value { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string RewardCode { get; set; }
        public string EDDCode { get; set; }
    }
}
