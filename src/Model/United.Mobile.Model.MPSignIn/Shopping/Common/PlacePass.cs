using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Shopping
{
    [Serializable()]
    public class PlacePass
    {
        public string ObjectName { get; set; } = "United.Definition.Shopping";

        public int PlacePassID { get; set; }

        public string Destination { get; set; }

        public string TxtPoweredBy { get; set; }

        public string TxtPlacepass { get; set; }

        public string PlacePassImageSrc { get; set; }

        public string MobileImageUrl { get; set; }

        public string OfferDescription { get; set; }

        public string PlacePassUrl { get; set; }

    }
}
