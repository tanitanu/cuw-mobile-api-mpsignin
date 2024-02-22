using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Common.CacheModels;

namespace United.Common.Helper
{
    public interface IUtility
    {
        List<MOBMobileCMSContentMessages> GetSDLMessageFromList(List<CMSContentMessage> list, string title);
        Task<List<CMSContentMessage>> GetSDLContentByGroupName(MOBRequest request, string guid, string token, string groupName, string docNameConfigEntry, bool useCache = false);
        Task<List<CacheCountry>> LoadCountries();
        Task<List<CMSContentMessage>> GetSDLContentByTitle(string cacheKey, MOBCSLContentMessagesRequest sdlReqeust);
    }
}
