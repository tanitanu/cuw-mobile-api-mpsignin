using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Mobile.MemberSignIn.Api.Controllers;
using United.Mobile.MemberSignIn.Domain;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Exception;
using United.Utility.Helper;
using Xunit;
using Constants = United.Mobile.Model.Constants;
using Microsoft.Extensions.Logging;
using United.Ebs.Logging.Enrichers;
using United.Service.Presentation.ReferenceDataModel;

namespace United.Mobile.Test.MemberSignInTests.Api
{
    public class MemberSignInControllerTests
    {
        private readonly Mock<ICacheLog<MemberSignInController>> _logger;
        private readonly Mock<IMemberSignInBusiness> _memberSignInBusiness;
        private readonly MemberSignInController _memberSignInController;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly Mock<IHeaders> _moqHeader;
        private readonly Mock<IRequestEnricher> _requestEnricher;
        private readonly Mock<IFeatureSettings> _featureSettings;

        public MemberSignInControllerTests()
        {
            _moqHeader = new Mock<IHeaders>();
            _logger = new Mock<ICacheLog<MemberSignInController>>();
            _memberSignInBusiness = new Mock<IMemberSignInBusiness>();
            _requestEnricher = new Mock<IRequestEnricher>();
            _featureSettings = new Mock<IFeatureSettings>();
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
            .Build();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _memberSignInController = new MemberSignInController(_logger.Object, _configuration, _moqHeader.Object, _memberSignInBusiness.Object, _requestEnricher.Object, _featureSettings.Object);
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
        public void MPSignInNeedHelp_Test()
        {
            var response = new MOBMPSignInNeedHelpResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44" };
            var request = new Request<MOBMPSignInNeedHelpRequest>()
            {
                Data = new MOBMPSignInNeedHelpRequest()
                {
                    SessionID = "0B4D8C69883C46EFB69177D68387BA73",
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
                }
            };
            _memberSignInController.ControllerContext = new ControllerContext();
            _memberSignInController.ControllerContext.HttpContext = new DefaultHttpContext();
            _memberSignInController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _memberSignInBusiness.Setup(p => p.MPSignInNeedHelp(request.Data)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _memberSignInController.MPSignInNeedHelp(request.Data);
            Assert.True(result.Result.TransactionId == "EE64E779-7B46-4836-B261-62AE35498B44");
            Assert.True(result.Result.Exception == null);
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void MPSignInNeedHelp_SystemException_Test()
        {
            var request = new Request<MOBMPSignInNeedHelpRequest>()
            {
                Data = new MOBMPSignInNeedHelpRequest()
                {
                    SessionID = "0B4D8C69883C46EFB69177D68387BA73",
                    MileagePlusNumber = "AW719636",
                    Application = new MOBApplication()
                    {
                        Id = 1,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.25",
                            Minor = "4.1.25"
                        }
                    }
                }
            };
            _memberSignInController.ControllerContext = new ControllerContext();
            _memberSignInController.ControllerContext.HttpContext = new DefaultHttpContext();
            _memberSignInController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _memberSignInBusiness.Setup(p => p.MPSignInNeedHelp(request.Data)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _memberSignInController.MPSignInNeedHelp(request.Data);
            Assert.True(result.Result.Exception.Code == "9999");
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void MPSignInNeedHelp_MOBUnitedException_Test()
        {
            var request = new Request<MOBMPSignInNeedHelpRequest>()
            {
                Data = new MOBMPSignInNeedHelpRequest()
                {
                    SessionID = "0B4D8C69883C46EFB69177D68387BA73",
                    MileagePlusNumber = "AW719636",
                    Application = new MOBApplication()
                    {
                        Id = 1,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.25",
                            Minor = "4.1.25"
                        }
                    }
                }
            };
            _memberSignInController.ControllerContext = new ControllerContext();
            _memberSignInController.ControllerContext.HttpContext = new DefaultHttpContext();
            _memberSignInController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _memberSignInBusiness.Setup(p => p.MPSignInNeedHelp(request.Data)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _memberSignInController.MPSignInNeedHelp(request.Data);
            Assert.True(result.Result.Exception.Message == "Error Message");
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void OneClickEnrollment_Test()
        {
            var response = new MOBJoinMileagePlusEnrollmentResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44" };
            var request = new Request<MOBJoinMileagePlusEnrollmentRequest>()
            {
                Data = new MOBJoinMileagePlusEnrollmentRequest()
                {
                    SessionId = "0B4D8C69883C46EFB69177D68387BA73",
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
                }
            };
            _memberSignInController.ControllerContext = new ControllerContext();
            _memberSignInController.ControllerContext.HttpContext = new DefaultHttpContext();
            _memberSignInController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _memberSignInBusiness.Setup(p => p.OneClickEnrollment(request.Data)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _memberSignInController.OneClickEnrollment(request.Data);
            Assert.True(result.Result.TransactionId == "EE64E779-7B46-4836-B261-62AE35498B44");
            Assert.True(result.Result.Exception == null);
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void OneClickEnrollment_SystemException_Test()
        {
            var request = new Request<MOBJoinMileagePlusEnrollmentRequest>()
            {
                Data = new MOBJoinMileagePlusEnrollmentRequest()
                {
                    SessionId = "0B4D8C69883C46EFB69177D68387BA73",
                    Application = new MOBApplication()
                    {
                        Id = 1,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.25",
                            Minor = "4.1.25"
                        }
                    }
                }
            };
            _memberSignInController.ControllerContext = new ControllerContext();
            _memberSignInController.ControllerContext.HttpContext = new DefaultHttpContext();
            _memberSignInController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _memberSignInBusiness.Setup(p => p.OneClickEnrollment(request.Data)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _memberSignInController.OneClickEnrollment(request.Data);
            Assert.True(result.Result.Exception.Code == "9999");
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void OneClickEnrollment_MOBUnitedException_Test()
        {
            var request = new Request<MOBJoinMileagePlusEnrollmentRequest>()
            {
                Data = new MOBJoinMileagePlusEnrollmentRequest()
                {
                    SessionId = "0B4D8C69883C46EFB69177D68387BA73",
                    Application = new MOBApplication()
                    {
                        Id = 1,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.25",
                            Minor = "4.1.25"
                        }
                    }
                }
            };
            _memberSignInController.ControllerContext = new ControllerContext();
            _memberSignInController.ControllerContext.HttpContext = new DefaultHttpContext();
            _memberSignInController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _memberSignInBusiness.Setup(p => p.OneClickEnrollment(request.Data)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _memberSignInController.OneClickEnrollment(request.Data);
            Assert.True(result.Result.Exception.Message == "Error Message");
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void SendResetAccountEmail_Test()
        {
            var response = new MOBTFAMPDeviceResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44" };
            var request = new Request<MOBTFAMPDeviceRequest>()
            {
                Data = new MOBTFAMPDeviceRequest()
                {
                    SessionID = "0B4D8C69883C46EFB69177D68387BA73",
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
                }
            };
            _memberSignInController.ControllerContext = new ControllerContext();
            _memberSignInController.ControllerContext.HttpContext = new DefaultHttpContext();
            _memberSignInController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _memberSignInBusiness.Setup(p => p.SendResetAccountEmail(request.Data)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _memberSignInController.SendResetAccountEmail(request.Data);
            Assert.True(result.Result.TransactionId == "EE64E779-7B46-4836-B261-62AE35498B44");
            Assert.True(result.Result.Exception == null);
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void SendResetAccountEmail_SystemException_Test()
        {
            var request = new Request<MOBTFAMPDeviceRequest>()
            {
                Data = new MOBTFAMPDeviceRequest()
                {
                    SessionID = "0B4D8C69883C46EFB69177D68387BA73",
                    MileagePlusNumber = "AW719636",
                    Application = new MOBApplication()
                    {
                        Id = 1,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.25",
                            Minor = "4.1.25"
                        }
                    }
                }
            };
            _memberSignInController.ControllerContext = new ControllerContext();
            _memberSignInController.ControllerContext.HttpContext = new DefaultHttpContext();
            _memberSignInController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _memberSignInBusiness.Setup(p => p.SendResetAccountEmail(request.Data)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _memberSignInController.SendResetAccountEmail(request.Data);
            Assert.True(result.Result.Exception.Code == "9999");
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void SendResetAccountEmail_MOBUnitedException_Test()
        {
            var request = new Request<MOBTFAMPDeviceRequest>()
            {
                Data = new MOBTFAMPDeviceRequest()
                {
                    SessionID = "0B4D8C69883C46EFB69177D68387BA73",
                    MileagePlusNumber = "AW719636",
                    Application = new MOBApplication()
                    {
                        Id = 1,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.25",
                            Minor = "4.1.25"
                        }
                    }
                }
            };
            _memberSignInController.ControllerContext = new ControllerContext();
            _memberSignInController.ControllerContext.HttpContext = new DefaultHttpContext();
            _memberSignInController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _memberSignInBusiness.Setup(p => p.SendResetAccountEmail(request.Data)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _memberSignInController.SendResetAccountEmail(request.Data);
            Assert.True(result.Result.Exception.Message == "Error Message");
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void MPEnrollmentWithSecurityQuestions_Test()
        {
            var response = new MOBMPMPEnRollmentResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44" };
            var request = new Request<MOBMPEnRollmentRequest>()
            {
                Data = new MOBMPEnRollmentRequest()
                {
                    SessionID = "0B4D8C69883C46EFB69177D68387BA73",
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
                }
            };
            _memberSignInController.ControllerContext = new ControllerContext();
            _memberSignInController.ControllerContext.HttpContext = new DefaultHttpContext();
            _memberSignInController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
          
        }
        [Fact]
        public void MPEnrollmentWithSecurityQuestions_SystemException_Test()
        {
            var request = new Request<MOBMPEnRollmentRequest>()
            {
                Data = new MOBMPEnRollmentRequest()
                {
                    SessionID = "0B4D8C69883C46EFB69177D68387BA73",
                    Application = new MOBApplication()
                    {
                        Id = 1,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.25",
                            Minor = "4.1.25"
                        }
                    }
                }
            };
            _memberSignInController.ControllerContext = new ControllerContext();
            _memberSignInController.ControllerContext.HttpContext = new DefaultHttpContext();
            _memberSignInController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
        }

        [Fact]
        public void MPEnrollmentWithSecurityQuestions_MOBUnitedException_Test()
        {
            var request = new Request<MOBMPEnRollmentRequest>()
            {
                Data = new MOBMPEnRollmentRequest()
                {
                    SessionID = "0B4D8C69883C46EFB69177D68387BA73",
                    Application = new MOBApplication()
                    {
                        Id = 1,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.25",
                            Minor = "4.1.25"
                        }
                    },
                }
            };
            _memberSignInController.ControllerContext = new ControllerContext();
            _memberSignInController.ControllerContext.HttpContext = new DefaultHttpContext();
            _memberSignInController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";


            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
        }

        [Fact]
        public void MPEnrollmentWithSecurityQuestions_MOBUnitedException1_Test()
        {
            var request = new Request<MOBMPEnRollmentRequest>()
            {
                Data = new MOBMPEnRollmentRequest()
                {
                    SessionID = "0B4D8C69883C46EFB69177D68387BA73",
                    Application = new MOBApplication()
                    {
                        Id = 1,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.25",
                            Minor = "4.1.25"
                        }
                    },
                }
            };
            _memberSignInController.ControllerContext = new ControllerContext();
            _memberSignInController.ControllerContext.HttpContext = new DefaultHttpContext();
            _memberSignInController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";


            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
        }


    }
}
