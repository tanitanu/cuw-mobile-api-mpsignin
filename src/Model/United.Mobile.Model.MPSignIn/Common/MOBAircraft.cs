using System;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBAircraft
    {
        private string code = string.Empty;
        private string shortName = string.Empty;
        private string longName = string.Empty;
        private string modelCode = string.Empty;

        public MOBAircraft()
        {
        }

        public string Code
        {
            get
            {
                return this.code;
            }
            set
            {
                this.code = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ShortName
        {
            get
            {
                return this.shortName;
            }
            set
            {
                this.shortName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LongName
        {
            get
            {
                return this.longName;
            }
            set
            {
                this.longName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ModelCode
        {
            get
            {
                return this.modelCode;
            }
            set
            {
                this.modelCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
