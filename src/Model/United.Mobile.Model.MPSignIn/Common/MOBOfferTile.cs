using System;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBOfferTile
    {

        public string Title { get; set; }

        public string Text1 { get; set; }

        public string Text2 { get; set; }

        public string Text3 { get; set; }

        public decimal Price { get; set; }

        public string CurrencyCode { get; set; }
        public Int32 Miles { get; set; }
        public string DisplayMiles { get; set; }

        public bool ShowUpliftPerMonthPrice { get; set; }
    }
}
