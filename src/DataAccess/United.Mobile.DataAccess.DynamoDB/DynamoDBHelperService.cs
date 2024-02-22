using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Common;
using United.Utility.Helper;

namespace United.Mobile.DataAccess.DynamoDB
{
    public class DynamoDBHelperService : IDynamoDBHelperService
    {
        private readonly ICacheLog<DynamoDBHelperService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDynamoDBService _dynamoDBService;
        public DynamoDBHelperService(
             ICacheLog<DynamoDBHelperService> logger
            , IConfiguration configuration
            , IDynamoDBService dynamoDBService
            )
        {
            _logger = logger;
            _configuration = configuration;
            _dynamoDBService = dynamoDBService;
        }

        public async Task<bool> IsTSAFlaggedAccount(string key, string sessionID)
        {
            #region
            //Database database = DatabaseFactory.CreateDatabase("ConnectionString - iPhone");
            //DbCommand dbCommand = (DbCommand)database.GetStoredProcCommand("usp_Select_TSA_Flagged_account");
            //database.AddInParameter(dbCommand, "@AccountNumber", DbType.String, accountNumber);

            //try
            //{
            //    using (IDataReader dataReader = database.ExecuteReader(dbCommand))
            //    {
            //        while (dataReader.Read())
            //        {
            //            if (Convert.ToInt32(dataReader["AccountExists"]) != 0)
            //            {
            //                flaggedAccount = true;
            //            }
            //        }
            //    }
            //}
            //catch (System.Exception) { }
            #endregion
            try
            {
                string tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("utb_TSA_Flagged_Account") ?? "cuw-tsa-flagged-account";
                return await _dynamoDBService.GetRecords<bool>(tableName, "Account001", key, sessionID).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("IsTSAFlaggedAccount {errormessage} ", ex.Message);
            }
            return false;

        }

        public async Task<bool> SaveTSAFlaggedAccount<T>( string key, T accNumber, string sessionID)
        {
            try
            {
                string tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("utb_TSA_Flagged_Account") ?? "cuw-tsa-flagged-account";
                return await _dynamoDBService.SaveRecords<T>(tableName, "Account001", key, accNumber, sessionID).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("IsTSAFlaggedAccount {errormessage} ", ex.Message);
            }
            return false;
        }


