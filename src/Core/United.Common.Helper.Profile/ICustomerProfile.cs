using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.Model;
using United.Mobile.Model.Common;

namespace United.Common.Helper.Profile
{
    public interface ICustomerProfile
    {
        Task<List<MOBCPProfile>> PopulateProfiles(string sessionId, string mileagePlusNumber, int customerId, List<United.Services.Customer.Common.Profile> profiles, MOBCPProfileRequest request, bool getMPSecurityDetails = false, string path = "", MOBApplication application = null);
        Task<MOBCustomerPreferencesResponse> RetrieveCustomerPreferences(MOBCustomerPreferencesRequest request, string token);
        Task<MOBCPProfile> GeteMailIDTFAMPSecurityDetails(MOBMPPINPWDValidateRequest request, string token);
        bool EnableYoungAdult(bool isReshop = false);
        Task<bool> ValidateMPNames(string token, string langCode, string title, string firstName, string middleName, string lastName, string suffix, string mileagePlusId, string sessionId, int appId, string appVersion, string deviceId);
    }
}
