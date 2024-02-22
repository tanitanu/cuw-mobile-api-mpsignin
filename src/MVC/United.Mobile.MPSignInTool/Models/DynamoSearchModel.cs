using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace United.Mobile.MPSignInTool.Models
{
    public class DynamoSearchModel
    {
        public string TransactionId { get; set; }
        public string TableName { get; set; } = "cuw-validate-mp-appid-deviceid";
        public string SecondaryKey { get; set; }
        public string Environment { get; set; }
        public List<SelectListItem> Environments { get; set; }
    }
}