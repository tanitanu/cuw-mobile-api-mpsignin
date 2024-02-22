using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public class MPFutureFlightCredit : IMPFutureFlightCredit
    {
        private readonly ICacheLog<MPFutureFlightCredit> _logger;
        private readonly IResilientClient _resilientClient;

        public MPFutureFlightCredit(ICacheLog<MPFutureFlightCredit> logger, [KeyFilter("MyAccountFutureFlightCreditClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<T> GetMPFutureFlightCredit<T>(string token, string callsource, string mileagePlusNumber, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for GetMPFutureFlightCreditFromCancelReservationService service call", transationId: sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/xml; charset=utf-8"}
                     };

                string path = "?opId=" + (string.IsNullOrEmpty(callsource)
                    ? mileagePlusNumber
                    : string.Format("{0}&clientId={1}", mileagePlusNumber, callsource));

                _logger.LogInformation("GetMPFutureFlightCreditFromCancelReservationService {@path}", path);

                var mpFutureFlightData = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(false);

                if (mpFutureFlightData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("GetMPFutureFlightCreditFromCancelReservationService {@url} error {@cslResponse}", mpFutureFlightData.url, mpFutureFlightData.response);
                    if (mpFutureFlightData.statusCode != HttpStatusCode.BadRequest)
                    {
                        throw new Exception(mpFutureFlightData.response);
                    }
                }

                _logger.LogInformation("GetMPFutureFlightCreditFromCancelReservationService {@url} {@cslResponse}", mpFutureFlightData.url, mpFutureFlightData.response);

                return XmlSerializerHelper.Deserialize<T>(mpFutureFlightData.response);
            }
        }
    }
}
