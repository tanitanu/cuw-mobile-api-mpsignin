using System;

namespace United.Mobile.Model.Shopping
{
    [Serializable()]
    public class RTIMandateContentToDisplayByMarket
    {
        public string HeaderMsg { get; set; }

        public string BodyMsg { get; set; }

        public RTIMandateContentDetail MandateContentDetail { get; set; }
    }

    [Serializable()]
    public class RTIMandateContentDetail
    {
        public string NavigateToLinkLabel { get; set; }

        public string NavigatePageTitle { get; set; }

        public string NavigatePageBody { get; set; }
    }
}