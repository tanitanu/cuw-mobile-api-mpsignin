using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.DataAccess.Tools
{
    public class CatalogRequest
    {
        public string AccessCode { get; set; } = "ACCESSCODE";
        public Application Application { get; set; }
        public string DeviceId { get; set; }
        public string LanguageCode { get; set; } = "en-US";
        public string TransactionId { get; set; }
    }
    public class Application
    {
        public int Id { get; set; }
        public bool IsProduction { get; set; } = false;
        public string Name { get; set; } = "Android";
        public Version Version { get; set; }
    }

    public class Version
    {
        public string Major { get; set; } = "4.1.76";
        public string Minor { get; set; } = "4.1.76";
    }
}
