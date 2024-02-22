using System;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBEmpBookingLapChild
    {
        private string gender;
        private string bday;
        private string redress;
        private string paxID;

        public string PaxID
        {
            get
            {
                return paxID;
            }
            set
            {
                paxID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Bday
        {
            get
            {
                return bday;
            }
            set
            {
                bday = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Redress
        {
            get
            {
                return redress;
            }
            set
            {
                redress = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int Age { get; set; }

        public string Gender
        {
            get
            {
                return gender;
            }
            set
            {
                gender = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBEmpName Name { get; set; }
    }
}
