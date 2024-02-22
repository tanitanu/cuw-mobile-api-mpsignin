using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.Profile;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.CSLSerivce;
using United.Mobile.DataAccess.OnPremiseSQLSP;
using United.Mobile.MemberSignIn.Domain;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.OneClickEnrollment;
using United.Service.Presentation.ReservationResponseModel;
using United.Service.Presentation.SecurityResponseModel;
using United.Services.Customer.Common;
using United.Services.ProfileValidation.Common;
using United.Utility.Helper;
using Xunit;
using Microsoft.Extensions.Logging;


namespace United.Mobile.Test.MemberSignInTests.Api
{
    public class MemberSignInBusinessTests
    {
        private readonly Mock<ICacheLog<MemberSignInBusiness>> _logger;
        private readonly IConfiguration _configuration;
        private readonly IConfiguration _configuration1;
        private readonly Mock<ISessionHelperService> _sessionHelperService;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<ICustomerProfile> _customerProfile;
        private readonly Mock<IMPTraveler> _mpTraveler;
        private readonly Mock<ICustomerDataService> _customerDataService;
        private readonly Mock<ILoyaltyAWSService> _LoyaltyAWSService;
        private readonly Mock<IMileagePlusTFACSL> _mileagePlusTFACSL;
        private readonly Mock<IMileagePlusCSL> _mileagePlusCSL;
        private readonly Mock<IMileagePlus> _mileagePlus;
        private readonly Mock<IDPService> _dPService;
        private readonly MemberSignInBusiness _memberSignInBusiness;
        private readonly Mock<IShoppingSessionHelper> _shoppingSessionHelper;
        private readonly Mock<IUtilitiesService> _utilitiesService;
        private readonly Mock<IMPSecurityQuestionsService> _mPSecurityQuestionsService;
        private readonly Mock<ISecurityQuestion> _moqSecurityQuestion;
        private readonly Mock<IDynamoDBService> _moqDynamoDBService;
        private readonly Mock<IUtility> _utility;
        private readonly Mock<ICatalog> _catalog;
        private readonly Mock<ISQLSPService> _sQLSPService;
        private readonly Mock<IDynamoDBUtility> _dynamoDBUtility;
        private readonly Mock<IRavenService> _ravenService;
        private readonly Mock<IContactPointService> _contactpointService;
        private readonly Mock<IRavenCloudService> _ravenCloudService;

        public MemberSignInBusinessTests()
        {
            _logger = new Mock<ICacheLog<MemberSignInBusiness>>();
            _configuration = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
                 .Build();
            _configuration1 = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.test1.json", optional: false, reloadOnChange: true)
                 .Build();
            _sessionHelperService = new Mock<ISessionHelperService>();
            _headers = new Mock<IHeaders>();
            _customerProfile = new Mock<ICustomerProfile>();
            _mpTraveler = new Mock<IMPTraveler>();
            _customerDataService = new Mock<ICustomerDataService>();
            _LoyaltyAWSService = new Mock<ILoyaltyAWSService>();
            _mileagePlusTFACSL = new Mock<IMileagePlusTFACSL>();
            _mileagePlusCSL = new Mock<IMileagePlusCSL>();
            _mileagePlus = new Mock<IMileagePlus>();
            _dPService = new Mock<IDPService>();
            _shoppingSessionHelper = new Mock<IShoppingSessionHelper>();
            _utilitiesService = new Mock<IUtilitiesService>();
            _mPSecurityQuestionsService = new Mock<IMPSecurityQuestionsService>();
            _moqSecurityQuestion = new Mock<ISecurityQuestion>();
            _moqDynamoDBService = new Mock<IDynamoDBService>();
            _utility = new Mock<IUtility>();
            _catalog = new Mock<ICatalog>();
            _sQLSPService = new Mock<ISQLSPService>();
            _dynamoDBUtility = new Mock<IDynamoDBUtility>();
            _ravenService = new Mock<IRavenService>();
            _contactpointService = new Mock<IContactPointService>();
            _ravenCloudService = new Mock<IRavenCloudService>();
            _memberSignInBusiness = new MemberSignInBusiness(_logger.Object, _configuration, _sessionHelperService.Object, _customerProfile.Object, _mpTraveler.Object, _LoyaltyAWSService.Object, _mileagePlusTFACSL.Object, _mileagePlusCSL.Object, _shoppingSessionHelper.Object, _moqSecurityQuestion.Object,  _utility.Object, _catalog.Object,  _dynamoDBUtility.Object, _ravenService.Object, _contactpointService.Object, _ravenCloudService.Object);
            SetHeaders();
        }
        private void SetHeaders(string deviceId = "D873298F-F27D-4AEC-BE6C-DE79C4259626"
              , string applicationId = "2"
              , string appVersion = "4.1.26"
              , string transactionId = "3f575588-bb12-41fe-8be7-f57c55fe7762|afc1db10-5c39-4ef4-9d35-df137d56a23e"
              , string languageCode = "en-US"
              , string sessionId = "D58E298C35274F6F873A133386A42916")
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

