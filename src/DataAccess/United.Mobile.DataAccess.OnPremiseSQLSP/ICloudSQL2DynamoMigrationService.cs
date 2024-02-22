using System.Threading.Tasks;

namespace United.Mobile.DataAccess.OnPremiseSQLSP
{
    public interface ICloudSQL2DynamoMigrationService
    {
        Task uatb_Device(string sessionID = "sessionID");
        Task uatb_Device_History();
        Task uatb_DevicePushToken();
        Task uatb_MileagePlusValidation_CSS();
        Task uatb_MileagePlusValidation();
        Task uatb_IsVBQWMDisplayed();
        Task uatb_EResBetaTester();
    }
}