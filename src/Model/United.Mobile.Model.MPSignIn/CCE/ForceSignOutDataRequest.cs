using System.ComponentModel.DataAnnotations;

namespace United.Mobile.Model.MPSignIn.CCE
{
    public class ForceSignOutDataRequest : Request
    {
        [Required]
        public string TableName { get; set; }
        [Required]
        public string SecondaryKey { get; set; }
    }
}
