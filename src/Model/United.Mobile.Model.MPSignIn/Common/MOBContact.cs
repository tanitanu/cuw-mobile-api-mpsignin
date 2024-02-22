using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBContact
    {
        public List<MOBEmail> Emails { get; set; }

        public List<MOBCPPhone> PhoneNumbers { get; set; }
    }
}
