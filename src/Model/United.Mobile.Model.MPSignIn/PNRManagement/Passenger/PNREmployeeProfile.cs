using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class PNREmployeeProfile
    {
        public string EmployeeID { get; set; }

        public string PassClassification { get; set; }

        public string CompanySeniorityDate { get; set; }
    }
}
