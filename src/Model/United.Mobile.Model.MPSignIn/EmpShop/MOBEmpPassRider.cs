using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBEmpPassRider 
    {
        private string gender;
        private string dependantId;        
        private string firstBookingBuckets;

        public MOBEmpPassRiderExtended EmpPassRiderExtended { get; set; }

        public MOBEmpRelationship EmpRelationshipObject { get; set; }

        public MOBEmpName Name { get; set; }

        public string BirthDate { get; set; }

        public string Gender
        {
            get { return this.gender; }
            set { this.gender = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public int Age { get; set; } = -1;

        public bool UnaccompaniedFirst { get; set; } = true;

        public bool MustUseCurrentYearPasses { get; set; } = false;

        public string DependantID
        {
            get { return this.dependantId; }
            set { this.dependantId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string FirstBookingBuckets
        {
            get { return this.firstBookingBuckets; }
            set { this.firstBookingBuckets = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public bool PrimaryFriend { get; set; }
        public MOBEmpTCDInfo EmpTCDInfo { get; set; }
    }
}
