namespace United.Mobile.Model.MPSignIn
{
    public class OnePassDynamoDBRequest
    {
        public string MileagePlusNumber { get; set; }
        public string PinCode { get; set; }


        public string UnhashedPinCode { get; set; }
        public bool PINPWDSecurityPwdUpdate { get; set; }
    }
}
