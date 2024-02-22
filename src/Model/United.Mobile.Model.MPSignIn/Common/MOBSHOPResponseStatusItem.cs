using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace United.Mobile.Model.Common
{
    public class MOBSHOPResponseStatusItem
    {
        public MOBSHOPResponseStatus Status { get; set; }

        public List<MOBItem> StatusMessages { get; set; }
    }
    
    
}
