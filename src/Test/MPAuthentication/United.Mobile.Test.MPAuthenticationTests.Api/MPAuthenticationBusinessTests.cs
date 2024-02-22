using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.EmployeeReservation;
using United.Common.Helper.Profile;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.CSLSerivce;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.MerchandizeService;
using United.Mobile.DataAccess.MPSignIn;
using United.Mobile.DataAccess.OnPremiseSQLSP;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.Common.CloudDynamoDB;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.MPSignIn;
using United.Mobile.MPAuthentication.Domain;
using United.Service.Presentation.ReferenceDataModel;
using United.Services.Customer.Common;
using United.Utility.Helper;
using Xunit;
using Constants = United.Mobile.Model.Constants;


namespace United.Mobile.Test.MPAuthenticationTests.Api
{
    public class MPAuthenticationBusinessTests
    {
        private readonly Mock<ICacheLog<MPAuthenticationBusiness>> _logger;
        private readonly IConfiguration _configuration;
        private readonly IConfiguration _configuration1;
        private readonly IConfiguration _configuration2;
        private readonly Mock<ISessionHelperService> _sessionHelperService;
        private readonly Mock<ICachingService> _CachingService;
        private readonly Mock<IDPService> _tokenService;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<IDataPowerFactory> _dataPowerFactory;
        private readonly Mock<IMPSecurityCheckDetailsService> _mpSecurityCheckDetailsService;
        private readonly Mock<ICustomerProfile> _customerProfile;
        private readonly Mock<IMileagePlus> _mileagePlus;
        private readonly Mock<IMileagePlusTFACSL> _mileagePlusTFACSL;
        private readonly Mock<IDynamoDBHelperService> _dynamoDBHelperService;
        private readonly Mock<IEmployeeTravelTypeService> _employeeProfileTravelType;
        private readonly Mock<IEResEmployeeProfileService> _eResEmployeeProfileService;
        private readonly Mock<IEmployeeReservations> _employeeReservations;
        private readonly Mock<IHomePageContentService> _homePageContentService;
        private readonly Mock<IShoppingSessionHelper> _shoppingSessionHelper;
        private readonly MPAuthenticationBusiness _mPAuthenticationBusiness;
        private readonly MPAuthenticationBusiness _mPAuthenticationBusiness1;
        private readonly Mock<ISecurityQuestion> _moqSecurityQuestion;
        private readonly Mock<IMerchOffersService> _merchOffersService;
        private readonly Mock<IUtility> _utility;
        private readonly Mock<Common.Helper.Profile.IMerchandizingServices> _merchandizingServices;
        private readonly Mock<ICorporateGetService> _corporateGetService;
        private readonly Mock<IUCBProfile> _ucbProfile;
        private readonly Mock<IUCBProfileService> _uCBProfileService;
        private readonly Mock<IHashPin> _moqHashPin;
        private readonly Mock<ISQLSPService> _sQLSPService;
        private readonly Mock<IDynamoDBUtility> _dynamoDBUtility;
        private readonly Mock<IHashedPin> _hashedPin;
        private readonly Mock<IFeatureSettings> _featureSettings;
        private readonly Mock<IFeatureToggles> _featureToggles;

        public MPAuthenticationBusinessTests()
        {
            _configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
               .Build();
            _configuration1 = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings1.test.json", optional: false, reloadOnChange: true)
              .Build();
            _configuration2 = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings2.test.json", optional: false, reloadOnChange: true)
             .Build();

            _logger = new Mock<ICacheLog<MPAuthenticationBusiness>>();
            _sessionHelperService = new Mock<ISessionHelperService>();
            _CachingService = new Mock<ICachingService>();
            _tokenService = new Mock<IDPService>();
            _headers = new Mock<IHeaders>();
            _dataPowerFactory = new Mock<IDataPowerFactory>();
            _mpSecurityCheckDetailsService = new Mock<IMPSecurityCheckDetailsService>();
            _customerProfile = new Mock<ICustomerProfile>();
            _mileagePlus = new Mock<IMileagePlus>();
            _mileagePlusTFACSL = new Mock<IMileagePlusTFACSL>();
            _dynamoDBHelperService = new Mock<IDynamoDBHelperService>();
            _employeeProfileTravelType = new Mock<IEmployeeTravelTypeService>();
            _eResEmployeeProfileService = new Mock<IEResEmployeeProfileService>();
            _employeeReservations = new Mock<IEmployeeReservations>();
            _homePageContentService = new Mock<IHomePageContentService>();
            _shoppingSessionHelper = new Mock<IShoppingSessionHelper>();
            _moqSecurityQuestion = new Mock<ISecurityQuestion>();
            _merchOffersService = new Mock<IMerchOffersService>();
            _utility = new Mock<IUtility>();
            _merchandizingServices = new Mock<Common.Helper.Profile.IMerchandizingServices>();
            _corporateGetService = new Mock<ICorporateGetService>();
            _ucbProfile = new Mock<IUCBProfile>();
            _uCBProfileService = new Mock<IUCBProfileService>();
            _moqHashPin = new Mock<IHashPin>();
            _sQLSPService = new Mock<ISQLSPService>();
            _dynamoDBUtility = new Mock<IDynamoDBUtility>();
            _hashedPin = new Mock<IHashedPin>();
            _featureSettings = new Mock<IFeatureSettings>();

            _mPAuthenticationBusiness = new MPAuthenticationBusiness(_logger.Object, _configuration, _sessionHelperService.Object, _tokenService.Object, _headers.Object, _dataPowerFactory.Object, _mpSecurityCheckDetailsService.Object, _mileagePlus.Object, _customerProfile.Object, _mileagePlusTFACSL.Object, _dynamoDBHelperService.Object, _employeeProfileTravelType.Object, _eResEmployeeProfileService.Object, _homePageContentService.Object, _shoppingSessionHelper.Object, _employeeReservations.Object, _moqSecurityQuestion.Object, _merchandizingServices.Object, _ucbProfile.Object, _uCBProfileService.Object, _moqHashPin.Object, _dynamoDBUtility.Object, _hashedPin.Object, _featureSettings.Object,_featureToggles.Object);

            _mPAuthenticationBusiness1 = new MPAuthenticationBusiness(_logger.Object, _configuration1, _sessionHelperService.Object, _tokenService.Object, _headers.Object, _dataPowerFactory.Object, _mpSecurityCheckDetailsService.Object, _mileagePlus.Object, _customerProfile.Object, _mileagePlusTFACSL.Object, _dynamoDBHelperService.Object, _employeeProfileTravelType.Object, _eResEmployeeProfileService.Object, _homePageContentService.Object, _shoppingSessionHelper.Object, _employeeReservations.Object, _moqSecurityQuestion.Object, _merchandizingServices.Object, _ucbProfile.Object, _uCBProfileService.Object, _moqHashPin.Object, _dynamoDBUtility.Object, _hashedPin.Object, _featureSettings.Object, _featureToggles.Object);

            SetupHttpContextAccessor();
            SetHeaders();
        }

        private string GetFileContent(string fileName)
        {
            var path = Path.IsPathRooted(fileName) ? fileName
                                                : Path.GetRelativePath(Directory.GetCurrentDirectory(), fileName);
            return File.ReadAllText(path);
        }

        private T GetContent<T>(string filename)
        {
            return JsonConvert.DeserializeObject<T>(GetFileContent(filename));
        }
        private void SetupHttpContextAccessor()
        {
            var guid = Guid.NewGuid().ToString();
            var context = new DefaultHttpContext();
            context.Request.Headers[Constants.HeaderAppIdText] = "1";
            context.Request.Headers[Constants.HeaderAppMajorText] = "1";
            context.Request.Headers[Constants.HeaderAppMinorText] = "0";
            context.Request.Headers[Constants.HeaderDeviceIdText] = guid;
            context.Request.Headers[Constants.HeaderLangCodeText] = "en-us";
            context.Request.Headers[Constants.HeaderRequestTimeUtcText] = DateTime.UtcNow.ToString();
            context.Request.Headers[Constants.HeaderTransactionIdText] = guid;
        }

