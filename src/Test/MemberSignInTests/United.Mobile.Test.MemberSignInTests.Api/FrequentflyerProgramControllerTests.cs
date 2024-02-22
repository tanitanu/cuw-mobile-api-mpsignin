using System;
using System.Collections.Generic;
using System.Text;
using United.Common.Helper;
using United.Ebs.Logging.Enrichers;

using United.Mobile.MemberSignIn.Domain;
using United.Utility.Helper;
using Microsoft.Extensions.Configuration;
using Moq;
using Microsoft.AspNetCore.Http;
using System.IO;
using United.Mobile.MemberSignIn.Api.Controllers;
using United.Service.Presentation.ReferenceDataModel;
using System.Configuration;
using United.Mobile.Model;
using Microsoft.AspNetCore.Mvc;
using United.Mobile.Model.Common;
using Xunit;
using United.Mobile.Model.MPSignIn.MPNumberToPNR;
using System.Threading.Tasks;
using United.Mobile.Model.Internal.Exception;

namespace United.Mobile.Test.MemberSignInTests.Api
{
    public class FrequentflyerProgramControllerTests
    {
        private readonly Mock<ICacheLog<FrequentflyerProgramController>> _logger;
        private readonly IConfiguration _configuration;
        private readonly Mock<IMPNumberToPnrBussiness> _mpNumberToPnrBussiness;
        private readonly Mock<IRequestEnricher> _requestEnricher;
        private readonly Mock<IHeaders> _moqHeader;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly FrequentflyerProgramController _frequentflyerProgramController;

