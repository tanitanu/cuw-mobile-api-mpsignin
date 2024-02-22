using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Exception;
using United.Mobile.Model.MPSignIn;
using United.Mobile.MPAccountProfile.Api.Controllers;
using United.Mobile.MPAccountProfile.Domain;
using United.Utility.Helper;
using Xunit;
using Microsoft.Extensions.Logging;
using United.Ebs.Logging.Enrichers;
using United.Service.Presentation.ReferenceDataModel;

namespace United.Mobile.Test.MPAccountProfileTests.Api
{
    public class MPAccountProfileControllerTest
    {
        private readonly Mock<ICacheLog<MPAccountProfileController>> _logger;
        private readonly MPAccountProfileController _mPAccountProfileController;
        private readonly IConfiguration _configuration;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<IMPAccountProfileBusiness> _mPAccountProfileBusiness;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IRequestEnricher> _requestEnricher;
        private readonly Mock<IFeatureSettings> _featureSettings;

        public MPAccountProfileControllerTest()
        {
            _logger = new Mock<ICacheLog<MPAccountProfileController>>();
            _headers = new Mock<IHeaders>();
            _mPAccountProfileBusiness = new Mock<IMPAccountProfileBusiness>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _requestEnricher = new Mock<IRequestEnricher>();
            _featureSettings = new Mock<IFeatureSettings>();
            _configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
           .Build();

            _mPAccountProfileController = new MPAccountProfileController(_logger.Object, _configuration, _headers.Object, _mPAccountProfileBusiness.Object, _requestEnricher.Object, _featureSettings.Object);

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
            return _mPAccountProfileController.HealthCheck();
        }

