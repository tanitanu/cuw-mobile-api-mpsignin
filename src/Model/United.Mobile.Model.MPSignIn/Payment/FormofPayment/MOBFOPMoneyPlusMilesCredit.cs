using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping.FormofPayment
{
    [Serializable()]
    public class FOPMoneyPlusMilesCredit
    {

        public Section PromoCodeMoneyMilesAlertMessage { get; set; }
        public FOPMoneyPlusMiles SelectedMoneyPlusMiles { get; set; }


        public List<MOBMobileCMSContentMessages> ReviewMMCMessage { get; set; }

        public List<MOBMobileCMSContentMessages> MMCMessages { get; set; }

        public List<FOPMoneyPlusMiles> MilesPlusMoneyOptions { get; set; }

        public string TotalMilesAvailable { get; set; }

        public string MileagePlusTraveler { get; set; }

    }

}
