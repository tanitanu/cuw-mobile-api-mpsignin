using System.ComponentModel.DataAnnotations;

namespace United.Mobile.Model.MPSignIn.CCE
{
    public class Request : IRequest
    {
        [Required]
        public string TransactionId { get; set; }
    }
}
