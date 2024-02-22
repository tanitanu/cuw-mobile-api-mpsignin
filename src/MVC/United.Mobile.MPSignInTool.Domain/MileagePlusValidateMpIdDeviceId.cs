namespace United.Mobile.MPSignInTool.Domain
{
    public class MileagePlusValidateMpIdDeviceId
    {
        public string MileagePlusNumber { get; set; }
        public string MPUserName { get; set; }
        public string HashPincode { get; set; }
        public string PinCode { get; set; }
        public string ApplicationID { get; set; }
        public string AppVersion { get; set; }
        public string DeviceID { get; set; }
        public string IsTokenValid { get; set; }
        public string TokenExpireDateTime { get; set; }
        public string TokenExpiryInSeconds { get; set; }
        public string IsTouchIDSignIn { get; set; }
        public string IsTokenAnonymous { get; set; }
        public string CustomerID { get; set; }
        public string DataPowerAccessToken { get; set; }
        public string UpdateDateTime { get; set; }
    }
}