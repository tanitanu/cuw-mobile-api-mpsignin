using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using United.Common.Helper.Profile;
using United.Common.Helper;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.CSLSerivce;
using United.Mobile.DataAccess.MPSignIn;
using United.Mobile.DataAccess.OnPremiseSQLSP;
using United.Mobile.MemberSignIn.Domain;
using United.Mobile.Model;
using United.Utility.Helper;
using Newtonsoft.Json;
using United.Mobile.Model.Common;
using United.Service.Presentation.CustomerModel;
using United.Service.Presentation.SecurityResponseModel;
using United.Services.ProfileValidation.Common;
using Xunit;
using United.Mobile.Model.MPSignIn.MPNumberToPNR;
using United.Mobile.Model.Internal.Common;
using United.Service.Presentation.ReservationResponseModel;

namespace United.Mobile.Test.MemberSignInTests.Api
{
    public class MPNumberToPnrBussinessTests
    {
        private readonly Mock<ICacheLog<MPNumberToPnrBussiness>> _logger;
        private readonly IConfiguration _configuration;
        private readonly IConfiguration _configuration1;
        private readonly Mock<ICustomerProfileService> _customerProfileService;
        private readonly Mock<ISessionHelperService> _sessionHelperService;
        private readonly Mock<IUCBProfileService> _ucbProfileService;
        private readonly Mock<IMemberProfileService> _memberProfileService;
        private readonly Mock<IPNRRetrievalService> _pNRRetrievalService;
        private readonly Mock<IDPService> _tokenService;
        private readonly MPNumberToPnrBussiness _MPNumberToPnrBussiness;
        private readonly Mock<IHeaders> _headers;

        public MPNumberToPnrBussinessTests()
        {
            _logger = new Mock<ICacheLog<MPNumberToPnrBussiness>>();
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
            _customerProfileService = new Mock<ICustomerProfileService>();
            _ucbProfileService = new Mock<IUCBProfileService>();
            _memberProfileService = new Mock<IMemberProfileService>();
            _pNRRetrievalService = new Mock<IPNRRetrievalService>();
            _tokenService = new Mock<IDPService>();
            _MPNumberToPnrBussiness = new MPNumberToPnrBussiness(_logger.Object, _configuration, _customerProfileService.Object, _sessionHelperService.Object, _ucbProfileService.Object, _memberProfileService.Object, _pNRRetrievalService.Object, _tokenService.Object);
            SetHeaders();
        }
        private void SetHeaders(string deviceId = "D3F8FB8D-D563-449F-B4CF-4BF0AD8D6DB3"
              , string applicationId = "2"
              , string appVersion = "4.1.98"
              , string transactionId = "A4E94A08-96F0-4D56-98D9-D05DB37C8FE1|7AB2F4A8-2583-4D95-8F6C-511032964877"
              , string languageCode = "en-US"
              , string sessionId = "ED87FEFE2BF649CEB4C0BA559B9EB5F3")
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
        [MemberData(nameof(TestDataGenerator.InputAddMpToPNR1), MemberType = typeof(TestDataGenerator))]
        public void AddMpToPnrEligibilityCheck_Test(MPSearchRequest requestPayload)
        {
            var reservationDetail = JsonConvert.DeserializeObject<ReservationDetail>(GetFileContent("AddMPToPNRReservationDetail.json"));
            var searchMemberInfoResponse = JsonConvert.DeserializeObject<SearchMemberInfoResponse>(GetFileContent("SearchMemberInfoResponse.json"));
             var emailAddressResponse = GetFileContent("EmailAddressResponse.json");
            _sessionHelperService.Setup(p => p.GetSession<ReservationDetail>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservationDetail);
            _tokenService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            _memberProfileService.Setup(p => p.SearchMemberInfo<SearchMemberInfoResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(searchMemberInfoResponse);
            _ucbProfileService.Setup(p => p.GetEmail(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emailAddressResponse);

            if (requestPayload.Application.IsProduction)
            {
                _configuration["addMpNumbersToPnrEnabled"] = "false";
            }
            var result = _MPNumberToPnrBussiness.AddMpToPnrEligibilityCheck(requestPayload);

            Assert.True(result.Result != null || result.Result.Exception != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.InputAddMpToPNR1), MemberType = typeof(TestDataGenerator))]
        public void SearchMPNumber_Test(MPSearchRequest requestPayload)
        {
            var reservationDetail = JsonConvert.DeserializeObject<ReservationDetail>(GetFileContent("AddMPToPNRReservationDetail.json"));
            var searchMemberInfoResponse = JsonConvert.DeserializeObject<SearchMemberInfoResponse>(GetFileContent("SearchMemberInfoResponse.json"));
            var emailAddressResponse = GetFileContent("EmailAddressResponse.json");
            _sessionHelperService.Setup(p => p.GetSession<ReservationDetail>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservationDetail);
            _tokenService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            _memberProfileService.Setup(p => p.SearchMemberInfo<SearchMemberInfoResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(searchMemberInfoResponse);
            _ucbProfileService.Setup(p => p.GetEmail(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(emailAddressResponse);

            if (requestPayload.Application.IsProduction)
            {
                _configuration["addMpNumbersToPnrEnabled"] = "false";
            }
            var result = _MPNumberToPnrBussiness.SearchMPNumber(requestPayload);

            Assert.True(result.Result != null || result.Result.Exception != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.InputAddMpToPNR2), MemberType = typeof(TestDataGenerator))]
        public void AddMPNumberToPnr_Test(AddMPNumberToPnrRequest requestPayload)
        {
            var reservationDetail = JsonConvert.DeserializeObject<ReservationDetail>(GetFileContent("AddMPToPNRReservationDetail.json"));
            var addMPNumberToPnrInfoResponse = GetFileContent("AddMPNumberToPnrResponse.json");
            _sessionHelperService.Setup(p => p.GetSession<ReservationDetail>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservationDetail);
            _tokenService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Sample Token");
            _pNRRetrievalService.Setup(p => p.UpdateTravelerInfo(It.IsAny<string>(),It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(addMPNumberToPnrInfoResponse);
            _sessionHelperService.Setup(p => p.SaveSession<bool>(It.IsAny<bool>(), It.IsAny<string>(),It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(true);

            if (requestPayload.Application.IsProduction)
            {
                _configuration["addMpNumbersToPnrEnabled"] = "false";
            }
            var result = _MPNumberToPnrBussiness.AddMPNumberToPnr(requestPayload);

            Assert.True(result.Result != null || result.Result.Exception != null);
        }

    }

}
