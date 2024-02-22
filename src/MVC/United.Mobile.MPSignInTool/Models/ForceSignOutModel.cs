using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using United.Mobile.MPSignInTool.Domain;

namespace United.Mobile.MPSignInTool.Models
{
    public class ForceSignOutModel
    {
        public string MileagePlusNumber { get; set; }
        public string Environment { get; set; } = "DevelopmentClient";
        public List<SelectListItem> Environments { get; set; }
        public List<MPSignInDynamoRecords> Records { get; set; }        
    }
}