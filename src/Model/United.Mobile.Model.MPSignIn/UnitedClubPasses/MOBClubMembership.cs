using System;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBClubMembership
    {
        private string mpNumber = string.Empty;
        private string firstName = string.Empty;
        private string middleName = string.Empty;
        private string lastName = string.Empty;
        private string membershipTypeCode = string.Empty;
        private string membershipTypeDescription = string.Empty;
        private string effectiveDate = string.Empty;
        private string expirationDate = string.Empty;
        private string companionMPNumber = string.Empty;
        private string barCodeString;

        public string MPNumber
        {
            get => this.mpNumber;
            set => this.mpNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }

        public string FirstName
        {
            get => this.firstName;
            set => this.firstName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string MiddleName
        {
            get => this.middleName;
            set => this.middleName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string LastName
        {
            get => this.lastName;
            set => this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string MembershipTypeCode
        {
            get => this.membershipTypeCode;
            set => this.membershipTypeCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }

        public string MembershipTypeDescription
        {
            get => this.membershipTypeDescription;
            set => this.membershipTypeDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string EffectiveDate
        {
            get => this.effectiveDate;
            set => this.effectiveDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string ExpirationDate
        {
            get => this.expirationDate;
            set => this.expirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string CompanionMPNumber
        {
            get => this.companionMPNumber;
            set => this.companionMPNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
        }

        public bool IsPrimary { get; set; }

        public byte[] BarCode { get; set; }

        public string BarCodeString
        {
            get => this.barCodeString;
            set => this.barCodeString = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
    }
}
