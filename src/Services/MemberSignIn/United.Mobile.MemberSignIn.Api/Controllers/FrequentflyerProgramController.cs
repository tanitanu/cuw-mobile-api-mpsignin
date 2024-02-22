using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Threading.Tasks;
using United.Mobile.Model.MPSignIn.MPNumberToPNR;
using United.Mobile.Model;
using United.Utility.Helper;
using United.Mobile.MemberSignIn.Domain;
using System;
using United.Ebs.Logging.Enrichers;
using United.Common.Helper;
using United.Mobile.Model.Internal.Exception;
using United.Utility.Serilog;

namespace United.Mobile.MemberSignIn.Api.Controllers
{
    [Route("membersigninservice/api/")]
    [ApiController]
    public class FrequentflyerProgramController : ControllerBase
    {
        private readonly ICacheLog<FrequentflyerProgramController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMPNumberToPnrBussiness _mpNumberToPnrBussiness;
        private readonly IRequestEnricher _requestEnricher;
        private readonly IHeaders _headers;

        public FrequentflyerProgramController(
            ICacheLog<FrequentflyerProgramController> logger,
            IConfiguration configuration,
            IMPNumberToPnrBussiness mpNumberToPnrBussiness,
            IRequestEnricher requestEnricher,
            IHeaders headers)
        {
            _logger = logger;
            _configuration = configuration;
            _mpNumberToPnrBussiness = mpNumberToPnrBussiness;
            _requestEnricher = requestEnricher;
            _requestEnricher.Add("Application", "United.Mobile.MemberSignIn.Api");
            _requestEnricher.Add("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
            _headers = headers;
        }

        [HttpPost]
        [Route("FrequentflyerProgram/SearchMPNumber")]
        public async Task<MPSearchResponse> SearchMPNumber([FromBody] MPSearchRequest request)
        {
            var response = new MPSearchResponse();
            IDisposable timer = null;
            try
            {
                await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId).ConfigureAwait(false);

                _logger.LogInformation("SearchMPNumber {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (_logger.BeginTimedOperation("Total time taken for SearchMPNumber call", transationId: Request.Headers[United.Mobile.Model.Constants.HeaderTransactionIdText].ToString()))
                {
                    response = await _mpNumberToPnrBussiness.SearchMPNumber(request).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("SearchMPNumber Error {@UnitedException}, {@StackTrace}", uaex.Message, uaex.StackTrace);
                response.Exception = new MOBException
                {
                    Message = uaex.Message

                };
                if (_configuration.GetValue<bool>("SessionExpiryMessageRedirectToHomerBookingMain"))
                {
                    response.Exception.Code = uaex.Code;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("SearchMPNumber Error {@Exception}, {@StackTrace}", ex.Message, ex.StackTrace);

                if (!_configuration.GetValue<bool>("SurfaceErrorToClient"))
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    response.Exception = new MOBException("9999", ex.Message);
                }
            }
            response.CallDuration = (timer != null) ? ((TimedOperation)timer).GetElapseTime() : 0;
            _logger.LogInformation("SearchMPNumber {@ClientResponse}", JsonConvert.SerializeObject(response));
            return response;
        }

        [HttpPost]
        [Route("FrequentflyerProgram/AddMpToPnrEligibilityCheck")]
        public async Task<MOBAddMpToPnrEligibilityResponse> AddMpToPnrEligibilityCheck([FromBody] MPSearchRequest request)
        {
            var response = new MOBAddMpToPnrEligibilityResponse();
            IDisposable timer = null;
            try
            {
                await _headers.SetHttpHeader(request.DeviceId, request.Application?.Id.ToString(), request.Application?.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId).ConfigureAwait(false);
                _logger.LogInformation("AddMpToPnrEligibilityCheck {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for AddMpToPnrEligibilityCheck business call", transationId: request.SessionId))
                {
                    response = await _mpNumberToPnrBussiness.AddMpToPnrEligibilityCheck(request).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("AddMpToPnrEligibilityCheck Error {@UnitedException}, {@StackTrace}", uaex.Message, uaex.StackTrace);
                response.Exception = new MOBException
                {
                    Message = uaex.Message

                };
                if (_configuration.GetValue<bool>("SessionExpiryMessageRedirectToHomerBookingMain"))
                {
                    response.Exception.Code = uaex.Code;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("AddMpToPnrEligibilityCheck Error {@Exception}, {@StackTrace}", ex.Message, ex.StackTrace);

                if (!_configuration.GetValue<bool>("SurfaceErrorToClient"))
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    response.Exception = new MOBException("9999", ex.Message);
                }
            }
            response.CallDuration = (timer != null) ? ((TimedOperation)timer).GetElapseTime() : 0;
            _logger.LogInformation("AddMpToPnrEligibilityCheck {@ClientResponse}", JsonConvert.SerializeObject(response));
            return response;
        }


        [HttpPost]
        [Route("FrequentflyerProgram/AddMPNumberToPnr")]
        public async Task<AddMPNumberToPnrResponse> AddMPNumberToPnr([FromBody] AddMPNumberToPnrRequest request)
        {
            var response = new AddMPNumberToPnrResponse();
            IDisposable timer = null;
            try
            {
                await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId).ConfigureAwait(false);

                _logger.LogInformation("AddMPNumberToPnr {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (_logger.BeginTimedOperation("Total time taken for AddMPNumberToPnr call", transationId: Request.Headers[United.Mobile.Model.Constants.HeaderTransactionIdText].ToString()))
                {
                    response = await _mpNumberToPnrBussiness.AddMPNumberToPnr(request).ConfigureAwait(false);
                }

            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("AddMPNumberToPnr Error {@UnitedException}, {@StackTrace}", uaex.Message, uaex.StackTrace);
                response.Exception = new MOBException
                {
                    Message = uaex.Message

                };
                if (_configuration.GetValue<bool>("SessionExpiryMessageRedirectToHomerBookingMain"))
                {
                    response.Exception.Code = uaex.Code;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("AddMPNumberToPnr Error {@Exception}, {@StackTrace}", ex.Message, ex.StackTrace);

                if (!_configuration.GetValue<bool>("SurfaceErrorToClient"))
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    response.Exception = new MOBException("9999", ex.Message);
                }
            }
            response.CallDuration = (timer != null) ? ((TimedOperation)timer).GetElapseTime() : 0;
            _logger.LogInformation("AddMPNumberToPnr {@ClientResponse}, {@PNR}", JsonConvert.SerializeObject(response), request.PNR);
            return response;
        }


    }
}
