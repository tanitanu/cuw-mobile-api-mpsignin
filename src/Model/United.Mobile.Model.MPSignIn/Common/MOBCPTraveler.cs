using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using United.Mobile.Model.Common.SSR;
using United.Mobile.Model.Internal.Common;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBCPTraveler
    {
        private string title = string.Empty;
        private string firstName = string.Empty;
        private string middleName = string.Empty;
        private string lastName = string.Empty;
        private string suffix = string.Empty;
        private string genderCode = string.Empty;
        private string birthDate = string.Empty;
        private string key = string.Empty;
        private int currentEliteLevel;
        private List<MOBEmail> emailAddresses;
        private List<MOBCPPhone> phones = new List<MOBCPPhone>();
        private List<MOBAddress> addresses = new List<MOBAddress>();
        private List<MOBPrefAirPreference> airPreferences = new List<MOBPrefAirPreference>();
        private List<MOBCreditCard> creditCards = new List<MOBCreditCard>();
        private string travelerTypeCode = string.Empty;
        private string travelerTypeDescription = string.Empty;
        private string travelProgramMemberId = string.Empty;
        private string knownTravelerNumber = string.Empty;
        private string redressNumber = string.Empty;

        private string ownerFirstName = string.Empty;
        private string ownerLastName = string.Empty;
        private string ownerMiddleName = string.Empty;
        private string ownerSuffix = string.Empty;
        private string ownerTitle = string.Empty;
        private string message = string.Empty;
        private string mpNameNotMatchMessage = string.Empty;
        private List<MOBEmail> reservationEmailAddresses;
        private List<MOBCPPhone> reservationPhones =new List<MOBCPPhone>();
        private string _employeeId = string.Empty;

        public string TotalFFCNewValueAfterRedeem { get; set; } = string.Empty;

        public string PNRCustomerID { get; set; }

        public List<MOBFOPFutureFlightCredit> FutureFlightCredits { get; set; }

        public string CslReservationPaxTypeCode { get; set; }


        public double IndividualTotalAmount { get; set; }

        public string PTCDescription { get; set; } = string.Empty; //Passenger type code description

        public bool IsEligibleForSeatSelection { get; set; } = true;

        public string Nationality { get; set; }

        public string CountryOfResidence { get; set; }

        //[JsonProperty("_employeeId")]
        public string EmployeeId
        {
            get { return this._employeeId; }
            set { this._employeeId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }

        }

        public MOBCPCubaSSR CubaTravelReason { get; set; }


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

        public string MiddleName
        {
            get
            {
                return this.middleName;
            }
            set
            {
                this.middleName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public string Suffix
        {
            get
            {
                return this.suffix;
            }
            set
            {
                this.suffix = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string GenderCode
        {
            get
            {
                return this.genderCode;
            }
            set
            {
                this.genderCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string BirthDate
        {
            get
            {
                return this.birthDate;
            }
            set
            {
                this.birthDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsProfileOwner { get; set; }

        public bool IsDeceased { get; set; }

        public bool IsExecutive { get; set; }

        public int CurrentEliteLevel { get; set; }
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

        public int CustomerId { get; set; }

        public int ProfileId { get; set; }

        public int ProfileOwnerId { get; set; }

        public MOBCPMileagePlus MileagePlus { get; set; }

        public List<MOBCPSecureTraveler> SecureTravelers { get; set; }

        public List<MOBBKLoyaltyProgramProfile> AirRewardPrograms { get; set; }

        public List<MOBCPPhone> Phones
        {
            get
            {
                return phones;
            }
            set
            {
                if (value != null)
                {
                    phones = value;
                }
            }
        }

        public List<MOBAddress> Addresses
        {
            get
            {
                return addresses;
            }
            set
            {
                if (value != null)
                {
                    addresses = value;
                }
            }
        }

        public List<MOBPrefAirPreference> AirPreferences
        {
            get
            {
                return airPreferences;
            }
            set
            {
                if (value != null)
                {
                    airPreferences = value;
                }
            }
        }

        public List<MOBEmail> EmailAddresses
        {
            get
            {
                return emailAddresses;
            }
            set
            {
                if (value != null)
                {
                    emailAddresses = value;
                }
            }
        }

        public List<MOBCreditCard> CreditCards
        {
            get
            {
                return creditCards;
            }
            set
            {
                if (value != null)
                {
                    creditCards = value;
                }
            }
        }

        public string TravelerTypeCode
        {
            get
            {
                return this.travelerTypeCode;
            }
            set
            {
                this.travelerTypeCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TravelerTypeDescription
        {
            get
            {
                return this.travelerTypeDescription;
            }
            set
            {
                this.travelerTypeDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TravelProgramMemberId
        {
            get
            {
                return this.travelProgramMemberId;
            }
            set
            {
                this.travelProgramMemberId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string KnownTravelerNumber
        {
            get
            {
                return this.knownTravelerNumber;
            }
            set
            {
                this.knownTravelerNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string RedressNumber
        {
            get
            {
                return this.redressNumber;
            }
            set
            {
                this.redressNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OwnerFirstName
        {
            get
            {
                return this.ownerFirstName;
            }
            set
            {
                this.ownerFirstName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OwnerLastName
        {
            get
            {
                return this.ownerLastName;
            }
            set
            {
                this.ownerLastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OwnerMiddleName
        {
            get
            {
                return this.ownerMiddleName;
            }
            set
            {
                this.ownerMiddleName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OwnerSuffix
        {
            get
            {
                return this.ownerSuffix;
            }
            set
            {
                this.ownerSuffix = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OwnerTitle
        {
            get
            {
                return this.ownerTitle;
            }
            set
            {
                this.ownerTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int PaxIndex { get; set; }

        public List<MOBSeat> Seats { get; set; }

        public MOBUASubscriptions Subscriptions { get; set; }

        public string TravelerNameIndex { get; set; }

        public bool IsTSAFlagON { get; set; }

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
        public string MPNameNotMatchMessage
        {
            get
            {
                return this.mpNameNotMatchMessage;
            }
            set
            {
                this.mpNameNotMatchMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public bool isMPNameMisMatch { get; set; } = false;

        public List<MOBEmail> ReservationEmailAddresses
        {
            get
            {
                return reservationEmailAddresses;
            }
            set
            {
                if (value != null)
                {
                    reservationEmailAddresses = value;
                }
            }
        }

        public List<MOBCPPhone> ReservationPhones
        {
            get
            {
                return reservationPhones;
            }
            set
            {
                if (value != null)
                {
                    reservationPhones = value;
                }
            }
        }
        public MOBMPSecurityUpdate MPSecurityUpdate { get; set; }

        public MOBCPCustomerMetrics CustomerMetrics { get; set; }

        public int PaxID { get; set; }

        public bool IsPaxSelected { get; set; }

        public List<MOBTravelSpecialNeed> SelectedSpecialNeeds { get; set; }

        public List<MOBItem> SelectedSpecialNeedMessages { get; set; }

        public bool IsMustRideTraveler { get; set; }
        public string PartnerRPCIds { get; set; }

    }
}
