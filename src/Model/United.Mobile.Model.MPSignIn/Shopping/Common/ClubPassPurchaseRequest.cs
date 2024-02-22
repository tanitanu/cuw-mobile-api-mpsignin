using System;

namespace United.Mobile.Model.Shopping
{
    [Serializable]
    public class ClubPassPurchaseRequest
    {

        private string firstName = string.Empty;
        private string lastName = string.Empty;
        private string email = string.Empty;
        private string mileagePlusNumber = string.Empty;
        public string FirstName
        {
            get => this.firstName;
            set => this.firstName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string LastName
        {
            get => this.lastName;
            set => this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string Email
        {
            get => this.email;
            set => this.email = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string MileagePlusNumber
        {
            get => this.mileagePlusNumber;
            set => this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }

        public int NumberOfPasses { get; set; }

        public double AmountPaid { get; set; }

        public bool IsTestSystem { get; set; }

    }
}
