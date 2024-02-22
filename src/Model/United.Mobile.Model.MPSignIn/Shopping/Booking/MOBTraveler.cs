using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.Shopping;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBTraveler
    {
        private string mileagePlusNumber;
        private string key;
        private string sharesPosition;
        private string allSeats;


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

        [XmlIgnore]
        public long CustomerId { get; set; }

        public MOBName Name { get; set; }

        public bool IsProfileOwner { get; set; }

        public List<MOBAddress> Addresses { get; set; }

        public List<MOBPhone> Phones { get; set; }

        public List<MOBEmail> Emails { get; set; }

        public List<MOBPaymentInfo> PaymentInfos { get; set; }

        public List<MOBPartnerCard> PartnerCards { get; set; }

        public List<MOBCreditCard> CreditCards { get; set; }

        public List<MOBSecureTraveler> SecureTravelers { get; set; }

        public List<MOBAirRewardProgram> AirRewardPrograms { get; set; }

        public string Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string SHARESPosition
        {
            get
            {
                return this.sharesPosition;
            }
            set
            {
                this.sharesPosition = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public List<MOBSeat> Seats { get; set; }

        public List<MOBSeatPrice> SeatPrices { get; set; }

        public string AllSeats
        {
            get
            {
                return this.allSeats;
            }
            set
            {
                if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(this.allSeats))
                    this.allSeats = "---";
                else if (string.IsNullOrEmpty(value))
                    this.allSeats += ", " + "---";
                else if (string.IsNullOrEmpty(this.allSeats))
                    this.allSeats = value;
                else
                    this.allSeats += ", " + value;
            }
        }

        public int CurrentEliteLevel { get; set; }

        public MOBEliteStatus EliteStatus { get; set; }

        public bool IsTSAFlagON { get; set; }

        public List<MOBPrefAirPreference> AirPreferences { get; set; }


        public List<MOBPrefContact> Contacts { get; set; }
    }
}
