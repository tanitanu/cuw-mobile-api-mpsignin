using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Ebs.Logging.Enrichers;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.Common.FeatureSettings;
using United.Mobile.Model.Internal.Exception;
using United.Mobile.MPAuthentication.Domain;
using United.Service.Presentation.ReferenceDataModel;
using United.Utility.Helper;
using United.Utility.Serilog;

namespace United.Mobile.MPAuthentication.Api.Controllers
{
    [Route("mpauthenticationservice/api/")]
    [ApiController]
    public class MPAuthenticationController : ControllerBase
    {
        private readonly ICacheLog<MPAuthenticationController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMPAuthenticationBusiness _mpAuthenticationBusiness;
        private readonly IHeaders _headers;
        private readonly IRequestEnricher _requestEnricher;
        private readonly IFeatureSettings _featureSettings;


        public MPAuthenticationController(ICacheLog<MPAuthenticationController> logger
            , IConfiguration configuration
            , IMPAuthenticationBusiness mpAuthenticationBusiness
            , IHeaders headers
            , IRequestEnricher requestEnricher
            , IFeatureSettings featureSettings
            )
        {
            _logger = logger;
            _configuration = configuration;
            _mpAuthenticationBusiness = mpAuthenticationBusiness;
            _headers = headers;
            _requestEnricher = requestEnricher;
            _featureSettings = featureSettings;
            _requestEnricher.Add("Application", "United.Mobile.MPAuthentication.Api");
            _requestEnricher.Add("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        }

        [HttpPost]
        [Route("MileagePlusCsl/ValidateMPSignInV2")]
        public async Task<MOBMPPINPWDValidateResponse> ValidateMPSignInV2([FromBody] MOBMPPINPWDValidateRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionID).ConfigureAwait(false);
            MOBMPPINPWDValidateResponse response = new MOBMPPINPWDValidateResponse();
            IDisposable timer = null;
            _requestEnricher.Add("MPNumber", request.MileagePlusNumber);
            try
            {               
                using (timer = _logger.BeginTimedOperation("Total time taken for ValidateMPSignInV2 business call", transationId: string.Empty))
                {
                    response = await _mpAuthenticationBusiness.ValidateMPSignInV2(request).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("ValidateMPSignInV2 Error {@UnitedException}, {@StackTrace}", uaex.Message, uaex.StackTrace);
                response.Exception = _configuration.GetValue<string>("MPValidationErrorMessage") != null ? new MOBException("9999", _configuration.GetValue<string>("MPValidationErrorMessage")) : new MOBException("9999", "The account information you entered is incorrect.");
                response.Exception.Message = uaex.Message;
                if (uaex != null && !string.IsNullOrEmpty(uaex.Message.Trim()) && !uaex.Message.Trim().Contains("ORA-") && !uaex.Message.Trim().Contains("PL/SQL"))
                {
                    response.Exception = new MOBException();
                    response.Exception.Message = uaex.Message;
                }
                else
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError("ValidateMPSignInV2 Error {@Exception}, {@StackTrace}", ex.Message, ex.StackTrace);
                if (!_configuration.GetValue<bool>("SurfaceErrorToClient"))
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    response.Exception = new MOBException("9999", "SessionId :" + request.SessionID + ex.Message);
                }
                response.Exception = new MOBException("9999", ex.Message);
            }

            response.CallDuration = (timer != null) ? ((TimedOperation)timer).GetElapseTime() : 0;
            _logger.LogInformation("ValidateMPSignInV2 {@clientResponse} {transactionId} {@mileagePlusNumber}", JsonConvert.SerializeObject(response), request.TransactionId, request?.MileagePlusNumber);

            return response;
        }

        [HttpPost]
        [Route("MileagePlusCsl/ValidateTFASecurityQuestionsV2")]
        public async Task<MOBTFAMPDeviceResponse> ValidateTFASecurityQuestionsV2([FromBody] MOBTFAMPDeviceRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionID).ConfigureAwait(false);
            MOBTFAMPDeviceResponse response = new MOBTFAMPDeviceResponse();
            IDisposable timer = null;
            _requestEnricher.Add("MPNumber", request.MileagePlusNumber);
            try
            {
                _logger.LogInformation("ValidateTFASecurityQuestionsV2 {@ClientRequest} {@mileagePlusNumber}", JsonConvert.SerializeObject(request), request?.MileagePlusNumber);
                using (timer = _logger.BeginTimedOperation("Total time taken for ValidateTFASecurityQuestionsV2", transationId: string.Empty))
                {
                    response = await _mpAuthenticationBusiness.ValidateTFASecurityQuestionsV2(request).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("ValidateTFASecurityQuestionsV2 Error {@UnitedException}, {@StackTrace}", uaex.Message, uaex.StackTrace);
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
                _logger.LogError("ValidateTFASecurityQuestionsV2 Error {@Exception}, {@StackTrace}", ex.Message, ex.StackTrace);

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
            _logger.LogInformation("ValidateTFASecurityQuestionsV2 {@ClientResponse} {@mileagePlusNumber}", JsonConvert.SerializeObject(response), request?.MileagePlusNumber);

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