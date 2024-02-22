using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBCorporateTravelType
    {

        public List<MOBCorporateTravelTypeItem> CorporateTravelTypes { get; set; }

        public bool CorporateCustomer { get; set; }

        public bool CorporateCustomerBEAllowed { get; set; }


        public MOBCorporateDetails CorporateDetails { get; set; }

    }
}
