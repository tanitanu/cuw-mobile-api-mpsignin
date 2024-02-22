using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBPremierQualifierTracker
    {
        public string PremierQualifierTrackerTitle { get; set; }
        public string PremierQualifierTrackerCurrentValue { get; set; }
        public string PremierQualifierTrackerCurrentChaseFlexValue { get; set; }
        public string PremierQualifierTrackerCurrentChaseFlexText { get; set; }
        public string PremierQualifierTrackerCurrentText { get; set; }
        public string PremierQualifierTrackerCurrentFlexValue { get; set; }
        public string PremierQualifierTrackerCurrentFlexTitle { get; set; }
        public string PremierQualifierTrackerThresholdValue { get; set; }
        public string PremierQualifierTrackerThresholdText { get; set; }
        public bool IsWaived { get; set; }
        public string Separator { get; set; }
        public string PremierQualifierTrackerThresholdPrefix { get; set; }
    }
}
