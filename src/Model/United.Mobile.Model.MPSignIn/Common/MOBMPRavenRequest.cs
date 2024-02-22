using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MPSignIn.Common
{
    [Serializable()]
    public class MOBMPRavenRequest
    {

        public MOBMPRavenRequest()
            : base()
        {
        }
        public MOBMPRavenBody Body { get; set; } = new MOBMPRavenBody();

        public MOBMPRavenHeader Header { get; set; } = new MOBMPRavenHeader();
    }

    [Serializable()]
    public class MOBMPRavenHeader
    {
        public MOBMPRavenHeader()
            : base()
        {
        }

        public MOBMPRavenEventHeader EventHeader { get; set; } = new MOBMPRavenEventHeader();
    }

    [Serializable()]
    public class MOBMPRavenEventHeader
    {
        public MOBMPRavenEventHeader()
            : base()
        {
        }
        public string EventID { get; set; } = string.Empty;

        public string EventName { get; set; } = string.Empty;
        public string EventCreationSystem { get; set; } = string.Empty;
        public string EventCreationTime { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;
        public string EventSubtype { get; set; } = string.Empty;
        public string EventExpriationTime { get; set; } = string.Empty;
    }

    [Serializable()]
    public class MOBMPRavenBody
    {

        private string languageCode = string.Empty;

        public MOBMPRavenBody()
            : base()
        {
        }
        public string LanguageCode
        {
            get
            {
                return this.languageCode;
            }
            set
            {
                this.languageCode = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public MOBMPRavenContactData ContactData { get; set; } = new MOBMPRavenContactData();

        public MOBMPRavenPresentation Presentation { get; set; } = new MOBMPRavenPresentation();
    }

    [Serializable()]
    public class MOBMPRavenContactData
    {

        public MOBMPRavenContactData()
            : base()
        {
        }

        public MOBMPRavenEmail Email { get; set; } = new MOBMPRavenEmail();
    }

    [Serializable()]
    public class MOBMPRavenEmail
    {

        public MOBMPRavenEmail()
            : base()
        {
        }
        public List<MOBMPRavenAddress> Address { get; set; } = new List<MOBMPRavenAddress>();
    }

    [Serializable()]
    public class MOBMPRavenAddress
    {
        private string type = string.Empty;
        private string id = string.Empty;

        public MOBMPRavenAddress()
            : base()
        {
        }
        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }

    [Serializable()]
    public class MOBMPRavenPresentation
    {

        public MOBMPRavenPresentation()
            : base()
        {
        }

        public MOBMPRavenAdditionalDtl AdditionalDtl { get; set; } = new MOBMPRavenAdditionalDtl();
    }

    [Serializable()]
    public class MOBMPRavenAdditionalDtl
    {
        public MOBMPRavenAdditionalDtl()
            : base()
        {
        }
        public List<MOBMPRavenParameterEntry> ParameterEntry { get; set; } = new List<MOBMPRavenParameterEntry>();
    }

    [Serializable()]
    public class MOBMPRavenParameterEntry
    {
        private string name = string.Empty;
        private string value = string.Empty;

        public MOBMPRavenParameterEntry()
            : base()
        {
        }
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
