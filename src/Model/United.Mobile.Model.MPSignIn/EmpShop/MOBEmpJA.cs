using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Common
{
    public class MOBEmpJA
    {
        //private MOBEmployee employee;

        //public MOBEmployee MEmployee
        //{
        //    get { return this.employee; }
        //    set { this.employee = value; }
        //}

        public List<MOBEmpJAByAirline> EmpJAByAirlines { get; set; }

        public List<MOBEmpPassRider> EmpPassRiders { get; set; }

        public List<MOBEmpPassRider> EmpSuspendedPassRiders { get; set; }

        public List<MOBEmpBuddy> EmpBuddies { get; set; }

        public List<Delegate> Delegates { get; set; }

        public MOBEmpRelationship EmpRelationshipObject { get; set; }
    }


    public class GetEmpIdByMpNumber
    {
        public string MPNumber { get; set; }
        public string EmployeeId { get; set; }
        public string ContinentalEmployeeId { get; set; }
        public object MPFirstName { get; set; }
        public object MPLastName { get; set; }
        public object MPLinkedDate { get; set; }
        public object MPLinkedBy { get; set; }
        public string MPLinkedId { get; set; }
        public string FileNumber { get; set; }
    }

}
