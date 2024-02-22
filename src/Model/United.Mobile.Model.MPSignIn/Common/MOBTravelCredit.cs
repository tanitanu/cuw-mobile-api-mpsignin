using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBTravelCredit
    {
        private string travelCreditAvailableText = string.Empty;
        private string travelCreditPageTitle = string.Empty;
        private string travelCreditHeaderTitle = string.Empty;
        private string travelCreditHeaderBody = string.Empty;
        private string travelCreditLearnAboutText = string.Empty;
        private string travelCertificateHeaderForTermsConditions;
        private string electronicTravelCertificateHeader = string.Empty;
        private string futureFlightCreditHeader = string.Empty;

        public List<MOBTravelCertificateOrFlightCredit> TravelCertificateOrFlightCreditActivity { get; set; }

        public string TravelCertificateHeaderForTermsConditions
        {
            get
            {
                return this.travelCertificateHeaderForTermsConditions;
            }
            set
            {
                this.travelCertificateHeaderForTermsConditions = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBTypeOption> TravelCertificateTermsConditions { get; set; }

        public string TravelCreditAvailableText
        {
            get
            {
                return this.travelCreditAvailableText;
            }
            set
            {
                this.travelCreditAvailableText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string TravelCreditPageTitle
        {
            get
            {
                return this.travelCreditPageTitle;
            }
            set
            {
                this.travelCreditPageTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string TravelCreditHeaderTitle
        {
            get
            {
                return this.travelCreditHeaderTitle;
            }
            set
            {
                this.travelCreditHeaderTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string TravelCreditHeaderBody
        {
            get
            {
                return this.travelCreditHeaderBody;
            }
            set
            {
                this.travelCreditHeaderBody = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string TravelCreditLearnAboutText
        {
            get
            {
                return this.travelCreditLearnAboutText;
            }
            set
            {
                this.travelCreditLearnAboutText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public MOBFutureFlightCreditDetails FutureFlightCreditDetails { get; set; }

        public string ElectronicTravelCertificateHeader
        {
            get
            {
                return this.electronicTravelCertificateHeader;
            }
            set
            {
                this.electronicTravelCertificateHeader = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string FutureFlightCreditHeader
        {
            get
            {
                return this.futureFlightCreditHeader;
            }
            set
            {
                this.futureFlightCreditHeader = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        [Serializable()]
        public class MOBTravelCertificateOrFlightCredit
        {
            private string futureFlightCreditLink = string.Empty;
            public List<MOBETCDetail> TravelCreditInfoBody { get; set; }

            public List<MOBETCDetail> TravelCreditInfoHeader { get; set; }
            public DateTime CertficateExpiryDate { get; set; }

            public string FutureFlightCreditLink
            {
                get
                {
                    return this.futureFlightCreditLink;
                }
                set
                {
                    this.futureFlightCreditLink = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
                }
            }
        }

        [Serializable()]
        public class MOBETCDetail
        {
            private string key = string.Empty;

            public string Key
            {
                get
                {
                    return this.key;
                }
                set
                {
                    this.key = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
                }
            }

            public List<string> Value { get; set; }
        }

        [Serializable()]
        public class MOBFutureFlightCreditDetails
        {
            private string pnr = string.Empty;
            private string btnText = string.Empty;
            private string multiFFCBtnText = string.Empty;

            public List<MOBCancelledFFCPNRDetails> CancelledFFCPNRList { get; set; }


            public string PNR
            {
                get
                {
                    return this.pnr;
                }
                set
                {
                    this.pnr = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
                }
            }
            public string LastName { get; set; } = string.Empty;
            public string MultiFFCText { get; set; } = string.Empty;
            public string MultiFFCBtnText
            {
                get
                {
                    return this.multiFFCBtnText;
                }
                set
                {
                    this.multiFFCBtnText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
                }
            }

            public string Text { get; set; } = string.Empty;
            public string BtnText
            {
                get
                {
                    return this.btnText;
                }
                set
                {
                    this.btnText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
                }
            }
        }
    }
}
