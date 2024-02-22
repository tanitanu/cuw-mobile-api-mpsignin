using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;
using United.Mobile.Model.Shopping;
using Newtonsoft.Json;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBMPPINPWDValidateResponse : MOBResponse
    {
        [JsonIgnore()]
        public string ObjectName { get; set; } = "United.Definition.MPPINPWD.MOBMPPINPWDValidateResponse";
        private readonly IConfiguration _configuration;
        public MOBMPPINPWDValidateResponse()
            : base()
        {
        }

        public MOBMPPINPWDValidateResponse(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public MOBYoungAdultTravelType YoungAdultTravelType { get; set; }
        public MOBSHOPResponseStatusItem ResponseStatusItem { get; set; }

        public MOBMPAccountValidationRequest Request { get; set; }

        public MOBMPAccountValidation AccountValidation { get; set; }

        public bool SecurityUpdate { get; set; }
        public string KtnNumber { get; set; }       
        public string PartnerRPCIds { get; set; }       

        public MOBMPPINPWDSecurityUpdateDetails MPSecurityUpdateDetails { get; set; }

        private string sessionID = string.Empty;
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public List<MOBItem> LandingPageMessages { get; set; }

        public bool isUASubscriptionsAvailable { get; set; }
        public MOBMPAccountSummary OPAccountSummary { get; set; }

        public MOBProfile Profile { get; set; }


        public MOBUASubscriptions UASubscriptions { get; set; }

        public bool IsExpertModeEnabled { get; set; }

        private string employeeId = string.Empty;
        public string EmployeeId
        {
            get
            {
                return this.employeeId;
            }
            set
            {
                this.employeeId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        private string displayEmployeeId = string.Empty;
        public string DisplayEmployeeId
        {
            get
            {
                return this.displayEmployeeId;
            }
            set
            {
                this.displayEmployeeId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public MOBEmpTravelTypeResponse EmpTravelTypeResponse { get; set; }


        public MOBCorporateTravelType CorporateEligibleTravelType { get; set; }

        private MOBMPTFARememberMeFlags rememberMEFlags;
        public MOBMPTFARememberMeFlags RememberMEFlags
        {
            get
            {
                if (rememberMEFlags == null)
                {
                    rememberMEFlags = new MOBMPTFARememberMeFlags(_configuration);
                }
                return this.rememberMEFlags;
            }
            set
            {
                this.rememberMEFlags = value;
            }
        }

        public bool ShowContinueAsGuestButton { get; set; }

        public MOBCPCustomerMetrics CustomerMetrics { get; set; }
    }

    #region MP Enroll

    [Serializable()]
    public class MOBMPEnRollmentRequest : MOBRequest
    {
        private readonly IConfiguration _configuration;
        public MOBMPEnRollmentRequest()
            : base()
        {

        }

        private string sessionID = string.Empty;
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public bool GetSecurityQuestions { get; set; }

        public MOBMPMPEnRollmentDetails MPEnrollmentDetails { get; set; }
    }

    [Serializable()]
    public class MOBMPMPEnRollmentResponse : MOBResponse
    {
        private readonly IConfiguration _configuration;
        public MOBMPMPEnRollmentResponse()
            : base()
        {
            if (_configuration != null)
            {
                needQuestionsCount = _configuration.GetValue<int>("NumberOfSecurityQuestionsNeedatPINPWDUpdate");
            }
        }
        private string mileagePlusNumber = string.Empty;
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

        public MOBMPEnRollmentRequest Request { get; set; }

        private string sessionID = string.Empty;
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private string mpEnrollMentCompleteMessage;
        public string MPEnrollMentCompleteMessage
        {
            get
            {
                return this.mpEnrollMentCompleteMessage;
            }
            set
            {
                this.mpEnrollMentCompleteMessage = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public MOBYoungAdultTravelType YoungAdultTravelType { get; set; }

        public MOBMPAccountSummary OPAccountSummary { get; set; }

        private int needQuestionsCount;
        public int NeedQuestionsCount
        {
            get
            {
                return this.needQuestionsCount;
            }
            set
            {
                this.needQuestionsCount = value;
            }
        }

        public List<Securityquestion> SecurityQuestions { get; set; }

        public List<MOBItem> MPEnrollmentMessages { get; set; }

        public MOBMPAccountValidation AccountValidation { get; set; }

        private MOBMPTFARememberMeFlags rememberMEFlags;
        public MOBMPTFARememberMeFlags RememberMEFlags
        {
            get
            {
                if (rememberMEFlags == null)
                {
                    rememberMEFlags = new MOBMPTFARememberMeFlags();
                }
                return this.rememberMEFlags;
            }
            set
            {
                this.rememberMEFlags = value;
            }
        }
    }

    [Serializable()]
    public class MOBMPMPEnRollmentDetails
    {
        public MOBMPMPEnRollmentDetails()
            : base()
        {
        }

        public MOBMPEnrollmentPersonalInfo PersonalInformation { get; set; }

        public MOBMPEnrollmentContactInfo ContactInformation { get; set; }

        public MOBMPEnrollmentSecurityInfo SecurityInformation { get; set; }

        public MOBMPEnrollmentSubscriptions SubscriptionPreferences { get; set; }
    }

    [Serializable]
    public class MOBMPEnrollmentSubscriptions
    {
        public bool UnitedNewsnDeals { get; set; }

        public bool MPPartnerOffers { get; set; }

        public bool MileagePlusProgram { get; set; }

        public bool MileagePlusStmt { get; set; }

    }

    [Serializable()]
    public class MOBMPEnrollmentSecurityInfo
    {
        private string telephonePIN;
        public string TelephonePIN
        {
            get { return telephonePIN; }
            set { telephonePIN = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string userName;
        public string UserName
        {
            get { return userName; }
            set { userName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string passWord;
        public string PassWord
        {
            get { return passWord; }
            set { passWord = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public List<Securityquestion> SelectedSecurityQuestions { get; set; }
    }

    [Serializable()]
    public class MOBMPEnrollmentContactInfo
    {
        private string streetAddress;

        public string StreetAddress
        {
            get { return streetAddress; }
            set { streetAddress = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string streetAddress2;

        public string StreetAddress2
        {
            get { return streetAddress2; }
            set { streetAddress2 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string cityRtown;

        public string CityRTown
        {
            get { return cityRtown; }
            set { cityRtown = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public MOBState State { get; set; }

        private string zipRpostalCode;

        public string ZipRpostalCode
        {
            get { return zipRpostalCode; }
            set { zipRpostalCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public MOBCountry Country { get; set; }

        private string phoneNumber;

        public string PhoneNumber
        {
            get { return phoneNumber; }
            set { phoneNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string phoneCountryCode = string.Empty;

        public string PhoneCountryCode
        {
            get
            {
                return this.phoneCountryCode;
            }
            set
            {
                this.phoneCountryCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        private string emailAddress;

        public string EmailAddress
        {
            get { return emailAddress; }
            set { emailAddress = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

    }

    [Serializable()]
    public class MOBMPEnrollmentPersonalInfo
    {
        private string title = string.Empty;
        private string firstName = string.Empty;
        private string middleName = string.Empty;
        private string lastName = string.Empty;
        private string suffix = string.Empty;
        private string birthDate = string.Empty;
        private string gender = string.Empty;

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

        public string Gender
        {
            get
            {
                return this.gender;
            }
            set
            {
                this.gender = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
    #region MPSignInNeedHelp
    [Serializable()]
    public class MOBMPSignInNeedHelpRequest : MOBRequest
    {
        public MOBMPSignInNeedHelpRequest()
            : base()
        {
        }
        public string SessionID { get; set; } = string.Empty;
        public string MileagePlusNumber { get; set; } = string.Empty;
        public MOBMPSecurityUpdatePath SecurityUpdateType { get; set; }
        public MOBMPSignInNeedHelpItems MPSignInNeedHelpItems { get; set; }
    }

    [Serializable()]
    public class MOBMPSignInNeedHelpResponse : MOBResponse
    {
        public MOBMPSignInNeedHelpResponse()
            : base()
        {
        }
        public string MileagePlusNumber { get; set; } = string.Empty;
        public MOBMPSignInNeedHelpRequest Request { get; set; }
        public MOBMPSignInNeedHelpItemsDetails MPSignInNeedHelpDetails { get; set; }
        public string SessionID { get; set; } = string.Empty;
        public string NeedHelpCompleteMessage { get; set; }
        public MOBMPSecurityUpdatePath NeedHelpSecurityPath { get; set; }
        public MOBMPTFARememberMeFlags RememberMEFlags { get; set; }
        public MOBMPErrorScreenType ErrorScreenType { get; set; }
        public Dictionary<string, string> SearchCriteria { get; set; }

    }


    [Serializable()]
    public class MOBMPSignInNeedHelpItems
    {
        public MOBMPSignInNeedHelpItems()
            : base()
        {

        }
        public List<Securityquestion> AnsweredSecurityQuestions { get; set; }
        public MOBName NeedHelpSignInInfo { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
        public string MileagePlusNumber { get; set; } = string.Empty;
        public string UpdatedPassword { get; set; } = string.Empty;

    }

    [Serializable()]
    public class MOBMPSignInNeedHelpItemsDetails
    {
        public List<Securityquestion> SecurityQuestions { get; set; }
        public List<MOBItem> NeedHelpMessages { get; set; }
        public MOBMPSignInNeedHelpItemsDetails()
            : base()
        {
        }
    }
    #endregion
    #endregion
    [Serializable()]
    public class MOBTFAMPDeviceRequest : MOBRequest
    {
        public MOBTFAMPDeviceRequest()
            : base()
        {
        }

        private string sessionID = string.Empty;
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private string mileagePlusNumber = string.Empty;
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

        public int CustomerID { get; set; }

        private string hashValue = string.Empty;
        public string HashValue
        {
            get
            {
                return this.hashValue;
            }
            set
            {
                this.hashValue = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public MOBMPSecurityUpdatePath tFAMPDeviceSecurityPath
        {
            get; set;
        }


        public List<Securityquestion> AnsweredSecurityQuestions { get; set; }

        public bool RememberDevice { get; set; }
    }
    [Serializable()]
    public class MOBTFAMPDeviceResponse : MOBResponse
    {
        private readonly IConfiguration _configuration;
        public MOBTFAMPDeviceResponse()
            : base()
        {
        }

        private string sessionID = string.Empty;
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private string mileagePlusNumber = string.Empty;
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

        public int CustomerID { get; set; }

        public string HashValue { get; set; } = string.Empty;

        public bool SecurityUpdate { get; set; } // This will be true only if Security Questions are anwered incorrectly and this will make client to check the TFAMPDeviceSecurityPath value if to go display thrid question or Accout is locked 

        public string KtnNumber { get; set; }      

        public MOBMPSecurityUpdatePath tFAMPDeviceSecurityPath { get; set; }
        public MOBTFAMPDeviceRequest Request { get; set; }

        public List<Securityquestion> SecurityQuestions { get; set; }

        public List<MOBItem> tFAMPDeviceMessages { get; set; }


        public string tFAMPDeviceCompleteMessage { get; set; }


        private MOBMPTFARememberMeFlags rememberMEFlags;
        public MOBMPTFARememberMeFlags RememberMEFlags
        {
            get
            {
                if (rememberMEFlags == null)
                {
                    rememberMEFlags = new MOBMPTFARememberMeFlags();
                }
                return this.rememberMEFlags;
            }
            set
            {
                this.rememberMEFlags = value;
            }
        }
        public bool ShowContinueAsGuestButton { get; set; }
        public MOBMPAccountSummary OPAccountSummary { get; set; }
        public MOBCorporateTravelType CorporateEligibleTravelType { get; set; }
        public MOBCPCustomerMetrics CustomerMetrics { get; set; }
        public string EmployeeId { get; set; }
        public MOBEmpTravelTypeResponse EmpTravelTypeResponse { get; set; }
        public MOBUASubscriptions UASubscriptions { get; set; }
        public bool IsUASubscriptionsAvailable { get; set; }
        
        public MOBYoungAdultTravelType YoungAdultTravelType { get; set; }

    }
}
