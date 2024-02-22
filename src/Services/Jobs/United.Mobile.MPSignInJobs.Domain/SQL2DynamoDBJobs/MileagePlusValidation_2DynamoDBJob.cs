using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Common;
using United.Mobile.MPSignInJobs.Domain.SQL2DynamoDBJobs;
using United.Utility.Helper;

namespace United.Mobile.MPSignInJobs.Domain
{
    public class MileagePlusValidation_2DynamoDBJob : IHostedService, IDisposable
    {
        private readonly ICacheLog<MileagePlusValidation_2DynamoDBJob> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISQL2DynamoDBJobBusiness _sql2DynamoDBJobBusiness;
        private readonly ICachingService _cachingService;

        private Timer timer;
        private int iterationIndex;

        public MileagePlusValidation_2DynamoDBJob(
            ICacheLog<MileagePlusValidation_2DynamoDBJob> logger
            , IConfiguration configuration
            , ISQL2DynamoDBJobBusiness sql2DynamoDBJobBusiness
            , ICachingService cachingService
            )
        {
            _sql2DynamoDBJobBusiness = sql2DynamoDBJobBusiness;
            _logger = logger;
            _configuration = configuration;
            _cachingService = cachingService;
        }
        public void Dispose()
        {
            timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //var getCacheData = _cachingService.GetCache<string>("SQL2DynamoDBJobAppsettings_MileagePlusValidation", "JOBD1").Result;
            //_logger.LogInformation("Get configuration settings from Cache for MileagePlusValidation table {@CacheResponse}", getCacheData);
            //var result = JsonConvert.DeserializeObject<SQL2DynamoDBJob>(getCacheData);
            //var triggerJobInSeconds = result.ExpiryTimeOut == 0 ? 3600 : result.ExpiryTimeOut;

            var startFlag = _configuration.GetValue<bool>("EnableMPSignInJobs");
            if (startFlag)
            {

                var triggerJobInSeconds = _configuration.GetValue<int>("TriggerMileagePlusValidation_2DynamoDBJobInSeconds") == 0 ? 3600 :
                                _configuration.GetValue<int>("TriggerMileagePlusValidation_2DynamoDBJobInSeconds");
                SQL2DynamoDBJob result = new SQL2DynamoDBJob();

                var delayStart = TimeSpan.FromSeconds(5); // 5 seconds after startup completed

                timer = new Timer(tsk =>
                {
                    try
                    {
                        _logger.LogInformation("MileagePlusValidation_2DynamoDBJob Started Iteration : {iterationIndex}", iterationIndex);
                        Interlocked.Increment(ref iterationIndex);
                        _sql2DynamoDBJobBusiness.uatb_MileagePlusValidation(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("MileagePlusValidation_2DynamoDBJob failed at Iteration : {iterationIndex} with exception: {message}", iterationIndex, ex.Message);

                    }

                }, null, delayStart, TimeSpan.FromSeconds(triggerJobInSeconds));
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
