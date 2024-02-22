using Autofac.Features.AttributeFilters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public class LoyaltyAWSService : ILoyaltyAWSService
    {
        private readonly ICacheLog<LoyaltyAWSService> _logger;
        private readonly IResilientClient _resilientClient;
        private readonly IConfiguration _configuration;

        public LoyaltyAWSService(
            ICacheLog<LoyaltyAWSService> logger, 
            [KeyFilter("LoyaltyAWSClientKey")] IResilientClient resilientClient, 
            IConfiguration configuration)
        {
            _logger = logger;
            _resilientClient = resilientClient;
            _configuration = configuration;
        }

        public async Task<string> OneClickEnrollment(string token, string requestData, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for OneClickEnrollment service call", sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token }
                     };
                _logger.LogInformation("OneClickEnrollment Service {@cslRequest}", requestData);

                var enrollmentData = await _resilientClient.PostHttpAsyncWithOptions("/mp/enroll/", requestData, headers).ConfigureAwait(false);

                if (enrollmentData.statusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation("OneClickEnrollment-service {@url} {@cslResponse}", enrollmentData.url, enrollmentData.response);
                    return enrollmentData.response;
                }

                _logger.LogError("OneClickEnrollment-service {@url} error {@cslResponse}", enrollmentData.url, enrollmentData.response);

                if (enrollmentData.response.Contains("400.93") || enrollmentData.response.Contains("Duplicate account(s) found"))
                {
                    throw new WebException(enrollmentData.response, WebExceptionStatus.ReceiveFailure);
                }
                else if (enrollmentData.response.Contains("500.102")) 
                {
                    throw new WebException(enrollmentData.response, WebExceptionStatus.Timeout);
                }
                else if (_configuration.GetValue<bool>("ShowFullEnrollmentErrorPopUp"))
                {
                    throw new WebException(enrollmentData.response, WebExceptionStatus.ReceiveFailure);
                }

                throw new Exception(enrollmentData.response);
            }
        }

        public async Task<string> SendActivationEmail(string token, string requestData, string deviceId, string transactionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for Loyalty Send Activation Email service call"))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Authorization", token);

                _logger.LogInformation("Loyalty Send Activation Email Service {@cslRequest}", requestData);

                var ravenResult = await _resilientClient.PostHttpAsyncWithOptions("/mp/enroll/sendemail/activate", requestData, headers).ConfigureAwait(false);

                if (ravenResult.statusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation("Loyalty Send Activation Email {@url} {@cslResponse} {@DeviceId} {@TransactionId}", ravenResult.url, ravenResult.response, deviceId, transactionId);
                    return ravenResult.response;
                }

                _logger.LogError("Loyalty Send Activation Email {@url} error {@cslResponse}", ravenResult.url, ravenResult);

                if (ravenResult.response.Contains("400.134"))
                {
                    throw new WebException(ravenResult.response, WebExceptionStatus.ReceiveFailure);
                }
                else if (ravenResult.response.Contains("500.102"))
                {
                    throw new WebException(ravenResult.response, WebExceptionStatus.Timeout);
                }
                throw new Exception(ravenResult.response);
            }
        }
    }
}
