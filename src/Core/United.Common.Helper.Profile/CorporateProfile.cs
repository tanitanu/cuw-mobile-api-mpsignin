using Microsoft.Extensions.Configuration;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.Shopping;
using United.Utility.Helper;

namespace United.Common.Helper.Profile
{
    public class CorporateProfile : ICorporateProfile
    {
        private readonly IConfiguration _configuration;
        public CorporateProfile(
             IConfiguration configuration
            )
        {
            _configuration = configuration;

        }

        public MOBCPCorporate PopulateCorporateData(United.Services.Customer.Common.Corporate corporateData, MOBApplication application = null)
        {
            MOBCPCorporate profileCorporateData = new MOBCPCorporate();
            if (corporateData != null && corporateData.IsValid)
            {
                profileCorporateData.CompanyName = corporateData.CompanyName;
                profileCorporateData.DiscountCode = corporateData.DiscountCode;
                profileCorporateData.FareGroupId = corporateData.FareGroupId;
                profileCorporateData.IsValid = corporateData.IsValid;
                profileCorporateData.VendorId = corporateData.VendorId;
                profileCorporateData.VendorName = corporateData.VendorName;
                if (IsEnableCorporateLeisureBooking(application))
                {
                    profileCorporateData.LeisureDiscountCode = corporateData.LeisureCode;
                }
                if (_configuration.GetValue<bool>("EnableIsArranger"))
                {
                    if (!string.IsNullOrEmpty(corporateData.IsArranger) && corporateData.IsArranger.ToUpper().Equals("TRUE"))
                    {
                        profileCorporateData.NoOfTravelers = string.IsNullOrEmpty(_configuration.GetValue<string>("TravelArrangerCount")) ? 1 : _configuration.GetValue<int>("TravelArrangerCount");
                        profileCorporateData.CorporateBookingType = CORPORATEBOOKINGTYPE.TravelArranger.ToString();
                    }
                }
            }
            return profileCorporateData;
        }
        private bool IsEnableCorporateLeisureBooking(MOBApplication application)
        {
            if (_configuration.GetValue<bool>("EnableCorporateLeisure"))
            {
                if (application != null && GeneralHelper.IsApplicationVersionGreater(application.Id, application.Version.Major, "CorpLiesureAndroidVersion", "CorpLiesureiOSVersion", "", "", true, _configuration))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
