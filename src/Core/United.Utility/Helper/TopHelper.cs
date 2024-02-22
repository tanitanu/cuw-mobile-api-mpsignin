using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using United.Mobile.Model;
using United.Mobile.Model.Internal.Exception;
using United.Utility.Enum;
using United.Utility.Extensions;

namespace United.Utility.Helper
{

    public class TopHelper
    {

        public static string FormatAmountForDisplay(string amt, CultureInfo ci, /*string currency,*/ bool roundup = true, bool isAward = false)
        {
            string newAmt = amt;
            decimal amount = 0;
            decimal.TryParse(amt, out amount);

            try
            {
                RegionInfo ri = new RegionInfo(ci?.Name);

                switch (ri.ISOCurrencySymbol.ToUpper())
                {
                    case "JPY":
                    case "EUR":
                    case "CAD":
                    case "GBP":
                    case "CNY":
                    case "USD":
                    case "AUD":
                    default:
                        newAmt = GetCurrencySymbol(ci, amount, roundup);
                        break;
                }

            }
            catch { }

            return isAward ? "+ " + newAmt : newAmt;
        }
        public static string GetCurrencySymbol(CultureInfo ci, /*string currencyCode,*/ decimal amount, bool roundup)
        {
            string result = string.Empty;

            try
            {
                if (amount > -1)
                {
                    if (roundup)
                    {
                        int newTempAmt = (int)decimal.Ceiling(amount);
                        try
                        {
                            var ri = new RegionInfo(ci.Name);
                            CultureInfo tempCi = Thread.CurrentThread.CurrentCulture;
                            Thread.CurrentThread.CurrentCulture = ci;
                            result = newTempAmt.ToString("c0");
                            Thread.CurrentThread.CurrentCulture = tempCi;

                        }
                        catch { }
                    }
                    else
                    {
                        try
                        {
                            var ri = new RegionInfo(ci.Name);
                            CultureInfo tempCi = Thread.CurrentThread.CurrentCulture;
                            Thread.CurrentThread.CurrentCulture = ci;
                            result = amount.ToString("c");
                            Thread.CurrentThread.CurrentCulture = tempCi;

                        }
                        catch { }
                    }
                }
                else
                {
                    if (roundup)
                    {
                        int newTempAmt = (int)decimal.Ceiling(amount);
                        //var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

                        //foreach (var ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
                        //{
                        try
                        {
                            var ri = new RegionInfo(ci.Name);
                            //if (ri.ISOCurrencySymbol.ToUpper() == currencyCode.ToUpper())
                            //{
                            //result = ri.CurrencySymbol;
                            CultureInfo tempCi = Thread.CurrentThread.CurrentCulture;
                            Thread.CurrentThread.CurrentCulture = ci;
                            //result = newTempAmt.ToString("c0", new CultureInfo("en-US"));
                            result = newTempAmt>0? newTempAmt.ToString("c0", new CultureInfo("en-US")): string.Format("({0})", (-newTempAmt).ToString("c0", new CultureInfo("en-US")));
                            Thread.CurrentThread.CurrentCulture = tempCi;
                            //break;
                            //}
                        }
                        catch { }
                        //}
                        //newAmt = newTempAmt.ToString();
                    }
                }

            }
            catch { }

            return result;
        }
        public static CultureInfo GetCultureInfo(string currencyCode)
        {
            CultureInfo culture = new CultureInfo("en-US");

            if (!string.IsNullOrEmpty(currencyCode))
            {
                var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

                foreach (var ci in cultures)
                {
                    try
                    {
                        var ri = new RegionInfo(ci.Name);
                        if (ri.ISOCurrencySymbol.ToUpper() == currencyCode.ToUpper())
                        {
                            culture = ci;
                            break;
                        }
                    }
                    catch { culture = new CultureInfo("en-US"); }
                }
            }

            return culture;
        }

        public static int GetAgeByDOB(string birthDate, string firstLOFDepDate)
        {
            var travelDate = DateTime.Parse(firstLOFDepDate);

            var birthDate1 = DateTime.Parse(birthDate);
            // Calculate the age.
            var age = travelDate.Year - birthDate1.Year;
            // Go back to the year the person was born in case of a leap year
            if (birthDate1 > travelDate.AddYears(-age)) age--;

            return age;
        }

        public static string ExceptionMessages(Exception ex)
        {
            if (ex.InnerException == null)
            {
                return ex.Message;
            }

            return ex.Message + " | " + ExceptionMessages(ex.InnerException);
        }

        public static bool IsYoungAdult(string birthDate)
        {
            if (string.IsNullOrEmpty(birthDate))
            {
                return false;
            }

            int age = GetAgeByDOB(birthDate, DateTime.Today.Date.ToString());
            return (age > 17 && age < 24);
        }
    }
}