        public FrequentflyerProgramControllerTests()
        {
            _logger = new Mock<ICacheLog<FrequentflyerProgramController>>();
            _mpNumberToPnrBussiness = new Mock<IMPNumberToPnrBussiness>();
            _requestEnricher = new Mock<IRequestEnricher>();
            _moqHeader = new Mock<IHeaders>();
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
            .Build();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _frequentflyerProgramController = new FrequentflyerProgramController(_logger.Object, _configuration, _mpNumberToPnrBussiness.Object, _requestEnricher.Object, _moqHeader.Object);
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
        private void SetHeaders(string deviceId = "D3F8FB8D-D563-449F-B4CF-4BF0AD8D6DB3"
       , string applicationId = "2"
       , string appVersion = "4.1.98"
       , string transactionId = "A4E94A08-96F0-4D56-98D9-D05DB37C8FE1|7AB2F4A8-2583-4D95-8F6C-511032964877"
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
        public void AddMpToPnrEligibilityCheck_Test()
        {
            var response = new MOBAddMpToPnrEligibilityResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44" };
            var request = new Request<MPSearchRequest>()
            {
                Data = new MPSearchRequest()
                {
                    SessionId = "ED87FEFE2BF649CEB4C0BA559B9EB5F3",
                    PNR = "ELR9RS",
                    LastName = "Patil",
                    Application = new MOBApplication()
                    {
                        Id = 1,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.98",
                            Minor = "4.1.98"
                        }
                    },
                    TransactionId = "A4E94A08-96F0-4D56-98D9-D05DB37C8FE1|7AB2F4A8-2583-4D95-8F6C-511032964877"
                }
            };
            _frequentflyerProgramController.ControllerContext = new ControllerContext();
            _frequentflyerProgramController.ControllerContext.HttpContext = new DefaultHttpContext();
            _frequentflyerProgramController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mpNumberToPnrBussiness.Setup(p => p.AddMpToPnrEligibilityCheck(request.Data)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _frequentflyerProgramController.AddMpToPnrEligibilityCheck(request.Data);
            Assert.True(result.Result.TransactionId == "A4E94A08-96F0-4D56-98D9-D05DB37C8FE1|7AB2F4A8-2583-4D95-8F6C-511032964877");
            Assert.True(result.Result.Exception == null);
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void AddMpToPnrEligibilityCheck_SystemException_Test()
        {
            var response = new MOBAddMpToPnrEligibilityResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44" };
            var request = new Request<MPSearchRequest>()
            {
                Data = new MPSearchRequest()
                {
                    SessionId = "ED87FEFE2BF649CEB4C0BA559B9EB5F3",
                    PNR = "ELR9RS",
                    LastName = "Patil",
                    Application = new MOBApplication()
                    {
                        Id = 1,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.98",
                            Minor = "4.1.98"
                        }
                    },
                    TransactionId = "A4E94A08-96F0-4D56-98D9-D05DB37C8FE1|7AB2F4A8-2583-4D95-8F6C-511032964877"
                }
            };
            _frequentflyerProgramController.ControllerContext = new ControllerContext();
            _frequentflyerProgramController.ControllerContext.HttpContext = new DefaultHttpContext();
            _frequentflyerProgramController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mpNumberToPnrBussiness.Setup(p => p.AddMpToPnrEligibilityCheck(request.Data)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _frequentflyerProgramController.AddMpToPnrEligibilityCheck(request.Data);
            Assert.True(result.Result.TransactionId == "A4E94A08-96F0-4D56-98D9-D05DB37C8FE1|7AB2F4A8-2583-4D95-8F6C-511032964877");
            Assert.True(result.Result.Exception == null);
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void AddMpToPnrEligibilityCheck_MOBUnitedException_Test()
        {
            var response = new MOBAddMpToPnrEligibilityResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44" };
            var request = new Request<MPSearchRequest>()
            {
                Data = new MPSearchRequest()
                {
                    SessionId = "ED87FEFE2BF649CEB4C0BA559B9EB5F3",
                    PNR = "ELR9RS",
                    LastName = "Patil",
                    Application = new MOBApplication()
                    {
                        Id = 1,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.98",
                            Minor = "4.1.98"
                        }
                    },
                    TransactionId = "A4E94A08-96F0-4D56-98D9-D05DB37C8FE1|7AB2F4A8-2583-4D95-8F6C-511032964877"
                }
            };
            _frequentflyerProgramController.ControllerContext = new ControllerContext();
            _frequentflyerProgramController.ControllerContext.HttpContext = new DefaultHttpContext();
            _frequentflyerProgramController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mpNumberToPnrBussiness.Setup(p => p.AddMpToPnrEligibilityCheck(request.Data)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _frequentflyerProgramController.AddMpToPnrEligibilityCheck(request.Data);
            Assert.True(result.Result.TransactionId == "A4E94A08-96F0-4D56-98D9-D05DB37C8FE1|7AB2F4A8-2583-4D95-8F6C-511032964877");
            Assert.True(result.Result.Exception == null);
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void SearchMPNumber_Test()
        {
            var response = new MPSearchResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44" };
            var request = new Request<MPSearchRequest>()
            {
                Data = new MPSearchRequest()
                {
                    SessionId = "ED87FEFE2BF649CEB4C0BA559B9EB5F3",
                    PNR = "ELR9RS",
                    LastName = "Patil",
                    Application = new MOBApplication()
                    {
                        Id = 1,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.98",
                            Minor = "4.1.98"
                        }
                    },
                    TransactionId = "A4E94A08-96F0-4D56-98D9-D05DB37C8FE1|7AB2F4A8-2583-4D95-8F6C-511032964877"
                }
            };
            _frequentflyerProgramController.ControllerContext = new ControllerContext();
            _frequentflyerProgramController.ControllerContext.HttpContext = new DefaultHttpContext();
            _frequentflyerProgramController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mpNumberToPnrBussiness.Setup(p => p.SearchMPNumber(request.Data)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _frequentflyerProgramController.SearchMPNumber(request.Data);
            Assert.True(result.Result.TransactionId == "A4E94A08-96F0-4D56-98D9-D05DB37C8FE1|7AB2F4A8-2583-4D95-8F6C-511032964877");
            Assert.True(result.Result.Exception == null);
            Assert.True(result.Result.CallDuration > 0);
        }

        [Fact]
        public void AddMPNumberToPnr_Test()
        {
            var response = new AddMPNumberToPnrResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44" };
            AddMPNumberToPnrTravelerInfo Person = new AddMPNumberToPnrTravelerInfo();
            Person.GivenName = "Ramesh";
            Person.LastName = "Patil";
            Person.MileagePlusNumber = "BBV33152";
            Person.CurrentTierLevel = 0;
            Person.SharesPosition = "1.1";

            AddMPNumberToPnrTraveler person1 = new AddMPNumberToPnrTraveler();
            person1.Person = Person;
            List<AddMPNumberToPnrTraveler> Traveler = new List<AddMPNumberToPnrTraveler>();
            Traveler.Add(person1);

            var request = new Request<AddMPNumberToPnrRequest>()
            {
                Data = new AddMPNumberToPnrRequest()
                {
                    SessionId = "ED87FEFE2BF649CEB4C0BA559B9EB5F3",
                    PNR = "ELR9RS",
                    Traveler = Traveler,
                    Application = new MOBApplication()
                    {
                        Id = 1,
                        Version = new MOBVersion()
                        {
                            Major = "4.1.98",
                            Minor = "4.1.98"
                        }
                    },
                    TransactionId = "A4E94A08-96F0-4D56-98D9-D05DB37C8FE1|7AB2F4A8-2583-4D95-8F6C-511032964877"
                }
            };
            _frequentflyerProgramController.ControllerContext = new ControllerContext();
            _frequentflyerProgramController.ControllerContext.HttpContext = new DefaultHttpContext();
            _frequentflyerProgramController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _mpNumberToPnrBussiness.Setup(p => p.AddMPNumberToPnr(request.Data)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _frequentflyerProgramController.AddMPNumberToPnr(request.Data);
            Assert.True(result.Result.TransactionId == "A4E94A08-96F0-4D56-98D9-D05DB37C8FE1|7AB2F4A8-2583-4D95-8F6C-511032964877");
            Assert.True(result.Result.Exception == null);
            Assert.True(result.Result.CallDuration > 0);
        }

    }
}
