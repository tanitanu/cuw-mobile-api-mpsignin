using United.Mobile.Model.Common;

namespace United.Mobile.Model.Common
{
    public class MOBModifyReservationRequest : MOBRequest
    {
        public string SessionId { get; set; } = string.Empty;

        public string HashPinCode { get; set; } = string.Empty;

        public string MileagePlusNumber { get; set; } = string.Empty;
    }
}
