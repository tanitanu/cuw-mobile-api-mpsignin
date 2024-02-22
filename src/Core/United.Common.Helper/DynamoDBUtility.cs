using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using United.Definition;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.OnPremiseSQLSP;
using United.Mobile.Model.Common;
using United.Mobile.Model.Common.CloudDynamoDB;
using United.Mobile.Model.DeviceInitialization;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.Internal.Common.DynamoDB;
using United.Mobile.Model.MPSignIn;
using United.Utility.Helper;

namespace United.Common.Helper
{
    public class DynamoDBUtility : IDynamoDBUtility
    {
        private readonly ICacheLog<DynamoDBUtility> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDynamoDBHelperService _dynamoDBHelperService;
        private readonly ISQLSPService _sQLSPService;
        private readonly ICachingService _cachingService;
        private readonly IUtility _utility;

        public DynamoDBUtility(ICacheLog<DynamoDBUtility> logger
            , IConfiguration configuration
            , IDynamoDBHelperService dynamoDBHelperService
            , ISQLSPService sQLSPService
            , ICachingService cachingService
            , IUtility utility)
        {
            _logger = logger;
            _configuration = configuration;
            _dynamoDBHelperService = dynamoDBHelperService;
            _sQLSPService = sQLSPService;
            _cachingService = cachingService;
            _utility = utility;
        }

        public async Task<bool> IsVBQWelcomeModelDisplayed(int applicationId, string deviceid, string mileagePlusNumber, string sessionid)
        {
            if (!_configuration.GetValue<bool>("DisableSQLSupport"))
            {
                await _sQLSPService.IsVBQWMDisplayed(applicationId, deviceid, mileagePlusNumber, sessionid).ConfigureAwait(false);
            }
            return await _dynamoDBHelperService.IsVBQWelcomeModelDisplayed(mileagePlusNumber, applicationId.ToString(), deviceid, sessionid).ConfigureAwait(false);
        }

        public async Task<EResBetaTester> GetEResBetaTesterItems(int applicationId, string appVersion, string mileageplusNumber, string sessionid)
        {
            if (!_configuration.GetValue<bool>("DisableSQLSupport"))
            {
                await _sQLSPService.IsEIsEResBetaTester(applicationId, appVersion, mileageplusNumber, sessionid).ConfigureAwait(false);
            }
            return await _dynamoDBHelperService.GetEResBetaTesterItems<EResBetaTester>(applicationId.ToString(), appVersion, mileageplusNumber, sessionid).ConfigureAwait(false);

        }

        public async Task<bool> InsertDevicePushToken(DeviceData data, string transactionId)
        {
            if (!_configuration.GetValue<bool>("DisableSQLSupport"))
            {
                await _sQLSPService.InsertDevicePushToken(data.Deviceid, data.ApplicationId, data.PushToken, transactionId).ConfigureAwait(false);
            }

            return await _dynamoDBHelperService.InsertDevicePushToken<DeviceData>(data, data.Deviceid, transactionId).ConfigureAwait(false);
        }

