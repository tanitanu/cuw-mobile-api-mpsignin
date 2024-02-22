using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Ebs.Logging.Enrichers;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.Common.FeatureSettings;
using United.Mobile.Model.Internal.Exception;
using United.Mobile.Model.MPSignIn;
using United.Mobile.MPAccountProfile.Domain;
using United.Utility.Helper;
using United.Utility.Serilog;

namespace United.Mobile.MPAccountProfile.Api.Controllers
{
    [Route("mpaccountprofileservice/api/")]
    [ApiController]
    public class MPAccountProfileController : ControllerBase
    {
        private readonly ICacheLog<MPAccountProfileController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly IMPAccountProfileBusiness _mPAccountProfileBusiness;
        private readonly IRequestEnricher _requestEnricher;
        private readonly IFeatureSettings _featureSettings;


        public MPAccountProfileController(ICacheLog<MPAccountProfileController> logger
            , IConfiguration configuration
            , IHeaders headers
            , IMPAccountProfileBusiness mPAccountProfileBusiness
            , IRequestEnricher requestEnricher
            , IFeatureSettings featureSettings
            )
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _mPAccountProfileBusiness = mPAccountProfileBusiness;
            _requestEnricher = requestEnricher;
            _featureSettings = featureSettings;
            _requestEnricher.Add("Application", "United.Mobile.MPAccountProfile.Api");
            _requestEnricher.Add("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        }

        [HttpPost]
        [Route("MileagePlus/GetContactUsDetails")]
        public async Task<MOBContactUsResponse> GetContactUsDetails(MOBContactUsRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, string.Empty).ConfigureAwait(false);
            var response = new MOBContactUsResponse();
            IDisposable timer = null;

            try
            {
                _logger.LogInformation("GetContactUsDetails {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for GetContactUsDetails business call", transationId: request.TransactionId))
                {
                    response = await _mPAccountProfileBusiness.GetContactUsDetails(request).ConfigureAwait(false);

                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("GetContactUsDetails Error {@UnitedException}, {@StackTrace}", uaex.Message, uaex.StackTrace);
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
                _logger.LogError("GetContactUsDetails Error {@Exception}, {@StackTrace}", ex.Message, ex.StackTrace);

                if (!_configuration.GetValue<bool>("SurfaceErrorToClient"))
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    response.Exception = new MOBException("9999", ex.Message);
                }
            }

            response.CallDuration = 0; 
            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
            }
            
            _logger.LogInformation("GetContactUsDetails {@ClientResponse}", JsonConvert.SerializeObject(response));
            return response;
        }

        [HttpPost]
        [Route("CustomerProfile/RetrieveCustomerPreferences")]
        public async Task<MOBCustomerPreferencesResponse> RetrieveCustomerPreferences(MOBCustomerPreferencesRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId).ConfigureAwait(false);
            MOBCustomerPreferencesResponse response = new MOBCustomerPreferencesResponse();
            IDisposable timer = null;

            try
            {
                using (timer = _logger.BeginTimedOperation("Total time taken for RetrieveCustomerPreferences business call", transationId: request.TransactionId))
                {
                    response = await _mPAccountProfileBusiness.RetrieveCustomerPreferences(request).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("RetrieveCustomerPreferences Warning {@UnitedException}, {@StackTrace}", uaex.Message, uaex.StackTrace);
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("RetrieveCustomerPreferences Error {@Exception}, {@StackTrace}", ex.Message, ex.StackTrace);
                response.Exception = new MOBException("99999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }


            response.CallDuration = (timer != null) ? ((TimedOperation)timer).GetElapseTime() : 0;
            _logger.LogInformation("RetrieveCustomerPreferences {@clientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("MileagePlus/GetAccountSummaryWithMemberCardPremierActivity")]
        public async Task<MOBMPAccountSummaryResponse> GetAccountSummaryWithMemberCardPremierActivity(MOBMPAccountValidationRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId).ConfigureAwait(false);
            var response = new MOBMPAccountSummaryResponse();
            IDisposable timer = null;

            try
            {
                _logger.LogInformation("GetAccountSummaryWithMemberCardPremierActivity {@clientRequest}", JsonConvert.SerializeObject(request));

                using (timer = _logger.BeginTimedOperation("Total time taken for GetAccountSummaryWithMemberCardPremierActivity business call", transationId: request.TransactionId))
                {
                    response = await _mPAccountProfileBusiness.GetAccountSummaryWithMemberCardPremierActivity(request).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("GetAccountSummaryWithMemberCardPremierActivity Warning {@UnitedException} {@stacktrace}", uaex.Message, uaex.StackTrace);
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetAccountSummaryWithMemberCardPremierActivity Error {@errorMessage} {@stacktrace}", ex.Message, ex.StackTrace);
                response.Exception = new MOBException("99999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = (timer != null) ? ((TimedOperation)timer).GetElapseTime() : 0;
            _logger.LogInformation("GetAccountSummaryWithMemberCardPremierActivity {@clientResponse}", JsonConvert.SerializeObject(response));

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
