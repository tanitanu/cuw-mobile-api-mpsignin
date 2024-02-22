using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MPSignIn.MPNumberToPNR
{
    public class MPSearchResponse : MOBResponse
    {
        public string Title { get; set; }
        public string Header { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public string AddMileagePlusButtonText { get; set; }
        public string TravelerNameLabel { get; set; }
        public string MileagePlusNumberLabel { get; set; }
        public string SessionId { get; set; }
        public Alert AlertInfo { get; set; }
        public List<TravelerInformation> TravelerInfo { get; set; }
    }

    public class Alert
    {
        //public bool IsError { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class TravelerInformation
    {
        public string TravelerName { get; set; }
        public string MileagePlusNumber { get; set; }
        public string MaskMileagePlusNumber { get; set; }
        public int CurrentTierLevel { get; set; }
        public string SharesPosition { get; set; }


    }

    public class SearchMPNumberInformation
    {
        public string SearchMpNumberError { get; set; }

    }
}