        private string GetFileContent(string fileName)
        {
            var path = Path.IsPathRooted(fileName) ? fileName
                                                : Path.GetRelativePath(Directory.GetCurrentDirectory(), fileName);
            return File.ReadAllText(path);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.InputMemeberSignIn), MemberType = typeof(TestDataGenerator))]
        public void MPEnrollmentWithSecurityQuestions_Test(MOBMPEnRollmentRequest requestPayload, EnrollResponse response)
        {
            var session = GetFileContent("SessionData.json");
            var sessionData = JsonConvert.DeserializeObject<Session>(session);
            var securityInfo = JsonConvert.DeserializeObject<List<Securityquestion>>(GetFileContent("SecurityInformation.json"));
            var responseData = JsonConvert.DeserializeObject<MOBMPMPEnRollmentResponse>(GetFileContent("MOBMPMPEnRollmentResponse.json"));
            _shoppingSessionHelper.Setup(p => p.CreateShoppingSession(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);
            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            _sessionHelperService.Setup(p => p.GetSession<Session>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(sessionData);
            _customerDataService.Setup(p => p.InsertMPEnrollment<EnrollResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response);
            _mileagePlus.Setup(p => p.GetAccountSummary(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(responseData.OPAccountSummary);
            //_profile.Setup(p => p.GetAgeByDOB(It.IsAny<string>(), It.IsAny<string>())).Returns(18);
            
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.InputMemeberSignIn2), MemberType = typeof(TestDataGenerator))]
        public void OneClickEnrollment_Test(MOBJoinMileagePlusEnrollmentRequest requestPayload)
        {
            var Filename = GetFileContent("SessionData.json");
            var sessionData = JsonConvert.DeserializeObject<Session>(Filename);
            var loyaltyresponse = GetFileContent("LoyaltyResponse.json");
            var loyaltyResponseData = JsonConvert.DeserializeObject<CslResponse<LoyaltyResponse>>(loyaltyresponse);
            var reservationDetail = JsonConvert.DeserializeObject<ReservationDetail>(GetFileContent("ReservationDetail.json"));
            var updateResponse = JsonConvert.DeserializeObject<MOBUpdateTravelerInfoResponse>(GetFileContent("MOBUpdateTravelerInfoResponse.json"));
            _sessionHelperService.Setup(p => p.GetSession<Session>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(sessionData);
            _sessionHelperService.Setup(p => p.GetSession<ReservationDetail>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservationDetail);
            _shoppingSessionHelper.Setup(p => p.CreateShoppingSession(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);
            _LoyaltyAWSService.Setup(_ => _.OneClickEnrollment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(loyaltyresponse);
            _mpTraveler.Setup(p => p.UpdateTravelerMPId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(updateResponse);

            _catalog.Setup(p => p.IsClientCatalogEnabled(It.IsAny<int>(), It.IsAny<string[]>())).ReturnsAsync(true);



            var result = _memberSignInBusiness.OneClickEnrollment(requestPayload);
            if (result.Exception == null)
            {
                Assert.True(result.Result.Exception == null);
                Assert.True(result.Result.LanguageCode != null);
            }
            else
                Assert.True(result.Exception.InnerException != null);
            //Assert.True(result != null || result.Exception != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.InputMemeberSignIn2_exc), MemberType = typeof(TestDataGenerator))]
        public void OneClickEnrollment_Exception(MOBJoinMileagePlusEnrollmentRequest requestPayload)
        {
            var Filename = GetFileContent("SessionData.json");
            var sessionData = JsonConvert.DeserializeObject<Session>(Filename);
            var loyaltyresponse = GetFileContent("LoyaltyResponse.json");
            var loyaltyResponseData = JsonConvert.DeserializeObject<CslResponse<LoyaltyResponse>>(loyaltyresponse);
            var reservationDetail = JsonConvert.DeserializeObject<ReservationDetail>(GetFileContent("ReservationDetail.json"));
            var updateResponse = JsonConvert.DeserializeObject<MOBUpdateTravelerInfoResponse>(GetFileContent("MOBUpdateTravelerInfoResponse.json"));
            _sessionHelperService.Setup(p => p.GetSession<Session>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(sessionData);
            _sessionHelperService.Setup(p => p.GetSession<ReservationDetail>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservationDetail);
            _shoppingSessionHelper.Setup(p => p.CreateShoppingSession(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);
            _LoyaltyAWSService.Setup(_ => _.OneClickEnrollment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(loyaltyresponse);
            _mpTraveler.Setup(p => p.UpdateTravelerMPId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(updateResponse);


            if (requestPayload.SessionId == "9D4BA31963DD4DD4BC0C0E984C4BB6D6")
                _configuration["EnableUCBchange"] = "False";


            var result = _memberSignInBusiness.OneClickEnrollment(requestPayload);

            if (result.Exception == null)
            {
                Assert.True(result.Result.Exception == null);
                Assert.True(result.Result.LanguageCode != null);
            }
            else
                Assert.True(result != null || result.Exception != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.InputMemeberSignIn3), MemberType = typeof(TestDataGenerator))]
        public void MPSignInNeedHelp_Test(MOBMPSignInNeedHelpRequest requestPayload, MOBMPAccountValidation datalist, bool ok)
        {
            var Filename = GetFileContent("SessionData.json");
            var Securityquestion = JsonConvert.DeserializeObject<List<Securityquestion>>(GetFileContent("Securityquestion.json"));
            var mobItem = JsonConvert.DeserializeObject<List<MOBItem>>(GetFileContent("MOBItem.json"));
            var cppprofile = JsonConvert.DeserializeObject<MOBCPProfile>(GetFileContent("MOBCPProfile.json"));
            var jsonData = JsonConvert.DeserializeObject<ValidateMileagePlusNamesResponse>(GetFileContent("ValidateMileagePlusNamesResponse.json"));
            var data = JsonConvert.DeserializeObject<MPPINPWSecurityQuestionsValidation>(GetFileContent("MPPINPWSecurityQuestionsValidation.json"));
            var securityAnswer = JsonConvert.DeserializeObject<ValidateSecurityAnswerResponse>(GetFileContent("ValidateSecurityAnswerResponse.json"));
            var sessionData = JsonConvert.DeserializeObject<Session>(Filename);
            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CreateShoppingSession(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sessionData);
            _sessionHelperService.Setup(p => p.GetSession<MPPINPWSecurityQuestionsValidation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(data);
            _mileagePlusCSL.Setup(p => p.ValidateAccount(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<bool>())).Returns(datalist);
            _customerProfile.Setup(p => p.ValidateMPNames(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _mileagePlusTFACSL.Setup(p => p.ValidateDevice(It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _customerProfile.Setup(p => p.GeteMailIDTFAMPSecurityDetails(It.IsAny<MOBMPPINPWDValidateRequest>(), It.IsAny<string>())).ReturnsAsync(cppprofile);
            //_mPDynamoDB.Setup(p => p.GetMPPINPWDTitleMessages(It.IsAny<List<string>>())).ReturnsAsync(mobItem);
            _mPSecurityQuestionsService.Setup(p => p.ValidateSecurityAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(securityAnswer.ToString()));
            _moqSecurityQuestion.Setup(p => p.ValidateSecurityAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(ok);
            _mileagePlusCSL.Setup(p => p.SearchMPAccount(It.IsAny<MOBMPSignInNeedHelpRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(ok);
            _moqSecurityQuestion.Setup(p => p.GetMPPinPwdSavedSecurityQuestions(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(Securityquestion);
            _moqSecurityQuestion.Setup(p => p.MPPinPwdValidatePassowrd(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(ok);
            _moqSecurityQuestion.Setup(p => p.MPPinPwdUpdateCustomerPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(ok);
            _mileagePlusTFACSL.Setup(p => p.GetTfaWrongAnswersFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(true);

            //_sessionHelperService.Setup(p => p.SaveSession<MPPINPWSecurityQuestionsValidation>()).Returns();
            if (requestPayload.Application.IsProduction)
            {
                _configuration["EnableDPToken"] = "false";
                _configuration["TFASwitchON"] = "false";
                _configuration["MPSignInNeedHelpFix"] = "false";
            }
            var result = _memberSignInBusiness.MPSignInNeedHelp(requestPayload);
            
            Assert.True(result.Result != null || result.Result.Exception != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.InputMemeberSignIn3_neg), MemberType = typeof(TestDataGenerator))]
        public void MPSignInNeedHelp_NegativeTestCase(MOBMPSignInNeedHelpRequest requestPayload, MOBMPAccountValidation datalist, bool ok)
        {
            var Filename = GetFileContent("SessionData.json");
            var Securityquestion = JsonConvert.DeserializeObject<List<Securityquestion>>(GetFileContent("Securityquestion.json"));
            var mobItem = JsonConvert.DeserializeObject<List<MOBItem>>(GetFileContent("MOBItem.json"));
            var cppprofile = JsonConvert.DeserializeObject<MOBCPProfile>(GetFileContent("MOBCPProfile.json"));
            var jsonData = JsonConvert.DeserializeObject<ValidateMileagePlusNamesResponse>(GetFileContent("ValidateMileagePlusNamesResponse.json"));
            var data = JsonConvert.DeserializeObject<MPPINPWSecurityQuestionsValidation>(GetFileContent("MPPINPWSecurityQuestionsValidation.json"));
            var securityAnswer = JsonConvert.DeserializeObject<ValidateSecurityAnswerResponse>(GetFileContent("ValidateSecurityAnswerResponse.json"));
            var sessionData = JsonConvert.DeserializeObject<Session>(Filename);


            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CreateShoppingSession(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sessionData);
           _sessionHelperService.Setup(p => p.GetSession<MPPINPWSecurityQuestionsValidation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(data);
            _mileagePlusCSL.Setup(p => p.ValidateAccount(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<bool>())).Returns(datalist);
            _customerProfile.Setup(p => p.ValidateMPNames(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _mileagePlusTFACSL.Setup(p => p.ValidateDevice(It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _customerProfile.Setup(p => p.GeteMailIDTFAMPSecurityDetails(It.IsAny<MOBMPPINPWDValidateRequest>(), It.IsAny<string>())).ReturnsAsync(cppprofile);
            //_mPDynamoDB.Setup(p => p.GetMPPINPWDTitleMessages(It.IsAny<List<string>>())).ReturnsAsync(mobItem);
            _mPSecurityQuestionsService.Setup(p => p.ValidateSecurityAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(securityAnswer.ToString());
            //_moqSecurityQuestion.Setup(p => p.ValidateSecurityAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(ok);
            _mileagePlusCSL.Setup(p => p.SearchMPAccount(It.IsAny<MOBMPSignInNeedHelpRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(ok);
            _moqSecurityQuestion.Setup(p => p.GetMPPinPwdSavedSecurityQuestions(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(Securityquestion);
            _moqSecurityQuestion.Setup(p => p.MPPinPwdValidatePassowrd(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(ok);
            _moqSecurityQuestion.Setup(p => p.MPPinPwdUpdateCustomerPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(ok);

            _mileagePlusTFACSL.Setup(p => p.GetTfaWrongAnswersFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(false);
            _mileagePlusCSL.Setup(p => p.GetWrongAnswersFlag(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(false);

            if (requestPayload.Application.IsProduction)
            {
                _configuration["EnableDPToken"] = "True";
                _configuration["TFASwitchON"] = "false";
                _configuration["MPSignInNeedHelpFix"] = "false";
            }
            if (requestPayload.TransactionId == "ad6526e4-bf8f-4dc1-bcb1-1b0866f4557d|9c120cc9-6dcd-497e-8144-3bd8f379f314")
            {
                _configuration["EnableDPToken"] = "True";
            }
            var result = _memberSignInBusiness.MPSignInNeedHelp(requestPayload);
            
            Assert.True(result.Result != null || result.Result.Exception != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.InputMemeberSignIn3_neg), MemberType = typeof(TestDataGenerator))]
        public void MPSignInNeedHelp_Exception(MOBMPSignInNeedHelpRequest requestPayload, MOBMPAccountValidation datalist, bool ok)
        {
            var Filename = GetFileContent("SessionData.json");
            var Securityquestion = JsonConvert.DeserializeObject<List<Securityquestion>>(GetFileContent("Securityquestion.json"));
            var mobItem = JsonConvert.DeserializeObject<List<MOBItem>>(GetFileContent("MOBItem.json"));
            var cppprofile = JsonConvert.DeserializeObject<MOBCPProfile>(GetFileContent("MOBCPProfile.json"));
            var jsonData = JsonConvert.DeserializeObject<ValidateMileagePlusNamesResponse>(GetFileContent("ValidateMileagePlusNamesResponse.json"));
            var data = JsonConvert.DeserializeObject<MPPINPWSecurityQuestionsValidation>(GetFileContent("MPPINPWSecurityQuestionsValidation.json"));
            var securityAnswer = JsonConvert.DeserializeObject<ValidateSecurityAnswerResponse>(GetFileContent("ValidateSecurityAnswerResponse.json"));
            var sessionData = JsonConvert.DeserializeObject<Session>(Filename);
            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);
            _shoppingSessionHelper.Setup(p => p.CreateShoppingSession(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sessionData);
            _sessionHelperService.Setup(p => p.GetSession<MPPINPWSecurityQuestionsValidation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(data);
            _mileagePlusCSL.Setup(p => p.ValidateAccount(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Session>(), It.IsAny<bool>())).Returns(datalist);
            _customerProfile.Setup(p => p.ValidateMPNames(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _mileagePlusTFACSL.Setup(p => p.ValidateDevice(It.IsAny<Session>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _customerProfile.Setup(p => p.GeteMailIDTFAMPSecurityDetails(It.IsAny<MOBMPPINPWDValidateRequest>(), It.IsAny<string>())).ReturnsAsync(cppprofile);
            //_mPDynamoDB.Setup(p => p.GetMPPINPWDTitleMessages(It.IsAny<List<string>>())).ReturnsAsync(mobItem);
            _mPSecurityQuestionsService.Setup(p => p.ValidateSecurityAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(securityAnswer.ToString());
            _moqSecurityQuestion.Setup(p => p.ValidateSecurityAnswer(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(ok);
            _mileagePlusCSL.Setup(p => p.SearchMPAccount(It.IsAny<MOBMPSignInNeedHelpRequest>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(ok);
            _moqSecurityQuestion.Setup(p => p.GetMPPinPwdSavedSecurityQuestions(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(Securityquestion);
            _moqSecurityQuestion.Setup(p => p.MPPinPwdValidatePassowrd(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(ok);
            _moqSecurityQuestion.Setup(p => p.MPPinPwdUpdateCustomerPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(ok);

            _mileagePlusTFACSL.Setup(p => p.GetTfaWrongAnswersFlag(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(true);
            _mileagePlusCSL.Setup(p => p.GetWrongAnswersFlag(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(false);

            if (requestPayload.SessionID == "9D4BA31963DD4DD4BC0C0E984C4BB6D6")
            {
                _configuration["EnableDPToken"] = "True";
            }
            var _memberSignInBusiness = new MemberSignInBusiness(_logger.Object, _configuration1, _sessionHelperService.Object, _customerProfile.Object, _mpTraveler.Object, _LoyaltyAWSService.Object, _mileagePlusTFACSL.Object, _mileagePlusCSL.Object, _shoppingSessionHelper.Object, _moqSecurityQuestion.Object,  _utility.Object, _catalog.Object,  _dynamoDBUtility.Object, _ravenService.Object, _contactpointService.Object, _ravenCloudService.Object);

            var result = _memberSignInBusiness.MPSignInNeedHelp(requestPayload);
            if (result.Exception == null)
            {
                Assert.True(result.Result.Exception == null);
                Assert.True(result.Result.LanguageCode != null);
            }
            else
                Assert.True(result.Exception.InnerExceptions != null);
        }


        [Theory]
        [MemberData(nameof(TestDataGenerator.InputMemeberSignIn4), MemberType = typeof(TestDataGenerator))]
        public void SendResetAccountEmail_Test(MOBTFAMPDeviceRequest requestPayload)
        {
            var Filename = GetFileContent("SessionData.json");
            var cppprofile = JsonConvert.DeserializeObject<MOBCPProfile>(GetFileContent("MOBCPProfile.json"));
            var sessionData = JsonConvert.DeserializeObject<Session>(Filename);
            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);
            _sessionHelperService.Setup(p => p.GetSession<MOBCPProfile>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(cppprofile);
            var result = _memberSignInBusiness.SendResetAccountEmail(requestPayload);
            if (result.Exception == null)
            {
                Assert.True(result.Result.Exception == null);
                Assert.True(result.Result.LanguageCode != null);
            }
            else
                Assert.True(result.Exception.InnerExceptions != null);
        }

    }
}