        public async Task<T> GetEResBetaTesterItems<T>(string applicationId, string appVersion, string mileageplusNumber, string sessionId)
        {
            try
            {
                string tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("uatb_EResBetaTester") ?? "cuw-eresbetatester";
                string key = applicationId + "::" + appVersion;
                return await _dynamoDBService.GetRecords<T>(tableName, "EResBetaTester001", key, sessionId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("GetEResBetaTesterItems {errormessage} ", ex.Message);
            }

            return default;
        }

        public async Task<T> GetAuthToken<T>(string accountNumber, int applicationId, string deviceId, string sessionId)
        {
            #region 
            /* SQL storedProc : "uasp_Get_MileagePlus_AuthToken_CSS"

            /// CSS Token length is 36 and Data Power Access Token length is more than 1500 to 1700 chars
            //if (iSDPAuthentication)
            //{
            //    SPname = "uasp_select_MileagePlusAndPin_DP";
            //}
            //else
            //{
            //    SPname = "uasp_select_MileagePlusAndPin_CSS";
            //}


            //Database database = DatabaseFactory.CreateDatabase("ConnectionString - iPhone");
            //DbCommand dbCommand = (DbCommand)database.GetStoredProcCommand(SPname);
            //database.AddInParameter(dbCommand, "@MileagePlusNumber", DbType.String, accountNumber);
            //database.AddInParameter(dbCommand, "@HashPincode", DbType.String, hashPinCode);
            //database.AddInParameter(dbCommand, "@ApplicationID", DbType.Int32, applicationId);
            //database.AddInParameter(dbCommand, "@AppVersion", DbType.String, appVersion);
            //database.AddInParameter(dbCommand, "@DeviceID", DbType.String, deviceId);

            try
            {
                using (IDataReader dataReader = database.ExecuteReader(dbCommand))
                {
                    while (dataReader.Read())
                    {
                        if (Convert.ToInt32(dataReader["AccountFound"]) == 1)
                        {
                            ok = true;
                            validAuthToken = dataReader["AuthenticatedToken"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex) { string msg = ex.Message; }
             */
            #endregion

            //SQL storedProc : "uasp_Get_MileagePlus_AuthToken_CSS"
            #region //SQL query to get storedProc MPData
            /*
             USE [iPhone]
                GO

                declare @MileagePlusNumber [varchar](32)
                declare @ApplicationID [int]
                declare @AppVersion [varchar](50)
                declare @DeviceID [varchar](256)

                set @MileagePlusNumber = 'AW791957'
                set @AppVersion = '4.1.30'
                set @ApplicationID = 1
                set @DeviceID = 'd007548c-addf-43fb-8f46-6870aec49647'

                declare @custID bigint
                set @custID = (select top 1 CustomerID from uatb_MileagePlusDevice nolock where MileagePlusNumber = @MileagePlusNumber order by InsertDateTime desc )

                select @custID
                SELECT top 1 * , @custID as CustID, getdate() SystemDate
                FROM uatb_MileagePlusValidation_CSS (NOLOCK)
                WHERE
                (MileagePlusNumber = @MileagePlusNumber or MPUserName = @MileagePlusNumber)
                and ApplicationID = @ApplicationID
                --and DeviceID = @DeviceID
                and IsTokenValid = 1
             */
            #endregion

            try
            {
                var key = string.Format("{0}::{1}::{2}", accountNumber, applicationId, deviceId);
                string tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("uatb_MileagePlusValidation_CSS") ?? "cuw-validate-mp-appid-deviceid";
                var responseData = await _dynamoDBService.GetRecords<string>(tableName, "ValidateHashPinAndGetAuthToken_" + sessionId, key, sessionId).ConfigureAwait(false);

                return string.IsNullOrEmpty(responseData) ? default : JsonConvert.DeserializeObject<T>(responseData);
            }
            catch (Exception ex)
            {
                _logger.LogError("ValidateHashPinAndGetAuthToken {errormessage} ", ex.Message);
            }
            return default;
        }

        public async Task<T> GetDeviceIdAppIdMPNumber<T>(string key, string transactionId)
        {
            try
            {
                string tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("uatb_MileagePlusValidation_CSS") ?? "cuw-validate-mp-appid-deviceid";
                var responseData = await _dynamoDBService.GetRecords<string>(tableName, "GetDeviceIdAppIdMPNumber_" + transactionId, key, transactionId).ConfigureAwait(false);

                return JsonConvert.DeserializeObject<T>(responseData);
            }
            catch (Exception ex)
            {
                _logger.LogError("GetDeviceIdAppIdMPNumber {errormessage} and {transactionId}", ex.Message, transactionId);
            }
            return default;
        }

        public async Task<bool> SaveMPAppIdDeviceId<T>(T data, string sessionId, string key, string secondaryKey = "001", string transactionId = "transId")
        {
            string tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("uatb_MileagePlusValidation_CSS") ?? "cuw-validate-mp-appid-deviceid";
            string _transactionId = string.IsNullOrEmpty(transactionId) ? "MileagePlusValidationCSS001" : transactionId;
            int _absoluteExpirationDays = _configuration.GetValue<int>("AbsoluteExpirationDays");
            if (_absoluteExpirationDays == 0)
                _absoluteExpirationDays = 36500;
            var requestData = JsonConvert.SerializeObject(data);
            return await _dynamoDBService.SaveRecords<string>(tableName, _transactionId, key, secondaryKey, requestData, sessionId, _absoluteExpirationDays);
        }

        public async Task<bool> IsVBQWelcomeModelDisplayed(string mileagePlusNumber, string applicationId, string deviceId, string sessionId)
        {
            #region
            //Database database = DatabaseFactory.CreateDatabase("ConnectionString - iPhone");
            //DbCommand dbCommand = (DbCommand)database.GetStoredProcCommand("uasp_Get_IsVBQWMDisplayed");
            //database.AddInParameter(dbCommand, "@MileagePlusNumber", DbType.String, MileagePlusNumber);
            //database.AddInParameter(dbCommand, "@ApplicationID", DbType.Int16, ApplicationID);
            //database.AddInParameter(dbCommand, "@DeviceID", DbType.String, DeviceID);
            //try
            //{
            //    using (IDataReader dataReader = database.ExecuteReader(dbCommand))
            //    {
            //        while (dataReader.Read())
            //        {
            //            ok = Convert.ToBoolean(dataReader["IsVBQWMDisplayed"));
            //        }
            //    }
            //}
            //catch (System.Exception ex) { string msg = ex.Message; }
            #endregion
            try
            {
                //MileagePlusNumber::ApplicationID::DeviceID
                var key = string.Format("{0}::{1}::{2}", mileagePlusNumber, applicationId, deviceId);
                string tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("uatb_IsVBQWMDisplayed");
                return await _dynamoDBService.GetRecords<bool>(tableName, "VBQWelcomeModel001", key, sessionId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("IsVBQWelcomeModelDisplayed {errormessage} ", ex.Message);
            }
            return default;
        }

        public async Task<bool> InsertDevicePushToken<T>(T data, string key, string sessionID)
        {
            try
            {
                string tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("uatb_DevicePushToken");
                if (string.IsNullOrEmpty(tableName))
                {
                    _logger.LogError("InsertDevicePushToken- TableName is not mentioned");
                }
                return await _dynamoDBService.SaveRecords<T>(tableName, "DevicePushToken", key, data, sessionID).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("InsertDevicePushToken error {@ErrorMessage} and {@StackTrace}", ex.Message, ex.StackTrace);
            }

            return false;
        }

        public async Task<bool> RegisterDevice<T>(T data, string key, string transactionId)
        {
            try
            {
                string tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("uatb_Device");
                if (string.IsNullOrEmpty(tableName))
                {
                    _logger.LogError("RegisterDevice- TableName is not mentioned");
                }
                return await _dynamoDBService.SaveRecords<T>(tableName, "RegisterDevice", key, data, transactionId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("RegisterDevice error {@ErrorMessage} and {@StackTrace}", ex.Message, ex.StackTrace);
            }

            return false;
        }

        public async Task<bool> RegisterDeviceHistory<T>(T data, string key, string transactionId)
        {
            try
            {
                string tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("uatb_Device_History");
                if (string.IsNullOrEmpty(tableName))
                {
                    _logger.LogError("RegisterDeviceHistory- TableName is not mentioned");
                }
                return await _dynamoDBService.SaveRecords<T>(tableName, "RegisterDeviceHistory", key, data, transactionId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("RegisterDeviceHistory error {@ErrorMessage} and {@StackTrace}", ex.Message, ex.StackTrace);
            }

            return false;
        }

        public async Task<bool> InsertMileagePlusAndHash<T>(T data, string key, string sessionId)
        {
            string tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("uatb_MileagePlusValidation");
            if (string.IsNullOrEmpty(tableName))
            {
                _logger.LogError("Mileageplusvalidation- TableName does not exist");
            }
            return await _dynamoDBService.SaveRecords<T>(tableName, "InsertMileagePlusAndHash", key, data, sessionId).ConfigureAwait(false);
        }

        public async Task<bool> InsertMilagePlusDevice<T>(T data, string key, string sessionId)
        {
            try
            {
                string tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("uatb_MileagePlusDevice");
                if (string.IsNullOrEmpty(tableName))
                {
                    _logger.LogError("InsertMilagePlusDevice- TableName does not exist");
                }
                return await _dynamoDBService.SaveRecords<T>(tableName, "InsertMilagePlusDevice", key, data, sessionId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError("InsertMilagePlusDevice error {@ErrorMessage} and {@StackTrace}", ex.Message, ex.StackTrace);
            }

            return false;
        }
    }
}


