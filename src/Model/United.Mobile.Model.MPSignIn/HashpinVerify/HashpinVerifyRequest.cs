using System.ComponentModel.DataAnnotations;

namespace United.Mobile.Model.MPSignIn.HashpinVerify
{
    public class HashpinVerifyRequest
    {
        [Required]
        public string HashValue { get; set; }
        [Required]
        public string MpNumber { get; set; }
        [Required]
        public string ServiceName { get; set; }  
        [Required]
        public string SessionID { get; set; }
        [Required]
        public string TransactionId { get; set; } = string.Empty;
        [Required]
        public string ApplicationId { get; set; }
        public string AppVersion { get; set; }
        [Required]
        public string DeviceId { get; set; } = string.Empty;


    }
}
