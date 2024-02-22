using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.OneClickEnrollment
{
    public class CSLEnrollCustomer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        //public string MiddleName { get; set; }
        //public string Suffix { get; set; }
        public string Title { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        //public string Nationality { get; set; }
        public string CountryOfResidency { get; set; }
        public string EnrollmentSourceCode { get; set; }
        //public string UserId { get; set; }
        //public string Password { get; set; }
        public bool UseAddressValidation { get; set; }
        //public string LanguageCode { get; set; }
        public Address Address { get; set; }
        public Phone Phone { get; set; }
        public Email Email { get; set; }        
        public List<CSLMarketingPreference> MarketingPreferences { get; set; }
        public List<string> MarketingSubscriptions { get; set; }
        public List<CSLSecurityQuestion> SecurityQuestions { get; set; }
      
    }

    public class CSLEnrollUCB : CSLEnrollCustomer
    {
        public bool IsBatchEnrollment { get; set; }
        public bool ApplyDuplicateCheck { get; set; }
        public bool ValidateAddress { get; set; }
        public bool IsPartner { get; set; }
        public DateTime BirthDate { get; set; }
        public string SourceCode { get; set; }

    }
}
