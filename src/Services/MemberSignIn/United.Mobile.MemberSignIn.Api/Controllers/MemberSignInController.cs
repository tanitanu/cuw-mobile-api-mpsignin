using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Ebs.Logging.Enrichers;
using United.Mobile.MemberSignIn.Domain;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.Common.FeatureSettings;
using United.Mobile.Model.Internal.Exception;
using United.Service.Presentation.ReferenceDataModel;
using United.Utility.Helper;
using United.Utility.Serilog;

namespace United.Mobile.MemberSignIn.Api.Controllers
{
    [Route("membersigninservice/api")]
    [ApiController]
    public class MemberSignInController : ControllerBase
    {
        private readonly ICacheLog<MemberSignInController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly IMemberSignInBusiness _memberSignInBusiness;
        private readonly IRequestEnricher _requestEnricher;
        private readonly IFeatureSettings _featureSettings;


        public MemberSignInController(ICacheLog<MemberSignInController> logger
            , IConfiguration configuration
            , IHeaders headers
            , IMemberSignInBusiness memberSignInBusiness
            , IRequestEnricher requestEnricher
            , IFeatureSettings featureSettings
            )
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _memberSignInBusiness = memberSignInBusiness;
            _requestEnricher = requestEnricher;
            _featureSettings = featureSettings;
            _requestEnricher.Add("Application", "United.Mobile.MemberSignIn.Api");
            _requestEnricher.Add("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        }

        [HttpPost]
        [Route("MileagePlusCsl/OneClickEnrollment")]
        public async Task<MOBJoinMileagePlusEnrollmentResponse> OneClickEnrollment(MOBJoinMileagePlusEnrollmentRequest request)
        {
            var response = new MOBJoinMileagePlusEnrollmentResponse();
            IDisposable timer = null;
            try
            {
                await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId).ConfigureAwait(false);

                _logger.LogInformation("OneClickEnrollment {@ClientRequest}", JsonConvert.SerializeObject(request));

                using (timer = _logger.BeginTimedOperation("Total time taken for OneClickEnrollment business call", transationId: request.TransactionId))
                {
                    response = await _memberSignInBusiness.OneClickEnrollment(request).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("OneClickEnrollment Warning {@UnitedException}, {@StackTrace}", uaex.Message, uaex.StackTrace);

                response.Exception = new MOBException
                {
                    Message = uaex.Message
                };
            }
            catch (Exception ex)
            {
                MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);

                _logger.LogError("OneClickEnrollment Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = !_configuration.GetValue<bool>("SurfaceErrorToClient") ? new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage")) : new MOBException("9999", ex.Message);
            }

            response.CallDuration = (timer != null) ? ((TimedOperation)timer).GetElapseTime() : 0;
            _logger.LogInformation("OneClickEnrollment {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("MileagePlusCsl/MPSignInNeedHelp")]
        public async Task<MOBMPSignInNeedHelpResponse> MPSignInNeedHelp(MOBMPSignInNeedHelpRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionID).ConfigureAwait(false);
            var response = new MOBMPSignInNeedHelpResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("MPSignInNeedHelp {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for MPSignInNeedHelp business call", transationId: request.TransactionId))
                {
                    response = await _memberSignInBusiness.MPSignInNeedHelp(request).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("MPSignInNeedHelp Error {@UnitedException}, {@StackTrace}", uaex.Message, uaex.StackTrace);
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
                _logger.LogError("MPSignInNeedHelp Error {@Exception}, {@StackTrace}", ex.Message, ex.StackTrace);

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
            _logger.LogInformation("MPSignInNeedHelp {@ClientResponse}", JsonConvert.SerializeObject(response));
            return response;
        }

        [HttpPost]
        [Route("MileagePlusCsl/SendResetAccountEmail")]
        public async Task<MOBTFAMPDeviceResponse> SendResetAccountEmail(MOBTFAMPDeviceRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionID).ConfigureAwait(false);
            var response = new MOBTFAMPDeviceResponse();
            IDisposable timer = null;

            _logger.LogInformation("SendResetAccountEmail {@clientRequest}", JsonConvert.SerializeObject(request));

            try
            {
                using (timer = _logger.BeginTimedOperation("Total time taken for SendResetAccountEmail business call", transationId: request.TransactionId))
                {
                    response = await _memberSignInBusiness.SendResetAccountEmail(request).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("SendResetAccountEmail Error {@UnitedException}, {@StackTrace}", uaex.Message, uaex.StackTrace);

                if (uaex != null && !string.IsNullOrEmpty(uaex.Message.Trim()) && !uaex.Message.Trim().Contains("ORA-") && !uaex.Message.Trim().Contains("PL/SQL"))
                {
                    response.Exception = new MOBException
                    {
                        Message = uaex.Message
                    };
                }
                else
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<String>("Booking2OGenericExceptionMessage"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("SendResetAccountEmail Error {@Exception}, {@StackTrace}", ex.Message, ex.StackTrace);

                if (!_configuration.GetValue<bool>("SurfaceErrorToClient"))
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    response.Exception = new MOBException("9999", "SessionId :" + request.SessionID + ex.Message);
                }
            }

            response.CallDuration = (timer != null) ? ((TimedOperation)timer).GetElapseTime() : 0;
            _logger.LogInformation("SendResetAccountEmail {@clientResponse}", JsonConvert.SerializeObject(response));
            return response;
        }

        [HttpGet("GetFeatureSettings")]
        public GetFeatureSettingsResponse GetFeatureSettings()
        {
            GetFeatureSettingsResponse response = new GetFeatureSettingsResponse();
            try
            {
                response = _featureSettings.GetFeatureSettings();
            }
            catch (Exception ex)
            {
                _logger.LogError("GetFeatureSettings Error {exceptionstack} and {transactionId}", JsonConvert.SerializeObject(ex), "GetFeatureSettings_TransId");
                response.Exception = new MOBException("9999", JsonConvert.SerializeObject(ex));
            }
            return response;
        }
        [HttpPost("RefreshFeatureSettingCache")]
        public async Task<MOBResponse> RefreshFeatureSettingCache(MOBFeatureSettingsCacheRequest request)
        {
            MOBResponse response = new MOBResponse();
            try
            {
                await _featureSettings.RefreshFeatureSettingCache(request);
            }
            catch (Exception ex)
            {
                _logger.LogError("RefreshFeatureSettingCache Error {exceptionstack} and {transactionId}", JsonConvert.SerializeObject(ex), "RefreshRetrieveAllFeatureSettings_TransId");
                response.Exception = new MOBException("9999", JsonConvert.SerializeObject(ex));
            }
            return response;
        }

        [HttpGet]
        [Route("HealthCheck")]
        public string HealthCheck()
        {
            return "Healthy";
        }

        [HttpGet]
        [Route("version")]
        public virtual string Version()
        {
            string serviceVersionNumber = null;

            try
            {
                serviceVersionNumber = Environment.GetEnvironmentVariable("SERVICE_VERSION_NUMBER");
            }
            catch
            {
                // Suppress any exceptions
            }
            finally
            {
                serviceVersionNumber = (null == serviceVersionNumber) ? "0.0.0" : serviceVersionNumber;
            }

            return serviceVersionNumber;
        }

        [HttpGet]
        [Route("environment")]
        public virtual string ApiEnvironment()
        {
            try
            {
                return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            }
            catch
            {
                // Suppress any exceptions
            }

            return "Unknown";
        }
    }
}
