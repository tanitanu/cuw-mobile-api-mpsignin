using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Ebs.Logging.Enrichers;
using United.Mobile.DeviceInitialization.Domain;
using United.Mobile.Model;
using United.Mobile.Model.DeviceInitialization;
using United.Mobile.Model.Internal.Exception;
using United.Utility.Helper;
using United.Utility.Serilog;


namespace United.Mobile.DeviceInitialization.Api.Controllers
{
    [ApiController]
    [Route("deviceinitializationservice/api")]
    public class DeviceInitializationController : ControllerBase
    {
        private readonly ICacheLog<DeviceInitializationController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly IDeviceInitializationBusiness _deviceInitializationBusiness;
        private readonly IRequestEnricher _requestEnricher;

        public DeviceInitializationController(ICacheLog<DeviceInitializationController> logger
            , IConfiguration configuration
            , IHeaders headers
            , IDeviceInitializationBusiness deviceInitializationBusiness
            , IRequestEnricher requestEnricher
            )
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _deviceInitializationBusiness = deviceInitializationBusiness;
            _requestEnricher = requestEnricher;
            _requestEnricher.Add("Application", "United.Mobile.DeviceInitialization.Api");
            _requestEnricher.Add("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        }

        [HttpGet]
        [Route("InitializeServices/InsertPushTokenToDB")]
        public async Task<bool> InsertPushTokenToDB(string accessCode, string transactionId, string deviceId, string apnsDeviceId, int applicationId)
        {
            await _headers.SetHttpHeader(deviceId, applicationId.ToString(), string.Empty, transactionId, string.Empty, string.Empty).ConfigureAwait(false);
            bool response = false;
            try
            {
                _logger.LogInformation("InsertPushTokenToDB {AccessCode} {TransactionId} {DeviceId} {apnsDeviceId} {ApplicationId}", GeneralHelper.RemoveCarriageReturn(accessCode), GeneralHelper.RemoveCarriageReturn(transactionId), GeneralHelper.RemoveCarriageReturn(deviceId), GeneralHelper.RemoveCarriageReturn(apnsDeviceId), applicationId);
                using (_logger.BeginTimedOperation("Total time taken for InsertPushTokenToDB business call", transationId: Request.Headers[Constants.HeaderTransactionIdText].ToString()))
                {
                    response = await _deviceInitializationBusiness.InsertPushTokenToDB(accessCode, transactionId, deviceId, apnsDeviceId, applicationId).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("InsertPushTokenToDB Error {@UnitedException} {@StackTrace}", uaex.Message, uaex.StackTrace);
            }
            catch (Exception ex)
            {
                _logger.LogError("InsertPushTokenToDB Error {@Exception}, {@StackTrace}", ex.Message, ex.StackTrace);
            }

            _logger.LogInformation("RegisterDevice {@clientResponse}", response);

            return response;
        }

        [HttpGet]
        [Route("InitializeServices/RegisterDevice")]
        public async Task<DeviceResponse> RegisterDevice(string accessCode, string transactionId, string identifierForVendor, string name, string model, string localizedModel, string systemName, string systemVersion, string applicationId, string applicationVersion)
        {
            await _headers.SetHttpHeader(identifierForVendor, applicationId.ToString(), systemVersion, transactionId, string.Empty, string.Empty).ConfigureAwait(false);
            var response = new DeviceResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("RegisterDevice {AccessCode} {TransactionId} {IdentifierForVendor} {Name} {Model} {LocalizedModel} {SystemName} {SystemVersion} {ApplicationId} {AppVersion}", GeneralHelper.RemoveCarriageReturn(accessCode), GeneralHelper.RemoveCarriageReturn(transactionId), GeneralHelper.RemoveCarriageReturn(identifierForVendor), GeneralHelper.RemoveCarriageReturn(name), GeneralHelper.RemoveCarriageReturn(model), GeneralHelper.RemoveCarriageReturn(localizedModel), GeneralHelper.RemoveCarriageReturn(systemName), GeneralHelper.RemoveCarriageReturn(systemVersion), applicationId, GeneralHelper.RemoveCarriageReturn(applicationVersion));
                using (timer = _logger.BeginTimedOperation("Total time taken for RegisterDevice business call", transationId: transactionId))
                {
                    response = await _deviceInitializationBusiness.RegisterDevice(accessCode, transactionId, identifierForVendor, name, model, localizedModel, systemName, systemVersion, applicationId, applicationVersion).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("RegisterDevice Error {@UnitedException}, {@StackTrace}", uaex.Message, uaex.StackTrace);
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
                _logger.LogError("RegisterDevice Error {@Exception}, {@StackTrace}", ex.Message, ex.StackTrace);

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

            _logger.LogInformation("RegisterDevice {@clientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpGet]
        [Route("tool/LegalDocumentUpdateToCache")]
        public async Task<bool> LegalDocumentUpdateToCache(string key, string transactionId, bool IsForceInsert = false)
        {
            bool response = default;
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("LegalDocumentUpdateToCache {@key} {@TransactionId} {@IsForceInsert}", GeneralHelper.RemoveCarriageReturn(key), GeneralHelper.RemoveCarriageReturn(transactionId), IsForceInsert);
                using (timer = _logger.BeginTimedOperation("Total time taken for RegisterDevice business call", transationId: transactionId))
                {
                    response = await _deviceInitializationBusiness.LegalDocumentUpdateToCache(key, transactionId, IsForceInsert).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("LegalDocumentUpdateToCache Error {@UnitedException}, {@StackTrace}, {@transactionID}", GeneralHelper.RemoveCarriageReturn(uaex.Message), GeneralHelper.RemoveCarriageReturn(uaex.StackTrace), GeneralHelper.RemoveCarriageReturn(transactionId));
            }
            catch (Exception ex)
            {
                _logger.LogError("LegalDocumentUpdateToCache Error {@Exception}, {@StackTrace}, {@transactionID}", GeneralHelper.RemoveCarriageReturn(ex.Message), GeneralHelper.RemoveCarriageReturn(ex.StackTrace), GeneralHelper.RemoveCarriageReturn(transactionId));
            }

            _logger.LogInformation("LegalDocumentUpdateToCache {@clientResponse}, {@transactionID}", JsonConvert.SerializeObject(response), GeneralHelper.RemoveCarriageReturn(transactionId));

            return response;
        }

        [HttpGet]
        [Route("tool/MPHashPin")]
        public async Task<bool> FetchMPHashPin(string mpnumber, string deviceId, int applicationId, string transactionId)
        {
            bool response = default;
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("FetchMPHashPin {@mpnumber}, {@deviceid}, {@appid} {@transactionId}", GeneralHelper.RemoveCarriageReturn(mpnumber), GeneralHelper.RemoveCarriageReturn(deviceId), applicationId, GeneralHelper.RemoveCarriageReturn(transactionId));
                using (timer = _logger.BeginTimedOperation("Total time taken for RegisterDevice business call", transationId: transactionId))
                {
                    response = await _deviceInitializationBusiness.FetchMPHashPin(mpnumber, deviceId, applicationId, transactionId).ConfigureAwait(false);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("FetchMPHashPin Error {@UnitedException}, {@StackTrace}, {@transactionID}", GeneralHelper.RemoveCarriageReturn(uaex.Message), GeneralHelper.RemoveCarriageReturn(uaex.StackTrace), GeneralHelper.RemoveCarriageReturn(transactionId));
            }
            catch (Exception ex)
            {
                _logger.LogError("FetchMPHashPin Error {@Exception}, {@StackTrace}, {@transactionID}", GeneralHelper.RemoveCarriageReturn(ex.Message), GeneralHelper.RemoveCarriageReturn(ex.StackTrace), GeneralHelper.RemoveCarriageReturn(transactionId));
            }

            _logger.LogInformation("FetchMPHashPin {@clientResponse}, {@transactionID}", GeneralHelper.RemoveCarriageReturn(JsonConvert.SerializeObject(response)), GeneralHelper.RemoveCarriageReturn(transactionId));

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
