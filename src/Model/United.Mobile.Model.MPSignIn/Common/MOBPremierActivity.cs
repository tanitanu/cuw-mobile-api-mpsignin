using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBPremierActivity
    {
        public string PremierActivityTitle { get; set; }
        public string PremierActivityYear { get; set; }
        public string PremierActivityStatus { get; set; }
        public MOBPremierQualifierTracker PQM { get; set; }
        public MOBPremierQualifierTracker PQS { get; set; }
        public MOBPremierQualifierTracker PQD { get; set; }
        public List<MOBKVP> KeyValueList { get; set; }
    }
}
