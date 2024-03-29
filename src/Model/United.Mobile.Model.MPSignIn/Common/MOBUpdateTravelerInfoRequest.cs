﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition;
using United.Mobile.Model.PNRManagement;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBUpdateTravelerInfoRequest : MOBRequest
    {
        private string sessionId;
        private string token = string.Empty;
        private string recordLocator;

        public string SessionId { get { return this.sessionId; } set { this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }
        public string Token { get { return this.token; } set { this.token = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }
        public string RecordLocator { get { return this.recordLocator; } set { this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }
        public List<MOBPNRPassenger> TravelersInfo { get; set; }

    }
}
