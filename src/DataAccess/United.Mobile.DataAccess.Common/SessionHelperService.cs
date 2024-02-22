using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using United.Ebs.Logging.Enrichers;
using United.Mobile.Model.Common;
using United.Utility.Helper;

namespace United.Mobile.DataAccess.Common
{
    public class SessionHelperService : ISessionHelperService
    {
        private readonly ICacheLog<SessionHelperService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISessionOnCloudService _sessionOnCloudService;
        private readonly IApplicationEnricher _applicationEnricher;

        public string SessionID { get; set; }

        public SessionHelperService(ICacheLog<SessionHelperService> logger
            , IConfiguration configuration
            , ISessionOnCloudService sessionOnCloudService
            , IApplicationEnricher applicationEnricher)
        {
            _logger = logger;
            _configuration = configuration;
            _sessionOnCloudService = sessionOnCloudService;
            _applicationEnricher = applicationEnricher;
        }

        public async Task<T> GetSession<T>(string sessionID, string objectName, List<string> vParams = null, bool isReadOnPrem = false)
        {
            if (string.IsNullOrEmpty(sessionID))
            {
                return default;
            }

            try
            {
                //  getsessionValueOnCloud 
                var sessionResponse = await _sessionOnCloudService?.GetSession(sessionID, objectName, vParams, sessionID, isReadOnPrem);
                bool isSessionDataSucceed = false;
                if (!string.IsNullOrEmpty(sessionResponse))
                {
                    var sessionData = JsonConvert.DeserializeObject<SessionResponse>(sessionResponse);

                    if (!string.IsNullOrEmpty(sessionData?.Data) && sessionData.Succeed)
                    {
                        try
                        {
                            if (typeof(T) == typeof(string))
                            {
                                return sessionData.Data;
                            }
                            return JsonConvert.DeserializeObject<T>(sessionData.Data);
                        }
                        catch (Exception)
                        {
                        }

                        try
                        {
                            T typeInstance = default(T);
                            StringReader memoryStream = new StringReader(sessionData.Data);
                            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                            typeInstance = (T)xmlSerializer.Deserialize(memoryStream);
                            return typeInstance;
                        }
                        catch (Exception)
                        {
                        }

                        try
                        {
                            var deserValue = (sessionData.Data.Contains(@"<?xml")) ?
                                           XmlSerializerHelper.GetObjectFromXmlData<string>(sessionData.Data) :
                                           JsonConvert.DeserializeObject<T>(sessionData.Data);

                            return await Task.FromResult(JsonConvert.DeserializeObject<T>(deserValue)).ConfigureAwait(false);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    isSessionDataSucceed = sessionData.Succeed;

                }

            }
            catch (Exception ex)
            { throw ex; }

            return default;
        }

        public async Task<bool> SaveSession<T>(T data, string sessionID, List<string> validateParams, string objectName = "", int sessionTimeSpanInSecs = 5400, bool saveToOnPrem = false)
        {
            var sessionObjectName = (string.IsNullOrEmpty(objectName)) ? typeof(T).FullName : objectName;

            //SaveSessionValue from OnCloud Service
            await _sessionOnCloudService?.SaveSessionONCloud<T>(data, sessionID, validateParams, sessionObjectName, TimeSpan.FromSeconds(sessionTimeSpanInSecs), sessionID);
            if (_configuration.GetValue<bool>("SaveSessionToOnPrem") && saveToOnPrem)
            {
                _ = Task.Run( () => {
                    try
                    {
                        var saveData = XmlSerializerHelper.SaveObjectFromXmlData<T>(data);
                        _sessionOnCloudService?.SaveSessionOnPrem(saveData, sessionID, validateParams, sessionObjectName, TimeSpan.FromSeconds(sessionTimeSpanInSecs), sessionID);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Exception - SaveSessionOnPrem {@Exception}, {@ValidateParam}", JsonConvert.SerializeObject(ex), JsonConvert.SerializeObject(validateParams));
                    }
                });
            }

            return true;
        }
    }
}
