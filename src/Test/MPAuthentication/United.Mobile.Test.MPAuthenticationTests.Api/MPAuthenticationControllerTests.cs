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
using United.Mobile.MPAuthentication.Api.Controllers;
using United.Mobile.MPAuthentication.Domain;
using United.Utility.Helper;
using Xunit;
using Constants = United.Mobile.Model.Constants;
using Microsoft.Extensions.Logging;
using United.Ebs.Logging.Enrichers;
using United.Service.Presentation.ReferenceDataModel;

namespace United.Mobile.Test.MPAuthenticationTests.Api
{
    public class MPAuthenticationControllerTests
    {
        private readonly Mock<ICacheLog<MPAuthenticationController>> _logger;
        private readonly Mock<IMPAuthenticationBusiness> _mPAuthenticationBusiness;
        private readonly MPAuthenticationController _mPAuthenticationController;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly Mock<IHeaders> _moqHeader;
        private readonly Mock<IRequestEnricher> _requestEnricher;
        private readonly Mock<IFeatureSettings> _featureSettings;

        public MPAuthenticationControllerTests()
        {
            _moqHeader = new Mock<IHeaders>();
            _logger = new Mock<ICacheLog<MPAuthenticationController>>();
            _mPAuthenticationBusiness = new Mock<IMPAuthenticationBusiness>();
            _requestEnricher = new Mock<IRequestEnricher>();
            _featureSettings = new Mock<IFeatureSettings>();
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appSettings.test.json", optional: false, reloadOnChange: true)
            .Build();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _mPAuthenticationController = new MPAuthenticationController(_logger.Object, _configuration, _mPAuthenticationBusiness.Object, _moqHeader.Object, _requestEnricher.Object, _featureSettings.Object);
            SetupHttpContextAccessor();
            SetHeaders();
        }

