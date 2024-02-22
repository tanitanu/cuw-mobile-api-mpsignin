using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Tools;
using United.Utility.Helper;

namespace United.Mobile.MPSignInTool.Domain
{
    public class DynamoSearchToolBusiness : IDynamoSearchToolBusiness
    {
        private readonly ICacheLog<DynamoSearchToolBusiness> _logger;
        private readonly IDevelopmentService _devService;
        private readonly IQAService _qaService;
        private readonly IStageService _stageService;
        private readonly IProdService _prodService;
        private readonly IDevelopmentOnPremService _onPremDevservice;
        private readonly IQAOnPremService _onPremQAservice;
        private readonly IStageOnPremService _onPremStageService;
        private readonly IProdOnPremService _onPremProdService;

        public DynamoSearchToolBusiness(
            ICacheLog<DynamoSearchToolBusiness> logger,
            IDevelopmentService devService,
            IQAService qaService,
            IStageService stageService,
            IProdService prodService,
            IDevelopmentOnPremService developmentOnPremService,
            IQAOnPremService qAOnPremService,
            IStageOnPremService onPremStageService,
            IProdOnPremService onPremProdService)
        {
            _logger = logger;
            _devService = devService;
            _qaService = qaService;
            _stageService = stageService;
            _prodService = prodService;

            _onPremDevservice = developmentOnPremService;
            _onPremQAservice = qAOnPremService;
            _onPremStageService = onPremStageService;
            _onPremProdService = onPremProdService;
        }

        public async Task<List<MPSignInDynamoRecords>> GetRecordsFromDynamo(string secondaryKey, string env, string transactionId)
        {
            _logger.LogInformation("GetRecordsFromDynamo @{secondaryKey} {@env} {@transactionId}", secondaryKey, env, transactionId);
            try
            {
                string tableName = "cuw-validate-mp-appid-deviceid";

                string response = env switch
                {
                    "DevelopmentClient" => await _devService.GetAllValidateMpAppidDeviceIdByMP(tableName, secondaryKey, transactionId).ConfigureAwait(false),
                    "QAClient" => await _qaService.GetAllValidateMpAppidDeviceIdByMP(tableName, secondaryKey, transactionId).ConfigureAwait(false),
                    "StageClient" => await _stageService.GetAllValidateMpAppidDeviceIdByMP(tableName, secondaryKey, transactionId).ConfigureAwait(false),
                    "ProdClient" => await _prodService.GetAllValidateMpAppidDeviceIdByMP(tableName, secondaryKey, transactionId).ConfigureAwait(false),
                    _ => throw new ArgumentException("Selected Env is not valid."),
                };

                return JsonConvert.DeserializeObject<List<MPSignInDynamoRecords>>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error GetRecordsFromDynamo {@error} {@stacktrace}", ex.Message, ex.StackTrace);
            }
            return default;
        }

        public async Task<bool> ForceSignOut(string secondaryKey, string env, string transactionId)
        {
            _logger.LogInformation("ForceSignOut @{secondaryKey} {@env} {@transactionId}", secondaryKey, env, transactionId);
            try
            {
                string tableName = "cuw-validate-mp-appid-deviceid";

                bool response = false;
                bool dynamoResponse = false;
                bool sqlResponse = false;
                switch (env)
                {
                    case "DevelopmentClient":
                        dynamoResponse = await _devService.DynamoForceSignOut(tableName, secondaryKey, transactionId).ConfigureAwait(false);
                        sqlResponse = await _onPremDevservice.SQLForceSignOut(secondaryKey, transactionId).ConfigureAwait(false);
                        break;
                    case "QAClient":
                        dynamoResponse = await _qaService.DynamoForceSignOut(tableName, secondaryKey, transactionId).ConfigureAwait(false);
                        sqlResponse = await _onPremQAservice.SQLForceSignOut(secondaryKey, transactionId).ConfigureAwait(false);
                        break;
                    case "StageClient":
                        dynamoResponse = await _stageService.DynamoForceSignOut(tableName, secondaryKey, transactionId).ConfigureAwait(false);
                        sqlResponse = await _onPremStageService.SQLForceSignOut(secondaryKey, transactionId).ConfigureAwait(false);
                        break;
                    case "ProdClient":
                        dynamoResponse = await _prodService.DynamoForceSignOut(tableName, secondaryKey, transactionId).ConfigureAwait(false);
                        sqlResponse = await _onPremProdService.SQLForceSignOut(secondaryKey, transactionId).ConfigureAwait(false);
                        break;
                    default:
                        throw new ArgumentException("Selected Env is not valid.");
                }

                if (dynamoResponse && !sqlResponse)
                    response = true;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ForceSignOut {@error} {@stacktrace}", ex.Message, ex.StackTrace);
            }
            return default;

        }
    }
}