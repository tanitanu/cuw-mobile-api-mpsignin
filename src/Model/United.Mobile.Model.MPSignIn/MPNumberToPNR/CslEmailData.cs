using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MPSignIn.MPNumberToPNR
{
    public class CslEmailData
    {
        public List<Email> Emails { get; set; }
        public string CustomerId { get; set; }
        public string LoyaltyId { get; set; }
    }
    public class Email
    {
        public string TypeDescription { get; set; }
        public DateTime InsertDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
        public bool PrimaryIndicator { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsVerified { get; set; }
        public DateTime DiscontinuedDate { get; set; }
        public string Key { get; set; }
        public string Type { get; set; }
        public string OldType { get; set; } = null;
        public int SequenceNumber { get; set; }
        public string Address { get; set; }
        public string InsertId { get; set; }
        public string UpdateId { get; set; }
        public bool DayOfTravelNotification { get; set; }
        public string LanguageCode { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? WrongEmailDate { get; set; }
        public string Remark { get; set; }
        public bool IsDefault { get; set; }
    }
}
