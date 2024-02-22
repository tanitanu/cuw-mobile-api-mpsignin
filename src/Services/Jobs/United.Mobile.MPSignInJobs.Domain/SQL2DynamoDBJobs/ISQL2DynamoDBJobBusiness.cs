using System.Threading.Tasks;
using United.Mobile.MPSignInJobs.Domain.SQL2DynamoDBJobs;

namespace United.Mobile.MPSignInJobs.Domain
{
    public interface ISQL2DynamoDBJobBusiness
    {
        Task uatb_Device(SQL2DynamoDBJob apps_config);
        Task uatb_Device_History(SQL2DynamoDBJob apps_config);
        Task uatb_DevicePushToken(SQL2DynamoDBJob apps_config);
        Task uatb_MileagePlusValidation_CSS(SQL2DynamoDBJob apps_config);
        Task uatb_MileagePlusValidation(SQL2DynamoDBJob apps_config);
        Task utb_TSA_Flagged_Account(SQL2DynamoDBJob apps_config);
        Task uatb_IsVBQWMDisplayed(SQL2DynamoDBJob apps_config);
        Task uatb_EResBetaTester(SQL2DynamoDBJob apps_config);
    }
}
