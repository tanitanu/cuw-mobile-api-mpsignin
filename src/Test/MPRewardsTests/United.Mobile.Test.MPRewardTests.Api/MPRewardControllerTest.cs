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
using United.Mobile.Model.MPRewards;
using United.Mobile.MPRewards.Api.Controllers;
using United.Mobile.MPRewards.Domain;
using United.Utility.Helper;
using Xunit;
using Constants = United.Mobile.Model.Constants;
using Microsoft.Extensions.Logging;
using United.Ebs.Logging.Enrichers;
using United.Service.Presentation.ReferenceDataModel;

namespace United.Mobile.Test.MPRewardTests.Api
{
    public class MPRewardControllerTest
    {
        private readonly Mock<ICacheLog<MPRewardsController>> _logger;
        private readonly Mock<IMPRewardsBusiness> _mPRewardsBusiness;
        private readonly MPRewardsController _mPRewardsController;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly Mock<IHeaders> _moqHeader;
        private readonly Mock<IRequestEnricher> _requestEnricher;
        private readonly Mock<IFeatureSettings> _featureSettings;

        public MPRewardControllerTest()
        {
            _moqHeader = new Mock<IHeaders>();
            _logger = new Mock<ICacheLog<MPRewardsController>>();
            _mPRewardsBusiness = new Mock<IMPRewardsBusiness>();
            _requestEnricher = new Mock<IRequestEnricher>();
            _featureSettings = new Mock<IFeatureSettings>();
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
            .Build();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _mPRewardsController = new MPRewardsController(_logger.Object, _configuration, _moqHeader.Object, _mPRewardsBusiness.Object, _requestEnricher.Object, _featureSettings.Object);
            SetupHttpContextAccessor();
            SetHeaders();
        }
        [Fact]
        public string HealthCheck_Test()
        {
            return _mPRewardsController.HealthCheck();
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
        public void GetCancelFFCPnrsByMPNumber_Test()
        {
            var response = new MOBCancelFFCPNRsByMPNumberResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44" };
            var request = new MOBCancelFFCPNRsByMPNumberRequest()
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
            _mPRewardsController.ControllerContext = new ControllerContext();
            _mPRewardsController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPRewardsController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mPRewardsBusiness.Setup(p => p.GetCancelFFCPnrsByMPNumber(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mPRewardsController.GetCancelFFCPnrsByMPNumber(request);
            Assert.True(result.Result.TransactionId == "EE64E779-7B46-4836-B261-62AE35498B44");
            Assert.True(result.Result.Exception == null);
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void GetCancelFFCPnrsByMPNumber_SystemException_Test()
        {
            var request = new MOBCancelFFCPNRsByMPNumberRequest()
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
            };
            _mPRewardsController.ControllerContext = new ControllerContext();
            _mPRewardsController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPRewardsController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mPRewardsBusiness.Setup(p => p.GetCancelFFCPnrsByMPNumber(request)).ThrowsAsync(new Exception("Error Message"));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mPRewardsController.GetCancelFFCPnrsByMPNumber(request);
            Assert.True(result.Result.Exception.Code == "9999");
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void GetCancelFFCPnrsByMPNumber_MOBUnitedException_Test()
        {
            var request = new MOBCancelFFCPNRsByMPNumberRequest()
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
            };
            _mPRewardsController.ControllerContext = new ControllerContext();
            _mPRewardsController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPRewardsController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mPRewardsBusiness.Setup(p => p.GetCancelFFCPnrsByMPNumber(request)).ThrowsAsync(new MOBUnitedException("Error Message"));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mPRewardsController.GetCancelFFCPnrsByMPNumber(request);
            Assert.True(result.Result.Exception.Message == "Error Message");
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void GetAccountPlusPointsDetails_Test()
        {
            var response = new MOBMPPlusPointsResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44" };
            var request = new MOBMPPlusPointsRequest()
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
            _mPRewardsController.ControllerContext = new ControllerContext();
            _mPRewardsController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPRewardsController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mPRewardsBusiness.Setup(p => p.GetAccountPlusPointsDetails(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mPRewardsController.GetAccountPlusPointsDetails(request);
            Assert.True(result.Result.TransactionId == "EE64E779-7B46-4836-B261-62AE35498B44");
            Assert.True(result.Result.Exception == null);
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void GetAccountPlusPointsDetails_SystemException_Test()
        {
            var request = new MOBMPPlusPointsRequest()
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
            };
            _mPRewardsController.ControllerContext = new ControllerContext();
            _mPRewardsController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPRewardsController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mPRewardsBusiness.Setup(p => p.GetAccountPlusPointsDetails(request)).ThrowsAsync(new Exception("Error Message"));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mPRewardsController.GetAccountPlusPointsDetails(request);
            Assert.True(result.Result.Exception.Code == "10000");
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void GetAccountPlusPointsDetails_MOBUnitedException_Test()
        {
            var request = new MOBMPPlusPointsRequest()
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
            };
            _mPRewardsController.ControllerContext = new ControllerContext();
            _mPRewardsController.ControllerContext.HttpContext = new DefaultHttpContext();
            _mPRewardsController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mPRewardsBusiness.Setup(p => p.GetAccountPlusPointsDetails(request)).ThrowsAsync(new MOBUnitedException("Error Message"));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _mPRewardsController.GetAccountPlusPointsDetails(request);
            Assert.True(result.Result.CallDuration > 0);
        }



    }

}
