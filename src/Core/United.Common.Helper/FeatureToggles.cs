using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using United.Utility.Helper;

namespace United.Common.Helper
{
    public class FeatureToggles : IFeatureToggles
    {
        private readonly IFeatureSettings _featureSettings;
        private readonly IConfiguration _configuration;
        public FeatureToggles(IConfiguration configuration, IFeatureSettings featureSettings)
        {
            _featureSettings = featureSettings;
            _configuration = configuration;
        }

        public async Task<bool> IsEnableU4BForMultipax(int applicationId, string appVersion)
        {
            return (await _featureSettings.GetFeatureSettingValue("EnableU4BForMultipax").ConfigureAwait(false) &&
                GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableU4BForMultipax_AppVersion"), _configuration.GetValue<string>("IPhone_EnableU4BForMultipax_AppVersion")));
        }
    }
}
