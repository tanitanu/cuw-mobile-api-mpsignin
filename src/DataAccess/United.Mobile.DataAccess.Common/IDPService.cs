using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal;
using United.Mobile.Model.Internal.Common;
using United.Utility.Http;

namespace United.Mobile.DataAccess.Common
{
    public interface IDPService
    {
        public Task<string> GetAnonymousToken(int applicationId, string deviceId, IConfiguration configuration);

        Task<DPTokenResponse> GetSSOToken(int applicationId, string mpNumber, IConfiguration configuration, string configSSOSectionKey = "dpSSOTokenOption", string dpSSOConfig = "dpSSOTokenConfig", IResilientClient resilientSSOClient = null);
    }
}