        [Fact]
        public void GetContactUsDetails_Test()
        {
            var response = new MOBContactUsResponse();
            var request = new MOBContactUsRequest()
            {
                TransactionId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685|a43aa991-9d57-40f0-9b83-79f34d880e7c",
                DeviceId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685",
                Application = new MOBApplication() { Id = 1, Version = new MOBVersion() { Major = "4.1.48" } }
            };

            _mPAccountProfileController.ControllerContext = new ControllerContext();
            _mPAccountProfileController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPAccountProfileController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mPAccountProfileBusiness.Setup(p => p.GetContactUsDetails(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mPAccountProfileController.GetContactUsDetails(request);
            Assert.False(result.Result.TransactionId == "EE64E779-7B46-4836-B261-62AE35498B44");
            Assert.True(result.Result.Exception == null);
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void GetContactUsDetails_SystemException_Test()
        {
            var response = new MOBContactUsResponse();
            var request = new MOBContactUsRequest()
            {
                TransactionId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685|a43aa991-9d57-40f0-9b83-79f34d880e7c",
                DeviceId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685",
                Application = new MOBApplication() { Id = 1, Version = new MOBVersion() { Major = "4.1.48" } }
            };

            _mPAccountProfileController.ControllerContext = new ControllerContext();
            _mPAccountProfileController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPAccountProfileController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mPAccountProfileBusiness.Setup(p => p.GetContactUsDetails(request)).ThrowsAsync(new Exception("Error Message"));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mPAccountProfileController.GetContactUsDetails(request);
            Assert.True(result.Result.Exception.Code == "9999");
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void GetContactUsDetails_MOBUnitedException_Test()
        {
            var response = new MOBContactUsResponse();
            var request = new MOBContactUsRequest()
            {
                TransactionId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685|a43aa991-9d57-40f0-9b83-79f34d880e7c",
                DeviceId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685",
                Application = new MOBApplication() { Id = 1, Version = new MOBVersion() { Major = "4.1.48" } }
            };

            _mPAccountProfileController.ControllerContext = new ControllerContext();
            _mPAccountProfileController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPAccountProfileController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mPAccountProfileBusiness.Setup(p => p.GetContactUsDetails(request)).ThrowsAsync(new MOBUnitedException("Error Message"));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mPAccountProfileController.GetContactUsDetails(request);
            Assert.True(result.Result.Exception.Message == "Error Message");
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void RetrieveCustomerPreferences_Test()
        {
            var response = new MOBCustomerPreferencesResponse();
            var request = new MOBCustomerPreferencesRequest()
            {
                SessionId = "67945321097C4CF58FFC7DF9565CB276",
                TransactionId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685|a43aa991-9d57-40f0-9b83-79f34d880e7c",
                DeviceId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685",
                Application = new MOBApplication() { Id = 1, Version = new MOBVersion() { Major = "4.1.48" } }
            };

            _mPAccountProfileController.ControllerContext = new ControllerContext();
            _mPAccountProfileController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPAccountProfileController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mPAccountProfileBusiness.Setup(p => p.RetrieveCustomerPreferences(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mPAccountProfileController.RetrieveCustomerPreferences(request);
            Assert.False(result.Result.TransactionId == "EE64E779-7B46-4836-B261-62AE35498B44");
            Assert.True(result.Result.Exception == null);
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void RetrieveCustomerPreferences_SystemException_Test()
        {
            var response = new MOBCustomerPreferencesResponse();
            var request = new MOBCustomerPreferencesRequest()
            {
                SessionId = "67945321097C4CF58FFC7DF9565CB276",
                TransactionId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685|a43aa991-9d57-40f0-9b83-79f34d880e7c",
                DeviceId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685",
                Application = new MOBApplication() { Id = 1, Version = new MOBVersion() { Major = "4.1.48" } }
            };

            _mPAccountProfileController.ControllerContext = new ControllerContext();
            _mPAccountProfileController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPAccountProfileController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mPAccountProfileBusiness.Setup(p => p.RetrieveCustomerPreferences(request)).ThrowsAsync(new Exception("Error Message"));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mPAccountProfileController.RetrieveCustomerPreferences(request);
            Assert.False(result.Result.Exception.Code == "9999");
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void RetrieveCustomerPreferences_MOBUnitedException_Test()
        {
            var response = new MOBCustomerPreferencesResponse();
            var request = new MOBCustomerPreferencesRequest()
            {
                SessionId = "67945321097C4CF58FFC7DF9565CB276",
                TransactionId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685|a43aa991-9d57-40f0-9b83-79f34d880e7c",
                DeviceId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685",
                Application = new MOBApplication() { Id = 1, Version = new MOBVersion() { Major = "4.1.48" } }
            };

            _mPAccountProfileController.ControllerContext = new ControllerContext();
            _mPAccountProfileController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPAccountProfileController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mPAccountProfileBusiness.Setup(p => p.RetrieveCustomerPreferences(request)).ThrowsAsync(new MOBUnitedException("Error Message"));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mPAccountProfileController.RetrieveCustomerPreferences(request);
            Assert.True(result.Result.Exception.Message == "Error Message");
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void GetAccountSummaryWithMemberCardPremierActivity_Test()
        {
            var response = new MOBMPAccountSummaryResponse();
            var request = new MOBMPAccountValidationRequest()
            {
                SessionId = "67945321097C4CF58FFC7DF9565CB276",
                TransactionId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685|a43aa991-9d57-40f0-9b83-79f34d880e7c",
                DeviceId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685",
                Application = new MOBApplication() { Id = 1, Version = new MOBVersion() { Major = "4.1.48" } }
            };

            _mPAccountProfileController.ControllerContext = new ControllerContext();
            _mPAccountProfileController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPAccountProfileController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mPAccountProfileBusiness.Setup(p => p.GetAccountSummaryWithMemberCardPremierActivity(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mPAccountProfileController.GetAccountSummaryWithMemberCardPremierActivity(request);
            //Assert.True(result.Result.TransactionId == "67945321097C4CF58FFC7DF9565CB276");
            Assert.True(result.Result.Exception == null);
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void GetAccountSummaryWithMemberCardPremierActivity_SystemException_Test()
        {
            var response = new MOBMPAccountSummaryResponse();
            var request = new MOBMPAccountValidationRequest()
            {
                SessionId = "67945321097C4CF58FFC7DF9565CB276",
                TransactionId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685|a43aa991-9d57-40f0-9b83-79f34d880e7c",
                DeviceId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685",
                Application = new MOBApplication() { Id = 1, Version = new MOBVersion() { Major = "4.1.48" } }
            };

            _mPAccountProfileController.ControllerContext = new ControllerContext();
            _mPAccountProfileController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPAccountProfileController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mPAccountProfileBusiness.Setup(p => p.GetAccountSummaryWithMemberCardPremierActivity(request)).ThrowsAsync(new Exception("Error Message"));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mPAccountProfileController.GetAccountSummaryWithMemberCardPremierActivity(request);
            Assert.False(result.Result.Exception.Code == "9999");
            //Assert.true(result != null);
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void GetAccountSummaryWithMemberCardPremierActivity_MOBUnitedException_Test()
        {
            var response = new MOBMPAccountSummaryResponse();
            var request = new MOBMPAccountValidationRequest()
            {
                SessionId = "67945321097C4CF58FFC7DF9565CB276",
                TransactionId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685|a43aa991-9d57-40f0-9b83-79f34d880e7c",
                DeviceId = "446e943d-d749-4f2d-8bf2-e0ba5d6da685",
                Application = new MOBApplication() { Id = 1, Version = new MOBVersion() { Major = "4.1.48" } }
            };

            _mPAccountProfileController.ControllerContext = new ControllerContext();
            _mPAccountProfileController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPAccountProfileController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mPAccountProfileBusiness.Setup(p => p.GetAccountSummaryWithMemberCardPremierActivity(request)).ThrowsAsync(new MOBUnitedException("Error Message"));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mPAccountProfileController.GetAccountSummaryWithMemberCardPremierActivity(request);
            Assert.True(result.Result.Exception.Message == "Error Message");
            Assert.True(result.Result.CallDuration > 0);
        }
    }
}
