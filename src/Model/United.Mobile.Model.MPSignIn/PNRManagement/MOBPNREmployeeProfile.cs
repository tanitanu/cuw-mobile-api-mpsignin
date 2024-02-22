using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.PNRManagement
{
    [Serializable]
    public class MOBPNREmployeeProfile
    {
        public string EmployeeID { get; set; }
        public string PassClassification { get; set; }
        public string CompanySeniorityDate { get; set; }
    }
}
