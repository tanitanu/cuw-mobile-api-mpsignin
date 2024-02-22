using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using United.Mobile.Model.Booking;
using United.Mobile.Model.Common;
using United.Mobile.Model.Common.Enum;

namespace United.Mobile.Model.MPSignIn
{
    [Serializable]
    public class MOBContactUsResponse : MOBResponse
    {
        public MOBContactUsUSACanada ContactUsUSACanada { get; set; }


        //private MOBInternationalCountryListInfo obj1;

        public MOBContactUSOutSideUSACanada ContactUSOutSideUSACanada { get; set; }

    }
    [Serializable()]
    public class MOBContactUsUSACanada
    {
        private string domesticContactTabHeaderText;
        public MOBContactUsUSACanada(IConfiguration configuration)
        {
            domesticContactTabHeaderText = configuration.GetValue<string>("DomesticContactUSTabHeaderText"); // U.S./Canada
        }
        public string DomesticContactTabHeaderText
        {
            get { return domesticContactTabHeaderText; }
            set { domesticContactTabHeaderText = System.Configuration.ConfigurationManager.AppSettings["DomesticContactUSTabHeaderText"]; }
        }

        public MOBContactUSUSACanadaContactTypePhone USACanadaContactTypePhone { get; set; }

        public MOBContactUSContactTypeEmail USACanadaContactTypeEmail { get; set; }

    }
    [Serializable]
    public class MOBContactUSUSACanadaContactTypePhone
    {

        public string ContactType { get; set; }

        public List<MOBContactUSUSACanadaPhoneNumber> PhoneNumbers { get; set; }
    }
    [Serializable]
    public class MOBContactUSContactTypeEmail
    {

        public string ContactType { get; set; }

        public List<MOBContactUSEmail> EmailAddresses { get; set; }

    }
    [Serializable]
    public class MOBContactUSUSACanadaPhoneNumber
    {
        private string contactUsDeskName = string.Empty;
        private string contactUsDeskDescription = string.Empty;
        private string contactUsDeskPhoneNumber = string.Empty;
        public string ContactUsDeskName
        {
            get
            {
                return this.contactUsDeskName;
            }
            set
            {
                this.contactUsDeskName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ContactUsDeskDescription
        {
            get
            {
                return this.contactUsDeskDescription;
            }
            set
            {
                this.contactUsDeskDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ContactUsDeskPhoneNumber
        {
            get
            {
                return this.contactUsDeskPhoneNumber;
            }
            set
            {
                this.contactUsDeskPhoneNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
    [Serializable]
    public class MOBContactUSEmail
    {
        private string contactUsDeskEmailName = string.Empty;
        private string contactUsDeskEmailAdress = string.Empty;
        public string ContactUsDeskEmailName
        {
            get
            {
                return this.contactUsDeskEmailName;
            }
            set
            {
                this.contactUsDeskEmailName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        [JsonProperty("contactUsDeskEmailAdress")]
        public string ContactUsDeskEmailAddress
        {
            get
            {
                return this.contactUsDeskEmailAdress;
            }
            set
            {
                this.contactUsDeskEmailAdress = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
    [Serializable()]
    public class MOBContactUSOutSideUSACanada
    {
        private string internationalTabHeaderText;
        public MOBContactUSOutSideUSACanada(IConfiguration configuration)
        {
            internationalTabHeaderText = configuration.GetValue<string>("InternationalContactUsTabHeaderText"); // Outside U.S./Canada
        }
        public string InternationalTabHeaderText
        {
            get { return internationalTabHeaderText; }
            set { internationalTabHeaderText = System.Configuration.ConfigurationManager.AppSettings["InternationalContactUsTabHeaderText"]; }
        }

        public string DefaultEmailAddressContactType { get; set; }

        public string InternaitonPhoneContactType { get; set; }

        /// <summary>
        /// Select a country to find an AT&T Direct Access Number.
        /// </summary>
        public string SelectCountryDefaultText { get; set; }

        /// <summary>
        /// For countries not listed , visit business.att.com/....
        /// </summary>
        public string SelectCountryFromListScreenText { get; set; }

        public string CountryListDefaultSelection { get; set; }

        public List<MOBContactUSEmail> InternationalDefaultEmailAddresses { get; set; }

        /// <summary>
        /// Dial the AT&T Direct Access Number for your city
        /// </summary>
        [JsonProperty("aTTAccessNumberDialInfoText")]
        public string ATTAccessNumberDialInfoText { get; set; }
        /// <summary>
        /// Once you dial the AT&T Direct Access Number, an English language voice prompt or an AT&T operator will ask you to enter this toll-free number
        /// </summary>
        private string howToUseOutsideUSACanadaATTDirectAccessNumberDescription = string.Empty;
        public string HowToUseOutsideUSACanadaATTDirectAccessNumberDescription
        {
            get
            {
                return this.howToUseOutsideUSACanadaATTDirectAccessNumberDescription;
            }
            set
            {
                this.howToUseOutsideUSACanadaATTDirectAccessNumberDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        private string outSideUSACanadaContactATTTollFreeNumber = string.Empty;
        public string OutSideUSACanadaContactATTTollFreeNumber
        {
            get
            {
                return this.outSideUSACanadaContactATTTollFreeNumber;
            }
            set
            {
                this.outSideUSACanadaContactATTTollFreeNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }


        public List<MOBContactUSOusideUSACanadaContactTypePhone> OutSideUSACanadaContactTypePhoneList { get; set; }

        public string ContactUSLocationDescription { get; set; }
        public string ContactUSLocationHyperlink { get; set; }
        public string ContactUSDirectAccessNumber { get; set; }

    }
    [Serializable]
    public class MOBContactUSOusideUSACanadaContactTypePhone
    {
        public MOBBKCountry Country { get; set; }

        public List<MOBContactAccessNumber> ContactAccessNumberList { get; set; } = null;
        // Define the internation phone number model. 
    }
    [Serializable()]
    public class MOBContactAccessNumber
    {
        private string city = string.Empty;

        public string City
        {
            get
            {
                return this.city;
            }
            set
            {
                this.city = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<string> ATTDirectAccessNumbers { get; set; }
    }
    [Serializable]
    public class MOBContactUsRequest : MOBRequest
    {
        //private string memberType;
        private string mileagePlusNumber = string.Empty;
        private string hashValue;

        public MOBMemberType MemberType { get; set; } = MOBMemberType.GENERAL;
        public Boolean IsCEO { get; set; }

    }
   
}
