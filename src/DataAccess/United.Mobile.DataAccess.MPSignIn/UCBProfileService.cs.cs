using Autofac.Features.AttributeFilters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.MPSignIn
{
    public class UCBProfileService : IUCBProfileService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<UCBProfileService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        public UCBProfileService([KeyFilter("UCBProfileClientKey")] IResilientClient resilientClient, 
            ICacheLog<UCBProfileService> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _resilientClient = resilientClient;
            _logger = logger;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetAllTravellers(string token, string mileagePlusNumber, string sessionId)
        {
            IDisposable timer = null;

            using (timer = _logger.BeginTimedOperation("Total time taken for GetAllTravellers service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token },
                          {"Accept","application/json" }
                     };
                string path = string.Format("/traveler/alltravelers/loyaltyid/{0}", mileagePlusNumber);
                _logger.LogInformation("GetAllTravellers {@path}", GeneralHelper.RemoveCarriageReturn(path));
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(false);
                _logger.LogInformation("GetAllTravellers {@url}", responseData.url);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetAllTravellers {@url} error {@cslResponse} {@mileagePlusNumber}", responseData.url, responseData.response, mileagePlusNumber);
                }
                else
                {
                    _logger.LogInformation("GetAllTravellers {@url} {@cslResponse} {@mileagePlusNumber}", responseData.url, responseData.response, mileagePlusNumber);
                }
                return responseData.response;
            }
        }

        public async Task<string> GetProfileOwner(string token, string mileagePlusNumber, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for GetProfileOwner service call", sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token },
                          {"Accept","application/json" }
                     };
                string path = string.Format("/profileowner/api/loyaltyId/{0}", mileagePlusNumber);
                _logger.LogInformation("GetProfileOwner {@path}", GeneralHelper.RemoveCarriageReturn(path));
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetProfileOwner {@RequestUrl} error {@cslResponse} {@mileagePlusNumber}", responseData.url, responseData.response, mileagePlusNumber);
                }
                else
                {
                    _logger.LogInformation("GetProfileOwner {@RequestUrl} {@cslResponse} {@mileagePlusNumber}", responseData.url, responseData.response, mileagePlusNumber);
                }
                return responseData.response;
            }
        }

        public async Task<string> GetCreditCardsData(string token, string mileagePlusNumber, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for GetCreditCardsData service call", sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token },
                          {"Accept","application/json" }
                     };
                string path = string.Format("/creditcards/loyaltyId/{0}", mileagePlusNumber);
                _logger.LogInformation("GetCreditCardsData {@path}", GeneralHelper.RemoveCarriageReturn(path));
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetCreditCardsData {@url} error {@cslResponse} {@mileagePlusNumber}", responseData.url, responseData.response, mileagePlusNumber);
                }
                else
                {
                    _logger.LogInformation("GetCreditCardsData {@url} {@cslResponse} {@mileagePlusNumber}", responseData.url, responseData.response, mileagePlusNumber);
                }
                return responseData.response;
            }
        }

        public async Task<string> GetLoginSecurityUpdate(string token, string mileagePlusNumber, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for GetLoginSecurityUpdate service call", sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token },
                          {"Accept","application/json" }
                     };
                string path = string.Format("/login/securityupdate/loyaltyId/{0}", mileagePlusNumber);
                _logger.LogInformation("GetLoginSecurityUpdate {@path}", path);
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetLoginSecurityUpdate {@url} error {@cslResponse} {@mileagePlusNumber}", responseData.url, responseData.response, mileagePlusNumber);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                    {
                        throw new Exception(responseData.response);
                    }
                }

                _logger.LogInformation("GetLoginSecurityUpdate {@url} {@cslResponse} {@mileagePlusNumber}", responseData.url, responseData.response, mileagePlusNumber);
                return responseData.response;
            }
        }

        public async Task<string> GetEmail(string token, string mpNumber)
        {
            var headers = new Dictionary<string, string>
                {
                    {
                        "Authorization",token
                    }
                };
            Tuple<string, HttpStatusCode, string> HttpResponse;
            using (_logger.BeginTimedOperation("Total time taken for Service call GetEmail", transationId: _httpContextAccessor.HttpContext.Request.Headers[United.Mobile.Model.Constants.HeaderTransactionIdText].ToString()))
            {
                _logger.LogInformation("csl-Profile-GetEmail-service {requestData}, url {requestUrl} and {headers}", mpNumber, $"Email/Loyaltyid/{mpNumber}/v1", headers);
                string ucbVersion = _configuration.GetSection("MemberProfileClient").GetValue<string>("ucbVersion");
                HttpResponse = await _resilientClient.GetHttpAsync($"/contactpoints/Email/Loyaltyid/{mpNumber}/{ucbVersion}?verification=true", headers).ConfigureAwait(false);
            }
            var response = HttpResponse.Item1;
            var statusCode = HttpResponse.Item2;
            var url = HttpResponse.Item3;
            if (statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("csl-Profile-GetEmail-service {requestData}, url {requestUrl} error {response} and {headers}", mpNumber, url, response, headers);
            }
            else
            {
                _logger.LogInformation("csl-Profile-GetEmail-service {requestData}, url {requestUrl}, {response} and {headers}", mpNumber, url, response, headers);
            }
            if ((statusCode != HttpStatusCode.OK) && (statusCode != HttpStatusCode.BadRequest))
            {
                throw new Exception(response);
            }
            return response;
        }

    }
}
