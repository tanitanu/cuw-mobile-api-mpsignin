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
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.MPRewards;
using United.Mobile.MPRewards.Domain;
using United.Utility.Helper;
using Xunit;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace United.Mobile.Test.MPRewardTests.Api
{
    public class MPRewardBusinessTest
    {
        private readonly Mock<ICacheLog<MPRewardsBusiness>> _logger;
        private IConfiguration _configuration;
        private readonly Mock<IMPFutureFlightCredit> _mPFutureFlightCredit;
        private readonly Mock<IMileagePlus> _mileagePlus;
        private readonly Mock<IDPService> _dpService;
        //private readonly Mock<ISeatEngine> _seatEngine;
        private readonly Mock<IShoppingSessionHelper> _shoppingSessionHelper;
        private readonly MPRewardsBusiness _mPRewardsBusiness;
        private readonly Mock<ISQLSPService> _sQLSPService;
        private readonly Mock<IDynamoDBService> _dynamoDBService;
        private readonly Mock<ISessionHelperService> _sessionHelperService;
        private readonly Mock<IHeaders> _headers;

        public MPRewardBusinessTest()
        {
            _logger = new Mock<ICacheLog<MPRewardsBusiness>>();
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.test.json", optional: false, reloadOnChange: true)
                .Build();
            _dpService = new Mock<IDPService>();
            _mileagePlus = new Mock<IMileagePlus>();
            _mPFutureFlightCredit = new Mock<IMPFutureFlightCredit>();
            //_seatEngine = new Mock<IsaacEngine>();
            _shoppingSessionHelper = new Mock<IShoppingSessionHelper>();
            _sQLSPService = new Mock<ISQLSPService>();
            _dynamoDBService = new Mock<IDynamoDBService>();
            _sessionHelperService = new Mock<ISessionHelperService>();
            _headers = new Mock<IHeaders>();
            _mPRewardsBusiness = new MPRewardsBusiness(_logger.Object, _configuration, _mileagePlus.Object, _sessionHelperService.Object, _shoppingSessionHelper.Object);
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
        [MemberData(nameof(MPRewardInput.InputMPRewards), MemberType = typeof(MPRewardInput))]
        public void GetAccountPlusPointsDetails_Test(string input)
        {
            var requestPayload = JsonConvert.DeserializeObject<MOBMPPlusPointsRequest>(input);
            var inputPayload = JsonConvert.DeserializeObject<MPSignIn>(input);
            var resp = GetFileContent("GetAccountPlusPointsDetailsResponse1.json");
            var responseData = JsonConvert.DeserializeObject<MOBPlusPoints>(resp);

            var mpsignin = GetFileContent("MPSignIn.json");
            var mpsigninData = JsonConvert.DeserializeObject<MOBPlusPoints>(mpsignin);

            _mileagePlus.Setup(p => p.GetPlusPointsFromLoyaltyBalanceService(It.IsAny<MOBMPAccountValidationRequest>(), It.IsAny<string>())).ReturnsAsync(responseData);

            //_sessionHelperService.Setup(p => p.GetSession<MPSignIn>(It.IsAny<HttpContextValues>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(input);

            _sessionHelperService.Setup(p => p.GetSession<MPSignIn>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).Returns(Task.FromResult(inputPayload));

            var result = _mPRewardsBusiness.GetAccountPlusPointsDetails(requestPayload);
            if (result?.Exception == null)
            {
                Assert.True(result.Result != null && result.Result.TransactionId != null);
            }
            else
                Assert.True(result.Exception != null && result.Exception.InnerException != null);
        }


        [Theory]
        [MemberData(nameof(MPRewardInput.InputMPRewards1), MemberType = typeof(MPRewardInput))]
        public void GetCancelFFCPnrsByMPNumber_Test(string input, bool input1)
        {
            var requestPayload = JsonConvert.DeserializeObject<MOBCancelFFCPNRsByMPNumberRequest>(input);
            var resp = GetFileContent("GetCancelFFCPnrsByMPNumberResponse.json");
            var responseData = JsonConvert.DeserializeObject<getResrvationResponse>(resp);

            _mPFutureFlightCredit.Setup(p => p.GetMPFutureFlightCredit<getResrvationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(responseData);


            _shoppingSessionHelper.Setup(p => p.ValidateHashPinAndGetAuthToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).
                Returns(Task.FromResult((true, "[{\"hashValue\":\"\",\"mileagePlusNumber\":\"\",\"sessionId\":\"0F2D7080F7BC40018C5F112607556C77\",\"accessCode\":\"ACCESSCODE\",\"application\":{\"id\":2,\"isProduction\":false,\"name\":\"Android\",\"version\":{\"major\":\"4.1.34\",\"minor\":\"4.1.34\"}},\"deviceId\":\"735b90d3-402f-4d78-b935-ae394a2b519d\",\"languageCode\":\"en-US\",\"transactionId\":\"735b90d3-402f-4d78-b935-ae394a2b519d|709444bc-71ab-4379-9eb7-5a9fb971bbdd\"},{\"hashValue\":\"\",\"mileagePlusNumber\":\"\",\"sessionId\":\"0F2D7080F7BC40018C5F112607556C77\",\"accessCode\":\"ACCESSCODE\",\"application\":{\"id\":2,\"isProduction\":false,\"name\":\"Android\",\"version\":{\"major\":\"4.1.34\",\"minor\":\"4.1.34\"}},\"deviceId\":\"735b90d3-402f-4d78-b935-ae394a2b519d\",\"languageCode\":\"en-US\",\"transactionId\":\"735b90d3-402f-4d78-b935-ae394a2b519d|709444bc-71ab-4379-9eb7-5a9fb971bbdd\",\"validRequest\":true}]")));

            var result = _mPRewardsBusiness.GetCancelFFCPnrsByMPNumber(requestPayload);
            if (result?.Exception == null)
            {
                Assert.True(result.Result != null && result.Result.TransactionId != null);
            }
            else
                Assert.True(result.Exception != null && result.Exception.InnerException != null);
        }


        [Theory]
        [MemberData(nameof(MPRewardInput.InputMPRewards11), MemberType = typeof(MPRewardInput))]
        public void GetCancelFFCPnrsByMPNumber_UnitedDataServicesAreUnavailable(string input, bool input1)
        {
            var requestPayload = JsonConvert.DeserializeObject<MOBCancelFFCPNRsByMPNumberRequest>(input);
            var resp = GetFileContent("GetCancelFFCPnrsByMPNumberResponse1.json");
            var responseData = JsonConvert.DeserializeObject<getResrvationResponse>(resp);

            _mPFutureFlightCredit.Setup(p => p.GetMPFutureFlightCredit<getResrvationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(responseData);


            var result = _mPRewardsBusiness.GetCancelFFCPnrsByMPNumber(requestPayload);
            if (result?.Exception == null)
            {
                Assert.True(result.Result != null && result.Result.TransactionId != null);
            }
            else
                Assert.True(result.Exception != null && result.Exception.InnerException != null);
        }

        //[Theory]
        //[MemberData(nameof(MPRewardInput.InputMPRewards2), MemberType = typeof(MPRewardInput))]
        //public void GetActionDetailsForOffers_Test(string input)
        //{
        //    var requestPayload = JsonConvert.DeserializeObject<MOBGetActionDetailsForOffersRequest>(input);
        //    var session = GetFileContent("GetActionDetailsForOffersResponse.json");
        //    var sessionData = JsonConvert.DeserializeObject<Session>(session);

        //    var manageres = GetFileContent("Manageresrequest.json");
        //    var manageresData = JsonConvert.DeserializeObject<MOBPNRByRecordLocatorResponse>(manageres);

        //    var seatmap = GetFileContent("SeatMapRequest.json");
        //    var seatmapData = JsonConvert.DeserializeObject<MOBSeatChangeInitializeResponse>(seatmap);

        //    _shoppingSessionHelper.Setup(p => p.CreateShoppingSession(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(sessionData);

        //    //_manageReservation.Setup(p=>p.GetPNRByRecordLocatorCommonMethod(It.IsAny<MOBPNRByRecordLocatorRequest>())).ReturnsAsync(manageresData);

        //    //_seatEngine.Setup(p => p.SeatChangeInitialize(It.IsAny<MOBSeatChangeInitializeRequest>())).ReturnsAsync(seatmapData);

        //    var result = _mPRewardsBusiness.GetActionDetailsForOffers(requestPayload);
        //    if (result?.Exception == null)
        //    {
        //        Assert.True(result.Result != null && result.Result.TransactionId != null);
        //    }
        //    else
        //        Assert.True(result.Exception != null && result.Exception.InnerException != null);
        //}
    }

}
