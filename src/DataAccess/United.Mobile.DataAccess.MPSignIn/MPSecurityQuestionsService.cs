using Autofac.Features.AttributeFilters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;
using United.Utility.Serilog;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public class MPSecurityQuestionsService : IMPSecurityQuestionsService
    {
        private readonly ICacheLog<MPSecurityQuestionsService> _logger;
        private readonly IResilientClient _resilientClient;

        public MPSecurityQuestionsService(ICacheLog<MPSecurityQuestionsService> logger, [KeyFilter("MPSecurityQuestionsClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<string> GetMPPINPWDSecurityQuestions(string token, string requestData, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for GetMPPINPWDSecurityQuestions service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                      {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

                string path = "/GetAllSecurityQuestions";
                _logger.LogInformation("GetMPPINPWDSecurityQuestions-service {@request} {@path}", requestData, path);
                var mpSecurityQuestions = await _resilientClient.PostHttpAsyncWithOptions(path, requestData).ConfigureAwait(false);

                if (mpSecurityQuestions.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetMPPINPWDSecurityQuestions-service {@url} error {@cslResponse}", mpSecurityQuestions.url, mpSecurityQuestions.response);
                    if (mpSecurityQuestions.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(mpSecurityQuestions.response);
                }

                _logger.LogInformation("GetMPPINPWDSecurityQuestions-service {@url} {@cslResponse}", mpSecurityQuestions.url, mpSecurityQuestions.response);

                return mpSecurityQuestions.response;
            }
        }
        public async Task<string> ValidateSecurityAnswer(string token, string request, string sessionId, string path)
        {
            IDisposable timer = null;
            string returnValue = string.Empty;
            string callTime4Tuning = string.Empty;

            Stopwatch cslCallDurationstopwatch1;
            cslCallDurationstopwatch1 = new Stopwatch();
            cslCallDurationstopwatch1.Reset();
            cslCallDurationstopwatch1.Start();

            using (timer = _logger.BeginTimedOperation("Total time taken for ValidateSecurityAnswer service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json;charset=utf-8"},
                          { "Authorization", token }
                     };
                _logger.LogInformation("ValidateSecurityAnswer-service {@request} {@path}", request, path);

                var vSecurityQuestions = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers).ConfigureAwait(false);
                returnValue = vSecurityQuestions.response;

                if (vSecurityQuestions.statusCode != HttpStatusCode.OK)
                    _logger.LogError("ValidateSecurityAnswer-service {@url} error {@cslResponse}", vSecurityQuestions.url, returnValue);
                else
                    _logger.LogInformation("ValidateSecurityAnswer {@url} {@cslResponse}", vSecurityQuestions.url, returnValue);
            }
            if (cslCallDurationstopwatch1.IsRunning)
            {
                cslCallDurationstopwatch1.Stop();
            }

            callTime4Tuning = (cslCallDurationstopwatch1.ElapsedMilliseconds / (double)1000).ToString();
            _logger.LogInformation("ValidateSecurityAnswer Service call duration {@duration}", callTime4Tuning + "seconds");

            return returnValue;
        }

        public async Task<string> AddDeviceAuthentication(string token, string requestData, string sessionId, string path)
        {
            using (_logger.BeginTimedOperation("Total time taken for AddDeviceAuthentication service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token }
                     };
                _logger.LogInformation("AddDeviceAuthentication-service {@request} {@path}", requestData, path);

                var mpSecurityQuestions = await _resilientClient.PostHttpAsyncWithOptions(path, requestData, headers).ConfigureAwait(false);

                if (mpSecurityQuestions.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("AddDeviceAuthentication-service {@url} error {@cslResponse}", mpSecurityQuestions.url, mpSecurityQuestions.response);
                    if (mpSecurityQuestions.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(mpSecurityQuestions.response);
                }

                _logger.LogInformation("AddDeviceAuthentication-service {@url} {@cslResponse}", mpSecurityQuestions.url, mpSecurityQuestions.response);

                return mpSecurityQuestions.response;
            }
        }
    }
}