        [Fact]
        public string HealthCheck_Test()
        {
            return _mPAuthenticationController.HealthCheck();
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
            _moqHeader.Setup(_ => _.ContextValues).Returns(
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
        public void ValidateMPSignInV2_Test()
        {
            var response = new MOBMPPINPWDValidateResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", RememberMEFlags = new MOBMPTFARememberMeFlags { } };
            var request = new MOBMPPINPWDValidateRequest()
            {
                MileagePlusNumber = "AW719636",
                Application = new MOBApplication()
                {
                    Id = 1,
                    Version = new MOBVersion()
                    {
                        Major = "4.1.25",
                        Minor = "4.1.25"
                    }
                },
                TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44"

            };
            _mPAuthenticationController.ControllerContext = new ControllerContext();
            _mPAuthenticationController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPAuthenticationController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mPAuthenticationBusiness.Setup(p => p.ValidateMPSignInV2(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mPAuthenticationController.ValidateMPSignInV2(request);
            Assert.True(result.Result.TransactionId == "EE64E779-7B46-4836-B261-62AE35498B44");
            Assert.True(result.Result.Exception == null);
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void ValidateMPSignInV2_Exception_Test()
        {
            var request = new Request<MOBMPPINPWDValidateRequest>()
            {
                Data = new MOBMPPINPWDValidateRequest()
                {
                    MileagePlusNumber = "AW719636",
                    Application = new MOBApplication()
                    {
                        Id = 2,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.23"
                        }
                    },
                }
            };

            _mPAuthenticationController.ControllerContext = new ControllerContext();
            _mPAuthenticationController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPAuthenticationController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";
            _mPAuthenticationBusiness.Setup(p => p.ValidateMPSignInV2(request.Data)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            httpContext.Request.Headers[Constants.ApplicationIdText].ToString();
            httpContext.Request.Headers[Constants.HeaderDeviceIdText].ToString();
            httpContext.Request.Headers[Constants.ApplicationVersionText].ToString();

            var result = _mPAuthenticationController.ValidateMPSignInV2(request.Data);
            Assert.True(result.Result != null && result.Result.Exception.Message.Equals("Error Message"));
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void ValidateMPSignInV2_MOBUnitedException_Test()
        {
            var request = new Request<MOBMPPINPWDValidateRequest>()
            {
                Data = new MOBMPPINPWDValidateRequest()
                {
                    MileagePlusNumber = "AW719636",
                    Application = new MOBApplication()
                    {
                        Id = 2,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.23"
                        }
                    },
                }
            };

            _mPAuthenticationController.ControllerContext = new ControllerContext();
            _mPAuthenticationController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPAuthenticationController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";
            _mPAuthenticationBusiness.Setup(p => p.ValidateMPSignInV2(request.Data)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            httpContext.Request.Headers[Constants.ApplicationIdText].ToString();
            httpContext.Request.Headers[Constants.HeaderDeviceIdText].ToString();
            httpContext.Request.Headers[Constants.ApplicationVersionText].ToString();

            var result = _mPAuthenticationController.ValidateMPSignInV2(request.Data);
            Assert.True(result.Result != null && result.Result.Exception.Message.Equals("Error Message"));
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void ValidateTFASecurityQuestionsV2_Test()
        {
            var response = new MOBTFAMPDeviceResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44" };
            var request = new Request<MOBTFAMPDeviceRequest>()
            {
                Data = new MOBTFAMPDeviceRequest()
                {
                    MileagePlusNumber = "AW719636",
                    Application = new MOBApplication()
                    {
                        Id = 2,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.23"
                        }
                    },
                    TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44"
                }
            };

            _mPAuthenticationController.ControllerContext = new ControllerContext();
            _mPAuthenticationController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPAuthenticationController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";
            _mPAuthenticationBusiness.Setup(p => p.ValidateTFASecurityQuestionsV2(request.Data)).Returns(Task.FromResult(response));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            httpContext.Request.Headers[Constants.ApplicationIdText].ToString();
            httpContext.Request.Headers[Constants.HeaderDeviceIdText].ToString();
            httpContext.Request.Headers[Constants.ApplicationVersionText].ToString();

            var result = _mPAuthenticationController.ValidateTFASecurityQuestionsV2(request.Data);
            Assert.True(result != null && result.Result.TransactionId == "EE64E779-7B46-4836-B261-62AE35498B44");
            Assert.True(result.Result.CallDuration > 0);
            Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void ValidateTFASecurityQuestionsV2_Exception_Test()
        {
            var request = new Request<MOBTFAMPDeviceRequest>()
            {
                Data = new MOBTFAMPDeviceRequest()
                {
                    MileagePlusNumber = "AW719636",
                    Application = new MOBApplication()
                    {
                        Id = 2,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.23"
                        }
                    },
                }
            };

            _mPAuthenticationController.ControllerContext = new ControllerContext();
            _mPAuthenticationController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPAuthenticationController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";
            _mPAuthenticationBusiness.Setup(p => p.ValidateTFASecurityQuestionsV2(request.Data)).ThrowsAsync(new Exception("Sorry, something went wrong. Please try again."));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            httpContext.Request.Headers[Constants.ApplicationIdText].ToString();
            httpContext.Request.Headers[Constants.HeaderDeviceIdText].ToString();
            httpContext.Request.Headers[Constants.ApplicationVersionText].ToString();

            var result = _mPAuthenticationController.ValidateTFASecurityQuestionsV2(request.Data);
            Assert.True(result.Result != null && result.Result.Exception.Message.Equals("Sorry, something went wrong. Please try again."));
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void ValidateTFASecurityQuestionsV2_Exception_Test1()
        {
            var request = new Request<MOBTFAMPDeviceRequest>()
            {
                Data = new MOBTFAMPDeviceRequest()
                {
                    MileagePlusNumber = "AW719636",
                    Application = new MOBApplication()
                    {
                        Id = 2,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.23"
                        }
                    },
                }
            };

            _mPAuthenticationController.ControllerContext = new ControllerContext();
            _mPAuthenticationController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPAuthenticationController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";
            _mPAuthenticationBusiness.Setup(p => p.ValidateTFASecurityQuestionsV2(request.Data)).ThrowsAsync(new Exception("Sorry, something went wrong. Please try again."));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            httpContext.Request.Headers[Constants.ApplicationIdText].ToString();
            httpContext.Request.Headers[Constants.HeaderDeviceIdText].ToString();
            httpContext.Request.Headers[Constants.ApplicationVersionText].ToString();

            if (request.Data.MileagePlusNumber == "AW719636") 
            { 
                _configuration["SurfaceErrorToClient"] = "True";
            }

            var result = _mPAuthenticationController.ValidateTFASecurityQuestionsV2(request.Data);
            Assert.True(result.Result != null && result.Result.Exception.Message.Equals("Sorry, something went wrong. Please try again."));
            Assert.True(result.Result.CallDuration > 0);
        }


        [Fact]
        public void ValidateTFASecurityQuestionsV2_MOBUnitedException_Test()
        {
            var request = new Request<MOBTFAMPDeviceRequest>()
            {
                Data = new MOBTFAMPDeviceRequest()
                {
                    MileagePlusNumber = "AW719636",
                    Application = new MOBApplication()
                    {
                        Id = 2,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.23"
                        }
                    },
                }
            };

            _mPAuthenticationController.ControllerContext = new ControllerContext();
            _mPAuthenticationController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPAuthenticationController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";
            _mPAuthenticationBusiness.Setup(p => p.ValidateTFASecurityQuestionsV2(request.Data)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            httpContext.Request.Headers[Constants.ApplicationIdText].ToString();
            httpContext.Request.Headers[Constants.HeaderDeviceIdText].ToString();
            httpContext.Request.Headers[Constants.ApplicationVersionText].ToString();

            var result = _mPAuthenticationController.ValidateTFASecurityQuestionsV2(request.Data);
            Assert.True(result.Result != null && result.Result.Exception.Message.Equals("Error Message"));
            Assert.True(result.Result.CallDuration > 0);
        }

    }
}

