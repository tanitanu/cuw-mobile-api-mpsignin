using Autofac.Features.AttributeFilters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Http;
using Microsoft.Extensions.Configuration;
using United.Utility.Helper;

namespace United.Common.Helper.Profile
{
    public class ContactPointService : IContactPointService
    {
        private readonly ICacheLog<ContactPointService> _logger;
        private readonly IResilientClient _contactPointClient;

        public ContactPointService(
            ICacheLog<ContactPointService> logger,
            [KeyFilter("contactPointConfigKey")] IResilientClient contactPointClient)
        {
            _contactPointClient = contactPointClient;
            _logger = logger;
        }

        public async Task<string> GetPrimaryEmail(string token, string mpNumber, string transactionId)
        {
            var headers = new Dictionary<string, string>
                {
                    {
                        "Authorization",token
                    }
                };
            Tuple<string, HttpStatusCode, string> HttpResponse;
            using (_logger.BeginTimedOperation("Total time taken for Service call GetPrimaryEmail", transationId: transactionId))
            {
                _logger.LogInformation("csl-Profile-GetPrimaryEmail-service {requestData}, url {requestUrl} and {headers}", mpNumber, 
                    $"Email/Loyaltyid/{mpNumber}?primary", headers);              
                HttpResponse = await _contactPointClient.GetHttpAsync($"Email/Loyaltyid/{mpNumber}?primary", headers).ConfigureAwait(false);                
            }
            var response = HttpResponse.Item1;
            var statusCode = HttpResponse.Item2;
            var url = HttpResponse.Item3;
            if (statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("csl-Profile-GetPrimaryEmail-service {requestData}, url {requestUrl} error {response} and {headers}", mpNumber, url, response, headers);
            }
            else
            {
                _logger.LogInformation("csl-Profile-GetPrimaryEmail-service {requestData}, url {requestUrl}, {response} and {headers}", mpNumber, url, response, headers);
            }
            if ((statusCode != HttpStatusCode.OK) && (statusCode != HttpStatusCode.BadRequest))
            {
                throw new Exception(response);
            }
            return response;
        }
    }
}