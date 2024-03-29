﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBCreditCard
    {
        private string key = string.Empty;
        private string cardType = string.Empty;
        private string cardTypeDescription = string.Empty;
        private string description = string.Empty;
        private string expireMonth = string.Empty;
        private string expireYear = string.Empty;
        private string unencryptedCardNumber = string.Empty;
        private string encryptedCardNumber = string.Empty;
        private string displayCardNumber = string.Empty;
        private string CIDCVV2 = string.Empty;
        private string ccName = string.Empty;
        private string addressKey = string.Empty;
        private string phoneKey = string.Empty;
        private string message = string.Empty;
        private string accountNumberToken = string.Empty;
        private string persistentToken = string.Empty;
        private string securityCodeToken = string.Empty;
        private string barCode = string.Empty;
        private string billedSeperateText;

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

        public string CardType
        {
            get
            {
                return this.cardType;
            }
            set
            {
                this.cardType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string CardTypeDescription
        {
            get
            {
                return this.cardTypeDescription;
            }
            set
            {
                this.cardTypeDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ExpireMonth
        {
            get
            {
                return this.expireMonth;
            }
            set
            {
                this.expireMonth = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ExpireYear
        {
            get
            {
                return this.expireYear;
            }
            set
            {
                this.expireYear = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsPrimary { get; set; }
        public string UnencryptedCardNumber
        {
            get { return this.unencryptedCardNumber; }
            set { this.unencryptedCardNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string EncryptedCardNumber
        {
            get { return this.encryptedCardNumber; }
            set { this.encryptedCardNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string DisplayCardNumber
        {
            get { return this.displayCardNumber; }
            set { this.displayCardNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string cIDCVV2
        {
            get { return this.CIDCVV2; }
            set { this.CIDCVV2 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        [JsonProperty("cCName")]
        public string cCName
        {
            get { return this.ccName; }
            set { this.ccName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string AddressKey
        {
            get
            {
                return this.addressKey;
            }
            set
            {
                this.addressKey = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PhoneKey
        {
            get
            {
                return this.phoneKey;
            }
            set
            {
                this.phoneKey = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
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
        public string AccountNumberToken
        {
            get
            {
                return this.accountNumberToken;
            }
            set
            {
                this.accountNumberToken = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string PersistentToken
        {
            get
            {
                return this.persistentToken;
            }
            set
            {
                this.persistentToken = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string SecurityCodeToken
        {
            get
            {
                return this.securityCodeToken;
            }
            set
            {
                this.securityCodeToken = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string BarCode
        {
            get
            {
                return this.barCode;
            }
            set
            {
                this.barCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public bool IsCorporate { get; set; }
        public bool IsMandatory { get; set; }
        public string BilledSeperateText
        {
            get
            {
                return this.billedSeperateText;
            }
            set
            {
                this.billedSeperateText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public bool IsValidForTPIPurchase { get; set; }
        public bool IsOAEPPaddingCatalogEnabled { get; set; }
    }

    [Serializable]
    public enum MOBFormofPayment
    {
        [EnumMember(Value = "CreditCard")]
        CreditCard,
        [EnumMember(Value = "PayPal")]
        PayPal,
        [EnumMember(Value = "PayPalCredit")]
        PayPalCredit,
        [EnumMember(Value = "ApplePay")]
        ApplePay,
        [EnumMember(Value = "Masterpass")]
        Masterpass,
        [EnumMember(Value = "VisaCheckout")]
        VisaCheckout,
        [EnumMember(Value = "MilesFOP")]
        MilesFormOfPayment,
        [EnumMember(Value = "ETC")]
        ETC,
        [EnumMember(Value = "Uplift")]
        Uplift,
        [EnumMember(Value = "FFC")]
        FFC,
        [EnumMember(Value = "TB")]
        TB
    }

}
