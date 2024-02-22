using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBApplePay
    {
        public string CardNameWithLastFourDigits { get; set; }

        public string ApplePayLoadJSON { get; set; }

        public MOBAddress BillingAddress { get; set; }

        public MOBEmail EmailAddress { get; set; }

        public MOBName CardHolderName { get; set; }

        public string LastFourDigits
        {
            get
            {
                string fourDigits = string.Empty;
                if (!String.IsNullOrEmpty(this.CardNameWithLastFourDigits) && this.CardNameWithLastFourDigits.Length >= 4)
                    fourDigits = this.CardNameWithLastFourDigits.Substring(this.CardNameWithLastFourDigits.Length - 4).Trim();

                return fourDigits;
            }
        }

        public string CardName
        {
            get
            {
                string cardName = string.Empty;
                if (!String.IsNullOrEmpty(this.CardNameWithLastFourDigits))
                    cardName = FilterAlphabetsFromString(this.CardNameWithLastFourDigits); //this.cardNameWithLastFourDigits.Substring(0, this.cardNameWithLastFourDigits.Length - 4).Trim();

                return cardName;
            }
        }

        public string CurrencyCode
        {
            get { return "USD"; }
        }


        public static string FilterAlphabetsFromString(string numericText)
        {
            string allNumerics = string.Empty;
            var regex = new Regex(@"[^A-Za-z]");
            if (!string.IsNullOrEmpty(numericText))
            {
                allNumerics = regex.Replace(numericText, "");
            }
            return allNumerics;

        }


    }

    
}
