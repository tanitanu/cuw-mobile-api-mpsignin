using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBProfile
    {
        private string mileagePlusNumber = string.Empty;

        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string CustomerId { get; set; }

        [XmlIgnore]
        public int ProfileId { get; set; }

        public MOBName OwnerName { get; set; }

        public List<MOBTraveler> Travelers { get; set; }

        public bool IsOneTimeProfileUpdateSuccess { get; set; }

        public bool IsProfileOwnerTSAFlagON { get; set; }

        public List<MOBTypeOption> DisclaimerList { get; set; } = null;
    }
}
