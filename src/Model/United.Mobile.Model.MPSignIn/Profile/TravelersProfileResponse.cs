using System.Collections.Generic;
using United.Mobile.Model.CSLModels;

namespace United.Definition.CSLModels.CustomerProfile
{
    public class TravelersProfileResponse: Base
    {
        public List<TravelerProfileResponse> Travelers
        {
            get;
            set;
        }

    }
}
