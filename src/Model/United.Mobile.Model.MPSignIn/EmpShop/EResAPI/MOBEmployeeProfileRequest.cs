namespace United.Mobile.Model.Common
{

    public class MOBEmployeeProfileRequest
    {

        public string EmployeeId { get; set; }
        public bool IsPassRiderLoggedIn { get; set; }
        public bool IsLogOn { get; set; }
        public string PassRiderLoggedInID { get; set; }
        public string PassRiderLoggedInUser { get; set; }

    }
}
