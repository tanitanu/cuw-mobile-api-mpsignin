using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.OnPremiseSQLSP;
using United.Mobile.MPSignInJobs.Domain.SQL2DynamoDBJobs;
using United.Utility.Helper;

namespace United.Mobile.MPSignInJobs.Domain
{
    public class SQL2DynamoDBJobBusiness : ISQL2DynamoDBJobBusiness
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheLog<SQL2DynamoDBJobBusiness> _logger;
        private readonly ISQL2DynamoMigrationService _SQL2DynamoMigrationService;
        private readonly ICloudSQL2DynamoMigrationService _cloudSQL2DynamoMigrationService;
        private readonly ICachingService _cachingService;

        public SQL2DynamoDBJobBusiness(
            IConfiguration configuration
            , ICacheLog<SQL2DynamoDBJobBusiness> logger
            , ISQL2DynamoMigrationService SQL2DynamoMigrationService
            , ICloudSQL2DynamoMigrationService cloudSQL2DynamoMigrationService
            , ICachingService cachingService
            )
        {
            _configuration = configuration;
            _logger = logger;
            _SQL2DynamoMigrationService = SQL2DynamoMigrationService;
            _cloudSQL2DynamoMigrationService = cloudSQL2DynamoMigrationService;
            _cachingService = cachingService;
        }

        public async Task uatb_Device(SQL2DynamoDBJob apps_config)
        {
            var getCacheData = _cachingService.GetCache<string>("SQL2DynamoDBJobAppsettings_Device", "JOBD1").Result;
            _logger.LogInformation("Get configuration settings from Cache for Device table {@CacheResponse}", getCacheData);
            apps_config = JsonConvert.DeserializeObject<SQL2DynamoDBJob>(getCacheData);

            if (apps_config.Enable)
            {
                if (apps_config.RunCloudService)
                {
                    await _cloudSQL2DynamoMigrationService.uatb_Device().ConfigureAwait(false);
                }
                else
                {
                    await _SQL2DynamoMigrationService.uatb_Device().ConfigureAwait(false);
                }
                return;
            }

            _logger.LogInformation("uatb_Device not Enabled");
        }

        public async Task uatb_Device_History(SQL2DynamoDBJob apps_config)
        {
            var getCacheData = _cachingService.GetCache<string>("SQL2DynamoDBJobAppsettings_DeviceHistory", "JOBD1").Result;
            _logger.LogInformation("Get configuration settings from Cache for DeviceHistory table {@CacheResponse}", getCacheData);
            apps_config = JsonConvert.DeserializeObject<SQL2DynamoDBJob>(getCacheData);

            if (apps_config.Enable)
            {
                if (apps_config.RunCloudService)
                {
                    await _cloudSQL2DynamoMigrationService.uatb_Device_History().ConfigureAwait(false);
                }
                else
                {
                    await _SQL2DynamoMigrationService.uatb_Device_History().ConfigureAwait(false);
                }
                return;
            }
            _logger.LogInformation("uatb_Device_History not Enabled");

        }

        public async Task uatb_DevicePushToken(SQL2DynamoDBJob apps_config)
        {
            var getCacheData = _cachingService.GetCache<string>("SQL2DynamoDBJobAppsettings_DevicePushToken", "JOBD1").Result;
            _logger.LogInformation("Get configuration settings from Cache for DevicePushToken table {@CacheResponse}", getCacheData);
            apps_config = JsonConvert.DeserializeObject<SQL2DynamoDBJob>(getCacheData);

            if (apps_config.Enable)
            {
                if (apps_config.RunCloudService)
                {
                    await _cloudSQL2DynamoMigrationService.uatb_DevicePushToken().ConfigureAwait(false);
                }
                else
                {
                    await _SQL2DynamoMigrationService.uatb_DevicePushToken().ConfigureAwait(false);
                }
                return;
            }
            _logger.LogInformation("uatb_DevicePushToken not Enabled");

        }

