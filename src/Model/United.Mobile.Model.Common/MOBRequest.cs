using System;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBRequest
    {
        public string AccessCode { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;
        public MOBApplication Application { get; set; }
        public string DeviceId { get; set; } = string.Empty;
    }
}