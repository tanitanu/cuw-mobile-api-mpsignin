using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBCPProfileRequest : MOBRequest
    {

        public string Path { get; set; } = string.Empty;
        public bool IsProfileCallForFareLockScreen { get; set; }

        public bool IncludeAllTravelerData { get; set; }
        public bool IncludeAddresses { get; set; }
        public bool IncludeEmailAddresses { get; set; }
        public bool IncludePhones { get; set; }
        public bool IncludeCreditCards { get; set; }
        public bool IncludeSubscriptions { get; set; }
        public bool IncludeTravelMarkets { get; set; }
        public bool IncludeCustomerProfitScore { get; set; }
        public bool IncludePets { get; set; }
        public bool IncludeCarPreferences { get; set; }
        public bool IncludeDisplayPreferences { get; set; }
        public bool IncludeHotelPreferences { get; set; }
        public bool IncludeAirPreferences { get; set; }
        public bool IncludeContacts { get; set; }
        public bool IncludePassports { get; set; }
        public bool IncludeSecureTravelers { get; set; }
        public bool IncludeFlexEQM { get; set; }
        public bool IncludeServiceAnimals { get; set; }
        public bool IncludeSpecialRequests { get; set; }
        public bool IncludePosCountyCode { get; set; }

        public bool ProfileOwnerOnly { get; set; }
        public bool ReturnAllSavedTravelers { get; set; }
        private string cartId = string.Empty;
        private string sessionId = string.Empty;
        private string token = string.Empty;
        private string mileagePlusNumber = string.Empty;
        private string hashPinCode = string.Empty;

        public string CartId
        {
            get
            {
                return this.cartId;
            }
            set
            {
                this.cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SessionId
        {
            get
            {
                return sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Token
        {
            get
            {
                return token;
            }
            set
            {
                this.token = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MileagePlusNumber
        {
            get
            {
                return mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public int CustomerId { get; set; }

        public string HashPinCode
        {
            get
            {
                return hashPinCode;
            }
            set
            {
                this.hashPinCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ChaseAdType { get; set; }

        public string Flow { get; set; }
    }
}