        public async Task InsertMileagePlusAndHash(string mileagePlusNumber, string hashValue, string sessionId)
        {
            OnePassDynamoDBRequest mpObject = new OnePassDynamoDBRequest
            {
                MileagePlusNumber = mileagePlusNumber,
                PinCode = hashValue,
                UnhashedPinCode = string.Empty,
                PINPWDSecurityPwdUpdate = false
            };

            if (await _dynamoDBHelperService.InsertMileagePlusAndHash<OnePassDynamoDBRequest>(mpObject, mileagePlusNumber, sessionId))
            {
                if (!_configuration.GetValue<bool>("DisableSQLSupport"))
                {
                    try
                    {
                        await _sQLSPService.InsertMileagePlusAndHash(mileagePlusNumber, hashValue, sessionId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("InsertMileagePlusAndHash SQL error {@ErrorMessage} and {@StackTrace}", ex.Message, ex.StackTrace);
                    }
                }
            }
        }

        public async Task<bool> IsTSAFlaggedAccount(string accountNumber, string sessionid)
        {
            if (!_configuration.GetValue<bool>("DisableSQLSupport"))
            {
                var flag = await _sQLSPService.IsTSAFlaggedAccount(accountNumber, sessionid).ConfigureAwait(false);
                if (flag)
                {
                    await _dynamoDBHelperService.SaveTSAFlaggedAccount<bool>(accountNumber, true, sessionid).ConfigureAwait(false);
                }
            }

            return await _dynamoDBHelperService.IsTSAFlaggedAccount(accountNumber, sessionid).ConfigureAwait(false);
        }

        public async Task InsertUpdateMileagePlusAndPin(MileagePlusDetails cloudResult, bool iSDPAuthentication, string key, string sessionId, string transactionId = "T01")
        {
            if (_configuration.GetValue<bool>("EnableSaveHashpinParllelThread"))
            {
                //InsertUpdateMileagePlusAndPin_ParllelThread(cloudResult, iSDPAuthentication, key, sessionId, transactionId);
                await InsertUpdateMileagePlusAndPin_AsyncParallel(cloudResult, iSDPAuthentication, key, sessionId, transactionId);
                return;
            }
            if (!_configuration.GetValue<bool>("DisableSQLSupport"))
            {
                SQLDBRequest mpOnepass = new SQLDBRequest
                {
                    Data = new MOBMileagePlus()
                    {
                        MileagePlusNumber = cloudResult.MileagePlusNumber,
                        MPUserName = cloudResult.MPUserName,
                        HashPincode = cloudResult.HashPincode,
                        PinCode = cloudResult.PinCode,
                        ApplicationID = cloudResult.ApplicationID,
                        AppVersion = cloudResult.AppVersion,
                        DeviceID = cloudResult.DeviceID,
                        DataPowerAccessToken = cloudResult.DataPowerAccessToken,
                        AuthenticatedToken = cloudResult.AuthenticatedToken,
                        IsTokenValid = cloudResult.IsTokenValid,
                        TokenExpireDateTime = cloudResult.TokenExpireDateTime,
                        TokenExpiryInSeconds = cloudResult.TokenExpiryInSeconds,
                        IsTouchIDSignIn = cloudResult.IsTouchIDSignIn,
                        IsTokenAnonymous = cloudResult.IsTokenAnonymous,
                        CustomerID = cloudResult.CustomerID
                    },
                    TransactionId = transactionId,
                    SessionId = sessionId
                };

                var requestData = JsonConvert.SerializeObject(mpOnepass);
                try
                {
                    await _sQLSPService.InsertUpdateMileagePlusAndPinDP(requestData, iSDPAuthentication, sessionId).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("InsertUpdateMileagePlusAndPinDP SQL error {@ErrorMessage} and {@StackTrace}", ex.Message, ex.StackTrace);
                }
            }

            await _dynamoDBHelperService.SaveMPAppIdDeviceId<MileagePlusDetails>(cloudResult, sessionId, key, cloudResult.MileagePlusNumber, transactionId).ConfigureAwait(false);

            if (!_configuration.GetValue<bool>("DisableVerifyMileagePlusHashpinCacheSupport"))
            {
                var expiry = TimeSpan.FromSeconds(_configuration.GetSection("dpTokenConfig").GetValue<double>("tokenExpInSec"));
                await _cachingService.SaveCache<MileagePlusDetails>(key, cloudResult, transactionId, expiry);
            }
        }

        private Task InsertUpdateMileagePlusAndPin_ParllelThread(MileagePlusDetails cloudResult, bool iSDPAuthentication, string key, string sessionId, string transactionId = "T01")
        {

            if (!_configuration.GetValue<bool>("DisableSQLSupport"))
            {
                _ = Task.Run(() =>
                {
                    SQLDBRequest mpOnepass = new SQLDBRequest
                    {
                        Data = new MOBMileagePlus()
                        {
                            MileagePlusNumber = cloudResult.MileagePlusNumber,
                            MPUserName = cloudResult.MPUserName,
                            HashPincode = cloudResult.HashPincode,
                            PinCode = cloudResult.PinCode,
                            ApplicationID = cloudResult.ApplicationID,
                            AppVersion = cloudResult.AppVersion,
                            DeviceID = cloudResult.DeviceID,
                            DataPowerAccessToken = cloudResult.DataPowerAccessToken,
                            AuthenticatedToken = cloudResult.AuthenticatedToken,
                            IsTokenValid = cloudResult.IsTokenValid,
                            TokenExpireDateTime = cloudResult.TokenExpireDateTime,
                            TokenExpiryInSeconds = cloudResult.TokenExpiryInSeconds,
                            IsTouchIDSignIn = cloudResult.IsTouchIDSignIn,
                            IsTokenAnonymous = cloudResult.IsTokenAnonymous,
                            CustomerID = cloudResult.CustomerID
                        },
                        TransactionId = transactionId,
                        SessionId = sessionId
                    };

                    var requestData = JsonConvert.SerializeObject(mpOnepass);

                    _sQLSPService.InsertUpdateMileagePlusAndPinDP(requestData, iSDPAuthentication, sessionId);
                });
            }

            _ = Task.Run(() =>
           {
               _dynamoDBHelperService.SaveMPAppIdDeviceId<MileagePlusDetails>(cloudResult, sessionId, key, cloudResult.MileagePlusNumber, transactionId);
           });

            if (!_configuration.GetValue<bool>("DisableVerifyMileagePlusHashpinCacheSupport"))
            {
                _ = Task.Run(() =>
                {
                    var expiry = TimeSpan.FromSeconds(_configuration.GetSection("dpTokenConfig").GetValue<double>("tokenExpInSec"));
                    _cachingService.SaveCache<MileagePlusDetails>(key, cloudResult, transactionId, expiry);
                });
            }
            return default;
        }

        private async Task InsertUpdateMileagePlusAndPin_AsyncParallel(MileagePlusDetails cloudResult, bool iSDPAuthentication, string key, string sessionId, string transactionId = "T01")
        {
            if (await _dynamoDBHelperService.SaveMPAppIdDeviceId<MileagePlusDetails>(cloudResult, sessionId, key,
                cloudResult.MileagePlusNumber, transactionId))
            {
                var parallelMPPinTasks = new List<Task>();
                var expiry = TimeSpan.FromSeconds(_configuration.GetSection("dpTokenConfig").GetValue<double>("tokenExpInSec"));
                parallelMPPinTasks.Add(_cachingService.SaveCache<MileagePlusDetails>(key, cloudResult, transactionId, expiry));
                if (!_configuration.GetValue<bool>("DisableSQLSupport"))
                {
                    parallelMPPinTasks.Add(SaveMileagePlusAndPinToSql(cloudResult, iSDPAuthentication, sessionId, transactionId));
                }
                await Task.WhenAll(parallelMPPinTasks);
            }
        }

        private async Task<bool> SaveMileagePlusAndPinToSql(MileagePlusDetails cloudResult, bool iSDPAuthentication, string sessionId, string transactionId)
        {
            SQLDBRequest mpOnepass = new SQLDBRequest
            {
                Data = new MOBMileagePlus()
                {
                    MileagePlusNumber = cloudResult.MileagePlusNumber,
                    MPUserName = cloudResult.MPUserName,
                    HashPincode = cloudResult.HashPincode,
                    PinCode = cloudResult.PinCode,
                    ApplicationID = cloudResult.ApplicationID,
                    AppVersion = cloudResult.AppVersion,
                    DeviceID = cloudResult.DeviceID,
                    DataPowerAccessToken = cloudResult.DataPowerAccessToken,
                    AuthenticatedToken = cloudResult.AuthenticatedToken,
                    IsTokenValid = cloudResult.IsTokenValid,
                    TokenExpireDateTime = cloudResult.TokenExpireDateTime,
                    TokenExpiryInSeconds = cloudResult.TokenExpiryInSeconds,
                    IsTouchIDSignIn = cloudResult.IsTouchIDSignIn,
                    IsTokenAnonymous = cloudResult.IsTokenAnonymous,
                    CustomerID = cloudResult.CustomerID
                },
                TransactionId = transactionId,
                SessionId = sessionId
            };

            var requestData = JsonConvert.SerializeObject(mpOnepass);
            try
            {
                return await _sQLSPService.InsertUpdateMileagePlusAndPinDP(requestData, iSDPAuthentication, sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("InsertUpdateMileagePlusAndPinDP SQL error {@ErrorMessage} and {@StackTrace}", ex.Message, ex.StackTrace);
            }
            return false;
        }

        public async Task<int> RegisterDevice(DeviceRequest request)
        {
            int returnVal = 1;

            if (!_configuration.GetValue<bool>("DisableSQLSupport"))
            {
                returnVal = await _sQLSPService.RegisterDevice<int>(request, request.TransactionId).ConfigureAwait(false);
            }

            #region DynamoDB           

            string key = request.IdentifierForVendor + "::" + request.ApplicationId;
            await _dynamoDBHelperService.RegisterDevice<DeviceRequest>(request, key, request.TransactionId).ConfigureAwait(false);
            await _dynamoDBHelperService.RegisterDeviceHistory<DeviceRequest>(request, key, request.TransactionId).ConfigureAwait(false);

            #endregion

            return returnVal;

        }

        public async Task<List<MOBLegalDocument>> GetNewLegalDocumentsForTitles(string titles, string transactionId, bool isTermsnConditions)
        {
            List<MOBLegalDocument> documents = new List<MOBLegalDocument>();
            var captions = titles.Split(",");
            _logger.LogInformation("GetNewLegalDocumentsForTitles {@EnableSDLContentLegalDoc}", _configuration.GetValue<bool>("EnableSDLContentLegalDoc"));
            if (_configuration.GetValue<bool>("EnableSDLContentLegalDoc"))
            {
                var lstMessages = await _utility.GetSDLContentByTitle("SDLContent_DocumentLibrary", _configuration.GetSection("DocumentLibrarySDLContent").Get<MOBCSLContentMessagesRequest>()).ConfigureAwait(false);

                if (lstMessages != null && lstMessages.Any())
                {
                    foreach (var title in captions)
                    {
                        var messageContent = _utility.GetSDLMessageFromList(lstMessages, title.Trim('\''));

                        if (messageContent != null)
                        {
                            foreach (var message in messageContent)
                            {
                                //In SDL content adding extra double quotes for the html tags like href, styles.
                                //Give with single quote "&apos;" before and after the href, styles without spaces
                                //to save in SDL  
                                message.ContentFull = message.ContentFull.Replace("\"'", "'").Replace("'\"", "'");
                                var legalDocuments = JsonConvert.DeserializeObject<List<MOBLegalDocument>>(message.ContentFull);

                                documents.AddRange(legalDocuments);
                            }
                        }
                    }
                }

            }
            else
            {
                foreach (var title in captions)
                {
                    string documentLibrary = await _cachingService.GetCache<string>(title.Trim('\''), transactionId).ConfigureAwait(false);
                    var legalDocuments = JsonConvert.DeserializeObject<List<MOBLegalDocument>>(documentLibrary);
                    documents.AddRange(legalDocuments);
                }
            }

            return documents;
        }

        public async Task InsertMilagePlusDevice(MilagePlusDevice data, string sessionId)
        {
            if (!_configuration.GetValue<bool>("DisableSQLSupport"))
            {
                try
                {
                    await _sQLSPService.InsertMilagePlusDevice(data.DeviceId, data.ApplicationId, data.MileagePlusNumber, data.CustomerID, sessionId).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("InsertMilagePlusDevice SQL error {@ErrorMessage} and {@StackTrace}", ex.Message, ex.StackTrace);
                }
            }

            string key = $"{data.MileagePlusNumber}::{data.ApplicationId}::{data.DeviceId}";
            await _dynamoDBHelperService.InsertMilagePlusDevice<MilagePlusDevice>(data, key, sessionId).ConfigureAwait(false);
        }
        public async Task<bool> SaveNewLegalDocumentsForTitles(string title, string transactionId, bool IsForceInsert = false)
        {
            if (!IsForceInsert)
            {
                string documentLibrary = await _cachingService.GetCache<string>(title, transactionId).ConfigureAwait(false);

                if (string.IsNullOrEmpty(documentLibrary))
                {
                    return false;
                }
            }

            List<MOBLegalDocument> legaldocList = await _sQLSPService.GetNewLegalDocumentsForTitles(title, transactionId, true).ConfigureAwait(false);
            if (legaldocList.Count != 0)
            {
                _logger.LogInformation("SaveNewLegalDocumentsForTitles saving {@title} to Cache ", GeneralHelper.RemoveCarriageReturn(title));

                await _cachingService.SaveCache<List<MOBLegalDocument>>(title, legaldocList, transactionId, new System.TimeSpan(11000, 0, 0, 0)).ConfigureAwait(false);
                string documentLibrary = await _cachingService.GetCache<string>(title, transactionId).ConfigureAwait(false);
                return !string.IsNullOrEmpty(documentLibrary);
            }
            else
            {
                _logger.LogInformation("SaveNewLegalDocumentsForTitles {@title} not Found in SQL", GeneralHelper.RemoveCarriageReturn(title));
            }

            _logger.LogInformation("SaveNewLegalDocumentsForTitles {@title} not Found in SQL and not able to save to Cache ", GeneralHelper.RemoveCarriageReturn(title));

            return false;
        }

        public async Task<bool> SaveMPHashPin(string mpnumber, string deviceId, int applicationId, string transactionId)
        {
            var sqlResponse = await _sQLSPService.GetMPHashPin(mpnumber, deviceId, applicationId, transactionId).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(sqlResponse))
            {
                var response = JsonConvert.DeserializeObject<MileagePlusDetails>(sqlResponse);
                string Key = string.Format("{0}::{1}::{2}", mpnumber, applicationId, deviceId);
                await _dynamoDBHelperService.SaveMPAppIdDeviceId<MileagePlusDetails>(response, transactionId, Key, mpnumber, transactionId).ConfigureAwait(false);

                var hashResponse = await _dynamoDBHelperService.GetDeviceIdAppIdMPNumber<MileagePlusDetails>(Key, transactionId).ConfigureAwait(false);

                return hashResponse != null;
            }
            else
            {
                _logger.LogInformation("SaveMPHashPin {@MPNumber} data not Found in SQL", GeneralHelper.RemoveCarriageReturn(mpnumber));
            }

            return default;
        }

    }
}
