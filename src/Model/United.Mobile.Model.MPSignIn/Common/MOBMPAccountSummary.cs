using System;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBMPAccountSummary
    {
        private string mileagePlusNumber = string.Empty;
        private string balance = "0";
        private string balanceExpireDate = string.Empty;
        private string balanceExpireDisclaimer = string.Empty;
        private string noMileageExpiration = string.Empty;
        private string noMileageExpirationMessage = string.Empty;
        private string enrollDate = string.Empty;
        private string lastFlightDate = string.Empty;
        private string lastActivityDate = string.Empty;

        private string eliteMileage = "0";
        private string eliteSegment = "0";

        private string lastExpiredMileDate = string.Empty;
        //private MOBUnitedClubMemberShipDetails uAClubMemberShipDetails
        private string tsaMessage = string.Empty;

        private string fourSegmentMinimun = string.Empty;
        private string premierQualifyingDollars = string.Empty;
        public string pDQchasewavier { get; set; } = string.Empty;
        public string pDQchasewaiverLabel { get; set; } = string.Empty;
        private string millionMilerIndicator = string.Empty;

        private string membershipCardBarCodeString;
        private string hashValue;

        private string membershipCardExpirationDate = string.Empty;

        private string milesNeverExpireText;
        private string learnMoreTitle;
        private string learnMoreHeader;
        //private MOBVBQPremierActivity vBQPremierActivity;
        //private MOBVBQWelcomeModel vBQWelcomeModel;


        public MOBMPAccountSummary()
        {
        }

        public string BirthDate { get; set; }
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        [XmlIgnore]
        public long CustomerId { get; set; }

        public MOBName Name { get; set; }

        public string Balance
        {
            get
            {
                //The below commented changes made by Naresh to fix the mileage Plus balance summary(Mobile Web QC# 914) has broken iPhone and Android display issue and caused Android to crash - Venkat 04/17/2012
                //if (!String.IsNullOrEmpty(this.balance)) {
                //    int bal = int.Parse(balance);
                //    if (bal > 0) {
                //        string formattedBal = bal.ToString("#,#");
                //        return formattedBal;
                //    }
                //    return this.balance;
                //}
                return this.balance;
            }
            set
            {
                this.balance = string.IsNullOrEmpty(value) ? "0" : value.Trim();
            }
        }

        [System.Text.Json.Serialization.JsonIgnoreAttribute]
        [Newtonsoft.Json.JsonIgnore]
        public string FormattedBalance
        {
            get
            {
                if (!String.IsNullOrEmpty(this.balance))
                {
                    int bal = int.Parse(balance);
                    if (bal > 0)
                    {
                        string formattedBal = bal.ToString("#,#");
                        return formattedBal;
                    }
                    return this.balance;
                }
                return String.Empty;
            }
        }

        public string BalanceExpireDate
        {
            get
            {
                return this.balanceExpireDate;
            }
            set
            {
                this.balanceExpireDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string BalanceExpireDisclaimer
        {
            get
            {
                return this.balanceExpireDisclaimer;
            }
            set
            {
                this.balanceExpireDisclaimer = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string NoMileageExpiration
        {
            get
            {
                return this.noMileageExpiration;
            }
            set
            {
                this.noMileageExpiration = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string NoMileageExpirationMessage
        {
            get
            {
                return this.noMileageExpirationMessage;
            }
            set
            {
                this.noMileageExpirationMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBEliteStatus EliteStatus { get; set; }

        public string EnrollDate
        {
            get
            {
                return this.enrollDate;
            }
            set
            {
                this.enrollDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LastFlightDate
        {
            get
            {
                return this.lastFlightDate;
            }
            set
            {
                this.lastFlightDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LastActivityDate
        {
            get
            {
                return this.lastActivityDate;
            }
            set
            {
                this.lastActivityDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EliteMileage
        {
            get
            {
                return this.eliteMileage;
            }
            set
            {
                this.eliteMileage = string.IsNullOrEmpty(value) ? "0" : value.Trim();
            }
        }

        public string EliteSegment
        {
            get
            {
                return this.eliteSegment;
            }
            set
            {
                this.eliteSegment = string.IsNullOrEmpty(value) ? "0" : value.Trim();
            }
        }

        public string LastExpiredMileDate
        {
            get
            {
                return this.lastExpiredMileDate;
            }
            set
            {
                this.lastExpiredMileDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int LastExpiredMile { get; set; } = 0;

        public MOBUnitedClubMemberShipDetails uAClubMemberShipDetails { get; set; }
        //{
        //    get
        //    {
        //        return this.uAClubMemberShipDetails;
        //    }
        //    set
        //    {
        //        this.uAClubMemberShipDetails = value;
        //    }
        //}

        public bool HasUAClubMemberShip { get; set; }

        public bool IsMPAccountTSAFlagON { get; set; }

        public string TSAMessage
        {
            get
            {
                return this.tsaMessage;
            }
            set
            {
                this.tsaMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FourSegmentMinimun
        {
            get
            {
                return this.fourSegmentMinimun;
            }
            set
            {
                this.fourSegmentMinimun = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PremierQualifyingDollars
        {
            get
            {
                return this.premierQualifyingDollars;
            }
            set
            {
                this.premierQualifyingDollars = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string MillionMilerIndicator
        {
            get
            {
                return this.millionMilerIndicator;
            }
            set
            {
                this.millionMilerIndicator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public byte[] MembershipCardBarCode { get; set; }

        public string MembershipCardBarCodeString
        {
            get
            {
                return this.membershipCardBarCodeString;
            }
            set
            {
                this.membershipCardBarCodeString = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsCEO { get; set; }

        public string HashValue
        {
            get
            {
                return this.hashValue;
            }
            set
            {
                this.hashValue = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string MembershipCardExpirationDate
        {
            get
            {
                return this.membershipCardExpirationDate;
            }
            set
            {
                this.membershipCardExpirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool ShowChaseBonusTile { get; set; }

        public int LifetimeMiles { get; set; }

        public string ChasePromoType { get; set; }

        //Start - 221924 - Added by Nizam on 12/19/2017
        //private MOBMPStatusLiftBanner statusLiftBanner;

        public MOBMPStatusLiftBanner statusLiftBanner { get; set; }
        //{
        //    get { return statusLiftBanner; }
        //    set { statusLiftBanner = value; }
        //}
        //End - 221924 - Added by Nizam on 12/19/2017
        public MOBPremierActivity PremierActivity { get; set; }
        public MOBYearEndPremierActivity YearEndPremierActivity { get; set; }
        public int PremierActivityType { get; set; } //1=PremierActivity;2=YearEndActivity;3=Exception
        public string PremierTrackerLearnAboutTitle { get; set; }
        public string PremierTrackerLearnAboutHeader { get; set; }
        public string PremierTrackerLearnAboutText { get; set; }
        public MOBErrorPremierActivity ErrorPremierActivity { get; set; }
        public string PremierStatusTrackerText { get; set; }
        public string PremierStatusTrackerLink { get; set; }
        public bool IsHideMileageBalanceExpireDate { get; set; }
        public bool IsIncrementalUpgrade { get; set; }
        public MOBPlusPoints PlusPoints { get; set; }
        public string MilesNeverExpireText
        {
            get
            {
                return this.milesNeverExpireText;
            }
            set
            {
                this.milesNeverExpireText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string LearnMoreTitle
        {
            get
            {
                return this.learnMoreTitle;
            }
            set
            {
                this.learnMoreTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string LearnMoreHeader
        {
            get
            {
                return this.learnMoreHeader;
            }
            set
            {
                this.learnMoreHeader = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public MOBVBQWelcomeModel vBQWelcomeModel { get; set; }
        //{
        //    get
        //    {
        //        return this.vBQWelcomeModel;
        //    }
        //    set
        //    {
        //        this.vBQWelcomeModel = value;
        //    }
        //}
        public MOBVBQPremierActivity vBQPremierActivity { get; set; }
        //    get
        //    {
        //        return this.vBQPremierActivity;
        //    }
        //    set
        //    {
        //        this.vBQPremierActivity = value;
        //    }
        //}
        public MOBVBQYearEndPremierActivity YearEndVBQPremierActivity { get; set; }
        public bool IsChaseCardHolder { get; set; }

        public MOBTravelCredit TravelCreditInfo { get; set; }

    }
}
