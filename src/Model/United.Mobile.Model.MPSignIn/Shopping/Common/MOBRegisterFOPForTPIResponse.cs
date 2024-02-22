using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBRegisterFOPForTPIResponse : MOBResponse
    {
        private string displayAmount;
        private string confirmationResponseMessage = string.Empty;
        private string confirmationResponseEmailMessage = string.Empty;
        private string confirmationResponseEmail = string.Empty;
        private string recordLocator = string.Empty;
        private string lastName = string.Empty;

        public double Amount { get; set; }
        public string DisplayAmount
        {
            get => this.displayAmount;
            set => this.displayAmount = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
        public string ConfirmationResponseMessage
        {
            get => this.confirmationResponseMessage;
            set => this.confirmationResponseMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string ConfirmationResponseEmailMessage
        {
            get => this.confirmationResponseEmailMessage;
            set => this.confirmationResponseEmailMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
        public string ConfirmationResponseEmail
        {
            get => this.confirmationResponseEmail;
            set => this.confirmationResponseEmail = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
        public List<string> ConfirmationResponseDetailMessage { get; set; }
        public string RecordLocator
        {
            get => this.recordLocator;
            set => this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string LastName
        {
            get => this.lastName;
            set => this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
    }
}
