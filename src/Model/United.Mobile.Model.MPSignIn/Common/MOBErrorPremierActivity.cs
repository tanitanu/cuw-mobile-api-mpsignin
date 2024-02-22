using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBErrorPremierActivity
    {
        public string ErrorPremierActivityTitle { get; set; }

        public string ErrorPremierActivityText { get; set; }

        public bool ShowErrorIcon { get; set; }
    }
}
