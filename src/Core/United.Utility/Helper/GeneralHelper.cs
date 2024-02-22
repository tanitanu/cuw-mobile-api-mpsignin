using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace United.Utility.Helper
{
    public class GeneralHelper
    {
        public static bool IsApplicationVersionGreaterorEqual(int applicationID, string appVersion, string androidnontfaversion, string iphonenontfaversion)
        {
            #region Nizam Code for version check
            bool ValidTFAVersion = false;
            if (!string.IsNullOrEmpty(appVersion))
            {
                string nonTFAVersion = string.Empty;
                nonTFAVersion = applicationID == 1 ? iphonenontfaversion : androidnontfaversion;

                Regex regex = new Regex("[0-9.]");
                appVersion = string.Join("",
                    regex.Matches(appVersion).Cast<Match>().Select(match => match.Value).ToArray());
                if (appVersion != nonTFAVersion)
                {
                    ValidTFAVersion = IsVersion1Greater(appVersion, nonTFAVersion);
                }
                else
                    ValidTFAVersion = true;
            }
            #endregion

            return ValidTFAVersion;
        }

        public static bool IsVersion1Greater(string version1, string version2)
        {
            return SeperatedVersionCompareCommonCode(version1, version2);
        }

        public static bool IsVersion1Greater(string version1, string version2, bool regexAppVersion)
        {
            Regex regex = new Regex("[0-9.]");
            version1 = string.Join("", regex.Matches(version1).Cast<Match>().Select(match => match.Value).ToArray());
            return SeperatedVersionCompareCommonCode(version1, version2);
        }

        public static bool SeperatedVersionCompareCommonCode(string version1, string version2)
        {
            try
            {
                #region
                string[] version1Arr = version1.Trim().Split('.');
                string[] version2Arr = version2.Trim().Split('.');

                if (Convert.ToInt32(version1Arr[0]) > Convert.ToInt32(version2Arr[0]))
                {
                    return true;
                }
                else if (Convert.ToInt32(version1Arr[0]) == Convert.ToInt32(version2Arr[0]))
                {
                    if (Convert.ToInt32(version1Arr[1]) > Convert.ToInt32(version2Arr[1]))
                    {
                        return true;
                    }
                    else if (Convert.ToInt32(version1Arr[1]) == Convert.ToInt32(version2Arr[1]))
                    {
                        if (Convert.ToInt32(version1Arr[2]) > Convert.ToInt32(version2Arr[2]))
                        {
                            return true;
                        }
                        else if (Convert.ToInt32(version1Arr[2]) == Convert.ToInt32(version2Arr[2]))
                        {
                            if (!string.IsNullOrEmpty(version1Arr[3]) && !string.IsNullOrEmpty(version2Arr[3]))
                            {
                                if (Convert.ToInt32(version1Arr[3]) > Convert.ToInt32(version2Arr[3]))
                                {
                                    return true;
                                }
                            }

                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        public static bool ValidateAccessCode(string accessCode)
        {
            bool ok = false; // Make this false after implemening the access code check.

            if (!string.IsNullOrEmpty(accessCode))
            {
                switch (accessCode.ToUpper())
                {
                    case "UAWS-MOBILE-ACCESSCODE":
                        ok = true;
                        break;

                    case "ACCESSCODE":
                        ok = true;
                        break;

                    case "59AD27EB-9B93-47C2-B275-D45EC7DC524F":
                        ok = true;
                        break;

                    case "UAWS-1456e190-87c3-4304-a068-d05a93c6695f":
                        ok = true;
                        break;
                }
            }
            return ok;
        }

        public static bool IsApplicationVersionGreater(int applicationID, string appVersion, string androidnontfaversion,
          string iphonenontfaversion, string windowsnontfaversion, string mWebNonELFVersion, bool ValidTFAVersion, IConfiguration _configuration)
        {
            #region Priya Code for version check

            if (!string.IsNullOrEmpty(appVersion))
            {
                string AndroidNonTFAVersion = _configuration.GetValue<string>(androidnontfaversion) ?? "";
                string iPhoneNonTFAVersion = _configuration.GetValue<string>(iphonenontfaversion) ?? "";
                string WindowsNonTFAVersion = _configuration.GetValue<string>(windowsnontfaversion) ?? "";
                string MWebNonTFAVersion = _configuration.GetValue<string>(mWebNonELFVersion) ?? "";

                Regex regex = new Regex("[0-9.]");
                appVersion = string.Join("",
                    regex.Matches(appVersion).Cast<Match>().Select(match => match.Value).ToArray());
                if (applicationID == 1 && appVersion != iPhoneNonTFAVersion)
                {
                    ValidTFAVersion = IsVersion1Greater(appVersion, iPhoneNonTFAVersion);
                }
                else if (applicationID == 2 && appVersion != AndroidNonTFAVersion)
                {
                    ValidTFAVersion = IsVersion1Greater(appVersion, AndroidNonTFAVersion);
                }
                else if (applicationID == 6 && appVersion != WindowsNonTFAVersion)
                {
                    ValidTFAVersion = IsVersion1Greater(appVersion, WindowsNonTFAVersion);
                }
                else if (applicationID == 16 && appVersion != MWebNonTFAVersion)
                {
                    ValidTFAVersion = IsVersion1Greater(appVersion, MWebNonTFAVersion);
                }
            }
            #endregion

            return ValidTFAVersion;
        }

        public static bool IsApplicationVersionGreater2(int applicationID, string appVersion, string androidnontfaversion,
            string iphonenontfaversion, string windowsnontfaversion, string mWebNonELFVersion, IConfiguration _configuration)
        {
            #region Nizam Code for version check
            bool ValidTFAVersion = false;
            if (!string.IsNullOrEmpty(appVersion))
            {
                string AndroidNonTFAVersion = _configuration.GetValue<string>(androidnontfaversion) ?? "";
                string iPhoneNonTFAVersion = _configuration.GetValue<string>(iphonenontfaversion) ?? "";
                string WindowsNonTFAVersion = _configuration.GetValue<string>(windowsnontfaversion) ?? "";
                string MWebNonTFAVersion = _configuration.GetValue<string>(mWebNonELFVersion) ?? "";

                Regex regex = new Regex("[0-9.]");
                appVersion = string.Join("",
                    regex.Matches(appVersion).Cast<Match>().Select(match => match.Value).ToArray());
                if (applicationID == 1 && appVersion != iPhoneNonTFAVersion)
                {
                    ValidTFAVersion = IsVersion1Greater(appVersion, iPhoneNonTFAVersion);
                }
                else if (applicationID == 2 && appVersion != AndroidNonTFAVersion)
                {
                    ValidTFAVersion = IsVersion1Greater(appVersion, AndroidNonTFAVersion);
                }
                else if (applicationID == 6 && appVersion != WindowsNonTFAVersion)
                {
                    ValidTFAVersion = IsVersion1Greater(appVersion, WindowsNonTFAVersion);
                }
                else if (applicationID == 16 && appVersion != MWebNonTFAVersion)
                {
                    ValidTFAVersion = IsVersion1Greater(appVersion, MWebNonTFAVersion);
                }
            }
            #endregion

            return ValidTFAVersion;
        }

        public static string FormatDate(string data, string languageCode)
        {
            string formattedDate = string.Empty;

            DateTime d;
            if (DateTime.TryParseExact(data, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
            {
                CultureInfo cultureInfo = null;
                try
                {
                    cultureInfo = new CultureInfo(languageCode);
                }
                catch (System.Exception)
                {
                    cultureInfo = new CultureInfo("en-US");
                }
                formattedDate = d.ToString("d", cultureInfo);
            }

            return formattedDate;
        }

        public static CultureInfo EnableUSCultureInfo()
        {
            var oldCultureInfo = CultureInfo.DefaultThreadCurrentCulture;
            var cultureInfo = new CultureInfo("en-US");
            // cultureInfo.NumberFormat.CurrencySymbol = "€";
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            return oldCultureInfo;
        }

        public static void DisableUSCultureInfo(CultureInfo ci)
        {
            if(ci != null)
            {
                CultureInfo.DefaultThreadCurrentCulture = ci;
                CultureInfo.DefaultThreadCurrentUICulture = ci;
            }
        }

        public static string FormatDateOfBirth(DateTime dateTime)
        {
            return $"{dateTime.Month.ToString("00")}/{ dateTime.Day.ToString("00")}/{ dateTime.Year}";
        }


        public static bool ValidateNonTFAVersion(int applicationID, string appVersion, IConfiguration _configuration)
        {
            bool TFASwitchON = _configuration.GetValue<bool>("TFASwitchON");
            if (TFASwitchON)
            {
                var androidnontfaversion = "AndroidNonTFAVersion";
                var iphonenontfaversion = "iPhoneNonTFAVersion";
                var windowsnontfaversion = "WindowsNonTFAVersion";
                var mWebNonELFVersion = "MWebNonELFVersion";

                TFASwitchON = IsApplicationVersionGreater(applicationID, appVersion, androidnontfaversion, iphonenontfaversion, windowsnontfaversion, mWebNonELFVersion, TFASwitchON, _configuration);
            }
            return TFASwitchON;
        }

        public static bool IsEnableU4BCorporateBooking(int applicationId, string appVersion, IConfiguration _configuration)
        {
            return _configuration.GetValue<bool>("EnableU4BCorporateBooking") && IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableU4BCorporateBooking_AppVersion"), _configuration.GetValue<string>("IPhone_EnableU4BCorporateBooking_AppVersion"));
        }

        public static bool IsEnableU4BTravelAddONPolicy(int applicationId, string appVersion, IConfiguration _configuration)
        {
            return _configuration.GetValue<bool>("EnableU4BTravelAddOnPolicy") && IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableU4BTravelAddOnPolicy_AppVersion"), _configuration.GetValue<string>("IPhone_EnableU4BTravelAddOnPolicy_AppVersion"));
        }

        public static string RemoveCarriageReturn(string carriage)
        {
            return carriage.Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ');
        }
    }
}
