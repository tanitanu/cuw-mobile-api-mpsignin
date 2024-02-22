using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using United.Common.Helper;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.OnPremiseSQLSP;
using United.Mobile.DeviceInitialization.Domain;
using United.Mobile.Model;
using United.Mobile.Model.DeviceInitialization;
using United.Utility.Helper;
using Xunit;


namespace United.Mobile.Test.DeviceInitilizationTests.Api
{
    public class DeviceInitilizationBusinessTest
    {

        private readonly Mock<ICacheLog<DeviceInitializationBusiness>> _logger;
        private readonly IConfiguration _configuration;
        private readonly Mock<IDynamoDBHelperService> _dynamoDBHelperService;
        private readonly DeviceInitializationBusiness _deviceInitializationBusiness;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<ISessionHelperService> _sessionHelperService;
        private readonly Mock<IDynamoDBUtility> _dynamoDBUtility;

        public DeviceInitilizationBusinessTest()
        {
            _logger = new Mock<ICacheLog<DeviceInitializationBusiness>>();
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.test.json", optional: false, reloadOnChange: true)
                .Build();

            _sessionHelperService = new Mock<ISessionHelperService>();
            _dynamoDBHelperService = new Mock<IDynamoDBHelperService>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _headers = new Mock<IHeaders>();
            _dynamoDBUtility = new Mock<IDynamoDBUtility>();

            _deviceInitializationBusiness = new DeviceInitializationBusiness(_logger.Object, _dynamoDBUtility.Object);
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

        [Theory]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "5E98C6B880B84E89A8DBFC3C4897B4C5", "17C979E184CC495EA083D45F4DD9D19D", 1)]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "5E98C6B880B84E89A8DBFC3C4897B4C56", "17C979E184CC495EA083D45F4DD9D19DE", 1)]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "5E98C6B880B84E89A8DBFC3C4897B4C57", "17C979E184CC495EA083D45F4DD9D19DF", 1)]
        public void InsertPushTokenToDB_Test(string accessCode, string transactionId, string deviceId, string apnsDeviceId, int applicationId)
        {

            _dynamoDBUtility.Setup(p => p.InsertDevicePushToken(It.IsAny<DeviceData>(), It.IsAny<string>())).ReturnsAsync(true);

            var result = _deviceInitializationBusiness.InsertPushTokenToDB(accessCode, transactionId, deviceId, apnsDeviceId, applicationId);

            if (result.Exception == null)
                Assert.True(result.Result != null);
            else
                Assert.True(result.Exception.InnerExceptions != null && result.Exception.Message != null);
        }

        [Theory]
        [InlineData("A1234", "EE64E779-7B46-4836-B261-62AE35498B44", "5E98C6B880B84E89A8DBFC3C4897B4C5", "17C979E184CC495EA083D45F4DD9D19D", 1)]
        public void InsertPushTokenToDB_1(string accessCode, string transactionId, string deviceId, string apnsDeviceId, int applicationId)
        {

            _dynamoDBUtility.Setup(p => p.InsertDevicePushToken(It.IsAny<DeviceData>(), It.IsAny<string>()));

            var result = _deviceInitializationBusiness.InsertPushTokenToDB(accessCode, transactionId, deviceId, apnsDeviceId, applicationId);

            if (result.Exception == null)
                Assert.True(result?.Result != null);
            else
                Assert.True(result.Exception.InnerExceptions != null && result.Exception.Message != null);
        }

        [Theory]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "Test", "Abcd", "HP", "HP123", "T14", "2.1.4", "1", "2.1.5")]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "Test", "Abcd", "HP", "HP123", "T14", "2.1.5", "1", "2.1.2")]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "Test", "Abcd", "HP", "HP123", "T14", "2.1.6", "1", "2.1.8")]

        public void RegisterDevice_Test(string accessCode, string transactionId, string identifierForVendor, string name, string model, string localizedModel, string systemName, string systemVersion, string applicationId, string applicationVersion)
        {
            DeviceResponse deviceResponse = new DeviceResponse
            {
                DeviceID = 1234
            };


            _dynamoDBUtility.Setup(p => p.InsertDevicePushToken(It.IsAny<DeviceData>(),It.IsAny<string>()));
            _dynamoDBUtility.Setup(p => p.RegisterDevice(It.IsAny<DeviceRequest>())).ReturnsAsync(deviceResponse.DeviceID);

            var result = _deviceInitializationBusiness.RegisterDevice(accessCode, transactionId, identifierForVendor, name, model, localizedModel, systemName, systemVersion, applicationId, applicationVersion);

            if (result.Exception == null)
                Assert.True(result.Result != null);
            else
                Assert.True(result.Exception.InnerExceptions != null && result.Exception.Message != null);
        }
    }
}