        private void SetHeaders(string deviceId = "589d7852-14e7-44a9-b23b-a6db36657579"
      , string applicationId = "2"
      , string appVersion = "4.1.29"
      , string transactionId = "589d7852-14e7-44a9-b23b-a6db36657579|8f46e040-a200-495c-83ca-4fca2d7175fb"
      , string languageCode = "en-US"
      , string sessionId = "17C979E184CC495EA083D45F4DD9D19D")
        {
            _headers.Setup(_ => _.ContextValues).Returns(
           new HttpContextValues
           {
               Application = new Application()
               {
                   Id = Convert.ToInt32(applicationId),
                   Version = new Mobile.Model.Version
                   {
                       Major = string.IsNullOrEmpty(appVersion) ? 0 : int.Parse(appVersion.ToString().Substring(0, 1)),
                       Minor = string.IsNullOrEmpty(appVersion) ? 0 : int.Parse(appVersion.ToString().Substring(2, 1)),
                       Build = string.IsNullOrEmpty(appVersion) ? 0 : int.Parse(appVersion.ToString().Substring(4, 2))
                   }
               },
               DeviceId = deviceId,
               LangCode = languageCode,
               TransactionId = transactionId,
               SessionId = sessionId
           });
        }


        [Theory]
        [MemberData(nameof(MPAuthenticationInput.ValidateMPSignInV2Input), MemberType = typeof(MPAuthenticationInput))]
        public void ValidateMPSignInV2_Test(MOBMPPINPWDValidateRequest request, bool ok, MOBMPPINPWDValidateResponse response, string jsonResponse, DPAccessTokenResponse dPAccessTokenResponse, bool data)
        {

            bool val;
            if (response.AccountValidation.HashValue == "1")
                val = false;
            else
                val = ok;
            var sessionData = GetContent<Session>("SessionData.json");
            var EResBetaTester = GetContent<List<EResBetaTester>>("EResBetaTester.json");
            var mobItem = GetContent<List<MOBItem>>("MOBItem.json");
            var empTravelTypesAndJAResponse = GetContent<MOBEmpTravelTypeAndJAProfileResponse>("MOBEmpTravelTypeAndJAProfileResponse.json");
            var EmployeeTravelProfile = GetContent<Service.Presentation.PersonModel.EmployeeTravelProfile>("EmployeeTravelProfile.json");
            var cppprofile = GetContent<List<MOBCPProfile>>("MOBCPProfile.json");
            var cppprofile1 = GetContent<MOBCPProfile>("MOBCPProfile_1.json");
            var hashPinValidate1 = GetContent<HashPinValidate>("HashPinValidate.json");
            var merch = GetFileContent("MerchManagementOutput.json");
            var merchData = JsonConvert.DeserializeObject<Subscription>(merch);

            var milage = GetFileContent("MileagePlusDetails.json");
            var milageData = JsonConvert.DeserializeObject<MileagePlusDetails>(milage);
            var securityquestion = GetContent<List<Securityquestion>>("Securityquestion.json");


            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CreateShoppingSession(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CheckIsCSSTokenValid(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<string>())).Returns(Task.FromResult((true, sessionData)));

            _mileagePlusTFACSL.Setup(p => p.GetTfaWrongAnswersFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(ok);
            //_dynamoDBService.Setup(p => p.GetRecords<List<United.Mobile.Model.Internal.AccountManagement.EResBetaTester>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(EResBetaTester);
            _moqSecurityQuestion.Setup(p => p.GetMPPINPWDTitleMessagesForMPAuth(It.IsAny<List<string>>())).ReturnsAsync(mobItem);
            _dataPowerFactory.Setup(p => p.GetDPAuthenticatedToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(dPAccessTokenResponse);
            _tokenService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            _mpSecurityCheckDetailsService.Setup(p => p.GetMPSecurityCheckDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(jsonResponse);
            _customerProfile.Setup(p => p.PopulateProfiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<List<United.Services.Customer.Common.Profile>>(), It.IsAny<MOBCPProfileRequest>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<MOBApplication>())).ReturnsAsync(cppprofile);
            _mileagePlus.Setup(p => p.GetAccountSummary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary);
            _employeeReservations.Setup(p => p.GetTravelTypesAndJAProfile(It.IsAny<MOBEmpTravelTypeAndJAProfileRequest>())).Returns(empTravelTypesAndJAResponse);
            _mileagePlusTFACSL.Setup(p => p.GetTfaWrongAnswersFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(data);
            _customerProfile.Setup(p => p.GeteMailIDTFAMPSecurityDetails(It.IsAny<MOBMPPINPWDValidateRequest>(), It.IsAny<string>())).ReturnsAsync(cppprofile1);

            _dynamoDBUtility.Setup(p => p.GetEResBetaTesterItems(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _sQLSPService.Setup(p => p.ValidateHashPin<HashPinValidate>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(hashPinValidate1);

            _dynamoDBHelperService.Setup(p => p.SaveMPAppIdDeviceId<MileagePlusDetails>(It.IsAny<MileagePlusDetails>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _merchOffersService.Setup(p => p.GetSubscriptions<Subscription>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(merchData);

            _dynamoDBHelperService.Setup(p => p.GetAuthToken<MileagePlusDetails>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(milageData);

            _mileagePlusTFACSL.Setup(p => p.ValidateDevice(It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            _mileagePlusTFACSL.Setup(p => p.SignOutSession(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(true);

            //_sessionHelperService.Setup(p => p.SaveSession<List<MPSignIn>>(It.IsAny<MPSignIn>(), It.IsAny<HttpContextValues>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            //_sessionHelperService.Setup(p => p.SaveSession<List<MPPINPWSecurityQuestionsValidation>>(It.IsAny<HttpContextValues>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            _moqHashPin.Setup(p => p.ValidateHashPinAndGetAuthTokenDynamoDB(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(milageData);

            _moqSecurityQuestion.Setup(p => p.GetMPPinPwdSavedSecurityQuestions(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(securityquestion);


            if (request.Application.IsProduction)
                _configuration["EnabledMERCHChannels"] = "False";
            if (response.AccountValidation.IsTokenValid)
                _configuration["GetEmployeeIDFromGetProfileCustomerData"] = "False";
            if (response.AccountValidation.DeceasedAccount)
                _configuration["EnableMicroserviceAPIMigration"] = "False";
            if (response.AccountValidation.DeceasedAccount)
                _configuration["EnableEResAPIMigration"] = "False";
            if (!response.AccountValidation.IsTokenValid)
                _configuration["EnableEResAPIMigration"] = "False";
            if (response.AccountValidation.NeedToAcceptTNC)
                _configuration["NumberOfSecurityQuestionsNeedatPINPWDUpdate"] = "0";
            if (response.AccountValidation.HashValue == "2")
                _configuration["AndroidYoungAdultCorporateFixVersion"] = "";
            if (response.AccountValidation.HashValue == "3")
            {
                _configuration["AndroidYoungAdultCorporateFixVersion"] = "";
                _configuration["EnableEResAPIMigration"] = "False";
            }

            var result = _mPAuthenticationBusiness.ValidateMPSignInV2(request);

            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [MemberData(nameof(MPAuthenticationInput.ValidateMPSignInV2EmpInput), MemberType = typeof(MPAuthenticationInput))]
        public void ValidateMPSignInV2_EmployeeAccountTest(MOBMPPINPWDValidateRequest request
            , bool ok
            , MOBMPPINPWDValidateResponse response
            , DPAccessTokenResponse dPAccessTokenResponse
            , bool data)
        {

            bool val;
            if (response.AccountValidation.HashValue == "1")
                val = false;
            else
                val = ok;
            var sessionData = GetContent<Session>("SessionData.json");
            var empTravelTypesAndJAResponse = GetContent<MOBEmpTravelTypeAndJAProfileResponse>("MOBEmpTravelTypeAndJAProfileResponse.json");
            var EmployeeTravelProfile = GetContent<Service.Presentation.PersonModel.EmployeeTravelProfile>("EmployeeTravelProfile.json");
            var cppprofile = GetContent<List<MOBCPProfile>>("MOBCPProfile.json");
            var cppprofile1 = GetContent<MOBCPProfile>("MOBCPProfile_1.json");
            var hashPinValidate1 = GetContent<HashPinValidate>("HashPinValidate.json");
            var merch = GetFileContent("MerchManagementOutput.json");
            var merchData = JsonConvert.DeserializeObject<Subscription>(merch);

            var milage = GetFileContent("MileagePlusDetails.json");
            var milageData = JsonConvert.DeserializeObject<MileagePlusDetails>(milage);
            var EResBetaTester = GetContent<List<EResBetaTester>>("EResBetaTester.json");
            var securityquestion = GetContent<List<Securityquestion>>("Securityquestion.json");
            // var mobItem = GetContent<List<MOBItem>>("MOBItem.json");
            var mobItem = GetFileContent("MOBItem.json");
            var mobItemData = JsonConvert.DeserializeObject<List<MOBItem>>(mobItem);


            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CreateShoppingSession(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CheckIsCSSTokenValid(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<string>())).Returns(Task.FromResult((true, sessionData)));

            //_mileagePlusTFACSL.Setup(p => p.GetTfaWrongAnswersFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(true);
            //_dynamoDBService.Setup(p => p.GetRecords<List<United.Mobile.Model.Internal.AccountManagement.EResBetaTester>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(EResBetaTester);
            //_moqSecurityQuestion.Setup(p => p.GetMPPINPWDTitleMessagesForMPAuth(It.IsAny<List<string>>())).ReturnsAsync(mobItem);
            _dataPowerFactory.Setup(p => p.GetDPAuthenticatedToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(dPAccessTokenResponse);
            _tokenService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            ProfileResponse profileResponse = new ProfileResponse
            {
                Profiles = new List<Profile>
                {
                    new Profile
                    {
                        Travelers = new List<Traveler>
                        {
                            new Traveler
                            {
                                EmployeeId = "testempid"
                            }
                        }

                    }
                }

            };
            string jsonResponse = JsonConvert.SerializeObject(profileResponse);
            _mpSecurityCheckDetailsService.Setup(p => p.GetMPSecurityCheckDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(jsonResponse);
            _customerProfile.Setup(p => p.PopulateProfiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<List<United.Services.Customer.Common.Profile>>(), It.IsAny<MOBCPProfileRequest>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<MOBApplication>())).ReturnsAsync(cppprofile);
            _mileagePlus.Setup(p => p.GetAccountSummary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary);
            _employeeReservations.Setup(p => p.GetTravelTypesAndJAProfile(It.IsAny<MOBEmpTravelTypeAndJAProfileRequest>())).Returns(empTravelTypesAndJAResponse);
            _mileagePlusTFACSL.Setup(p => p.GetTfaWrongAnswersFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(true);
            _customerProfile.Setup(p => p.GeteMailIDTFAMPSecurityDetails(It.IsAny<MOBMPPINPWDValidateRequest>(), It.IsAny<string>())).ReturnsAsync(cppprofile1);

            _dynamoDBUtility.Setup(p => p.GetEResBetaTesterItems(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _sQLSPService.Setup(p => p.ValidateHashPin<HashPinValidate>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(hashPinValidate1);

            _dynamoDBHelperService.Setup(p => p.SaveMPAppIdDeviceId<MileagePlusDetails>(It.IsAny<MileagePlusDetails>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _merchOffersService.Setup(p => p.GetSubscriptions<Subscription>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(merchData);

            _dynamoDBHelperService.Setup(p => p.GetAuthToken<MileagePlusDetails>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(milageData);

            _mileagePlusTFACSL.Setup(p => p.ValidateDevice(It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _mileagePlusTFACSL.Setup(p => p.SignOutSession(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(true);

            //_sessionHelperService.Setup(p => p.SaveSession<List<MPSignIn>>(It.IsAny<MPSignIn>(), It.IsAny<HttpContextValues>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            //_sessionHelperService.Setup(p => p.SaveSession<List<MPPINPWSecurityQuestionsValidation>>(It.IsAny<HttpContextValues>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            // _securityQuestion.Setup(p => p.GetMPPinPwdSavedSecurityQuestions(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(securityquestion);

            _moqSecurityQuestion.Setup(p => p.GetMPPINPWDTitleMessagesForMPAuth(It.IsAny<List<string>>())).ReturnsAsync(mobItemData);

            _uCBProfileService.Setup(p => p.GetAllTravellers(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("true");

            _hashedPin.Setup(p => p.HashPinGeneration(It.IsAny<string>())).Returns("united2019");


            if (request.Application.IsProduction)
                _configuration["EnabledMERCHChannels"] = "False";
            if (response.AccountValidation.IsTokenValid)
                _configuration["GetEmployeeIDFromGetProfileCustomerData"] = "False";
            if (response.AccountValidation.DeceasedAccount)
                _configuration["EnableMicroserviceAPIMigration"] = "False";
            if (response.AccountValidation.DeceasedAccount)
                _configuration["EnableEResAPIMigration"] = "False";
            if (!response.AccountValidation.IsTokenValid)
                _configuration["EnableEResAPIMigration"] = "False";
            if (response.AccountValidation.NeedToAcceptTNC)
                _configuration["NumberOfSecurityQuestionsNeedatPINPWDUpdate"] = "0";
            if (response.AccountValidation.HashValue == "2")
                _configuration["AndroidYoungAdultCorporateFixVersion"] = "";
            if (response.AccountValidation.HashValue == "3")
            {
                _configuration["AndroidYoungAdultCorporateFixVersion"] = "";
                _configuration["EnableEResAPIMigration"] = "False";
            }

            var result = _mPAuthenticationBusiness.ValidateMPSignInV2(request);

            Assert.True(result.Exception != null || result.Result != null);
        }


        [Theory]
        [MemberData(nameof(MPAuthenticationInput.ValidateMPSignInV2CorporateInput), MemberType = typeof(MPAuthenticationInput))]
        public void ValidateMPSignInV2_CorporateAccountTest(MOBMPPINPWDValidateRequest request
            , bool ok
            , MOBMPPINPWDValidateResponse response
            , DPAccessTokenResponse dPAccessTokenResponse
            , bool data)
        {

            bool val;
            if (response.AccountValidation.HashValue == "1")
                val = false;
            else
                val = ok;
            var sessionData = GetContent<Session>("SessionData.json");
            var EResBetaTester = GetContent<List<EResBetaTester>>("EResBetaTester.json");
            //var mobItem = GetContent<List<MOBItem>>("MOBItem.json");
            var empTravelTypesAndJAResponse = GetContent<MOBEmpTravelTypeAndJAProfileResponse>("MOBEmpTravelTypeAndJAProfileResponse.json");
            var EmployeeTravelProfile = GetContent<Service.Presentation.PersonModel.EmployeeTravelProfile>("EmployeeTravelProfile.json");
            var cppprofile = GetContent<List<MOBCPProfile>>("MOBCPProfile.json");
            var cppprofile1 = GetContent<MOBCPProfile>("MOBCPProfile_1.json");
            var hashPinValidate1 = GetContent<HashPinValidate>("HashPinValidate.json");
            var merch = GetFileContent("MerchManagementOutput.json");
            var merchData = JsonConvert.DeserializeObject<Subscription>(merch);

            var milage = GetFileContent("MileagePlusDetails.json");
            var milageData = JsonConvert.DeserializeObject<MileagePlusDetails>(milage);
            var mobItem = GetFileContent("MOBItem.json");
            var mobItemData = JsonConvert.DeserializeObject<List<MOBItem>>(mobItem);


            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CreateShoppingSession(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CheckIsCSSTokenValid(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<string>())).Returns(Task.FromResult((true, sessionData)));

            _mileagePlusTFACSL.Setup(p => p.GetTfaWrongAnswersFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(true);
            //_dynamoDBService.Setup(p => p.GetRecords<List<United.Mobile.Model.Internal.AccountManagement.EResBetaTester>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(EResBetaTester);
            //_moqSecurityQuestion.Setup(p => p.GetMPPINPWDTitleMessagesForMPAuth(It.IsAny<List<string>>())).ReturnsAsync(mobItem);
            _dataPowerFactory.Setup(p => p.GetDPAuthenticatedToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(dPAccessTokenResponse);
            _tokenService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            ProfileResponse profileResponse = new ProfileResponse
            {
                Profiles = new List<Profile>
                {
                    new Profile
                    {
                        Travelers = new List<Traveler>
                        {
                            new Traveler
                            {
                                EmployeeId = "testempid"
                            }
                        }

                    }
                }

            };
            string jsonResponse = JsonConvert.SerializeObject(profileResponse);
            _mpSecurityCheckDetailsService.Setup(p => p.GetMPSecurityCheckDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(jsonResponse);
            _customerProfile.Setup(p => p.PopulateProfiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<List<United.Services.Customer.Common.Profile>>(), It.IsAny<MOBCPProfileRequest>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<MOBApplication>())).ReturnsAsync(cppprofile);
            _mileagePlus.Setup(p => p.GetAccountSummary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary);
            _employeeReservations.Setup(p => p.GetTravelTypesAndJAProfile(It.IsAny<MOBEmpTravelTypeAndJAProfileRequest>())).Returns(empTravelTypesAndJAResponse);
            _mileagePlusTFACSL.Setup(p => p.GetTfaWrongAnswersFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(true);
            _customerProfile.Setup(p => p.GeteMailIDTFAMPSecurityDetails(It.IsAny<MOBMPPINPWDValidateRequest>(), It.IsAny<string>())).ReturnsAsync(cppprofile1);

            _dynamoDBUtility.Setup(p => p.GetEResBetaTesterItems(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _sQLSPService.Setup(p => p.ValidateHashPin<HashPinValidate>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(hashPinValidate1);

            _dynamoDBHelperService.Setup(p => p.SaveMPAppIdDeviceId<MileagePlusDetails>(It.IsAny<MileagePlusDetails>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _merchOffersService.Setup(p => p.GetSubscriptions<Subscription>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(merchData);

            _dynamoDBHelperService.Setup(p => p.GetAuthToken<MileagePlusDetails>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(milageData);

            _mileagePlusTFACSL.Setup(p => p.ValidateDevice(It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            _mileagePlusTFACSL.Setup(p => p.SignOutSession(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(true);

            //_sessionHelperService.Setup(p => p.SaveSession<List<MPSignIn>>(It.IsAny<MPSignIn>(), It.IsAny<HttpContextValues>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            //_sessionHelperService.Setup(p => p.SaveSession<List<MPPINPWSecurityQuestionsValidation>>(It.IsAny<HttpContextValues>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _moqSecurityQuestion.Setup(p => p.GetMPPINPWDTitleMessagesForMPAuth(It.IsAny<List<string>>())).ReturnsAsync(mobItemData);

            _uCBProfileService.Setup(p => p.GetAllTravellers(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("true");


            if (request.Application.IsProduction)
                _configuration["EnabledMERCHChannels"] = "False";
            if (response.AccountValidation.IsTokenValid)
                _configuration["GetEmployeeIDFromGetProfileCustomerData"] = "False";
            if (response.AccountValidation.DeceasedAccount)
                _configuration["EnableMicroserviceAPIMigration"] = "False";
            if (response.AccountValidation.DeceasedAccount)
                _configuration["EnableEResAPIMigration"] = "False";
            if (!response.AccountValidation.IsTokenValid)
                _configuration["EnableEResAPIMigration"] = "False";
            if (response.AccountValidation.NeedToAcceptTNC)
                _configuration["NumberOfSecurityQuestionsNeedatPINPWDUpdate"] = "0";
            if (response.AccountValidation.HashValue == "2")
                _configuration["AndroidYoungAdultCorporateFixVersion"] = "";
            if (response.AccountValidation.HashValue == "3")
            {
                _configuration["AndroidYoungAdultCorporateFixVersion"] = "";
                _configuration["EnableEResAPIMigration"] = "False";
            }

            var result = _mPAuthenticationBusiness.ValidateMPSignInV2(request);

            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [MemberData(nameof(MPAuthenticationInput.ValidateMPSignInV2TouchIdInput), MemberType = typeof(MPAuthenticationInput))]
        public void ValidateMPSignInV2_TouchId(MOBMPPINPWDValidateRequest request
            , bool ok
            , MOBMPPINPWDValidateResponse response
            , DPAccessTokenResponse dPAccessTokenResponse
            , bool data)
        {

            bool val;
            if (response.AccountValidation.HashValue == "1")
                val = false;
            else
                val = ok;
            var sessionData = GetContent<Session>("SessionData.json");
            var empTravelTypesAndJAResponse = GetContent<MOBEmpTravelTypeAndJAProfileResponse>("MOBEmpTravelTypeAndJAProfileResponse.json");
            var EmployeeTravelProfile = GetContent<Service.Presentation.PersonModel.EmployeeTravelProfile>("EmployeeTravelProfile.json");
            var cppprofile = GetContent<List<MOBCPProfile>>("MOBCPProfile.json");
            var cppprofile1 = GetContent<MOBCPProfile>("MOBCPProfile_1.json");
            var hashPinValidate1 = GetContent<HashPinValidate>("HashPinValidate.json");
            var merch = GetFileContent("MerchManagementOutput.json");
            var merchData = JsonConvert.DeserializeObject<Subscription>(merch);

            var milage = GetFileContent("MileagePlusDetails.json");
            var milageData = JsonConvert.DeserializeObject<MileagePlusDetails>(milage);
            var EResBetaTester = GetContent<List<EResBetaTester>>("EResBetaTester.json");
            var securityquestion = GetContent<List<Securityquestion>>("Securityquestion.json");
            // var mobItem = GetContent<List<MOBItem>>("MOBItem.json");
            var mobItem = GetFileContent("MOBItem.json");
            var mobItemData = JsonConvert.DeserializeObject<List<MOBItem>>(mobItem);



            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CreateShoppingSession(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CheckIsCSSTokenValid(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<string>())).Returns(Task.FromResult((true, sessionData)));

            _dataPowerFactory.Setup(p => p.GetDPAuthenticatedToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(dPAccessTokenResponse);
            _tokenService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            ProfileResponse profileResponse = new ProfileResponse
            {
                Profiles = new List<Profile>
                {
                    new Profile
                    {
                        Travelers = new List<Traveler>
                        {
                            new Traveler
                            {
                                EmployeeId = "testempid"
                            }
                        }

                    }
                }

            };
            string jsonResponse = JsonConvert.SerializeObject(profileResponse);
            _mpSecurityCheckDetailsService.Setup(p => p.GetMPSecurityCheckDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(jsonResponse);
            _customerProfile.Setup(p => p.PopulateProfiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<List<United.Services.Customer.Common.Profile>>(), It.IsAny<MOBCPProfileRequest>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<MOBApplication>())).ReturnsAsync(cppprofile);
            _mileagePlus.Setup(p => p.GetAccountSummary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary);
            _employeeReservations.Setup(p => p.GetTravelTypesAndJAProfile(It.IsAny<MOBEmpTravelTypeAndJAProfileRequest>())).Returns(empTravelTypesAndJAResponse);
            _mileagePlusTFACSL.Setup(p => p.GetTfaWrongAnswersFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(true);
            _customerProfile.Setup(p => p.GeteMailIDTFAMPSecurityDetails(It.IsAny<MOBMPPINPWDValidateRequest>(), It.IsAny<string>())).ReturnsAsync(cppprofile1);

            _dynamoDBUtility.Setup(p => p.GetEResBetaTesterItems(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            //_validateHashPinService.Setup(p => p.ValidateHashPin<HashPinValidate>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(hashPinValidate1);

            _dynamoDBHelperService.Setup(p => p.SaveMPAppIdDeviceId<MileagePlusDetails>(It.IsAny<MileagePlusDetails>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _merchOffersService.Setup(p => p.GetSubscriptions<Subscription>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(merchData);

            _dynamoDBHelperService.Setup(p => p.GetAuthToken<MileagePlusDetails>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(milageData);

            _mileagePlusTFACSL.Setup(p => p.ValidateDevice(It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            _mileagePlusTFACSL.Setup(p => p.SignOutSession(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(true);

            _moqSecurityQuestion.Setup(p => p.GetMPPINPWDTitleMessagesForMPAuth(It.IsAny<List<string>>())).ReturnsAsync(mobItemData);

            _uCBProfileService.Setup(p => p.GetAllTravellers(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("true");

            _moqHashPin.Setup(p => p.ValidateHashPinAndGetAuthTokenDynamoDB(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(milageData);

            MOBMicroserviceBaseResponse<MOBEmployeeProfileTravelTypeResponse> mOBMicroserviceBaseResponse = new MOBMicroserviceBaseResponse<MOBEmployeeProfileTravelTypeResponse>()
            {
                Data = new MOBEmployeeProfileTravelTypeResponse()
                {
                    TravelTypeResponse = new MOBEmpTravelTypeResponse
                    {
                        AdvanceBookingDays = 1,
                        DisplayEmployeeId = "1",
                        EResTransactionId = "BD6CE5D9E05C41DE9A7ED828DE17B457"
                    },
                    EmployeeJAResponse = new MOBEmployeeProfileResponse
                    {
                        AllowImpersonation = true
                    }
                },
                Status = 1,
                MachineName = "AW7890",
                Title = "Hash"

            };
            var empresponse = JsonConvert.SerializeObject(mOBMicroserviceBaseResponse);

            _employeeProfileTravelType.Setup(p => p.GetTravelType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(empresponse);

            //_employeeProfileTravelType.Setup(p => p.GetTravelType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("true");
            _moqSecurityQuestion.Setup(p => p.GetMPPinPwdSavedSecurityQuestions(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(securityquestion);


            if (request.Application.IsProduction)
                _configuration["EnabledMERCHChannels"] = "False";
            if (response.AccountValidation.IsTokenValid)
                _configuration["GetEmployeeIDFromGetProfileCustomerData"] = "False";
            if (response.AccountValidation.DeceasedAccount)
                _configuration["EnableMicroserviceAPIMigration"] = "False";
            if (response.AccountValidation.DeceasedAccount)
                _configuration["EnableEResAPIMigration"] = "False";
            if (!response.AccountValidation.IsTokenValid)
                _configuration["EnableEResAPIMigration"] = "False";
            if (response.AccountValidation.NeedToAcceptTNC)
                _configuration["NumberOfSecurityQuestionsNeedatPINPWDUpdate"] = "0";
            if (response.AccountValidation.HashValue == "2")
                _configuration["AndroidYoungAdultCorporateFixVersion"] = "";
            if (response.AccountValidation.HashValue == "3")
            {
                _configuration["AndroidYoungAdultCorporateFixVersion"] = "";
                _configuration["EnableEResAPIMigration"] = "False";
            }

            var result = _mPAuthenticationBusiness.ValidateMPSignInV2(request);

            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [MemberData(nameof(MPAuthenticationInput.ValidateMPSignInV2ExcInput), MemberType = typeof(MPAuthenticationInput))]
        public void ValidateMPSignInV2_Exe(MOBMPPINPWDValidateRequest request
            , bool ok
            , MOBMPPINPWDValidateResponse response
            , string jsonResponse
            , DPAccessTokenResponse dPAccessTokenResponse
            , bool data)
        {

            bool val;
            if (response.AccountValidation.HashValue == "1")
                val = false;
            else
                val = ok;
            var sessionData = GetContent<Session>("SessionData.json");
            var EResBetaTester = GetContent<List<EResBetaTester>>("EResBetaTester.json");
            var mobItem = GetContent<List<MOBItem>>("MOBItem.json");
            var empTravelTypesAndJAResponse = GetContent<MOBEmpTravelTypeAndJAProfileResponse>("MOBEmpTravelTypeAndJAProfileResponse.json");
            var EmployeeTravelProfile = GetContent<Service.Presentation.PersonModel.EmployeeTravelProfile>("EmployeeTravelProfile.json");
            var cppprofile = GetContent<List<MOBCPProfile>>("MOBCPProfile.json");
            var cppprofile1 = GetContent<MOBCPProfile>("MOBCPProfile_1.json");
            var hashPinValidate1 = GetContent<HashPinValidate>("HashPinValidate.json");
            var merch = GetFileContent("MerchManagementOutput.json");
            var merchData = JsonConvert.DeserializeObject<Subscription>(merch);

            var milage = GetFileContent("MileagePlusDetails.json");
            var milageData = JsonConvert.DeserializeObject<MileagePlusDetails>(milage);


            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CreateShoppingSession(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CheckIsCSSTokenValid(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<string>())).Returns(Task.FromResult((true, sessionData)));

            _mileagePlusTFACSL.Setup(p => p.GetTfaWrongAnswersFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(ok);
            //_dynamoDBService.Setup(p => p.GetRecords<List<United.Mobile.Model.Internal.AccountManagement.EResBetaTester>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(EResBetaTester);
            //_mileagePlusTFACSL.Setup(p => p.GetMPPinPwdSavedSecurityQuestions(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));
            _moqSecurityQuestion.Setup(p => p.GetMPPINPWDTitleMessagesForMPAuth(It.IsAny<List<string>>())).ReturnsAsync(mobItem);
            _dataPowerFactory.Setup(p => p.GetDPAuthenticatedToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(dPAccessTokenResponse);
            _tokenService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            _mpSecurityCheckDetailsService.Setup(p => p.GetMPSecurityCheckDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(jsonResponse);
            _customerProfile.Setup(p => p.PopulateProfiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<List<United.Services.Customer.Common.Profile>>(), It.IsAny<MOBCPProfileRequest>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<MOBApplication>())).ReturnsAsync(cppprofile);
            _mileagePlus.Setup(p => p.GetAccountSummary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary);
            _employeeReservations.Setup(p => p.GetTravelTypesAndJAProfile(It.IsAny<MOBEmpTravelTypeAndJAProfileRequest>())).Returns(empTravelTypesAndJAResponse);
            _mileagePlusTFACSL.Setup(p => p.GetTfaWrongAnswersFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(data);
            _customerProfile.Setup(p => p.GeteMailIDTFAMPSecurityDetails(It.IsAny<MOBMPPINPWDValidateRequest>(), It.IsAny<string>())).ReturnsAsync(cppprofile1);

            _dynamoDBUtility.Setup(p => p.GetEResBetaTesterItems(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _sQLSPService.Setup(p => p.ValidateHashPin<HashPinValidate>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(hashPinValidate1);

            _dynamoDBHelperService.Setup(p => p.SaveMPAppIdDeviceId<MileagePlusDetails>(It.IsAny<MileagePlusDetails>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _merchOffersService.Setup(p => p.GetSubscriptions<Subscription>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(merchData);

            _dynamoDBHelperService.Setup(p => p.GetAuthToken<MileagePlusDetails>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(milageData);

            _mileagePlusTFACSL.Setup(p => p.ValidateDevice(It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            _mileagePlusTFACSL.Setup(p => p.SignOutSession(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(true);

            _mileagePlus.Setup(p => p.GetAccountSummary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary);

            //_sessionHelperService.Setup(p => p.SaveSession<List<MPSignIn>>(It.IsAny<MPSignIn>(), It.IsAny<HttpContextValues>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            //_sessionHelperService.Setup(p => p.SaveSession<MPPINPWSecurityQuestionsValidation>(It.IsAny<HttpContextValues>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);


            if (request.Application.IsProduction)
                _configuration1["EnabledMERCHChannels"] = "False";
            if (response.AccountValidation.IsTokenValid)
                _configuration1["GetEmployeeIDFromGetProfileCustomerData"] = "False";
            if (response.AccountValidation.DeceasedAccount)
                _configuration1["EnableMicroserviceAPIMigration"] = "False";
            if (response.AccountValidation.DeceasedAccount)
                _configuration1["EnableEResAPIMigration"] = "False";
            if (!response.AccountValidation.IsTokenValid)
                _configuration1["EnableEResAPIMigration"] = "False";
            if (response.AccountValidation.NeedToAcceptTNC)
                _configuration1["NumberOfSecurityQuestionsNeedatPINPWDUpdate"] = "0";
            if (response.AccountValidation.HashValue == "2")
                _configuration1["AndroidYoungAdultCorporateFixVersion"] = "";
            if (response.AccountValidation.HashValue == "3")
            {
                _configuration1["AndroidYoungAdultCorporateFixVersion"] = "";
                _configuration1["EnableEResAPIMigration"] = "False";
            }

            var result = _mPAuthenticationBusiness1.ValidateMPSignInV2(request);


            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [MemberData(nameof(MPAuthenticationInput.ValidateMPSignInV2ExceptionInput), MemberType = typeof(MPAuthenticationInput))]
        public void ValidateMPSignInV2_Exception(MOBMPPINPWDValidateRequest request, bool ok, MOBMPPINPWDValidateResponse response, string jsonResponse, DPAccessTokenResponse dPAccessTokenResponse, bool data)
        {

            bool val;
            if (response.AccountValidation.HashValue == "1")
                val = false;
            else
                val = ok;
            var sessionData = GetContent<Session>("SessionData.json");
            var EResBetaTester = GetContent<List<EResBetaTester>>("EResBetaTester.json");
            var mobItem = GetContent<List<MOBItem>>("MOBItem.json");
            var empTravelTypesAndJAResponse = GetContent<MOBEmpTravelTypeAndJAProfileResponse>("MOBEmpTravelTypeAndJAProfileResponse.json");
            var EmployeeTravelProfile = GetContent<Service.Presentation.PersonModel.EmployeeTravelProfile>("EmployeeTravelProfile.json");
            var cppprofile = GetContent<List<MOBCPProfile>>("MOBCPProfile.json");
            var cppprofile1 = GetContent<MOBCPProfile>("MOBCPProfile_1.json");
            var hashPinValidate1 = GetContent<HashPinValidate>("HashPinValidate.json");
            var merch = GetFileContent("MerchManagementOutput.json");
            var merchData = JsonConvert.DeserializeObject<Subscription>(merch);

            var milage = GetFileContent("MileagePlusDetails.json");
            var milageData = JsonConvert.DeserializeObject<MileagePlusDetails>(milage);
            var securityquestion = GetContent<List<Securityquestion>>("Securityquestion.json");


            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CreateShoppingSession(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CheckIsCSSTokenValid(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<string>())).Returns(Task.FromResult((true, sessionData)));

            _mileagePlusTFACSL.Setup(p => p.GetTfaWrongAnswersFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(ok);
            //_dynamoDBService.Setup(p => p.GetRecords<List<United.Mobile.Model.Internal.AccountManagement.EResBetaTester>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(EResBetaTester);
            _moqSecurityQuestion.Setup(p => p.GetMPPINPWDTitleMessagesForMPAuth(It.IsAny<List<string>>())).ReturnsAsync(mobItem);
            _dataPowerFactory.Setup(p => p.GetDPAuthenticatedToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(dPAccessTokenResponse);
            _tokenService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            _mpSecurityCheckDetailsService.Setup(p => p.GetMPSecurityCheckDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(jsonResponse);
            _customerProfile.Setup(p => p.PopulateProfiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<List<United.Services.Customer.Common.Profile>>(), It.IsAny<MOBCPProfileRequest>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<MOBApplication>())).ReturnsAsync(cppprofile);
            _mileagePlus.Setup(p => p.GetAccountSummary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary);
            _employeeReservations.Setup(p => p.GetTravelTypesAndJAProfile(It.IsAny<MOBEmpTravelTypeAndJAProfileRequest>())).Returns(empTravelTypesAndJAResponse);
            _mileagePlusTFACSL.Setup(p => p.GetTfaWrongAnswersFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(data);
            _customerProfile.Setup(p => p.GeteMailIDTFAMPSecurityDetails(It.IsAny<MOBMPPINPWDValidateRequest>(), It.IsAny<string>())).ReturnsAsync(cppprofile1);

            _dynamoDBUtility.Setup(p => p.GetEResBetaTesterItems(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _sQLSPService.Setup(p => p.ValidateHashPin<HashPinValidate>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(hashPinValidate1);

            _dynamoDBHelperService.Setup(p => p.SaveMPAppIdDeviceId<MileagePlusDetails>(It.IsAny<MileagePlusDetails>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _merchOffersService.Setup(p => p.GetSubscriptions<Subscription>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(merchData);

            _dynamoDBHelperService.Setup(p => p.GetAuthToken<MileagePlusDetails>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(milageData);

            _mileagePlusTFACSL.Setup(p => p.ValidateDevice(It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            _mileagePlusTFACSL.Setup(p => p.SignOutSession(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(true);

            //_sessionHelperService.Setup(p => p.SaveSession<List<MPSignIn>>(It.IsAny<MPSignIn>(), It.IsAny<HttpContextValues>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            //_sessionHelperService.Setup(p => p.SaveSession<List<MPPINPWSecurityQuestionsValidation>>(It.IsAny<HttpContextValues>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            _moqHashPin.Setup(p => p.ValidateHashPinAndGetAuthTokenDynamoDB(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(milageData);

            _moqSecurityQuestion.Setup(p => p.GetMPPinPwdSavedSecurityQuestions(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(securityquestion);


            if (request.Application.IsProduction)
                _configuration["EnabledMERCHChannels"] = "False";
            if (response.AccountValidation.IsTokenValid)
                _configuration["GetEmployeeIDFromGetProfileCustomerData"] = "False";
            if (response.AccountValidation.DeceasedAccount)
                _configuration["EnableMicroserviceAPIMigration"] = "False";
            if (response.AccountValidation.DeceasedAccount)
                _configuration["EnableEResAPIMigration"] = "False";
            if (!response.AccountValidation.IsTokenValid)
                _configuration["EnableEResAPIMigration"] = "False";
            if (response.AccountValidation.NeedToAcceptTNC)
                _configuration["NumberOfSecurityQuestionsNeedatPINPWDUpdate"] = "0";
            if (response.AccountValidation.HashValue == "2")
                _configuration["AndroidYoungAdultCorporateFixVersion"] = "";
            if (response.AccountValidation.HashValue == "3")
            {
                _configuration["AndroidYoungAdultCorporateFixVersion"] = "";
                _configuration["EnableEResAPIMigration"] = "False";

            }


            var result = _mPAuthenticationBusiness1.ValidateMPSignInV2(request);

            Assert.True(result.Exception != null || result.Result != null);
        }


        [Theory]
        [MemberData(nameof(MPAuthenticationInput.ValidateMPSignInV2Input_neg), MemberType = typeof(MPAuthenticationInput))]
        public void ValidateMPSignInV2_InformationIncorrect(MOBMPPINPWDValidateRequest request
            , bool ok
            , MOBMPPINPWDValidateResponse response
            , string jsonResponse
            , DPAccessTokenResponse dPAccessTokenResponse
            , bool data)
        {

            bool val;
            if (response.AccountValidation.HashValue == "1")
                val = false;
            else
                val = ok;
            var sessionData = GetContent<Session>("SessionData.json");
            var EResBetaTester = GetContent<List<EResBetaTester>>("EResBetaTester.json");
            var mobItem = GetContent<List<MOBItem>>("MOBItem.json");
            var empTravelTypesAndJAResponse = GetContent<MOBEmpTravelTypeAndJAProfileResponse>("MOBEmpTravelTypeAndJAProfileResponse.json");
            var EmployeeTravelProfile = GetContent<Service.Presentation.PersonModel.EmployeeTravelProfile>("EmployeeTravelProfile.json");
            var cppprofile = GetContent<List<MOBCPProfile>>("MOBCPProfile.json");
            var cppprofile1 = GetContent<MOBCPProfile>("MOBCPProfile_1.json");
            var hashPinValidate1 = GetContent<HashPinValidate>("HashPinValidate.json");
            var merch = GetFileContent("MerchManagementOutput.json");
            var merchData = JsonConvert.DeserializeObject<Subscription>(merch);

            var milage = GetFileContent("MileagePlusDetails.json");
            var milageData = JsonConvert.DeserializeObject<MileagePlusDetails>(milage);


            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CreateShoppingSession(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CheckIsCSSTokenValid(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<string>())).Returns(Task.FromResult((true, sessionData)));

            _mileagePlusTFACSL.Setup(p => p.GetTfaWrongAnswersFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(ok);
            _moqSecurityQuestion.Setup(p => p.GetMPPINPWDTitleMessagesForMPAuth(It.IsAny<List<string>>())).ReturnsAsync(mobItem);
            _dataPowerFactory.Setup(p => p.GetDPAuthenticatedToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(dPAccessTokenResponse);
            _tokenService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            _mpSecurityCheckDetailsService.Setup(p => p.GetMPSecurityCheckDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(jsonResponse);
            _customerProfile.Setup(p => p.PopulateProfiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<List<United.Services.Customer.Common.Profile>>(), It.IsAny<MOBCPProfileRequest>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<MOBApplication>())).ReturnsAsync(cppprofile);
            _mileagePlus.Setup(p => p.GetAccountSummary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary);
            _employeeReservations.Setup(p => p.GetTravelTypesAndJAProfile(It.IsAny<MOBEmpTravelTypeAndJAProfileRequest>())).Returns(empTravelTypesAndJAResponse);
            _mileagePlusTFACSL.Setup(p => p.GetTfaWrongAnswersFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(data);
            _customerProfile.Setup(p => p.GeteMailIDTFAMPSecurityDetails(It.IsAny<MOBMPPINPWDValidateRequest>(), It.IsAny<string>())).ReturnsAsync(cppprofile1);


            _sQLSPService.Setup(p => p.ValidateHashPin<HashPinValidate>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(hashPinValidate1);

            _dynamoDBHelperService.Setup(p => p.SaveMPAppIdDeviceId<MileagePlusDetails>(It.IsAny<MileagePlusDetails>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _merchOffersService.Setup(p => p.GetSubscriptions<Subscription>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(merchData);

            _dynamoDBHelperService.Setup(p => p.GetAuthToken<MileagePlusDetails>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(milageData);

            _mileagePlusTFACSL.Setup(p => p.ValidateDevice(It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            _mileagePlusTFACSL.Setup(p => p.SignOutSession(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(true);





            var result = _mPAuthenticationBusiness1.ValidateMPSignInV2(request);


            Assert.True(result.Result != null || result.Result.Exception != null);
            Assert.True(result.Result != null || result.Result.Exception != null);
        }


        [Theory]
        [MemberData(nameof(MPAuthenticationInput.SecurityQuestionsV2Input), MemberType = typeof(MPAuthenticationInput))]
        public void ValidateTFASecurityQuestionsV2_Test(string input, bool input1)
        {
            var requestPayload = JsonConvert.DeserializeObject<MOBTFAMPDeviceRequest>(input);

            var session = GetFileContent("SessionDataV2.json");
            var sessionData = JsonConvert.DeserializeObject<Session>(session);

            var response = GetFileContent("ValidateTFASecurityQuestionsV2Response.json");
            var profileresponse = GetFileContent("ProfileResponse_1.json");
            var MOBCPProfile = GetFileContent("MOBCPProfile.json");
            var MOBItem = GetFileContent("MOBItem.json");
            var traveltype = GetFileContent("EmpProfileTravelTypeResponse.json");
            var cslResponse = GetFileContent("MOBStatusLiftBannerResponse.json");
            var merch = GetFileContent("MerchManagementOutput.json");

            var responseData = JsonConvert.DeserializeObject<MOBTFAMPDeviceResponse>(response);
            var validateData = JsonConvert.DeserializeObject<MPPINPWSecurityQuestionsValidation>(response);
            var mpsigninData = JsonConvert.DeserializeObject<MPSignIn>(response);
            var mppinData = JsonConvert.DeserializeObject<MOBMPPINPWDValidateRequest>(response);
            // var accData = GetContent<MOBMPAccountSummary>(response);
            var profileData = JsonConvert.DeserializeObject<ProfileResponse>(profileresponse);
            var mobcpresponse = JsonConvert.DeserializeObject<List<MOBCPProfile>>(MOBCPProfile);
            var itemresponse = JsonConvert.DeserializeObject<List<MOBItem>>(MOBItem);
            var empdata = JsonConvert.DeserializeObject<MOBEmpTravelTypeAndJAProfileResponse>(response);
            var statusdata = JsonConvert.DeserializeObject<MemberPromotion<MOBStatusLiftBannerResponse>>(cslResponse);
            var merchData = JsonConvert.DeserializeObject<Subscription>(merch);

            _sessionHelperService.Setup(p => p.GetSession<MPSignIn>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mpsigninData);

            _sessionHelperService.Setup(p => p.GetSession<MOBMPPINPWDValidateRequest>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mppinData);

            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);

            _sessionHelperService.Setup(p => p.GetSession<MPPINPWSecurityQuestionsValidation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(validateData);

            _moqSecurityQuestion.Setup(p => p.ValidateSecurityAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(input1);

            _mileagePlus.Setup(p => p.GetAccountSummary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(responseData.OPAccountSummary);

            _tokenService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            _mpSecurityCheckDetailsService.Setup(p => p.GetMPSecurityCheckDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(profileresponse);

            _customerProfile.Setup(p => p.PopulateProfiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<List<Services.Customer.Common.Profile>>(), It.IsAny<MOBCPProfileRequest>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<MOBApplication>())).ReturnsAsync(mobcpresponse);

            _moqSecurityQuestion.Setup(p => p.GetMPPINPWDTitleMessagesForMPAuth(It.IsAny<List<string>>())).ReturnsAsync(itemresponse);


            _merchOffersService.Setup(p => p.GetSubscriptions<Subscription>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(merchData);


            _customerProfile.Setup(p => p.EnableYoungAdult(It.IsAny<bool>())).Returns(true);
            _employeeReservations.Setup(p => p.GetTravelTypesAndJAProfile(It.IsAny<MOBEmpTravelTypeAndJAProfileRequest>())).Returns(empdata);
            _employeeProfileTravelType.Setup(p => p.GetTravelType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(traveltype);



            var result = _mPAuthenticationBusiness.ValidateTFASecurityQuestionsV2(requestPayload);
            if (result?.Exception == null)
            {
                Assert.True(result.Result != null && result.Result.TransactionId != null);
            }
            else
                Assert.True(result.Exception != null && result.Exception.InnerException != null);
        }

        [Theory]
        [MemberData(nameof(MPAuthenticationInput.SecurityQuestionsV2EmpInput), MemberType = typeof(MPAuthenticationInput))]
        public void ValidateTFASecurityQuestionsV2_EmployeeAccountTest(string input, bool input1)
        {
            var requestPayload = JsonConvert.DeserializeObject<MOBTFAMPDeviceRequest>(input);

            var session = GetFileContent("SessionDataV2.json");
            var sessionData = JsonConvert.DeserializeObject<Session>(session);

            var response = GetFileContent("ValidateTFASecurityQuestionsV2Response.json");
            var profileresponse = GetFileContent("ProfileResponse_1.json");
            var MOBCPProfile = GetFileContent("MOBCPProfile.json");
            var MOBItem = GetFileContent("MOBItem.json");
            var traveltype = GetFileContent("EmpProfileTravelTypeResponse.json");
            var cslResponse = GetFileContent("MOBStatusLiftBannerResponse.json");
            var merch = GetFileContent("MerchManagementOutput.json");

            var responseData = JsonConvert.DeserializeObject<MOBTFAMPDeviceResponse>(response);
            var validateData = JsonConvert.DeserializeObject<MPPINPWSecurityQuestionsValidation>(response);
            var mpsigninData = JsonConvert.DeserializeObject<MPSignIn>(response);
            var mppinData = JsonConvert.DeserializeObject<MOBMPPINPWDValidateRequest>(response);
            //var accData = GetContent<MOBMPAccountSummary>(response);
            var profileData = JsonConvert.DeserializeObject<ProfileResponse>(profileresponse);
            var mobcpresponse = JsonConvert.DeserializeObject<List<MOBCPProfile>>(MOBCPProfile);
            var itemresponse = JsonConvert.DeserializeObject<List<MOBItem>>(MOBItem);
            var empdata = JsonConvert.DeserializeObject<MOBEmpTravelTypeAndJAProfileResponse>(response);
            var statusdata = JsonConvert.DeserializeObject<MemberPromotion<MOBStatusLiftBannerResponse>>(cslResponse);
            var merchData = JsonConvert.DeserializeObject<Subscription>(merch);

            _sessionHelperService.Setup(p => p.GetSession<MPSignIn>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mpsigninData);

            _sessionHelperService.Setup(p => p.GetSession<MOBMPPINPWDValidateRequest>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mppinData);

            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);

            _sessionHelperService.Setup(p => p.GetSession<MPPINPWSecurityQuestionsValidation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(validateData);

            _moqSecurityQuestion.Setup(p => p.ValidateSecurityAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(input1);

            _mileagePlus.Setup(p => p.GetAccountSummary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(responseData.OPAccountSummary);

            _tokenService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            _mpSecurityCheckDetailsService.Setup(p => p.GetMPSecurityCheckDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(profileresponse);

            _customerProfile.Setup(p => p.PopulateProfiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<List<Services.Customer.Common.Profile>>(), It.IsAny<MOBCPProfileRequest>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<MOBApplication>())).ReturnsAsync(mobcpresponse);

            _moqSecurityQuestion.Setup(p => p.GetMPPINPWDTitleMessagesForMPAuth(It.IsAny<List<string>>())).ReturnsAsync(itemresponse);


            _merchOffersService.Setup(p => p.GetSubscriptions<Subscription>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(merchData);


            _customerProfile.Setup(p => p.EnableYoungAdult(It.IsAny<bool>())).Returns(true);
            _employeeReservations.Setup(p => p.GetTravelTypesAndJAProfile(It.IsAny<MOBEmpTravelTypeAndJAProfileRequest>())).Returns(empdata);
            _employeeProfileTravelType.Setup(p => p.GetTravelType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(traveltype);
            //_sessionHelperService.Setup(p => p.GetSession<MPPINPWSecurityQuestionsValidation>(It.IsAny<HttpContextValues>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mobresponse);

            var result = _mPAuthenticationBusiness.ValidateTFASecurityQuestionsV2(requestPayload);
            if (result?.Exception == null)
            {
                Assert.True(result.Result != null && result.Result.TransactionId != null);
            }
            else
                Assert.True(result.Exception != null && result.Exception.InnerException != null);
        }

        [Theory]
        [MemberData(nameof(MPAuthenticationInput.SecurityQuestionsV2CorporateInput), MemberType = typeof(MPAuthenticationInput))]
        public void ValidateTFASecurityQuestionsV2_CorporateAccountTest(string input, bool input1)
        {
            var requestPayload = JsonConvert.DeserializeObject<MOBTFAMPDeviceRequest>(input);

            var session = GetFileContent("SessionDataV2.json");
            var sessionData = JsonConvert.DeserializeObject<Session>(session);

            var response = GetFileContent("ValidateTFASecurityQuestionsV2Response.json");
            var profileresponse = GetFileContent("ProfileResponse_1.json");
            var MOBCPProfile = GetFileContent("MOBCPProfile.json");
            var MOBItem = GetFileContent("MOBItem.json");
            var traveltype = GetFileContent("EmpProfileTravelTypeResponse.json");
            var cslResponse = GetFileContent("MOBStatusLiftBannerResponse.json");
            var merch = GetFileContent("MerchManagementOutput.json");

            var responseData = JsonConvert.DeserializeObject<MOBTFAMPDeviceResponse>(response);
            var validateData = JsonConvert.DeserializeObject<MPPINPWSecurityQuestionsValidation>(response);
            var mpsigninData = JsonConvert.DeserializeObject<MPSignIn>(response);
            var mppinData = JsonConvert.DeserializeObject<MOBMPPINPWDValidateRequest>(response);
            // var accData = GetContent<MOBMPAccountSummary>(response);
            var profileData = JsonConvert.DeserializeObject<ProfileResponse>(profileresponse);
            var mobcpresponse = JsonConvert.DeserializeObject<List<MOBCPProfile>>(MOBCPProfile);
            var itemresponse = JsonConvert.DeserializeObject<List<MOBItem>>(MOBItem);
            var empdata = JsonConvert.DeserializeObject<MOBEmpTravelTypeAndJAProfileResponse>(response);
            var statusdata = JsonConvert.DeserializeObject<MemberPromotion<MOBStatusLiftBannerResponse>>(cslResponse);
            var merchData = JsonConvert.DeserializeObject<Subscription>(merch);

            _sessionHelperService.Setup(p => p.GetSession<MPSignIn>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mpsigninData);

            _sessionHelperService.Setup(p => p.GetSession<MOBMPPINPWDValidateRequest>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mppinData);

            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);

            _sessionHelperService.Setup(p => p.GetSession<MPPINPWSecurityQuestionsValidation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(validateData);

            _moqSecurityQuestion.Setup(p => p.ValidateSecurityAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(input1);

            _mileagePlus.Setup(p => p.GetAccountSummary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(responseData.OPAccountSummary);

            _tokenService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            _mpSecurityCheckDetailsService.Setup(p => p.GetMPSecurityCheckDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(profileresponse);

            _customerProfile.Setup(p => p.PopulateProfiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<List<Services.Customer.Common.Profile>>(), It.IsAny<MOBCPProfileRequest>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<MOBApplication>())).ReturnsAsync(mobcpresponse);

            _moqSecurityQuestion.Setup(p => p.GetMPPINPWDTitleMessagesForMPAuth(It.IsAny<List<string>>())).ReturnsAsync(itemresponse);


            _merchOffersService.Setup(p => p.GetSubscriptions<Subscription>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(merchData);


            _customerProfile.Setup(p => p.EnableYoungAdult(It.IsAny<bool>())).Returns(true);
            _employeeReservations.Setup(p => p.GetTravelTypesAndJAProfile(It.IsAny<MOBEmpTravelTypeAndJAProfileRequest>())).Returns(empdata);
            _employeeProfileTravelType.Setup(p => p.GetTravelType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(traveltype);

            var result = _mPAuthenticationBusiness.ValidateTFASecurityQuestionsV2(requestPayload);
            if (result?.Exception == null)
            {
                Assert.True(result.Result != null && result.Result.TransactionId != null);
            }
            else
                Assert.True(result.Exception != null && result.Exception.InnerException != null);
        }

        [Theory]
        [MemberData(nameof(MPAuthenticationInput.SecurityQuestionsV2ExceptionInput), MemberType = typeof(MPAuthenticationInput))]
        public void ValidateTFASecurityQuestionsV2Exception(string input, bool input1)
        {
            var requestPayload = JsonConvert.DeserializeObject<MOBTFAMPDeviceRequest>(input);

            var session = GetFileContent("SessionDataV2.json");
            var sessionData = JsonConvert.DeserializeObject<Session>(session);

            var response = GetFileContent("ValidateTFASecurityQuestionsV2Response.json");
            var profileresponse = GetFileContent("ProfileResponse_1.json");
            var MOBCPProfile = GetFileContent("MOBCPProfile.json");
            var MOBItem = GetFileContent("MOBItem.json");
            var traveltype = GetFileContent("EmpProfileTravelTypeResponse.json");
            var cslResponse = GetFileContent("MOBStatusLiftBannerResponse.json");
            var merch = GetFileContent("MerchManagementOutput.json");
            var valres = GetFileContent("MOBMPPINPWDValidateResponse.json");

            var responseData = JsonConvert.DeserializeObject<MOBTFAMPDeviceResponse>(response);
            var validateData = JsonConvert.DeserializeObject<MPPINPWSecurityQuestionsValidation>(response);
            var mpsigninData = JsonConvert.DeserializeObject<MPSignIn>(response);
            var mppinData = JsonConvert.DeserializeObject<MOBMPPINPWDValidateRequest>(response);
            // var accData = GetContent<MOBMPAccountSummary>(response);
            var profileData = JsonConvert.DeserializeObject<ProfileResponse>(profileresponse);
            var mobcpresponse = JsonConvert.DeserializeObject<List<MOBCPProfile>>(MOBCPProfile);
            var itemresponse = JsonConvert.DeserializeObject<List<MOBItem>>(MOBItem);
            var empdata = JsonConvert.DeserializeObject<MOBEmpTravelTypeAndJAProfileResponse>(response);
            var statusdata = JsonConvert.DeserializeObject<MemberPromotion<MOBStatusLiftBannerResponse>>(cslResponse);
            var merchData = JsonConvert.DeserializeObject<Subscription>(merch);
            //var valresData = JsonConvert.DeserializeObject<MOBMPPINPWDValidateResponse>(valres);



            _sessionHelperService.Setup(p => p.GetSession<MPSignIn>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mpsigninData);

            _sessionHelperService.Setup(p => p.GetSession<MOBMPPINPWDValidateRequest>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mppinData);

            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);

            //_sessionHelperService.Setup(p => p.GetSession<MPPINPWSecurityQuestionsValidation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(validateData);


            _mileagePlus.Setup(p => p.GetAccountSummary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(responseData.OPAccountSummary);

            _tokenService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            _mpSecurityCheckDetailsService.Setup(p => p.GetMPSecurityCheckDetails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(profileresponse);

            _customerProfile.Setup(p => p.PopulateProfiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<List<Services.Customer.Common.Profile>>(), It.IsAny<MOBCPProfileRequest>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<MOBApplication>())).ReturnsAsync(mobcpresponse);

            _moqSecurityQuestion.Setup(p => p.GetMPPINPWDTitleMessagesForMPAuth(It.IsAny<List<string>>())).ReturnsAsync(itemresponse);


            _merchOffersService.Setup(p => p.GetSubscriptions<Subscription>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(merchData);


            //_customerProfile.Setup(p => p.EnableYoungAdult(It.IsAny<bool>())).Returns(true);
            _employeeReservations.Setup(p => p.GetTravelTypesAndJAProfile(It.IsAny<MOBEmpTravelTypeAndJAProfileRequest>())).Returns(empdata);
            _employeeProfileTravelType.Setup(p => p.GetTravelType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(traveltype);

            _moqSecurityQuestion.Setup(p => p.ValidateSecurityAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(input1);

            //_sessionHelperService.Setup(p => p.GetSession<MOBMPPINPWDValidateResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(valresData);
            //_sessionHelperService.Setup(p => p.GetSession<MOBMPPINPWDValidateResponse>(It.IsAny<HttpContextValues>(), It.IsAny<string>(), It.IsAny<string>())).ResturnsAsync();


            var result = _mPAuthenticationBusiness.ValidateTFASecurityQuestionsV2(requestPayload);
            if (result?.Exception == null)
            {
                Assert.True(result.Result != null && result.Result.TransactionId != null);
            }
            else
                Assert.True(result.Exception != null && result.Exception.InnerException != null);
        }

    }
}

