using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.Shopping;

namespace United.Common.Helper.Profile
{
    public interface IMPTraveler
    {
        Task<(List<MOBCPTraveler> mobTravelersOwnerFirstInList, bool isProfileOwnerTSAFlagOn, List<MOBKVP> savedTravelersMPList)> PopulateTravelers(List<United.Services.Customer.Common.Traveler> travelers, string mileagePluNumber, bool isProfileOwnerTSAFlagOn, bool isGetCreditCardDetailsCall, MOBCPProfileRequest request, string sessionid, bool getMPSecurityDetails = false, string path = "");
        Task<MOBUpdateTravelerInfoResponse> UpdateTravelerMPId(string deviceId, string accessCode, string recordLocator, string sessionId, string transactionId, string languageCode, int applicationId, string appVersion, string mileagePlusNumber, string firstName, string lastName, string sharesPosition, string token);
        string GetYAPaxDescByDOB();
        List<Mobile.Model.Common.MOBAddress> PopulateTravelerAddresses(List<United.Services.Customer.Common.Address> addresses, MOBApplication application = null, string flow = null);
        List<MOBCPSecureTraveler> PopulatorSecureTravelers(List<United.Services.Customer.Common.SecureTraveler> secureTravelers, ref bool isTSAFlag, bool correctDate, string sessionID, int appID, string deviceID);
        Task<MOBCPMileagePlus> GetCurrentEliteLevelFromAirPreferences(List<United.Services.Customer.Common.AirPreference> airPreferences, string sessionid);
        List<MOBCPPhone> PopulatePhones(List<United.Services.Customer.Common.Phone> phones, bool onlyDayOfTravelContact);
        List<MOBEmail> PopulateEmailAddresses(List<Services.Customer.Common.Email> emailAddresses, bool onlyDayOfTravelContact);
        List<MOBPrefAirPreference> PopulateAirPrefrences(List<United.Services.Customer.Common.AirPreference> airPreferences);
        Task<List<MOBTypeOption>> GetProfileDisclaimerList();
        bool IsCorporateLeisureFareSelected(List<MOBSHOPTrip> trips);
        bool IsInternationalBilling(MOBApplication application, string countryCode, string flow);
        bool IsInternationalBillingAddress_CheckinFlowEnabled(MOBApplication application);
    }

}
