using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBCPMileagePlus
    {
        private string activeStatusCode = string.Empty;
        private string activeStatusDescription = string.Empty;
        private string closedStatusCode = string.Empty;
        private string closedStatusDescription = string.Empty;
        private string currentEliteLevelDescription = string.Empty;
        private string encryptedPin = string.Empty;
        private string enrollDate = string.Empty;
        private string enrollSourceCode = string.Empty;
        private string enrollSourceDescription = string.Empty;
        private string futureEliteDescription = string.Empty;
        private string instantEliteExpirationDate = string.Empty;
        private string key = string.Empty;
        private string lastActivityDate = string.Empty;
        private string lastFlightDate = string.Empty;
        private string lastStatementDate = string.Empty;
        private string mileageExpirationDate = string.Empty;
        private string nextYearEliteLevelDescription = string.Empty;
        private string mileagePlusId = string.Empty;
        private string mileagePlusPin = string.Empty;
        private string priorUnitedAccountNumber = string.Empty;
        private string vendorCode;
        private string premierLevelExpirationDate = string.Empty;
        private string travelBankAccountNumber;
        private string travelBankCurrencyCode;
        private string travelBankExpirationDate;

        public int AccountBalance { get; set; }

        public string ActiveStatusCode
        {
            get
            {
                return this.activeStatusCode;
            }
            set
            {
                this.activeStatusCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ActiveStatusDescription
        {
            get
            {
                return this.activeStatusDescription;
            }
            set
            {
                this.activeStatusDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public double TravelBankBalance { get; set; }
        public int AllianceEliteLevel { get; set; }

        public string ClosedStatusCode
        {
            get
            {
                return this.closedStatusCode;
            }
            set
            {
                this.closedStatusCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ClosedStatusDescription
        {
            get
            {
                return this.closedStatusDescription;
            }
            set
            {
                this.closedStatusDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int CurrentEliteLevel { get; set; }

        public string CurrentEliteLevelDescription
        {
            get
            {
                return this.currentEliteLevelDescription;
            }
            set
            {
                this.currentEliteLevelDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public decimal CurrentYearMoneySpent { get; set; }

        public int EliteMileageBalance { get; set; }

        public int EliteSegmentBalance { get; set; }

        public int EliteSegmentDecimalPlaceValue { get; set; }

        public string EncryptedPin
        {
            get
            {
                return this.encryptedPin;
            }
            set
            {
                this.encryptedPin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EnrollDate
        {
            get
            {
                return this.enrollDate;
            }
            set
            {
                this.enrollDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EnrollSourceCode
        {
            get
            {
                return this.enrollSourceCode;
            }
            set
            {
                this.enrollSourceCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EnrollSourceDescription
        {
            get
            {
                return this.enrollSourceDescription;
            }
            set
            {
                this.enrollSourceDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int FlexPqmBalance { get; set; }

        public int FutureEliteLevel { get; set; }

        public string FutureEliteDescription
        {
            get
            {
                return this.futureEliteDescription;
            }
            set
            {
                this.futureEliteDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string InstantEliteExpirationDate
        {
            get
            {
                return this.instantEliteExpirationDate;
            }
            set
            {
                this.instantEliteExpirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsCEO { get; set; }

        public bool IsClosedPermanently { get; set; }

        public bool IsClosedTemporarily { get; set; }

        public bool IsCurrentTrialEliteMember { get; set; }

        public bool IsFlexPqm { get; set; }

        public bool IsInfiniteElite { get; set; }

        public bool IsLifetimeCompanion { get; set; }

        public bool IsLockedOut { get; set; }

        public bool IsMergePending { get; set; }

        public bool IsUnitedClubMember { get; set; }

        public bool IsPresidentialPlus { get; set; }

        public string Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LastActivityDate
        {
            get
            {
                return this.lastActivityDate;
            }
            set
            {
                this.lastActivityDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int LastExpiredMile { get; set; }

        public string LastFlightDate
        {
            get
            {
                return this.lastFlightDate;
            }
            set
            {
                this.lastFlightDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int LastStatementBalance { get; set; }

        public string LastStatementDate
        {
            get
            {
                return this.lastStatementDate;
            }
            set
            {
                this.lastStatementDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int LifetimeEliteLevel { get; set; }

        public int LifetimeEliteMileageBalance { get; set; }

        public string MileageExpirationDate
        {
            get
            {
                return this.mileageExpirationDate;
            }
            set
            {
                this.mileageExpirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int NextYearEliteLevel { get; set; }

        public string NextYearEliteLevelDescription
        {
            get
            {
                return this.nextYearEliteLevelDescription;
            }
            set
            {
                this.nextYearEliteLevelDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MileagePlusId
        {
            get
            {
                return this.mileagePlusId;
            }
            set
            {
                this.mileagePlusId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MileagePlusPin
        {
            get
            {
                return this.mileagePlusPin;
            }
            set
            {
                this.mileagePlusPin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PriorUnitedAccountNumber
        {
            get
            {
                return this.priorUnitedAccountNumber;
            }
            set
            {
                this.priorUnitedAccountNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int StarAllianceEliteLevel { get; set; }

        public int MpCustomerId { get; set; }

        public string VendorCode
        {
            get { return vendorCode; }
            set { vendorCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string PremierLevelExpirationDate
        {
            get
            {
                return this.premierLevelExpirationDate;
            }
            set
            {
                this.premierLevelExpirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        

        public MOBInstantElite InstantElite { get; set; }

        public string TravelBankAccountNumber { get => travelBankAccountNumber; set => travelBankAccountNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        public string TravelBankCurrencyCode { get => travelBankCurrencyCode; set => travelBankCurrencyCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        public string TravelBankExpirationDate { get => travelBankExpirationDate; set => travelBankExpirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
    }
}
