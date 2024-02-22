using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBInfoWarningMessages
    {
        public string Order { get; set; }

        public string IconType { get; set; }

        public List<string> Messages { get; set; }

        public string ButtonLabel { get; set; }
    
        public string HeaderMessage { get; set; }

        public bool IsCollapsable { get; set; }

        public bool IsExpandByDefault { get; set; }

    }
}
