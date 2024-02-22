using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.MPSignInDatabaseManager;
using United.Mobile.DataAccess.OnPremiseSQLSP;
using United.Mobile.Model.Common.CloudDynamoDB;
using United.Mobile.Model.Internal.Exception;
using United.Mobile.Model.MPSignIn;
using United.Mobile.Model.MPSignIn.CCE;
using United.Mobile.Model.MPSignIn.HashpinVerify;
using United.Utility.Helper;
using System.Linq;

namespace United.Mobile.MPSignInCommon.Domain
{
    public class MPSignInCommonBusiness : IMPSignInCommonBusiness
    {
        private readonly ICacheLog<MPSignInCommonBusiness> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDynamoDBHelperService _dynamoDBHelperService;
        private readonly ISQLSPService _sqlSPService;
        private readonly IHashPin _hashPin;
        private readonly IDatabaseManagerService _databaseManagerService;
        

        public MPSignInCommonBusiness(
            ICacheLog<MPSignInCommonBusiness> logger
            , IConfiguration configuration
            , IDynamoDBHelperService dynamoDBHelperService
            , ISQLSPService sqlSPService
            , IHashPin hashPin
            , IDatabaseManagerService databaseManagerService           
            )
        {
            _logger = logger;
            _configuration = configuration;
            _dynamoDBHelperService = dynamoDBHelperService;
            _sqlSPService = sqlSPService;
            _hashPin = hashPin;
            _databaseManagerService = databaseManagerService;           
        }
        public async Task<MPTokenResponse> GetMileagePlusAndPinDP(string mileagePlusNumber, int applicationId, string deviceId, string appVersion, string hashPinCode, string serviceName, string transactionId)
        {
            MPTokenResponse response = new MPTokenResponse();
            try
            {
                _logger.LogInformation("GetMileagePlusAndPinDP Request {mpnumber} {applicationId} {deviceId} {appversion}", mileagePlusNumber, applicationId.ToString(), deviceId, appVersion);

                var dynamoRecord = await _dynamoDBHelperService.GetAuthToken<MileagePlusDetails>(mileagePlusNumber, applicationId, deviceId, string.Empty).ConfigureAwait(false);

                if (dynamoRecord?.DataPowerAccessToken != null)
                {
                    return new MPTokenResponse
                    {
                        AccountFound = 1,
                        AuthenticatedToken = dynamoRecord.DataPowerAccessToken
                    };
                }

                _logger.LogInformation("GetMileagePlusAndPinDP Record not found in DynamoDB Checking in SQL {transactionId}", transactionId);

                if (!_configuration.GetValue<bool>("DisableSQLSupport"))
                    return await _sqlSPService.GetAuthToken<MPTokenResponse>(mileagePlusNumber, hashPinCode, applicationId, deviceId, appVersion, transactionId).ConfigureAwait(false);

            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("GetMileagePlusAndPinDP error {@UnitedException}, {@stackTrack}", uaex.Message, uaex.StackTrace);
                response.Exception = new MOBException
                {
                    Message = uaex.Message
                };

            }
            catch (Exception ex)
            {
                _logger.LogError("GetMileagePlusAndPinDP error {@errormessage}", ex.Message);
                response.Exception = new MOBException("9999", ex.Message);
            }
            _logger.LogInformation("GetMileagePlusAndPinDP Response {recordcount}", response.AccountFound);
            return response;

        }

        public async Task<HashpinVerifyResponse> GetMPRecord(HashpinVerifyRequest request)
        {
            var response = new HashpinVerifyResponse();

            //Null value is being assigned to AppVersion from HomeScreen -- Fix is made to avoid the issue.
            if(string.IsNullOrEmpty(request.AppVersion))
            {
                request.AppVersion = "1.0.0";
            }

            var hashpinresponse = await _hashPin.GetMPRecordUsingHash_Cache(request.MpNumber, 0, request.HashValue, Convert.ToInt32(request.ApplicationId), request.DeviceId, request.AppVersion, request.TransactionId).ConfigureAwait(false);
            response.MPDetails = hashpinresponse.details;
            response.Message = hashpinresponse.message.ToString();
            response.TransactionId = request.TransactionId;

            _logger.LogInformation("GetMPRecord {@ServiceAction} {@message}", request.ServiceName, response.Message);

            if (response.MPDetails == null)
            {
                _logger.LogWarning("VerifyMileagePlusHashpin-UnAuthorized {@accountNumber} {@ServiceAction}", request.MpNumber, request.ServiceName);
                response.Message = "VerifyMileagePlusHashpin-UnAuthorized";
            }

            return response;
        }

        public async Task<CCEResponse> IsAccountExist(CCERequest request)
        {
            CCEResponse response = new CCEResponse();
            response.CCERequest = new CCERequest();

            var transactionId = System.Guid.NewGuid().ToString();
            if (request != null && !string.IsNullOrEmpty(request.TransactionId))
            {
                transactionId = request.TransactionId;
            }

            response.CCERequest.ClientId = request?.ClientId;
            response.CCERequest.TransactionId = request?.TransactionId;
            response.CCERequest.MileagePlusNumber = request?.MileagePlusNumber;
            response.IsAccountExist = false;

            if (_configuration.GetValue<bool>("DisableSQLService"))
            {
                var sqlResponse = await _sqlSPService.IsAccountExist<CCEResponse>(response.CCERequest.ClientId, response.CCERequest.MileagePlusNumber, response.CCERequest.TransactionId).ConfigureAwait(false);
                if (sqlResponse != null)
                {
                    response.IsAccountExist = sqlResponse.IsAccountExist;
                }
            }
            string tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("uatb_MileagePlusValidation_CSS") ?? "cuw-validate-mp-appid-deviceid";           
            string dynamoRecords = await _databaseManagerService.GetRecordsFromDynamoDB(response.CCERequest.MileagePlusNumber, response.CCERequest.TransactionId).ConfigureAwait(false);
            response.IsAccountExist = !string.IsNullOrEmpty(dynamoRecords);
            return response;
        }
    }
}
