using Microsoft.AspNetCore.Mvc;
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
using United.Mobile.Model.MPSignIn.CCE;
using United.Mobile.Model.MPSignIn.HashpinVerify;
using United.Mobile.MPSignInCommon.Domain;
using United.Service.Presentation.ReferenceDataModel;
using United.Utility.Helper;
using United.Utility.Serilog;

namespace United.Mobile.MPSignInCommon.Api.Controllers
{
    [Route("mpsignincommonservice/api/")]
    [ApiController]
    public class MPSignInCommonController : ControllerBase
    {
        private readonly ICacheLog<MPSignInCommonController> _logger;
        private readonly IHeaders _headers;
        private readonly IRequestEnricher _requestEnricher;
        private readonly IMPSignInCommonBusiness _mpsignincommonBusiness;
        private readonly IFeatureSettings _featureSettings;

        public MPSignInCommonController(
            ICacheLog<MPSignInCommonController> logger,
            IHeaders headers,
            IRequestEnricher requestEnricher,
            IMPSignInCommonBusiness mpsignincommonBusiness
            , IFeatureSettings featureSettings)
        {
            _logger = logger;
            _headers = headers;
            _requestEnricher = requestEnricher;
            _featureSettings = featureSettings;
            _mpsignincommonBusiness = mpsignincommonBusiness;
            _requestEnricher.Add("Application", "United.Mobile.MPSignInCommon.Api");
            _requestEnricher.Add("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        }


        [HttpGet]
        [Route("GetMileagePlusAndPinDP")]
        public async Task<MPTokenResponse> GetMileagePlusAndPinDP(string mileagePlusNumber, int applicationId, string deviceId, string appVersion, string hashPinCode, string serviceName)
        {
            string transactionId = Guid.NewGuid().ToString();
            await _headers.SetHttpHeader(deviceId, applicationId.ToString(), appVersion, transactionId, string.Empty, string.Empty).ConfigureAwait(false);
            _requestEnricher.Add("ServiceAction", serviceName);
            MPTokenResponse response = new MPTokenResponse();
            IDisposable timer = null;
            try
            {
                using (timer = _logger.BeginTimedOperation("Total time taken for GetMileagePlusAndPinDP business call", transationId: transactionId))
                {
                    response = await _mpsignincommonBusiness.GetMileagePlusAndPinDP(mileagePlusNumber, applicationId, deviceId, appVersion, hashPinCode, serviceName, transactionId).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("GetMileagePlusAndPinDP Error {@UnitedException}, {@StackTrace}", uaex.Message, uaex.StackTrace);
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (System.Exception ex)
            {
                _logger.LogError("GetMileagePlusAndPinDP Error {@Exception}, {@StackTrace}", ex.Message, ex.StackTrace);
                response.Exception = new MOBException("9999", ex.Message);
            }

            response.CallDuration = (timer != null) ? ((TimedOperation)timer).GetElapseTime() : 0;
            _logger.LogInformation("GetMileagePlusAndPinDP {@clientResponse} {transactionId}", JsonConvert.SerializeObject(response), transactionId);

            return response;
        }


        [HttpPost]
        [Route("VerifyMileagePlusHashpin")]
        public async Task<HashpinVerifyResponse> VerifyMileagePlusHashpin(HashpinVerifyRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.ApplicationId.ToString(), request.AppVersion, request.TransactionId, string.Empty, request.SessionID).ConfigureAwait(false);

            var response = new HashpinVerifyResponse();
            _requestEnricher.Add("ServiceAction", request.ServiceName);
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("VerifyMileagePlusHashpin {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for VerifyMileagePlusHashpin business call", transationId: request.TransactionId))
                {
                    response = await _mpsignincommonBusiness.GetMPRecord(request).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("VerifyMileagePlusHashpin Error {@UnitedException}, {@StackTrace}", uaex.Message, uaex.StackTrace);
                response.Exception = new MOBException()
                {
                    Message = uaex.Message,
                    Code = "999"
                };
            }
            catch (System.Exception ex)
            {
                _logger.LogError("VerifyMileagePlusHashpin Error {@Exception}, {@StackTrace}", ex.Message, ex.StackTrace);
                response.Exception = new MOBException()
                {
                    Message = ex.Message,
                    Code = "999"
                };
            }

            response.CallDuration = (timer != null) ? ((TimedOperation)timer).GetElapseTime() : 0;
            _logger.LogInformation("VerifyMileagePlusHashpin {@ClientResponse} {transactionId}", JsonConvert.SerializeObject(response), request.TransactionId);

            return response;

        }

        [HttpPost]
        [Route("IsAccountExist")]
        public async Task<CCEResponse> IsAccountExist(CCERequest request)
        {
            await _headers.SetHttpHeader(string.Empty, string.Empty, string.Empty, request.TransactionId, string.Empty, string.Empty).ConfigureAwait(false);

            var response = new CCEResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("IsAccountExist {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for IsAccountExist business call", transationId: request.TransactionId))
                {
                    response = await _mpsignincommonBusiness.IsAccountExist(request).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("IsAccountExist Error {@UnitedException}, {@StackTrace}", uaex.Message, uaex.StackTrace);
                response.Exception = new MOBException()
                {
                    Message = uaex.Message,
                    Code = "999"
                };
            }
            catch (System.Exception ex)
            {
                _logger.LogError("IsAccountExist Error {@Exception}, {@StackTrace}", ex.Message, ex.StackTrace);
                response.Exception = new MOBException()
                {
                    Message = ex.Message,
                    Code = "999"
                };
            }

            response.CallDuration = (timer != null) ? ((TimedOperation)timer).GetElapseTime() : 0;
            _logger.LogInformation("IsAccountExist {@ClientResponse} {transactionId}", JsonConvert.SerializeObject(response), request.TransactionId);

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
