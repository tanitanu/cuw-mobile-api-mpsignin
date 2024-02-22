using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.MPSignIn;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Common.CacheModels;
using United.Mobile.Model.MPSignIn;
using United.Services.FlightShopping.Common.Extensions;

namespace United.Common.Helper
{
    public class Utility : IUtility
    {
        private readonly ILogger<Utility> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDPService _dPService;
        private readonly ICachingService _cachingService;
        private readonly IHeaders _headers;
        private readonly ICMSContentService _cMSContentService;

        public Utility(ILogger<Utility> logger
            , IConfiguration configuration
            , IDPService dPService
            , ICachingService cachingService
            , IHeaders headers
            , ICMSContentService cMSContentService)
        {
            _logger = logger;
            _configuration = configuration;
            _dPService = dPService;
            _cachingService = cachingService;
            _headers = headers;
            _cMSContentService = cMSContentService;
        }

        public List<MOBMobileCMSContentMessages> GetSDLMessageFromList(List<CMSContentMessage> list, string title)
        {
            List<MOBMobileCMSContentMessages> listOfMessages = new List<MOBMobileCMSContentMessages>();
            list?.Where(l => l.Title.ToUpper().Equals(title.ToUpper()))?.ForEach(i => listOfMessages.Add(new MOBMobileCMSContentMessages()
            {
                Title = i.Title,
                ContentFull = i.ContentFull,
                HeadLine = i.Headline,
                ContentShort = i.ContentShort,
                LocationCode = i.LocationCode
            }));

            return listOfMessages;
        }

        public async Task<List<CMSContentMessage>> GetSDLContentByGroupName(MOBRequest request, string guid, string token, string groupName, string docNameConfigEntry, bool useCache = false)
        {
            MOBCSLContentMessagesResponse response = null;
            try
            {
                var res = await _cachingService.GetCache<string>(_configuration.GetValue<string>(docNameConfigEntry) + "MOBCSLContentMessagesResponse", _headers.ContextValues.TransactionId).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(res))
                {
                    response = JsonConvert.DeserializeObject<MOBCSLContentMessagesResponse>(res);
                }

                if (response != null && response.Messages != null) { return response.Messages; }
            }
            catch { }

            MOBCSLContentMessagesRequest sdlReqeust = new MOBCSLContentMessagesRequest
            {
                Lang = "en",
                Pos = "us",
                Channel = "mobileapp",
                Listname = new List<string>(),
                LocationCodes = new List<string>(),
                Groupname = groupName,
                Usecache = useCache
            };

            string jsonRequest = JsonConvert.SerializeObject(sdlReqeust);

            response = await _cMSContentService.GetMessages<MOBCSLContentMessagesResponse>(token, _headers.ContextValues.SessionId, jsonRequest).ConfigureAwait(false);

            if (response == null)
            {
                _logger.LogError("GetSDLContentByGroupName {CSL-CallError}", "CSL response is empty or null");
                return null;
            }

            if (response.Errors.Count > 0)
            {
                string errorMsg = String.Join(" ", response.Errors.Select(x => x.Message));
                _logger.LogError("GetSDLContentByGroupName {CSL-CallError}", errorMsg);
                return null;
            }

            if (response != null && (Convert.ToBoolean(response.Status) && response.Messages != null))
            {
                if (!_configuration.GetValue<bool>("DisableSDLEmptyTitleFix"))
                {
                    response.Messages = response.Messages.Where(l => l.Title != null)?.ToList();
                }
                string cacheResponse = JsonConvert.SerializeObject(response);
                await _cachingService.SaveCache<string>(_configuration.GetValue<string>(docNameConfigEntry) + "MOBCSLContentMessagesResponse", cacheResponse, _headers.ContextValues.TransactionId, new TimeSpan(1, 30, 0)).ConfigureAwait(false);
                _logger.LogInformation("GetSDLContentByGroupName {SDLResponse}", response);
            }

            return response.Messages;
        }

        public async Task<List<CMSContentMessage>> GetSDLContentByTitle(string cacheKey, MOBCSLContentMessagesRequest sdlReqeust)
        {
            MOBCSLContentMessagesResponse response = null;

            if (string.IsNullOrEmpty(cacheKey))
            {
                _logger.LogError("GetSDLContentByTitle cacheKey is empty");
                return null;
            }

            try
            {
                var res = await _cachingService.GetCache<string>(cacheKey, _headers.ContextValues.TransactionId).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(res))
                {
                    response = JsonConvert.DeserializeObject<MOBCSLContentMessagesResponse>(res);

                    //if (response?.Messages != null) { return response.Messages; }
                }
                else
                {
                    string token = await _dPService.GetAnonymousToken(_headers.ContextValues.Application.Id, _headers.ContextValues.DeviceId, _configuration).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(token))
                    {
                        _logger.LogError("GetSDLContentByTitle - token is null");
                        return null;
                    }

                    string jsonRequest = JsonConvert.SerializeObject(sdlReqeust);

                    response = await _cMSContentService.GetMessages<MOBCSLContentMessagesResponse>(token, _headers.ContextValues.SessionId, jsonRequest).ConfigureAwait(false);

                    if (response == null)
                    {
                        _logger.LogError("GetSDLContentByTitle {CSL-CallError}", "CSL response is empty or null");
                        return null;
                    }

                    if (response.Errors !=null && response.Errors.Count > 0)
                    {
                        string errorMsg = String.Join(" ", response.Errors.Select(x => x.Message));
                        _logger.LogError("GetSDLContentByTitle Error {@CSL-CallError}", errorMsg);
                        return null;
                    }

                    if ((Convert.ToBoolean(response?.Status) && response.Messages != null))
                    {
                        string cacheResponse = JsonConvert.SerializeObject(response);
                        await _cachingService.SaveCache<string>(cacheKey, cacheResponse, _headers.ContextValues.TransactionId, new TimeSpan(1, 30, 0)).ConfigureAwait(false);

                        _logger.LogInformation("GetSDLContentByTitle - saved SDLContent in cache");
                    }

                }
            }
            catch { }

            return response.Messages;
        }

        public async Task<List<CacheCountry>> LoadCountries()
        {
            List<CacheCountry> Countries = new List<CacheCountry>();
            try
            {
                var CountriesList = await _cachingService.GetCache<string>("CountriesContent", "CountriesContent01").ConfigureAwait(false);

                if (!string.IsNullOrEmpty(CountriesList))
                {
                    return JsonConvert.DeserializeObject<List<CacheCountry>>(CountriesList);
                }
            }
            catch { return Countries; }
            return default;
        }
    }
}
