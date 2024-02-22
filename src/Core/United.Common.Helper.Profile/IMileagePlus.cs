using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.MPSignIn.MemberProfile;
using United.Service.Presentation.PaymentModel;

namespace United.Common.Helper.Profile
{
    public interface IMileagePlus
    {
        Task<MOBMPAccountSummary> GetAccountSummary(string transactionId, string mileagePlusNumber, string languageCode, bool includeMembershipCardBarCode, string sessionId = "");
        Task<(string employeeId, string displayEmployeeId)> GetEmployeeId(string mileageplusNumber, string transactionId, string sessionId, string displayEmployeeId);
        void GetMPEliteLevelExpirationDateAndGenerateBarCode(MOBMPAccountSummary mpSummary, string premierLevelExpirationDate, MOBInstantElite instantElite);
        Task<MOBPlusPoints> GetPlusPointsFromLoyaltyBalanceService(MOBMPAccountValidationRequest req, string dpToken);
        bool IsPremierStatusTrackerSupportedVersion(int appId, string appVersion);
        Task<bool> GetProfile_AllTravelerData(string mileagePlusNumber, string transactionId, string dpToken, int applicationId, string appVersion, string deviceId);
        Task<(MOBMPAccountSummary mpSummary, Service.Presentation.ProductResponseModel.Subscription merchOut)> GetAccountSummaryWithPremierActivityV2(MOBMPAccountValidationRequest req, bool includeMembershipCardBarCode, string dpToken);
        Task<List<MOBCancelledFFCPNRDetails>> GetMPFutureFlightCreditFromCancelReservationService
           (string mileagePlusNumber, int applicationId, string version, string sessionId, string transactionId, string deviceId, string callsource);
    }
}