        public async Task uatb_MileagePlusValidation_CSS(SQL2DynamoDBJob apps_config)
        {
            var getCacheData = _cachingService.GetCache<string>("SQL2DynamoDBJobAppsettings_MileagePlusValidation_CSS", "JOBD1").Result;
            _logger.LogInformation("Get configuration settings from Cache for MileagePlusValidation table {@CacheResponse}", getCacheData);
            apps_config = JsonConvert.DeserializeObject<SQL2DynamoDBJob>(getCacheData);

            if (apps_config.Enable)
            {
                if (apps_config.RunCloudService)
                {
                    await _cloudSQL2DynamoMigrationService.uatb_MileagePlusValidation_CSS().ConfigureAwait(false);
                }
                else
                {
                    await _SQL2DynamoMigrationService.uatb_MileagePlusValidation_CSS().ConfigureAwait(false);
                }
                return;
            }
            _logger.LogInformation("uatb_MileagePlusValidation_CSS not Enabled");

        }

        public async Task uatb_MileagePlusValidation(SQL2DynamoDBJob apps_config)
        {
            var getCacheData = _cachingService.GetCache<string>("SQL2DynamoDBJobAppsettings_MileagePlusValidation", "JOBD1").Result;
            _logger.LogInformation("Get configuration settings from Cache for MileagePlusValidation table {@CacheResponse}", getCacheData);
            apps_config = JsonConvert.DeserializeObject<SQL2DynamoDBJob>(getCacheData);

            if (apps_config.Enable)
            {
                if (apps_config.RunCloudService)
                {
                    await _cloudSQL2DynamoMigrationService.uatb_MileagePlusValidation().ConfigureAwait(false);
                }
                else
                {
                    await _SQL2DynamoMigrationService.uatb_MileagePlusValidation().ConfigureAwait(false);
                }
                return;
            }
            _logger.LogInformation("uatb_MileagePlusValidation not Enabled");

        }

        public async Task utb_TSA_Flagged_Account(SQL2DynamoDBJob apps_config)
        {
            var getCacheData = _cachingService.GetCache<string>("SQL2DynamoDBJobAppsettings_TSA_Flagged_Account", "JOBD1").Result;
            _logger.LogInformation("Get configuration settings from Cache for TSA_Flagged_Account table {@CacheResponse}", getCacheData);
            apps_config = JsonConvert.DeserializeObject<SQL2DynamoDBJob>(getCacheData);

            if (apps_config.Enable)
            {
                if (apps_config.RunCloudService)
                {
                    await _cloudSQL2DynamoMigrationService.uatb_MileagePlusValidation_CSS().ConfigureAwait(false);
                }
                else
                {
                    await _SQL2DynamoMigrationService.uatb_MileagePlusValidation_CSS().ConfigureAwait(false);
                }
                return;
            }
            _logger.LogInformation("utb_TSA_Flagged_Account not Enabled");

        }

        public async Task uatb_IsVBQWMDisplayed(SQL2DynamoDBJob apps_config)
        {
            var getCacheData = _cachingService.GetCache<string>("SQL2DynamoDBJobAppsettings_IsVBQWMDisplayed", "JOBD1").Result;
            _logger.LogInformation("Get configuration settings from Cache for IsVBQWMDisplayed table {@CacheResponse}", getCacheData);
            apps_config = JsonConvert.DeserializeObject<SQL2DynamoDBJob>(getCacheData);

            if (apps_config.Enable)
            {
                if (apps_config.RunCloudService)
                {
                    await _cloudSQL2DynamoMigrationService.uatb_IsVBQWMDisplayed().ConfigureAwait(false);
                }
                else
                {
                    await _SQL2DynamoMigrationService.uatb_IsVBQWMDisplayed().ConfigureAwait(false);
                }
                return;
            }
            _logger.LogInformation("uatb_IsVBQWMDisplayed not Enabled");

        }

        public async Task uatb_EResBetaTester(SQL2DynamoDBJob apps_config)
        {
            var getCacheData = _cachingService.GetCache<string>("SQL2DynamoDBJobAppsettings_EResBetaTester", "JOBD1").Result;
            _logger.LogInformation("Get configuration settings from Cache for EResBetaTester table {@CacheResponse}", getCacheData);
            apps_config = JsonConvert.DeserializeObject<SQL2DynamoDBJob>(getCacheData);

            if (apps_config.Enable)
            {
                if (apps_config.RunCloudService)
                {
                    await _cloudSQL2DynamoMigrationService.uatb_EResBetaTester().ConfigureAwait(false);
                }
                else
                {
                    await _SQL2DynamoMigrationService.uatb_EResBetaTester().ConfigureAwait(false);
                }
                return;
            }
            _logger.LogInformation("uatb_EResBetaTester not Enabled");

        }
    }
}
