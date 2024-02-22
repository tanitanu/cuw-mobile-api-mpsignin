

using System.Collections.Generic;
using United.Mobile.Model.CSLModels;

namespace United.Definition.CSLModels.CustomerProfile
{
    public class SecureTravelerResponseData
    {
        public SecureTraveler SecureTraveler
        {
            get;
            set;
        }

        public List<SupplementaryTravelDocsDataMembers> SupplementaryTravelInfos
        {
            get;
            set;
        }
    }

}

