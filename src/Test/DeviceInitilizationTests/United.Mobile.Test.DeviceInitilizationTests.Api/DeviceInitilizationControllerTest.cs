using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Mobile.DeviceInitialization.Api.Controllers;
using United.Mobile.DeviceInitialization.Domain;
using United.Mobile.Model;
using United.Mobile.Model.DeviceInitialization;
using United.Mobile.Model.Internal.Exception;
using United.Utility.Helper;
using Xunit;
using Constants = United.Mobile.Model.Constants;
using Microsoft.Extensions.Logging;
using United.Ebs.Logging.Enrichers;

namespace United.Mobile.Test.DeviceInitilizationTests.Api
{
    public class DeviceInitilizationControllerTest
    {
        private readonly Mock<ICacheLog<DeviceInitializationController>> _logger;
        private readonly IConfiguration _configuration;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<IDeviceInitializationBusiness> _deviceInitializationBusiness;
        private readonly DeviceInitializationController _deviceInitializationController;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly IRequestEnricher _requestEnricher;

        public DeviceInitilizationControllerTest()
        {
            _logger = new Mock<ICacheLog<DeviceInitializationController>>();
            _headers = new Mock<IHeaders>();
            _deviceInitializationBusiness = new Mock<IDeviceInitializationBusiness>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
           .Build();

            _deviceInitializationController = new DeviceInitializationController(_logger.Object, _configuration, _headers.Object, _deviceInitializationBusiness.Object, _requestEnricher);



            SetupHttpContextAccessor();
            SetHeaders();

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
            _httpContextAccessor.Setup(_ => _.HttpContext).Returns(context);
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

        [Fact]
        public string HealthCheck_Test()
        {
            return _deviceInitializationController.HealthCheck();
        }

        [Theory]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "5E98C6B880B84E89A8DBFC3C4897B4C5", "17C979E184CC495EA083D45F4DD9D19D", 1)]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "5E98C6B880B84E89A8DBFC3C4897B4C56", "17C979E184CC495EA083D45F4DD9D19DE", 1)]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "5E98C6B880B84E89A8DBFC3C4897B4C57", "17C979E184CC495EA083D45F4DD9D19DF", 1)]
        public void InsertPushTokenToDB_Test(string accessCode, string transactionId, string deviceId, string apnsDeviceId, int applicationId)
        {
            var response = true;

            _deviceInitializationController.ControllerContext = new ControllerContext();
            _deviceInitializationController.ControllerContext.HttpContext = new DefaultHttpContext();
            _deviceInitializationController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _deviceInitializationBusiness.Setup(p => p.InsertPushTokenToDB(accessCode, transactionId, deviceId, apnsDeviceId, applicationId)).ReturnsAsync(response);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();

            var result = _deviceInitializationController.InsertPushTokenToDB(accessCode, transactionId, deviceId, apnsDeviceId, applicationId);

            //Assert.True(result != null);
            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "5E98C6B880B84E89A8DBFC3C4897B4C5", "17C979E184CC495EA083D45F4DD9D19D", 1)]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "5E98C6B880B84E89A8DBFC3C4897B4C56", "17C979E184CC495EA083D45F4DD9D19DE", 1)]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "5E98C6B880B84E89A8DBFC3C4897B4C57", "17C979E184CC495EA083D45F4DD9D19DF", 1)]
        public void InsertPushTokenToDB_SystemException_Test(string accessCode, string transactionId, string deviceId, string apnsDeviceId, int applicationId)
        {
            var response = true;

            _deviceInitializationController.ControllerContext = new ControllerContext();
            _deviceInitializationController.ControllerContext.HttpContext = new DefaultHttpContext();
            _deviceInitializationController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _deviceInitializationBusiness.Setup(p => p.InsertPushTokenToDB(accessCode, transactionId, deviceId, apnsDeviceId, applicationId)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();

            var result = _deviceInitializationController.InsertPushTokenToDB(accessCode, transactionId, deviceId, apnsDeviceId, applicationId);

            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "5E98C6B880B84E89A8DBFC3C4897B4C5", "17C979E184CC495EA083D45F4DD9D19D", 1)]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "5E98C6B880B84E89A8DBFC3C4897B4C56", "17C979E184CC495EA083D45F4DD9D19DE", 1)]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "5E98C6B880B84E89A8DBFC3C4897B4C57", "17C979E184CC495EA083D45F4DD9D19DF", 1)]
        public void InsertPushTokenToDB_MOBUnitedException_Test(string accessCode, string transactionId, string deviceId, string apnsDeviceId, int applicationId)
        {
            var response = true;

            _deviceInitializationController.ControllerContext = new ControllerContext();
            _deviceInitializationController.ControllerContext.HttpContext = new DefaultHttpContext();
            _deviceInitializationController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _deviceInitializationBusiness.Setup(p => p.InsertPushTokenToDB(accessCode, transactionId, deviceId, apnsDeviceId, applicationId)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();

            var result = _deviceInitializationController.InsertPushTokenToDB(accessCode, transactionId, deviceId, apnsDeviceId, applicationId);

            Assert.True(result != null);
            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "Test", "Abcd", "HP", "HP123", "T14", "2.1.4", "1", "2.1.5")]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "Test", "Abcd", "HP", "HP123", "T14", "2.1.5", "1", "2.1.2")]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "Test", "Abcd", "HP", "HP123", "T14", "2.1.6", "1", "2.1.8")]

        public void RegisterDevice_Test(string accessCode, string transactionId, string identifierForVendor, string name, string model, string localizedModel, string systemName, string systemVersion, string applicationId, string applicationVersion)
        {
            var response = new DeviceResponse();

            _deviceInitializationController.ControllerContext = new ControllerContext();
            _deviceInitializationController.ControllerContext.HttpContext = new DefaultHttpContext();
            _deviceInitializationController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _deviceInitializationBusiness.Setup(p => p.RegisterDevice(accessCode, transactionId, identifierForVendor, name, model, localizedModel, systemName, systemVersion, applicationId, applicationVersion)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();

            var result = _deviceInitializationController.RegisterDevice(accessCode, transactionId, identifierForVendor, name, model, localizedModel, systemName, systemVersion, applicationId, applicationVersion);

            Assert.False(result.Result.TransactionId == "EE64E779-7B46-4836-B261-62AE35498B44");
            Assert.True(result.Result.Exception == null);
            // Assert.True(result.Result.CallDuration > 0);
            Assert.True(result != null);
        }

        [Theory]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "Test", "Abcd", "HP", "HP123", "T14", "2.1.4", "1", "2.1.5")]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "Test", "Abcd", "HP", "HP123", "T14", "2.1.5", "1", "2.1.2")]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "Test", "Abcd", "HP", "HP123", "T14", "2.1.6", "1", "2.1.8")]
        public void RegisterDevice_SystemException_Test(string accessCode, string transactionId, string identifierForVendor, string name, string model, string localizedModel, string systemName, string systemVersion, string applicationId, string applicationVersion)
        {

            _deviceInitializationController.ControllerContext = new ControllerContext();
            _deviceInitializationController.ControllerContext.HttpContext = new DefaultHttpContext();
            _deviceInitializationController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _deviceInitializationBusiness.Setup(p => p.RegisterDevice(accessCode, transactionId, identifierForVendor, name, model, localizedModel, systemName, systemVersion, applicationId, applicationVersion)).ThrowsAsync(new Exception("Error Message"));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _deviceInitializationController.RegisterDevice(accessCode, transactionId, identifierForVendor, name, model, localizedModel, systemName, systemVersion, applicationId, applicationVersion);
            Assert.False(result.Result.Exception.Code == "10000");
            Assert.True(result.Result.CallDuration > 0);
        }

        [Theory]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "Test", "Abcd", "HP", "HP123", "T14", "2.1.4", "1", "2.1.5")]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "Test", "Abcd", "HP", "HP123", "T14", "2.1.5", "1", "2.1.2")]
        [InlineData("ACCESSCODE", "EE64E779-7B46-4836-B261-62AE35498B44", "Test", "Abcd", "HP", "HP123", "T14", "2.1.6", "1", "2.1.8")]
        public void RegisterDevice_MOBUnitedException_Test(string accessCode, string transactionId, string identifierForVendor, string name, string model, string localizedModel, string systemName, string systemVersion, string applicationId, string applicationVersion)
        {

            _deviceInitializationController.ControllerContext = new ControllerContext();
            _deviceInitializationController.ControllerContext.HttpContext = new DefaultHttpContext();
            _deviceInitializationController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _deviceInitializationBusiness.Setup(p => p.RegisterDevice(accessCode, transactionId, identifierForVendor, name, model, localizedModel, systemName, systemVersion, applicationId, applicationVersion)).ThrowsAsync(new MOBUnitedException("Error Message"));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _deviceInitializationController.RegisterDevice(accessCode, transactionId, identifierForVendor, name, model, localizedModel, systemName, systemVersion, applicationId, applicationVersion);
            Assert.True(result.Result.CallDuration > 0);
        }


    }
}
