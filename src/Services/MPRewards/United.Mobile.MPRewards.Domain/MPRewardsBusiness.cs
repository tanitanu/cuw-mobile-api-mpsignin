using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.Profile;
using United.Mobile.DataAccess.Common;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Exception;
using United.Mobile.Model.MPRewards;
using United.Utility.Helper;

namespace United.Mobile.MPRewards.Domain
{
    public class MPRewardsBusiness : IMPRewardsBusiness
    {
        private readonly ICacheLog<MPRewardsBusiness> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMileagePlus _mileagePlus;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IShoppingSessionHelper _shoppingSessionHelper;

        public MPRewardsBusiness(ICacheLog<MPRewardsBusiness> logger
            , IConfiguration configuration
            , IMileagePlus mileagePlus
            , ISessionHelperService sessionHelperService
            , IShoppingSessionHelper shoppingSessionHelper)
        {
            _logger = logger;
            _configuration = configuration;
            _mileagePlus = mileagePlus;
            _sessionHelperService = sessionHelperService;
            _shoppingSessionHelper = shoppingSessionHelper;
        }

        public async Task<MOBMPPlusPointsResponse> GetAccountPlusPointsDetails(MOBMPPlusPointsRequest request)
        {
            MOBMPPlusPointsResponse response = new MOBMPPlusPointsResponse();

            _logger.LogInformation("GetAccountPlusPointsDetails Request - {@clientRequest}", Newtonsoft.Json.JsonConvert.SerializeObject(request));

            if (string.IsNullOrEmpty(request.MileagePlusNumber) || string.IsNullOrEmpty(request.HashValue) || string.IsNullOrEmpty(request.SessionId))
            {
                response.Exception = new MOBException("10000", _configuration.GetValue<string>("SSOInputException"));
            }

            MOBMPAccountValidationRequest req = new MOBMPAccountValidationRequest
            {
                SessionId = request.SessionId,
                HashValue = request.HashValue,
                MileagePlusNumber = request.MileagePlusNumber,
                Application = request.Application,
                DeviceId = request.DeviceId
            };

            var disablePlusPointsMPNumberCheck = _configuration.GetValue<bool>("disablePlusPointsMPNumberCheck");
            var persistMpsignIn = await _sessionHelperService.GetSession<MPSignIn>(request.SessionId, new MPSignIn().ObjectName, new List<string>() { request.SessionId, new MPSignIn().ObjectName }).ConfigureAwait(false);

            if (persistMpsignIn != null && !disablePlusPointsMPNumberCheck)
            {
                if (string.Equals(persistMpsignIn.MPNumber, req.MileagePlusNumber, StringComparison.OrdinalIgnoreCase))
                {
                    response.PluspointsDetails = await _mileagePlus.GetPlusPointsFromLoyaltyBalanceService(req, string.Empty).ConfigureAwait(false);
                }
                else
                {
                    response.PluspointsDetails = null;
                }
            }
            else
            {
                response.PluspointsDetails = await _mileagePlus.GetPlusPointsFromLoyaltyBalanceService(req, string.Empty).ConfigureAwait(false);
            }

            return response;
        }

        public async Task<MOBCancelFFCPNRsByMPNumberResponse> GetCancelFFCPnrsByMPNumber(MOBCancelFFCPNRsByMPNumberRequest request)
        {
            var response = new MOBCancelFFCPNRsByMPNumberResponse();
            if (string.IsNullOrEmpty(request.MileagePlusNumber) || string.IsNullOrEmpty(request.HashValue))
            {
                response.Exception = new MOBException
                {
                    Code = "9999",
                    Message = _configuration.GetValue<string>("GenericExceptionMessage")
                };
            }
            else
            {
                string authToken = string.Empty;
                var tupleRes = await _shoppingSessionHelper.ValidateHashPinAndGetAuthToken
                     (request.MileagePlusNumber, request.HashValue, request.Application.Id, request.DeviceId,
                     request.Application.Version.Major, authToken, request.TransactionId, request.SessionId).ConfigureAwait(false);
                bool validRequest = tupleRes.returnValue;
                authToken = tupleRes.validAuthToken;

                if (!validRequest)
                {
                    _logger.LogError("GetCancelFFCPnrsByMPNumber - AuthToken is not valid");
                    response.Exception = new MOBException
                    {
                        Code = "9999",
                        Message = _configuration.GetValue<string>("GenericExceptionMessage")
                    };
                }
                else
                {
                    response.CancelledFFCPNRList = await _mileagePlus.GetMPFutureFlightCreditFromCancelReservationService
                  (request.MileagePlusNumber, request.Application.Id, request.Application.Version.Major,
                  request.SessionId, request.TransactionId, request.DeviceId, callsource: "mobileMyTrip").ConfigureAwait(false);

                    response.FutureFlightCreditLink = (response.CancelledFFCPNRList != null && response.CancelledFFCPNRList.Any());
                }
            }

            response.MileagePlusNumber = request.MileagePlusNumber;
            response.TransactionId = request.TransactionId;

            return response;
        }
    }
}
