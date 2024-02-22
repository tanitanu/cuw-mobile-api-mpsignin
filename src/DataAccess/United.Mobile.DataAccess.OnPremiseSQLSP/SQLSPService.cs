using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Definition;
using United.Mobile.Model.DeviceInitialization;
using United.Mobile.Model.MPSignIn.CCE;
using United.Service.Presentation.PersonalizationModel;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.OnPremiseSQLSP
{
    public class SQLSPService : ISQLSPService
    {
        private readonly ICacheLog<SQLSPService> _logger;
        private readonly IResilientClient _resilientClient;
        
        public SQLSPService(ICacheLog<SQLSPService> logger, [KeyFilter("OnPremSqlClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<T> RegisterDevice<T>(DeviceRequest requestData, string transId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };
           string requestUrl =  string.Format("/DeviceInitialization/RegisterDevice?accessCode={0}&transactionId={1}&identifierForVendor={2}&name={3}&model={4}&localizedModel={5}&systemName={6}&systemVersion={7}&applicationId={8}&applicationVersion={9}",
                requestData.AccessCode, requestData.TransactionId, requestData.IdentifierForVendor, requestData.Name, requestData.Model, requestData.LocalizedModel, requestData.SystemName, requestData.SystemVersion, requestData.ApplicationId, requestData.ApplicationVersion);
            using (_logger.BeginTimedOperation("Total time taken for RegisterDevice call", transationId: transId))
            {
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(requestUrl, headers).ConfigureAwait(false);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("RegisterDevice {requestUrl} error {response}", responseData.url, responseData.response);
                    if (responseData.statusCode == HttpStatusCode.NotFound)
                        return default;
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("RegisterDevice {requestUrl} info {response}", responseData.url, responseData.response);

                return JsonConvert.DeserializeObject<T>(responseData.response);
            }
        }

        public async Task<bool> InsertDevicePushToken(string deviceId, int applicationId, string pushToken, string transactionid)
        {
            using (_logger.BeginTimedOperation("Total time taken for InsertDevicePushToken service call", transationId: transactionid))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                {
                    {"Accept","application/json" }
                };

                 string requestData = $"/DeviceInitialization/InsertDevicePushToken?deviceId={deviceId}&applicationId={applicationId}&pushToken={pushToken}&transactionid={transactionid}";
                _logger.LogInformation("InsertDevicePushToken service {@pushToken}", pushToken);

                var responseData = await _resilientClient.GetHttpAsyncWithOptions(requestData, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("InsertDevicePushToken service {@RequestUrl} error {@Response}", responseData.url, responseData.response);
                }

                _logger.LogInformation("InsertDevicePushToken service {@RequestUrl} {@Response}", responseData.url, responseData.response);

                return string.IsNullOrEmpty(responseData.response) ? false : Convert.ToBoolean(responseData.response);
            }
        }

        public async Task<List<MOBLegalDocument>> GetNewLegalDocumentsForTitles(string titles, string transactionId, bool isTermsnConditions)
        {
            if (titles == null)
            {
                _logger.LogError("GetNewLegalDocumentsForTitles titles is null.");
                return default;
            }

            using (_logger.BeginTimedOperation("Total time taken for GetNewLegalDocumentsForTitles OnPrem service call", transationId: transactionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"}
                     };

                var requestObj = string.Format("/LegalDocument/GetNewLegalDocumentsForTitles?Titles={0}&transactionId={1}&isTermsnConditions={2}", titles, transactionId, isTermsnConditions);

                _logger.LogInformation("GetNewLegalDocumentsForTitles-OnPrem Service {@RequestObj}", requestObj);

                var responseData = await _resilientClient.GetHttpAsyncWithOptions(requestObj, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetNewLegalDocumentsForTitles-OnPrem Service {@RequestUrl} error {@Response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest && responseData.statusCode != HttpStatusCode.NoContent)
                        throw new Exception(null, new Exception(responseData.response));
                }

                _logger.LogInformation("GetNewLegalDocumentsForTitles-OnPrem Service {@RequestUrl} {@Response}", responseData.url, responseData.response);

                return JsonConvert.DeserializeObject<List<MOBLegalDocument>>(responseData.response);
            }
        }

        public async Task<T> ValidateHashPin<T>(string accountNumber, string hashPinCode, int applicationId, string deviceId, string appVersion, string transactionId, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for ValidateHashPin service call", sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept","application/json" }
                     };
                string requestData = string.Format("/ValidateHashPin/ValidateHashPinAndGetAuthToken?accountNumber={0}&hashPinCode={1}&applicationId={2}&deviceId={3}&appVersion={4}&transactionId={5}&sessionId={6}", accountNumber, hashPinCode, applicationId, deviceId, appVersion, transactionId, sessionId);
                _logger.LogInformation("ValidateHashPin service {@RequestData}", requestData);
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(requestData, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("ValidateHashPin service {@RequestUrl} error {@Response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("ValidateHashPin service {@RequestUrl} {@Response}", responseData.url, responseData.response);

                return JsonConvert.DeserializeObject<T>(responseData.response);
            }
        }

        public async Task<T> GetAuthToken<T>(string accountNumber, string hashPinCode, int applicationId, string deviceId, string appVersion, string transactionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for GetAuthToken service call", transactionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept","application/json" }
                     };
                string requestData = string.Format("/MPSignIn/MPAuthToken?mpNumber={0}&hashPinCode={1}&applicationId={2}&deviceId={3}&appVersion={4}&transactionId={5}", accountNumber, hashPinCode, applicationId, deviceId, appVersion, transactionId);
                _logger.LogInformation("GetAuthToken service {@RequestData}", requestData);
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(requestData, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetAuthToken service {@RequestUrl} error {@Response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("GetAuthToken service {@RequestUrl} {@Response}", responseData.url, responseData.response);

                return JsonConvert.DeserializeObject<T>(responseData.response);
            }
        }

        public async Task<T> GetMPRecord<T>(string accountNumber, int applicationId, string deviceId, string hashPinCode, string appVersion, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for GetMPRecord service call", sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept","application/json" }
                     };
                string requestData = string.Format("/MPSignIn/MPRecord?mpNumber={0}&hashValue={1}&applicationId={2}&deviceId={3}&appVersion={4}&transactionId={5}", accountNumber, hashPinCode, applicationId, deviceId, appVersion, sessionId);
                _logger.LogInformation("GetMPRecord service {@RequestData}", requestData);
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(requestData, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetMPRecord service {@RequestUrl} error {@Response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("GetMPRecord service {@RequestUrl} {@Response}", responseData.url, responseData.response);

                return JsonConvert.DeserializeObject<T>(responseData.response);
            }
        }

        public async Task<bool> InsertMileagePlusAndHash(string mileagePlusNumber, string hashValue, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for InsertUpdateMPCSSValidationDetails service call", sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                {
                    {"Accept","application/json" }
                };
                string requestData = string.Format("/MPSignIn/InsertMileagePlusAndHash?mpNumber={0}&hashValue={1}&sessionId={2}", mileagePlusNumber, hashValue, sessionId);
                StaticLog.Information(_logger, "InsertMileagePlusAndHash service {@requestData} ", requestData);

                var responseData = await _resilientClient.GetHttpAsyncWithOptions(requestData, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning("InsertMileagePlusAndHash SQLDB Service {requestUrl} error {@response} ", responseData.url, responseData.response);
                    //if (responseData.statusCode != HttpStatusCode.BadRequest)
                    //    throw new Exception(responseData.response);
                }

                _logger.LogInformation("InsertMileagePlusAndHash service {@RequestUrl} {@Response}", responseData.url, responseData.response);

                return string.IsNullOrEmpty(responseData.response) ? false : Convert.ToBoolean(responseData.response);
            }
        }

        public async Task<bool> InsertUpdateMileagePlusAndPinDP(string requestData, bool iSDPAuthentication, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for InsertUpdateMileagePlusAndPinDP service call", sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                {
                    {"Accept","application/json" }
                };

                _logger.LogInformation("InsertUpdateMileagePlusAndPinDP service {@RequestData}", requestData);
                string endPoint = string.Format("/MPSignIn/InsertUpdateMileagePlusAndPinDP?iSDPAuthentication={0}", iSDPAuthentication);

                var responseData = await _resilientClient.PostHttpAsyncWithOptions(endPoint, requestData, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning("InsertUpdateMileagePlusAndPinDP SQLDB Service {requestUrl} error {@response}", responseData.url, responseData.response);
                    //if (responseData.statusCode != HttpStatusCode.BadRequest)
                    //    throw new Exception(responseData.response);
                }

                _logger.LogInformation("InsertUpdateMileagePlusAndPinDP service {@RequestUrl} {@Response}", responseData.url, responseData.response);

                return string.IsNullOrEmpty(responseData.response) ? false : Convert.ToBoolean(responseData.response);
            }
        }

        public async Task<bool> IsTSAFlaggedAccount(string profileNumber, string sessionid)
        {
            using (_logger.BeginTimedOperation("Total time taken for IsTSAFlaggedAccount service call", transationId: sessionid))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                {
                    {"Accept","application/json" }
                };

                _logger.LogInformation("IsTSAFlaggedAccount service {@profileNumber}", profileNumber);
                string endPoint = string.Format("/MPSignIn/IsTSAFlaggedAccount?accountNumber={0}&SessionId={1}", profileNumber, sessionid);

                var responseData = await _resilientClient.GetHttpAsyncWithOptions(endPoint, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning("IsTSAFlaggedAccount SQLDB Service {requestUrl} error {@response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("IsTSAFlaggedAccount service {@RequestUrl} {@Response}", responseData.url, responseData.response);

                return string.IsNullOrEmpty(responseData.response) ? false : Convert.ToBoolean(responseData.response);
            }
        }

        public async Task<bool> IsEIsEResBetaTester(int applicationId, string applicationVersion, string profileNumber, string sessionid)
        {
            using (_logger.BeginTimedOperation("Total time taken for IsEIsEResBetaTester service call", transationId: sessionid))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                {
                    {"Accept","application/json" }
                };
                
                StaticLog.Information(_logger, "IsEIsEResBetaTester service {@profileNumber}", profileNumber);
                string endPoint = string.Format("/MPSignIn/IsEIsEResBetaTester?applicationId={0}&applicationVersion={1}&mileagePlusNumber={2}&sessionid={3}", applicationId, applicationVersion, profileNumber, sessionid);

                var responseData = await _resilientClient.PostHttpAsyncWithOptions(endPoint, profileNumber, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning("IsEIsEResBetaTester SQLDB Service {requestUrl} error {@response} ", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("IsEIsEResBetaTester service {@RequestUrl} {@Response}", responseData.url, responseData.response);

                return string.IsNullOrEmpty(responseData.response) ? false : Convert.ToBoolean(responseData.response);
            }
        }

        public async Task<bool> IsVBQWMDisplayed(int applicationId, string deviceid, string number, string sessionid)
        {
            using (_logger.BeginTimedOperation("Total time taken for IsVBQWMDisplayed service call", transationId: sessionid))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                {
                    {"Accept","application/json" }
                };

                StaticLog.Information(_logger, "IsVBQWMDisplayed service {@profileNumber}", number);
                string endPoint = string.Format("/MPSignIn/IsVBQWMDisplayed?applicationId={0}&deviceid={1}&mileagePlusNumber={2}&sessionid={3}", applicationId, deviceid, number, sessionid);

                var responseData = await _resilientClient.PostHttpAsyncWithOptions(endPoint, number, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning("IsVBQWMDisplayed SQLDB Service {requestUrl} error {@response} ", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("IsVBQWMDisplayed service {@RequestUrl} {@Response}", responseData.url, responseData.response);

                return string.IsNullOrEmpty(responseData.response) ? false : Convert.ToBoolean(responseData.response);
            }
        }

        public async Task<string> GetMPHashPin(string mpnumber, string deviceId, int applicationId, string transactionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for GetMPHashPin service call", transationId: transactionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                {
                    {"Accept","application/json" }
                };

                _logger.LogInformation("GetMPHashPin service {@mileagePlusNumber}", GeneralHelper.RemoveCarriageReturn(mpnumber));
                string endPoint = $"/MPSignIn/HashPinWithMP?applicationId={applicationId}&deviceid={deviceId}&mileagePlusNumber={mpnumber}&sessionid={transactionId}";

                var responseData = await _resilientClient.GetHttpAsyncWithOptions(endPoint, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning("GetMPHashPin SQLDB Service {requestUrl} error {@response} for {transactionId}", GeneralHelper.RemoveCarriageReturn(responseData.url), GeneralHelper.RemoveCarriageReturn(responseData.response), GeneralHelper.RemoveCarriageReturn(transactionId));
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("GetMPHashPin service {@RequestUrl} {@Response}", responseData.url, responseData.response);

                return responseData.response;
            }
        }

        public async Task<bool> InsertMilagePlusDevice(string deviceId, string applicationId, string profileNumber, string customerID, string sessionid)
        {
            using (_logger.BeginTimedOperation("Total time taken for InsertMilagePlusDevice service call", sessionid))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                {
                    {"Accept","application/json" }
                };
                string requestData = string.Format("/MPSignIn/InsertMilagePlusDevice?deviceId={0}&applicationId={1}&mpNumber={2}&customerID={3}&sessionId={4}", deviceId, applicationId, profileNumber, customerID, sessionid);
                
                StaticLog.Information(_logger, "InsertMilagePlusDevice service {@profileNumber} {@customerID}", profileNumber, customerID);
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(requestData, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning("InsertMilagePlusDevice SQLDB Service {requestUrl} error {@response}", responseData.url, responseData.response);
                    //if (responseData.statusCode != HttpStatusCode.BadRequest)
                    //    throw new Exception(responseData.response);
                }

                _logger.LogInformation("InsertMilagePlusDevice service {@RequestUrl} {@Response}", responseData.url, responseData.response);

                return string.IsNullOrEmpty(responseData.response) ? false : Convert.ToBoolean(responseData.response);
            }
        }

        public async Task<T> IsAccountExist<T>(string clientId, string profileNumber, string transactionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for IsAccountExist service call", transationId: transactionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                {
                    {"Accept","application/json" }
                };

                _logger.LogInformation("IsAccountExist service {@profileNumber} and {@transactionId}", profileNumber, transactionId);
                string endPoint = string.Format("/CCE/IsAccountExist");
                var data = new CCERequest { ClientId = clientId, MileagePlusNumber = profileNumber, TransactionId = transactionId };
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(endPoint, JsonConvert.SerializeObject(data), headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning("IsAccountExist SQLDB Service {requestUrl} error {@response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("IsAccountExist service {@RequestUrl} {@Response}", responseData.url, responseData.response);

                return JsonConvert.DeserializeObject<T>(responseData.response);
            }
        }
    }
}
