using System.Collections.Generic;

namespace United.Mobile.Model.Shopping
{
    public class MOBOnScreenAlert
    {
        private string title = string.Empty;
        private string message = string.Empty;

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Message
        {
            get
            {
                return this.message;
            }
            set
            {
                this.message = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBOnScreenActions> Actions { get; set; }
    }
}
