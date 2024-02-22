using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MPSignIn.MPNumberToPNR
{
    public class SeachMemberInfoRequest
    {
        private string firstName = string.Empty;
        private string lastName = string.Empty;
        private string emailAddress = string.Empty;
        private string dateOfBirth = string.Empty;
        private string countryOfResidence = string.Empty;
        private string phoneCountryCode = string.Empty;
        private string phoneAreaCode = string.Empty;
        private string phoneNumber = string.Empty;
        private string addressLine1 = string.Empty;
        private string city = string.Empty;
        private string state = string.Empty;
        private string postalCode = string.Empty;
        private string tierLevelCode = string.Empty;
        private string accountNumber = string.Empty;

        public string FirstName {
            get
            {
                return this.firstName;
            }
            set
            {
                this.firstName = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
        public string LastName {
            get
            {
                return this.lastName;
            }
            set
            {
                this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
        public string EmailAddress {
            get
            {
                return this.emailAddress;
            }
            set
            {
                this.emailAddress = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
        public string DateOfBirth
        {
            get
            {
                return this.dateOfBirth;
            }
            set
            {
                this.dateOfBirth = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
        public string CountryOfResidence {
            get
            {
                return this.countryOfResidence;
            }
            set
            {
                this.countryOfResidence = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
        public string PhoneCountryCode {
            get
            {
                return this.phoneCountryCode;
            }
            set
            {
                this.phoneCountryCode = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
        public string PhoneAreaCode {
            get
            {
                return this.phoneAreaCode;
            }
            set
            {
                this.phoneAreaCode = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
        public string PhoneNumber {
            get
            {
                return this.phoneNumber;
            }
            set
            {
                this.phoneNumber = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
        public string AddressLine1 {
            get
            {
                return this.addressLine1;
            }
            set
            {
                this.addressLine1 = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
        public string City {
            get
            {
                return this.city;
            }
            set
            {
                this.city = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
        public string State {
            get
            {
                return this.state;
            }
            set
            {
                this.state = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
        public string PostalCode {
            get
            {
                return this.postalCode;
            }
            set
            {
                this.postalCode = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
        public string TierLevelCode {
            get
            {
                return this.tierLevelCode;
            }
            set
            {
                this.tierLevelCode = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
        public string AccountNumber {
            get
            {
                return this.accountNumber;
            }
            set
            {
                this.accountNumber = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
    }

}
