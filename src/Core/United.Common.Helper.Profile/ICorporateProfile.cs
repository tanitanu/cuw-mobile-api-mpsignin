using United.Mobile.Model;
using United.Mobile.Model.Common;

namespace United.Common.Helper.Profile
{
    public interface ICorporateProfile
    {
        MOBCPCorporate PopulateCorporateData(United.Services.Customer.Common.Corporate corporateData, MOBApplication application = null);
    }
}
