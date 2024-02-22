using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.Profile;
using United.Definition;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.OnPremiseSQLSP;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.MPSignIn;
using United.Mobile.MPAccountProfile.Domain;
using United.Utility.Helper;
using Xunit;


namespace United.Mobile.Test.MPAccountProfileTests.Api
{
    public class MPAccountProfileBusinessTest
    {
        private readonly Mock<ICacheLog<MPAccountProfileBusiness>> _logger;
        private readonly MPAccountProfileBusiness _mPAccountProfileBusiness;
        private readonly IConfiguration _configuration;
        private readonly Mock<ICustomerProfile> _customerProfile;
        private readonly Mock<IMerchandizingServices> _merchandizingServices;
        private readonly Mock<IDPService> _dpService;
        private readonly Mock<IMileagePlus> _mileagePlus;
        private readonly Mock<IShoppingSessionHelper> _shoppingSessionHelper;
        private readonly Mock<IMPTraveler> _mPTraveler;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<IDynamoDBHelperService> _dynamoDBHelperService;
        private readonly Mock<IUtility> _utility;
        private readonly Mock<ICachingService> _cachingService;
        private readonly Mock<ISQLSPService> _sQLSPService;
        private readonly Mock<IDynamoDBUtility> _dynamoDBUtility;

        public MPAccountProfileBusinessTest()
        {
            _logger = new Mock<ICacheLog<MPAccountProfileBusiness>>();
            _customerProfile = new Mock<ICustomerProfile>();
            _merchandizingServices = new Mock<IMerchandizingServices>();
            _dpService = new Mock<IDPService>();
            _mileagePlus = new Mock<IMileagePlus>();
            _shoppingSessionHelper = new Mock<IShoppingSessionHelper>();
            _mPTraveler = new Mock<IMPTraveler>();
            _headers = new Mock<IHeaders>();
            _dynamoDBHelperService = new Mock<IDynamoDBHelperService>();
            _utility = new Mock<IUtility>();
            _cachingService = new Mock<ICachingService>();
            _sQLSPService = new Mock<ISQLSPService>();
            _dynamoDBUtility = new Mock<IDynamoDBUtility>();
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
                .Build();

            _mPAccountProfileBusiness = new MPAccountProfileBusiness(_logger.Object, _configuration, _customerProfile.Object, _merchandizingServices.Object, _dpService.Object, _mileagePlus.Object, _shoppingSessionHelper.Object, _headers.Object, _cachingService.Object, _dynamoDBUtility.Object);

            SetHeaders();
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

        private string GetFileContent(string fileName)
        {
            var path = Path.IsPathRooted(fileName) ? fileName
                                                : Path.GetRelativePath(Directory.GetCurrentDirectory(), fileName);
            return File.ReadAllText(path);
        }

        [Theory]
        [MemberData(nameof(Input.InputMemberProfile2), MemberType = typeof(Input))]
        public void GetContactUsDetails_Test(MOBContactUsRequest requestPayload)
        {
            var filename = GetFileContent("MOBLegalDocument.json");
            var List = JsonConvert.DeserializeObject<List<MOBLegalDocument>>(filename);
            var callingCard = JsonConvert.DeserializeObject<List<CallingCard>>(GetFileContent("CallingCard.json"));
            //_dynamoDBService.Setup(p => p.GetRecords<List<MOBLegalDocument>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(List);
            // _dynamoDBService.Setup(p => p.GetRecords<List<CallingCard>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(callingCard);
            _sQLSPService.Setup(p => p.GetNewLegalDocumentsForTitles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(List);
            _dynamoDBUtility.Setup(p => p.GetNewLegalDocumentsForTitles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(List);


            var result = _mPAccountProfileBusiness.GetContactUsDetails(requestPayload);
            if (result?.Exception == null)
            {
                Assert.True(result.Result != null && result.Result.TransactionId != null);
            }
            else
                Assert.True(result.Exception != null && result.Exception.InnerException != null);
        }

        [Theory]
        [MemberData(nameof(Input.InputMemberProfile), MemberType = typeof(Input))]
        public void RetrieveCustomerPreferences_Test(MOBCustomerPreferencesRequest requestPayload)
        {
            var session = GetFileContent("SessionData.json");
            var resp = GetFileContent("ResponseData.json");
            var responseData = JsonConvert.DeserializeObject<MOBCustomerPreferencesResponse>(resp);
            var sessionData = JsonConvert.DeserializeObject<Session>(session);
            _shoppingSessionHelper.Setup(p => p.GetShoppingSession(It.IsAny<string>())).ReturnsAsync(sessionData);
            _customerProfile.Setup(p => p.RetrieveCustomerPreferences(It.IsAny<MOBCustomerPreferencesRequest>(), It.IsAny<string>())).ReturnsAsync(responseData);
            var result = _mPAccountProfileBusiness.RetrieveCustomerPreferences(requestPayload);
            if (result?.Exception == null)
            {
                Assert.True(result.Result != null && result.Result.TransactionId != null);
                Assert.True(result.Result.SessionId != null);
            }
            else
                Assert.True(result.Exception != null && result.Exception.InnerException != null);
        }

        [Theory]
        [MemberData(nameof(Input.InputMemberProfile1), MemberType = typeof(Input))]
        public void GetAccountSummaryWithMemberCardPremierActivity_Test(bool ok, MOBMPAccountValidationRequest requestPayload, MOBMPAccountSummaryResponse response)
        {
            var profile = JsonConvert.DeserializeObject<MOBProfile>(GetFileContent("MOBProfile.json"));
            var resp = JsonConvert.DeserializeObject<MemberPromotion<MOBStatusLiftBannerResponse>>(GetFileContent("MOBStatusLiftBannerResponse.json"));


            _merchandizingServices.Setup(p => p.GetUASubscriptions(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response.UASubscriptions);
            _dpService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            _merchandizingServices.Setup(p => p.GetUASubscriptions(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response.UASubscriptions);
            //_mileagePlus.Setup(p => p.GetAccountSummaryWithPremierActivityV2(It.IsAny<MOBMPAccountValidationRequest>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary);
            _mileagePlus.Setup(p => p.GetProfile_AllTravelerData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary.IsMPAccountTSAFlagON);
            _shoppingSessionHelper.Setup(p => p.ValidateHashPinAndGetAuthToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult((true, "[{\"hashValue\":\"\",\"mileagePlusNumber\":\"\",\"sessionId\":\"0F2D7080F7BC40018C5F112607556C77\",\"accessCode\":\"ACCESSCODE\",\"application\":{\"id\":2,\"isProduction\":false,\"name\":\"Android\",\"version\":{\"major\":\"4.1.34\",\"minor\":\"4.1.34\"}},\"deviceId\":\"735b90d3-402f-4d78-b935-ae394a2b519d\",\"languageCode\":\"en-US\",\"transactionId\":\"735b90d3-402f-4d78-b935-ae394a2b519d|709444bc-71ab-4379-9eb7-5a9fb971bbdd\"},{\"hashValue\":\"\",\"mileagePlusNumber\":\"\",\"sessionId\":\"0F2D7080F7BC40018C5F112607556C77\",\"accessCode\":\"ACCESSCODE\",\"application\":{\"id\":2,\"isProduction\":false,\"name\":\"Android\",\"version\":{\"major\":\"4.1.34\",\"minor\":\"4.1.34\"}},\"deviceId\":\"735b90d3-402f-4d78-b935-ae394a2b519d\",\"languageCode\":\"en-US\",\"transactionId\":\"735b90d3-402f-4d78-b935-ae394a2b519d|709444bc-71ab-4379-9eb7-5a9fb971bbdd\",\"validRequest\":true}]")));
            _dynamoDBUtility.Setup(p => p.IsTSAFlaggedAccount(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary.IsMPAccountTSAFlagON);

            if (response.OPAccountSummary.IsCEO)
                _configuration["NewServieCall_GetProfile_AllTravelerData"] = "false";

            var result = _mPAccountProfileBusiness.GetAccountSummaryWithMemberCardPremierActivity(requestPayload);

            if (result?.Exception == null)
            {
                Assert.True(result.Result != null && result.Result.TransactionId != null);
            }
            else
                Assert.True(result.Exception != null && result.Exception.InnerException != null);
        }

        [Theory]
        [MemberData(nameof(Input.InputMemberProfile1), MemberType = typeof(Input))]
        public void GetAccountSummaryWithMemberCardPremierActivity_SessionExpired(bool ok, MOBMPAccountValidationRequest requestPayload, MOBMPAccountSummaryResponse response)
        {
            var profile = JsonConvert.DeserializeObject<MOBProfile>(GetFileContent("MOBProfile.json"));
            var resp = JsonConvert.DeserializeObject<MemberPromotion<MOBStatusLiftBannerResponse>>(GetFileContent("MOBStatusLiftBannerResponse.json"));


            _merchandizingServices.Setup(p => p.GetUASubscriptions(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response.UASubscriptions);
            _dpService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            _merchandizingServices.Setup(p => p.GetUASubscriptions(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response.UASubscriptions);
            //_mileagePlus.Setup(p => p.GetAccountSummaryWithPremierActivityV2(It.IsAny<MOBMPAccountValidationRequest>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary);
            _mileagePlus.Setup(p => p.GetProfile_AllTravelerData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary.IsMPAccountTSAFlagON);
            _dynamoDBUtility.Setup(p => p.IsTSAFlaggedAccount(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary.IsMPAccountTSAFlagON);

            if (response.OPAccountSummary.IsCEO)
                _configuration["NewServieCall_GetProfile_AllTravelerData"] = "false";


            var result = _mPAccountProfileBusiness.GetAccountSummaryWithMemberCardPremierActivity(requestPayload);

            if (result?.Exception == null)
            {
                Assert.True(result.Result != null && result.Result.TransactionId != null);
            }
            else
                Assert.True(result.Exception != null && result.Exception.InnerException != null);
        }

        [Theory]
        [MemberData(nameof(Input.InputMemberProfile1), MemberType = typeof(Input))]
        public void GetAccountSummaryWithMemberCardPremierActivity_NewVersionAvailable(bool ok, MOBMPAccountValidationRequest requestPayload, MOBMPAccountSummaryResponse response)
        {
            var profile = JsonConvert.DeserializeObject<MOBProfile>(GetFileContent("MOBProfile.json"));
            var resp = JsonConvert.DeserializeObject<MemberPromotion<MOBStatusLiftBannerResponse>>(GetFileContent("MOBStatusLiftBannerResponse.json"));


            _merchandizingServices.Setup(p => p.GetUASubscriptions(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response.UASubscriptions);
            _dpService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            _merchandizingServices.Setup(p => p.GetUASubscriptions(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response.UASubscriptions);
            //_mileagePlus.Setup(p => p.GetAccountSummaryWithPremierActivityV2(It.IsAny<MOBMPAccountValidationRequest>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary);
            _mileagePlus.Setup(p => p.GetProfile_AllTravelerData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary.IsMPAccountTSAFlagON);
            _dynamoDBUtility.Setup(p => p.IsTSAFlaggedAccount(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response.OPAccountSummary.IsMPAccountTSAFlagON);
            _mileagePlus.Setup(p => p.IsPremierStatusTrackerSupportedVersion(It.IsAny<int>(), It.IsAny<string>())).Returns(false);

            if (response.OPAccountSummary.IsCEO)
                _configuration["NewServieCall_GetProfile_AllTravelerData"] = "false";
            if (requestPayload.TransactionId == "4ee38c95-6ca4-43d0-a7bc-877b28d2f1d6|496ea4fd-7e43-49a3-956a-da3e75a09534")
            {
                _configuration["MyAccountForceUpdateToggle"] = "true";
            }


            var result = _mPAccountProfileBusiness.GetAccountSummaryWithMemberCardPremierActivity(requestPayload);

            if (result?.Exception == null)
            {
                Assert.True(result.Result != null && result.Result.TransactionId != null);
            }
            else
                Assert.True(result.Exception != null && result.Exception.InnerException != null);
        }

    }
}
