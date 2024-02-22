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
using United.Mobile.Model.MPRewards;
using United.Mobile.MPRewards.Domain;
using United.Service.Presentation.ReferenceDataModel;
using United.Utility.Helper;
using United.Utility.Serilog;

namespace United.Mobile.MPRewards.Api.Controllers
{
    [Route("mprewardsservice/api")]
    [ApiController]
    public class MPRewardsController : ControllerBase
    {
        private readonly ICacheLog<MPRewardsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly IMPRewardsBusiness _mPRewardsBusiness;
        private readonly IRequestEnricher _requestEnricher;
        private readonly IFeatureSettings _featureSettings;


        public MPRewardsController(ICacheLog<MPRewardsController> logger
            , IConfiguration configuration
            , IHeaders headers
            , IMPRewardsBusiness mPRewardsBusiness
            , IRequestEnricher requestEnricher
            , IFeatureSettings featureSettings)
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _mPRewardsBusiness = mPRewardsBusiness;
            _requestEnricher = requestEnricher;
            _featureSettings = featureSettings;
            _requestEnricher.Add("Application", "United.Mobile.MPRewards.Api");
            _requestEnricher.Add("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        }

        [HttpPost]
        [Route("MileagePlus/GetCancelFFCPnrsByMPNumber")]
        public async Task<MOBCancelFFCPNRsByMPNumberResponse> GetCancelFFCPnrsByMPNumber(MOBCancelFFCPNRsByMPNumberRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId).ConfigureAwait(false);
            var response = new MOBCancelFFCPNRsByMPNumberResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("GetCancelFFCPnrsByMPNumber {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for GetCancelFFCPnrsByMPNumber business call", transationId: request.TransactionId))
                {
                    response = await _mPRewardsBusiness.GetCancelFFCPnrsByMPNumber(request).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("GetCancelFFCPnrsByMPNumber Error {@UnitedException}, {@StackTrace}", uaex.Message, uaex.StackTrace);
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
                _logger.LogError("GetCancelFFCPnrsByMPNumber Error {@Exception}, {@StackTrace}", ex.Message, ex.StackTrace);

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
            _logger.LogInformation("GetCancelFFCPnrsByMPNumber {@clientResponse} {transactionId}", JsonConvert.SerializeObject(response), request.TransactionId);
            return response;
        }


        [HttpPost]
        [Route("MileagePlus/GetAccountPlusPointsDetails")]
        public async Task<MOBMPPlusPointsResponse> GetAccountPlusPointsDetails(MOBMPPlusPointsRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId).ConfigureAwait(false);
            var response = new MOBMPPlusPointsResponse();
            IDisposable timer = null;
            try
            {
                using (timer = _logger.BeginTimedOperation("Total time taken for GetAccountPlusPointsDetails business call", transationId: request.TransactionId))
                {
                    response = await _mPRewardsBusiness.GetAccountPlusPointsDetails(request).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("GetAccountPlusPointsDetails {@UnitedException}, {@StackTrace}", uaex.Message, uaex.StackTrace);
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetAccountPlusPointsDetails {@Exception}, {@StackTrace}", ex.Message, ex.StackTrace);
                response.Exception = new MOBException("10000", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = (timer != null) ? ((TimedOperation)timer).GetElapseTime() : 0;
            _logger.LogInformation("GetAccountPlusPointsDetails {@clientResponse} {transactionId}", JsonConvert.SerializeObject(response), request.TransactionId);

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
                // Suppress any exceptions//
            }

            return "Unknown";
        }
    }
}
