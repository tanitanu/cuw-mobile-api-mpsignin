using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public class LoyaltyUCBService : ILoyaltyUCBService
    {
        private readonly ICacheLog<LoyaltyUCBService> _logger;
        private readonly IResilientClient _resilientClient;

        public LoyaltyUCBService([KeyFilter("LoyaltyUCBClientKey")] IResilientClient resilientClient, ICacheLog<LoyaltyUCBService> logger)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<string> GetLoyaltyBalance(string token, string mpnumber, string sessionId)
        {
            try
            {
                _logger.LogInformation("GetLoyaltyBalance request {@mpnumber}", GeneralHelper.RemoveCarriageReturn(mpnumber));

                using (_logger.BeginTimedOperation("Total time taken for GetLoyaltyBalance service call", transationId: sessionId))
                {
                    Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token }
                     };
                    string path = string.Format("/LoyaltyID/{0}", mpnumber);
                    var glbData = await _resilientClient.GetHttpAsyncWithOptions(path, headers,true).ConfigureAwait(false);
                    _logger.LogInformation("GetLoyaltyBalance {@url}", glbData.url);

                    if (glbData.statusCode == HttpStatusCode.OK)
                    {
                        _logger.LogInformation("GetLoyaltyBalance {@requestUrl} and {@response}", glbData.url, glbData.response);
                        return glbData.response;
                    }
                        
                    _logger.LogError("GetLoyaltyBalance {@requestUrl} error {@response}", glbData.url, glbData.statusCode);
                }
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    var response = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                    var errorResponse = GeneralHelper.RemoveCarriageReturn(response);
                    _logger.LogError("GetLoyaltyBalance WebException {@errorResponse}", errorResponse);
                }
            }

            return default;
        }
    }
}
