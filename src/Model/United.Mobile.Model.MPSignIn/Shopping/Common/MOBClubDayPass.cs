using System;

namespace United.Mobile.Model.Shopping
{
    [Serializable]
    public class MOBClubDayPass
    {
        private string passCode = string.Empty;
        private string mileagePlusNumber = string.Empty;
        private string firstName = string.Empty;
        private string lastName = string.Empty;
        private string email = string.Empty;
        private string clubPassCode = string.Empty;
        private string purchaseDate = string.Empty;
        private string expirationDate = string.Empty;
        //public string expirationDateTime = string.Empty;



        public string PassCode
        {
            get
            {
                return this.passCode;
            }
            set
            {
                this.passCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string FirstName
        {
            get
            {
                return this.firstName;
            }
            set
            {
                this.firstName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LastName
        {
            get
            {
                return this.lastName;
            }
            set
            {
                this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Email
        {
            get
            {
                return this.email;
            }
            set
            {
                this.email = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ClubPassCode
        {
            get
            {
                return this.clubPassCode;
            }
            set
            {
                this.clubPassCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public double PaymentAmount { get; set; }

        public string PurchaseDate
        {
            get
            {
                return this.purchaseDate;
            }
            set
            {
                this.purchaseDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ExpirationDate
        {
            get
            {
                return this.expirationDate;
            }
            set
            {
                this.expirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public byte[] BarCode { get; set; }
        public EnElectronicClubPassesType ElectronicClubPassesType { get; set; }
        public string ExpirationDateTime { get; set; } = string.Empty;
    }

    [Serializable]
    public enum EnElectronicClubPassesType
    {
        PurchasedOTP = 0,
        ChaseCCOTP = 1,
        DefaultOTP = 2
    }

}
