using IDAutomation.NetStandard.PDF417.FontEncoder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.CSLSerivce;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.Model.Common;
using United.Mobile.Model.CSLModels;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.Internal.Exception;
using United.Service.Presentation.PaymentModel;
using United.Services.Customer.Common;
using United.TravelBank.Model;
using United.TravelBank.Model.BalancesDataModel;
using United.Utility.Helper;

namespace United.Common.Helper.Profile
{
    public class MileagePlus : IMileagePlus
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheLog<MileagePlus> _logger;
        private readonly IDPService _dPService;
        private readonly ILoyaltyAccountService _loyaltyAccountService;
        private readonly ILoyaltyWebService _loyaltyWebService;
        private readonly ICustomerDataService _customerDataService;
        private readonly ILoyaltyUCBService _loyaltyBalanceServices;
        private readonly IEmployeeService _employeeIdByMileageplusNumber;
        private readonly IMPFutureFlightCredit _mPFutureFlightCredit;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IMerchandizingServices _merchandizingServices;
        private readonly IHeaders _headers;
        private readonly IDynamoDBUtility _dynamoDBUtility;

        public MileagePlus(IConfiguration configuration
            , ICacheLog<MileagePlus> logger
            , IDPService dPService
            , ILoyaltyAccountService loyaltyAccountService
            , ILoyaltyWebService loyaltyWebService
            , ICustomerDataService customerDataService
            , IEmployeeService employeeIdByMileageplusNumber
            , ILoyaltyUCBService loyaltyBalanceServices
            , IMPFutureFlightCredit mPFutureFlightCredit
            , ISessionHelperService sessionHelperService
            , IMerchandizingServices merchandizingServices
            , IHeaders headers
            , IDynamoDBHelperService dynamoDBHelperService
            , IDynamoDBUtility dynamoDBUtility
           )
        {
            _configuration = configuration;
            _logger = logger;
            _dPService = dPService;
            _loyaltyAccountService = loyaltyAccountService;
            _loyaltyWebService = loyaltyWebService;
            _customerDataService = customerDataService;
            _employeeIdByMileageplusNumber = employeeIdByMileageplusNumber;
            _loyaltyBalanceServices = loyaltyBalanceServices;
            _mPFutureFlightCredit = mPFutureFlightCredit;
            _sessionHelperService = sessionHelperService;
            _merchandizingServices = merchandizingServices;
            _headers = headers;
            _dynamoDBUtility = dynamoDBUtility;
        }

        public async Task<MOBMPAccountSummary> GetAccountSummary(string transactionId, string mileagePlusNumber, string languageCode, bool includeMembershipCardBarCode, string sessionId = "")
        {
            if (_configuration.GetValue<bool>("EnableUCBPhase1_MobilePhase1Changes"))
            {
                return await GetAccountSummaryV2(transactionId, mileagePlusNumber, languageCode, includeMembershipCardBarCode, sessionId).ConfigureAwait(false);
            }
            else
            {

                MOBMPAccountSummary mpSummary = new MOBMPAccountSummary();
                AccountProfileInfoResponse objloyaltyProfileResponse = new AccountProfileInfoResponse();

                if (_configuration.GetValue<bool>("AddingExtraLogggingForAwardShopAccountSummary_Toggle"))
                {
                    if (string.IsNullOrWhiteSpace(mileagePlusNumber))
                    {
                        _logger.LogError("GetAccountSummary - Empty MPNumber Passed");
                    }
                }

                try
                {
                    bool fourSegmentMinimunWaivedMember = false;

                    string balanceExpireDisclaimer = string.Empty;
                    bool noMileageExpiration = false;

                    var _token = await _dPService.GetAnonymousToken(_headers.ContextValues.Application.Id, _headers.ContextValues.DeviceId, _configuration).ConfigureAwait(false);

                    var loyaltyProfileResponse = await _loyaltyAccountService.GetAccountProfileInfo<AccountProfileInfoResponse>(_token, mileagePlusNumber, _headers.ContextValues.SessionId).ConfigureAwait(false);

                    #region 55359, 81220 Bug Fix
                    //55359 and 81220 to check for closed, temporary closed and ClosedPermanently account-Alekhya 
                    if (loyaltyProfileResponse != null && loyaltyProfileResponse.AccountProfileInfo != null && (loyaltyProfileResponse.AccountProfileInfo.IsClosedTemporarily || loyaltyProfileResponse.AccountProfileInfo.IsClosedPermanently || loyaltyProfileResponse.AccountProfileInfo.IsClosed))
                    {
                        string exceptionMessage = _configuration.GetValue<string>("ErrorContactMileagePlus");
                        throw new MOBUnitedException(exceptionMessage);
                    }
                    //Changes end here
                    #endregion 55359, 81220 Bug Fix
                    if (loyaltyProfileResponse.AccountProfileInfo.BirthDate != null)
                    {
                        mpSummary.BirthDate = loyaltyProfileResponse.AccountProfileInfo.BirthDate.ToString();
                    }

                    mpSummary.Balance = loyaltyProfileResponse.AccountProfileInfo.CurrentBalance.ToString();
                    mpSummary.LastActivityDate = loyaltyProfileResponse.AccountProfileInfo.LastActivityDate != 0 ? DateTime.ParseExact(loyaltyProfileResponse.AccountProfileInfo.LastActivityDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy") : "";
                    mpSummary.LearnMoreTitle = _configuration.GetValue<string>("MilagePluslearnMoreText");
                    mpSummary.LearnMoreHeader = _configuration.GetValue<string>("MilagePluslearnMoreDesc");
                    mpSummary.MilesNeverExpireText = _configuration.GetValue<string>("MilagePlusMilesNeverExpire");
                    if (_configuration.GetValue<bool>("HideMileageBalanceExpireDate") == true)
                    {
                        mpSummary.BalanceExpireDate = "";
                        mpSummary.IsHideMileageBalanceExpireDate = true;
                    }
                    else
                    {
                        mpSummary.BalanceExpireDate = loyaltyProfileResponse.AccountProfileInfo.MilesExpireDate != 0 ? DateTime.ParseExact(loyaltyProfileResponse.AccountProfileInfo.MilesExpireDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy") : "";
                    }
                    mpSummary.BalanceExpireDisclaimer = _configuration.GetValue<bool>("HideMileageBalanceExpireDate") ? HttpUtility.HtmlDecode(_configuration.GetValue<string>("BalanceExpireDisclaimerNeverExpire")) : HttpUtility.HtmlDecode(_configuration.GetValue<string>("BalanceExpireDisclaimer"));
                    mpSummary.CustomerId = loyaltyProfileResponse.AccountProfileInfo.CustomerId;
                    mpSummary.EliteMileage = string.Format("{0:###,##0}", loyaltyProfileResponse.AccountProfileInfo.PremierQualifyingMilesBalance);
                    if (loyaltyProfileResponse.AccountProfileInfo.PremierQualifyingSegmentBalance == 0)
                    {
                        mpSummary.EliteSegment = "0";
                    }
                    else
                    {
                        mpSummary.EliteSegment = string.Format("{0:0.#}", loyaltyProfileResponse.AccountProfileInfo.PremierQualifyingSegmentBalance);
                    }
                    // test comments
                    mpSummary.EliteStatus = new MOBEliteStatus(_configuration)
                    {
                        Code = loyaltyProfileResponse.AccountProfileInfo.EliteLevel
                    };
                    mpSummary.EnrollDate = loyaltyProfileResponse.AccountProfileInfo.EnrollDate.ToString("MM/dd/yyyy");
                    //mpSummary.HasUAClubMemberShip = loyaltyProfileResponse.AccountProfileInfo.IsUnitedClubMember;
                    //mpSummary.LastExpiredMileDate = DateTime.ParseExact(loyaltyProfileResponse.AccountProfileInfo.MilesExpireLastActivityDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy");
                    mpSummary.LastFlightDate = (loyaltyProfileResponse.AccountProfileInfo.LastFlightDate != 0) ? DateTime.ParseExact(loyaltyProfileResponse.AccountProfileInfo.LastFlightDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy") : "";
                    mpSummary.MileagePlusNumber = loyaltyProfileResponse.AccountProfileInfo.AccountId;
                    mpSummary.Name = new MOBName
                    {
                        First = loyaltyProfileResponse.AccountProfileInfo.FirstName,
                        Last = loyaltyProfileResponse.AccountProfileInfo.LastName,
                        Middle = loyaltyProfileResponse.AccountProfileInfo.MiddleName,
                        Suffix = loyaltyProfileResponse.AccountProfileInfo.Suffix,
                        Title = loyaltyProfileResponse.AccountProfileInfo.Title
                    };

                    mpSummary.IsCEO = loyaltyProfileResponse.AccountProfileInfo.IsCeo;

                    mpSummary.LifetimeMiles = loyaltyProfileResponse.AccountProfileInfo.LifetimeMiles;

                    if (loyaltyProfileResponse.AccountProfileInfo.MillionMilerLevel == 0)
                    {
                        mpSummary.MillionMilerIndicator = string.Empty;
                    }
                    else
                    {
                        mpSummary.MillionMilerIndicator = loyaltyProfileResponse.AccountProfileInfo.MillionMilerLevel.ToString();
                    }

                    if (_configuration.GetValue<DateTime>("MP2014EnableDate") < DateTime.Now)
                    {
                        if (loyaltyProfileResponse != null && loyaltyProfileResponse.AccountProfileInfo != null)
                        {
                            bool isValidPqdAddress = false;
                            bool activeNonPresidentialPlusCardMember = false;
                            bool activePresidentialPlusCardMembe = false;
                            bool showChaseBonusTile = false;

                            //Migrate XML to CSL service call
                            //[CLEANUP API-MIGRATION]  Removed XML Service Calls
                            if (_configuration.GetValue<bool>("NewServieCall_GetProfile_PaymentInfos"))
                            {
                                var tupleRes = await IsValidPQDAddressV2("GetAccountSummary", transactionId, _token, mpSummary.MileagePlusNumber, isValidPqdAddress, activeNonPresidentialPlusCardMember, activePresidentialPlusCardMembe, fourSegmentMinimunWaivedMember, showChaseBonusTile).ConfigureAwait(false);
                                isValidPqdAddress = tupleRes.isValidPqdAddress;
                                activeNonPresidentialPlusCardMember = tupleRes.activeNonPresidentialPlusCardMember;
                                activePresidentialPlusCardMembe = tupleRes.activePresidentialPlusCardMembe;
                                showChaseBonusTile = tupleRes.isValidPqdAddress;
                                fourSegmentMinimunWaivedMember = tupleRes.fourSegmentMinimunWaivedMember;

                            }

                            mpSummary.ShowChaseBonusTile = showChaseBonusTile;
                            AssignCustomerChasePromoType(mpSummary, showChaseBonusTile);

                            noMileageExpiration = activeNonPresidentialPlusCardMember || activePresidentialPlusCardMembe;

                            if (fourSegmentMinimunWaivedMember)
                            {
                                mpSummary.FourSegmentMinimun = "Waived";
                            }
                            else if (loyaltyProfileResponse.AccountProfileInfo.MinimumSegment >= 4)
                            {
                                mpSummary.FourSegmentMinimun = "4 of 4";
                            }
                            else
                            {
                                mpSummary.FourSegmentMinimun = string.Format("{0} of 4", loyaltyProfileResponse.AccountProfileInfo.MinimumSegment);
                            }

                            if (loyaltyProfileResponse.AccountProfileInfo.PremierQualifyingDollars == 0 || !isValidPqdAddress)
                            {
                                if (!isValidPqdAddress)
                                {
                                    mpSummary.PremierQualifyingDollars = string.Empty;
                                }
                                else
                                {
                                    mpSummary.PremierQualifyingDollars = "0";
                                }
                            }
                            else
                            {
                                decimal pqd = 0;
                                try
                                {
                                    pqd = Convert.ToDecimal(loyaltyProfileResponse.AccountProfileInfo.PremierQualifyingDollars);
                                }
                                catch (Exception) { }
                                //Below are the two toggles used in Appsettings 
                                //< add key = "PqdAmount" value = "12000" /> < add key = "PqdText" value = "Over $12,000" />
                                //Work Items LOYAL-3236, LOYAL-3241
                                if (!string.IsNullOrEmpty(_configuration.GetValue<string>("PqdAmount")) && !string.IsNullOrEmpty(_configuration.GetValue<string>("PqdText")))
                                {
                                    if (pqd > Convert.ToDecimal(_configuration.GetValue<string>("PqdAmount")))
                                    {
                                        mpSummary.PremierQualifyingDollars = _configuration.GetValue<string>("PqdText");
                                    }
                                }
                                else
                                {
                                    mpSummary.PremierQualifyingDollars = pqd.ToString("C0");
                                }
                            }

                            string pdqchasewaiverLabel = string.Empty;
                            string pdqchasewavier = string.Empty;
                            if (isValidPqdAddress)
                            {
                                if (!string.IsNullOrEmpty(loyaltyProfileResponse.AccountProfileInfo.ChaseSpendingIndicator) && loyaltyProfileResponse.AccountProfileInfo.ChaseSpendingIndicator.Equals("Y"))
                                {
                                }
                                if (!string.IsNullOrEmpty(loyaltyProfileResponse.AccountProfileInfo.PresidentialPlusIndicator) && loyaltyProfileResponse.AccountProfileInfo.PresidentialPlusIndicator.Equals("Y"))
                                {
                                }
                                //[CLEANUP API-MIGRATION]
                                //if (!_configuration.GetValue<bool>("IsAvoidUAWSChaseMessagingSoapCall"))
                                //{
                                //    GetChaseMessage(activeNonPresidentialPlusCardMember, activePresidentialPlusCardMembe, chaseSpending, presidentialPlus, ref pdqchasewaiverLabel, ref pdqchasewavier, ref balanceExpireDisclaimer);
                                //    mpSummary.PDQchasewaiverLabel = pdqchasewaiverLabel;
                                //    mpSummary.PDQchasewavier = pdqchasewavier;
                                //}
                                if (!string.IsNullOrEmpty(balanceExpireDisclaimer) && !_configuration.GetValue<bool>("HideMileageBalanceExpireDate"))
                                {
                                    mpSummary.BalanceExpireDisclaimer = mpSummary.BalanceExpireDisclaimer + " " + balanceExpireDisclaimer;
                                }
                            }
                            if (!fourSegmentMinimunWaivedMember && !_configuration.GetValue<bool>("HideMileageBalanceExpireDate"))
                            {
                                mpSummary.BalanceExpireDisclaimer = mpSummary.BalanceExpireDisclaimer + " " + HttpUtility.HtmlDecode(_configuration.GetValue<string>("FouSegmentMessage"));
                            }
                        }
                    }

                    if (includeMembershipCardBarCode)
                    {
                        //%B[\w]{2,3}\d{5,6}\s{7}\d{4}(GL|1K|GS|PL|SL|\s\s)\s\s[\s\w\<\-\'\.]{35}\sUA
                        string eliteLevel = "";
                        switch (mpSummary.EliteStatus.Level)
                        {
                            case 0:
                                eliteLevel = "  ";
                                break;
                            case 1:
                                eliteLevel = "SL";
                                break;
                            case 2:
                                eliteLevel = "GL";
                                break;
                            case 3:
                                eliteLevel = "PL";
                                break;
                            case 4:
                                eliteLevel = "1K";
                                break;
                            case 5:
                                eliteLevel = "GS";
                                break;
                        }
                        string name = string.Format("{0}<{1}", mpSummary.Name.Last, mpSummary.Name.First);
                        if (!string.IsNullOrEmpty(mpSummary.Name.Middle))
                        {
                            name = name + " " + mpSummary.Name.Middle.Substring(0, 1);
                        }
                        name = String.Format("{0, -36}", name);

                        bool hasUnitedClubMemberShip = false;
                        var tupleResponse = await GetUnitedClubMembershipDetails(mpSummary.MileagePlusNumber, _headers.ContextValues.SessionId).ConfigureAwait(false);
                        mpSummary.uAClubMemberShipDetails = tupleResponse.clubMemberShipDetails;
                        hasUnitedClubMemberShip = tupleResponse.hasUnitedClubMemberShip;
                        mpSummary.HasUAClubMemberShip = hasUnitedClubMemberShip;
                        //if (hasUnitedClubMemberShip)
                        //{
                        mpSummary.MembershipCardExpirationDate = await this.GetMembershipCardExpirationDate(mpSummary.MileagePlusNumber, mpSummary.EliteStatus.Level).ConfigureAwait(false);
                        string expirationDate = _configuration.GetValue<string>("MPCardExpirationDate");
                        try
                        {
                            if (mpSummary.MembershipCardExpirationDate.Equals("Trial Status"))
                            {
                                expirationDate = string.Format("12/{0}", DateTime.Today.Year);
                            }
                            else
                            {
                                expirationDate = string.IsNullOrEmpty(mpSummary.MembershipCardExpirationDate) ? _configuration.GetValue<string>("MPCardExpirationDate") : mpSummary.MembershipCardExpirationDate.Substring(11, 2) + mpSummary.MembershipCardExpirationDate.Substring(16, 2);
                            }
                        }
                        catch (System.Exception) { }

                        //string expirationDate = _configuration.GetValue<string>"MPCardExpirationDate");
                        string barCodeData = string.Format("%B{0}       {1}{2}  {3}UA              ?", mpSummary.MileagePlusNumber, expirationDate, eliteLevel, name);
                        string allianceTierLevel = "   ";
                        switch (mpSummary.EliteStatus.Level)
                        {
                            case 0:
                                allianceTierLevel = "   ";
                                break;
                            case 1:
                                allianceTierLevel = "UAS";
                                break;
                            case 2:
                                allianceTierLevel = "UAG";
                                break;
                            case 3:
                                allianceTierLevel = "UAG";
                                break;
                            case 4:
                                allianceTierLevel = "UAG";
                                break;
                            case 5:
                                allianceTierLevel = "UAG";
                                break;
                        }
                        string allianceTierLevelExpirationDate = "    ";
                        if (!allianceTierLevel.Equals("   "))
                        {
                            if (mpSummary.MembershipCardExpirationDate.Equals("Trial Status"))
                            {
                                allianceTierLevelExpirationDate = string.Format("12/{0}", DateTime.Today.Year);
                            }
                            else
                            {
                                allianceTierLevelExpirationDate = string.IsNullOrEmpty(mpSummary.MembershipCardExpirationDate) ? _configuration.GetValue<string>("MPCardExpirationDate") : mpSummary.MembershipCardExpirationDate.Substring(11, 2) + mpSummary.MembershipCardExpirationDate.Substring(16, 2);
                            }
                        }

                        string paidLoungeIndicator = "N";
                        string paidLoungeExpireationDate = "      ";
                        if (mpSummary.uAClubMemberShipDetails != null)
                        {
                            if (string.IsNullOrEmpty(mpSummary.uAClubMemberShipDetails.PrimaryOrCompanion))
                            {
                                paidLoungeIndicator = "P";
                            }
                            else
                            {
                                paidLoungeIndicator = mpSummary.uAClubMemberShipDetails.PrimaryOrCompanion;
                            }
                            paidLoungeExpireationDate = Convert.ToDateTime(mpSummary.uAClubMemberShipDetails.DiscontinueDate).ToString("ddMMyyyy");
                        }

                        string starBarCodeData = string.Format("FFPC001UA{0}{1}{2}{3}{4}{5}{6}{7}^001", mpSummary.MileagePlusNumber.PadRight(16), mpSummary.Name.Last.PadRight(20), mpSummary.Name.First.PadRight(20), allianceTierLevel, allianceTierLevelExpirationDate, eliteLevel, paidLoungeIndicator, paidLoungeExpireationDate);

                        if (!string.IsNullOrEmpty(_configuration.GetValue<string>("ReturnMPMembershipBarcode")) && _configuration.GetValue<bool>("ReturnMPMembershipBarcode"))
                        {
                            if (!string.IsNullOrEmpty(_configuration.GetValue<string>("UseStarMembershipCardFormatDateTime")) && _configuration.GetValue<DateTime>("UseStarMembershipCardFormatDateTime") <= DateTime.Now)
                            {
                                mpSummary.MembershipCardBarCodeString = starBarCodeData.ToUpper();
                                mpSummary.MembershipCardBarCode = GetBarCode(starBarCodeData);
                            }
                            else
                            {
                                mpSummary.MembershipCardBarCodeString = barCodeData.ToUpper();
                                mpSummary.MembershipCardBarCode = GetBarCode(barCodeData);
                            }
                        }
                        else
                        {
                            mpSummary.MembershipCardBarCodeString = null;
                            mpSummary.MembershipCardBarCode = null;
                        }
                        //}
                    }

                    //bool noMileageExpiration = HasChaseNoMileageExpirationCard(mpSummary.MileagePlusNumber);
                    mpSummary.NoMileageExpiration = noMileageExpiration.ToString();

                    if (_configuration.GetValue<bool>("HideMileageBalanceExpireDate"))

                    {
                        mpSummary.NoMileageExpirationMessage = HttpUtility.HtmlDecode(_configuration.GetValue<string>("BalanceExpireDisclaimerNeverExpire"));
                    }
                    else if (noMileageExpiration)
                    {
                        mpSummary.NoMileageExpirationMessage = HttpUtility.HtmlDecode(_configuration.GetValue<string>("ChaseNoMileageExpirationMessage") + " " + balanceExpireDisclaimer);
                        if (!fourSegmentMinimunWaivedMember)
                        {
                            mpSummary.NoMileageExpirationMessage = HttpUtility.HtmlDecode(mpSummary.NoMileageExpirationMessage + " " + _configuration.GetValue<string>("FouSegmentMessage"));
                        }
                    }
                }
                catch (MOBUnitedException ex)
                {
                    throw new MOBUnitedException(ex.Message);
                }
                catch (System.Exception ex)
                {
                    throw new System.Exception(ex.Message);
                }
                finally
                {
                    try
                    {
                        //if (response != null)
                        //{
                        //    response.Close();
                        //}
                    }
                    catch
                    {
                        throw new System.Exception("United Data Services Not Available");
                    }
                }

                _logger.LogInformation("Loyalty Get Profile Response to client {@MpSummary}", JsonConvert.SerializeObject(mpSummary));

                if (_configuration.GetValue<string>("StartDateTimeToReturnEmptyMPExpirationDate") != null && _configuration.GetValue<string>("EndtDateTimeToReturnEmptyMPExpirationDate") != null && DateTime.ParseExact(_configuration.GetValue<string>("StartDateTimeToReturnEmptyMPExpirationDate").ToString().ToUpper().Trim(), "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture) <= DateTime.Now && DateTime.ParseExact(_configuration.GetValue<string>("EndtDateTimeToReturnEmptyMPExpirationDate").ToString().ToUpper().Trim(), "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture) >= DateTime.Now)
                {
                    mpSummary.MembershipCardExpirationDate = string.Empty;
                }
                return mpSummary;
            }
        }

        public async Task<(string employeeId, string displayEmployeeId)> GetEmployeeId(string mileageplusNumber, string transactionId, string sessionId, string displayEmployeeId)
        {
            string employeeId = string.Empty;

            var response = await _employeeIdByMileageplusNumber.GetEmployeeId(mileageplusNumber, transactionId, sessionId).ConfigureAwait(false);
            var empResponse = JsonConvert.DeserializeObject<GetEmpIdByMpNumber>(response);
            if (empResponse.MPLinkedId != null)
            {
                displayEmployeeId = empResponse.FileNumber;
                employeeId = empResponse.MPLinkedId;
            }
            else
            {
                displayEmployeeId = empResponse.EmployeeId;
                employeeId = empResponse.EmployeeId;
            }
            if (employeeId == null)
            {
                displayEmployeeId = string.Empty;
                employeeId = string.Empty;
            }
            return (employeeId, displayEmployeeId);
        }

        private async Task<(bool isValidPqdAddress, bool activeNonPresidentialPlusCardMember, bool activePresidentialPlusCardMembe, bool fourSegmentMinimunWaivedMember, bool chaseBonusTile)> IsValidPQDAddressV2(string callingMethodName, string transactionID, string token, string mpNumber, bool isValidPqdAddress, bool activeNonPresidentialPlusCardMember, bool activePresidentialPlusCardMembe, bool fourSegmentMinimunWaivedMember, bool chaseBonusTile)
        {
            try
            {
                //For MP 2015, we will show PDQ for all 
                isValidPqdAddress = true;

                Services.Customer.Common.ProfileRequest profileRequest = new Services.Customer.Common.ProfileRequest
                {
                    LoyaltyId = mpNumber,
                    RefreshCache = false,
                    DataSetting = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices"),
                    LangCode = "en-US"
                };

                List<string> requestStringList = new List<string>
                {
                    "PaymentInfos"
                };
                profileRequest.DataToLoad = requestStringList;

                string jsonRequest = JsonConvert.SerializeObject(profileRequest);
                var customerDataResponse = await _customerDataService.GetCustomerData<Services.Customer.Common.ProfileResponse>(token, _headers.ContextValues.SessionId, jsonRequest).ConfigureAwait(false);

                var response = customerDataResponse.response;
                if (response != null && response.Profiles != null && response.Profiles.Count > 0 && response.Profiles[0].Travelers != null &&
                    response.Profiles[0].Travelers.Count > 0 && response.Profiles[0].Travelers[0].MileagePlus != null &&
                    response.Profiles[0].Travelers[0].MileagePlus.PaymentInfos != null &&
                    response.Profiles[0].Travelers[0].MileagePlus.PaymentInfos.Count > 0)
                {
                    bool hasChaseCard = false;
                    foreach (Services.Customer.Common.PaymentInfo paymentInfo in response.Profiles[0].Travelers[0].MileagePlus.PaymentInfos)
                    {
                        if (!string.IsNullOrEmpty(paymentInfo.PartnerCode))
                        {
                            if (paymentInfo.PartnerCode.Equals("CH"))
                            {
                                hasChaseCard = true;
                                activeNonPresidentialPlusCardMember = true;
                            }

                            if (paymentInfo.IsPartnerCard && paymentInfo.PartnerCode.Equals("CH"))
                            {
                                if (!string.IsNullOrEmpty(paymentInfo.CardType) && _configuration.GetValue<string>("PresidentialPlusChaseCardTypes").IndexOf(paymentInfo.CardType) != -1)
                                {
                                    fourSegmentMinimunWaivedMember = !_configuration.GetValue<bool>("EnableVBQII") ? true : false;
                                }
                                if (!string.IsNullOrEmpty(paymentInfo.CardType) && _configuration.GetValue<string>("PreferredPresidentialPlusChaseCardTypes").IndexOf(paymentInfo.CardType) != -1)
                                {
                                    activePresidentialPlusCardMembe = true;
                                }

                                if (DateTime.Now >= _configuration.GetValue<DateTime>("ChaseBonusTileStartDate") && DateTime.Now < _configuration.GetValue<DateTime>("ChaseBonusTileEndDate"))
                                {
                                    if (!string.IsNullOrEmpty(paymentInfo.CardType) && _configuration.GetValue<string>("ChaseBonusTileChaseCardTypes").IndexOf(paymentInfo.CardType) == -1)
                                    {
                                        chaseBonusTile = true;
                                    }
                                }
                            }
                        }
                    }
                    if (!hasChaseCard)
                    {
                        if (DateTime.Now >= _configuration.GetValue<DateTime>("ChaseBonusTileStartDate") && DateTime.Now < _configuration.GetValue<DateTime>("ChaseBonusTileEndDate"))
                        {
                            chaseBonusTile = true;
                        }
                    }
                }
                else
                {
                    if (DateTime.Now >= _configuration.GetValue<DateTime>("ChaseBonusTileStartDate") && DateTime.Now < _configuration.GetValue<DateTime>("ChaseBonusTileEndDate"))
                    {
                        chaseBonusTile = true;
                    }
                }
            }
            catch (Exception ex)
            {
                string enterlog = string.IsNullOrEmpty(ex.StackTrace) ? ex.Message : ex.StackTrace;
                _logger.LogError("IsValidPQDAddressV2 {@Exception}", enterlog);
                if (DateTime.Now >= _configuration.GetValue<DateTime>("ChaseBonusTileStartDate") && DateTime.Now < _configuration.GetValue<DateTime>("ChaseBonusTileEndDate"))
                {
                    chaseBonusTile = true;
                }
            }
            return (isValidPqdAddress, activeNonPresidentialPlusCardMember, activePresidentialPlusCardMembe, fourSegmentMinimunWaivedMember, chaseBonusTile);
        }

        private void AssignCustomerChasePromoType(MOBMPAccountSummary mpSummary, bool showChaseBonusTile)
        {
            if (mpSummary != null)
            {
                if (showChaseBonusTile)
                {
                    if (mpSummary.EliteStatus != null && mpSummary.EliteStatus.Level > 0)
                    {
                        mpSummary.ChasePromoType = "70K";
                    }
                    else
                    {
                        mpSummary.ChasePromoType = "50K";
                    }
                }
                else
                {
                    mpSummary.ChasePromoType = "";
                }
            }
        }

        private async Task<(MOBUnitedClubMemberShipDetails clubMemberShipDetails, bool hasUnitedClubMemberShip, Service.Presentation.ProductResponseModel.Subscription merchOut)>
            GetUnitedClubMembershipDetails(string mpAccountNumber, string sessionID, string token = "")
        {
            #region
            bool hasUnitedClubMemberShip = false;
            MOBUnitedClubMemberShipDetails clubMemberShipDetails = null;
            Service.Presentation.ProductResponseModel.Subscription merchOut = null;
            try
            {
                var TransactionId = string.Empty;
                var tuple = await GetCurrentMembershipInfoV2(mpAccountNumber, sessionID, token).ConfigureAwait(false);
                MOBClubMembership mobClubMembership = tuple.clubMemberShip;
                merchOut = tuple.merchOut;

                if (mobClubMembership != null)
                {
                    #region
                    clubMemberShipDetails = new MOBUnitedClubMemberShipDetails();
                    hasUnitedClubMemberShip = true;
                    clubMemberShipDetails.MemberTypeCode = mobClubMembership.MembershipTypeCode;
                    if (_configuration.GetValue<string>("United_Club_Membership_Defalut_Desc") != null)
                    {
                        clubMemberShipDetails.MemberTypeDesc = _configuration.GetValue<string>("United_Club_Membership_Defalut_Desc").ToString();
                    }
                    else
                    {
                        clubMemberShipDetails.MemberTypeDesc = mobClubMembership.MembershipTypeDescription;
                    }
                    clubMemberShipDetails.EffectiveDate = Convert.ToDateTime(mobClubMembership.EffectiveDate).ToString("MM/dd/yyyy");
                    clubMemberShipDetails.DiscontinueDate = Convert.ToDateTime(mobClubMembership.ExpirationDate).ToString("MM/dd/yyyy");
                    if (!string.IsNullOrEmpty(mobClubMembership.CompanionMPNumber))
                    {
                        clubMemberShipDetails.CompanionMileagePlus = mobClubMembership.CompanionMPNumber;
                        clubMemberShipDetails.PrimaryOrCompanion = mobClubMembership.IsPrimary ? "P" : "C";
                    }
                    #endregion
                }
            }
            catch (System.Exception)
            {
                hasUnitedClubMemberShip = false;
            }
            return (clubMemberShipDetails, hasUnitedClubMemberShip, merchOut);
            #endregion
        }

        private async Task<string> GetMembershipCardExpirationDate(string mpAccountNumber, int eliteStatus)
        {
            string membershipCardExpirationDate = string.Empty;

            bool isInstantElite = await IsInstantElite(mpAccountNumber).ConfigureAwait(false);

            if (isInstantElite)
            {
                membershipCardExpirationDate = "Trial Status";
            }
            else
            {
                if (eliteStatus != 0)
                {
                    if (DateTime.Today.Month == 1)
                    {
                        membershipCardExpirationDate = string.Format("Valid thru 01/{0}", DateTime.Today.Year);
                    }
                    else
                    {
                        membershipCardExpirationDate = string.Format("Valid thru 01/{0}", DateTime.Today.AddYears(1).Year);
                    }
                }
            }

            return membershipCardExpirationDate;
        }

        private async Task<bool> IsInstantElite(string mileagePlusNumber)
        {
            if (!_configuration.GetValue<bool>("EnableLoyaltyWebserviceWPCL"))
            {
                return false;
            }
            bool ok = false;

            try
            {
                string token = await _dPService.GetAnonymousToken(_headers.ContextValues.Application.Id, _headers.ContextValues.DeviceId, _configuration).ConfigureAwait(false);
                string loyaltyWPCLUrl = await _loyaltyWebService.GetLoyaltyData(token, mileagePlusNumber, _headers.ContextValues.TransactionId).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(loyaltyWPCLUrl) && loyaltyWPCLUrl.ToUpper().IndexOf("<ISTRIALELITE>TRUE</ISTRIALELITE>") != -1)
                {
                    ok = true;
                }
            }
            catch (System.Exception) { }

            return ok;
        }

        private byte[] GetBarCode(string data)
        {
            string dataToEncode = data;
            bool applyTilde = true;
            bool truncate = true;
            int ModuleSize = 5;
            int QuietZone = 3;
            int TotalRows = 3;
            int TotalCols = 5;
            int ECLevel = 8;
            PDF417 obj = new PDF417();
            byte[] bmpstream = obj.EncodePDF417(dataToEncode, applyTilde, ECLevel, EncodingModes.Binary, TotalCols, TotalRows, truncate, QuietZone, ModuleSize);
            return bmpstream;
        }

        public void GetMPEliteLevelExpirationDateAndGenerateBarCode(MOBMPAccountSummary mpSummary, string premierLevelExpirationDate, MOBInstantElite instantElite) // these 2 new properties will be set from Cusotmer Profile Serivce in Mp Validate Sign Action "premierLevelExpirationDate" & "instantElite"
        {
            //config value - "Keep_MREST_MP_EliteLevel_Expiration_Logic" was missing in all config file of MRest app
            if (!_configuration.GetValue<bool>("Keep_MREST_MP_EliteLevel_Expiration_Logic"))
            {
                #region Current Channel Side MP Expiratoin & BarCode Logic
                mpSummary.MembershipCardExpirationDate = string.Empty;
                string expirationDate = _configuration.GetValue<string>("MPCardExpirationDate");
                string consolidatedCode = string.Empty; string instantEliteExpirationDate = string.Empty;
                if (instantElite != null)
                {
                    consolidatedCode = instantElite.ConsolidatedCode;
                    instantEliteExpirationDate = instantElite.ExpirationDate;
                }
                expirationDate = GetMPExpirationDateFromProfileAndQualService(mpSummary, premierLevelExpirationDate, consolidatedCode, instantEliteExpirationDate, expirationDate);
                #region eliteLevel, name for Bar Code Generation
                string eliteLevel = "";
                switch (mpSummary.EliteStatus.Level)
                {
                    case 0:
                        eliteLevel = "  ";
                        break;
                    case 1:
                        eliteLevel = "SL";
                        break;
                    case 2:
                        eliteLevel = "GL";
                        break;
                    case 3:
                        eliteLevel = "PL";
                        break;
                    case 4:
                        eliteLevel = "1K";
                        break;
                    case 5:
                        eliteLevel = "GS";
                        break;
                }
                string name = string.Format("{0}<{1}", mpSummary.Name.Last, mpSummary.Name.First);
                if (!string.IsNullOrEmpty(mpSummary.Name.Middle))
                {
                    name = name + " " + mpSummary.Name.Middle.Substring(0, 1);
                }
                name = String.Format("{0, -36}", name);
                #endregion
                string barCodeData = string.Format("%B{0}       {1}{2}  {3}UA              ?", mpSummary.MileagePlusNumber, expirationDate, eliteLevel, name); // **==>> This is the one Bar code generation for all general members
                string allianceTierLevel = "   ";
                switch (mpSummary.EliteStatus.Level)
                {
                    case 0:
                        allianceTierLevel = "   ";
                        break;
                    case 1:
                        allianceTierLevel = "UAS";
                        break;
                    case 2:
                        allianceTierLevel = "UAG";
                        break;
                    case 3:
                        allianceTierLevel = "UAG";
                        break;
                    case 4:
                        allianceTierLevel = "UAG";
                        break;
                    case 5:
                        allianceTierLevel = "UAG";
                        break;
                }
                string allianceTierLevelExpirationDate = "    ";
                if (!allianceTierLevel.Equals("   "))
                {
                    allianceTierLevelExpirationDate = expirationDate;
                }
                string paidLoungeIndicator = "N";
                string paidLoungeExpireationDate = "      ";
                if (mpSummary.uAClubMemberShipDetails != null)
                {
                    if (string.IsNullOrEmpty(mpSummary.uAClubMemberShipDetails.PrimaryOrCompanion))
                    {
                        paidLoungeIndicator = "P";
                    }
                    else
                    {
                        paidLoungeIndicator = mpSummary.uAClubMemberShipDetails.PrimaryOrCompanion;
                    }
                    paidLoungeExpireationDate = Convert.ToDateTime(mpSummary.uAClubMemberShipDetails.DiscontinueDate).ToString("ddMMyyyy");
                }

                string starBarCodeData = string.Format("FFPC001UA{0}{1}{2}{3}{4}{5}{6}{7}^001", mpSummary.MileagePlusNumber.PadRight(16), mpSummary.Name.Last.PadRight(20), mpSummary.Name.First.PadRight(20), allianceTierLevel, allianceTierLevelExpirationDate, eliteLevel, paidLoungeIndicator, paidLoungeExpireationDate); // **==>> This is the one Bar code generation for all general members

                if (_configuration.GetValue<bool>("ReturnMPMembershipBarcode"))
                {
                    if (!string.IsNullOrEmpty(_configuration.GetValue<string>("UseStarMembershipCardFormatDateTime")) && _configuration.GetValue<DateTime>("UseStarMembershipCardFormatDateTime") <= DateTime.Now)
                    {
                        mpSummary.MembershipCardBarCodeString = starBarCodeData.ToUpper();
                        mpSummary.MembershipCardBarCode = GetBarCode(starBarCodeData);
                    }
                    else
                    {
                        mpSummary.MembershipCardBarCodeString = barCodeData.ToUpper();
                        mpSummary.MembershipCardBarCode = GetBarCode(barCodeData);
                    }
                }
                else
                {
                    mpSummary.MembershipCardBarCodeString = null;
                    mpSummary.MembershipCardBarCode = null;
                }
                #endregion Current Channel Side MP Expiration & BarCode Logic
            }
            //return mpSummary;
        }

        private static string GetMPExpirationDateFromProfileAndQualService(MOBMPAccountSummary mpSummary, string premierLevelExpirationDate, string consolidatedCode, string instantEliteExpirationDate, string expirationDate)
        {
            if (!string.IsNullOrEmpty(premierLevelExpirationDate))
            {
                #region
                DateTime expDateTime = Convert.ToDateTime(premierLevelExpirationDate);//  DateTime.ParseExact(premierLevelExpirationDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                if (expDateTime.Month < 10)
                {
                    expirationDate = "0" + expDateTime.Month.ToString() + expDateTime.Year.ToString().Substring(2, 2);
                    mpSummary.MembershipCardExpirationDate = string.Format("Valid thru {0}/{1}", "0" + expDateTime.Month.ToString(), expDateTime.Year.ToString());
                }
                else
                {
                    expirationDate = expDateTime.Month.ToString() + expDateTime.Year.ToString().Substring(2, 2);
                    mpSummary.MembershipCardExpirationDate = string.Format("Valid thru {0}/{1}", expDateTime.Month.ToString(), expDateTime.Year.ToString());
                }
                #endregion
            }
            if (string.IsNullOrEmpty(premierLevelExpirationDate) && !string.IsNullOrEmpty(consolidatedCode) && consolidatedCode.ToUpper().Equals("TRIAL", StringComparison.OrdinalIgnoreCase))
            {
                #region
                if (consolidatedCode.ToUpper().Equals("TRIAL", StringComparison.OrdinalIgnoreCase))
                {
                    mpSummary.MembershipCardExpirationDate = "Valid thru Trial";
                }

                if (!string.IsNullOrEmpty(instantEliteExpirationDate))
                {
                    DateTime expDateTime = Convert.ToDateTime(instantEliteExpirationDate); //DateTime.ParseExact(instantElite.ExpirationDate.Trim(), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                    if (expDateTime.Month < 10)
                    {
                        expirationDate = "0" + expDateTime.Month.ToString() + expDateTime.Year.ToString().Substring(2, 2);
                    }
                    else
                    {
                        expirationDate = expDateTime.Month.ToString() + expDateTime.Year.ToString().Substring(2, 2);
                    }

                }
                else
                {
                    expirationDate = string.Format("12{0}", DateTime.Today.Year.ToString().Substring(2, 2));
                }
                #endregion
            }

            return expirationDate;
        }

        /// <summary>
        /// Takes the Request And Token And returns the pluspoiunts Object. 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="dpToken"></param>
        /// <returns></returns>
        public async Task<MOBPlusPoints> GetPlusPointsFromLoyaltyBalanceService(MOBMPAccountValidationRequest req, string dpToken)
        {
            string jsonResponse = string.Empty;
            try
            {
                //if (Utility.GetBooleanConfigValue("ByPassGetPremierActivityRequestValidationnGetCachedDPTOken") == true) // If the value of ByPassGetPremierActivityRequestValidationnGetCachedDPTOken is true then go get FLIFO Dp Token which we currenlty used by Flight Status NOTE: Alreday confirmed with Greg & Bob that they are not using internal to validate DP Token.
                dpToken = await _dPService.GetAnonymousToken(req.Application.Id, req.DeviceId, _configuration).ConfigureAwait(false);

                jsonResponse = await _loyaltyBalanceServices.GetLoyaltyBalance(dpToken, req.MileagePlusNumber, req.SessionId).ConfigureAwait(false);
            }
            catch (System.Net.WebException wex)
            {
                if (wex.Response != null)
                {
                    var errorResponse = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                    _logger.LogWarning("GetPlusPointsFromLoyaltyBalanceService - LoyaltyBalance_Exception {@errormessage} {@stacktrace}", errorResponse, GeneralHelper.RemoveCarriageReturn(JsonConvert.SerializeObject(wex.StackTrace)));
                    throw new System.Exception(wex.Message);
                }
            }
            if (!string.IsNullOrEmpty(jsonResponse))
            {
                return GetPlusPointsFromJson(jsonResponse, req);
            }
            return null;
        }

        /// <summary>
        /// Takes the service response and returns the plus points object. 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private MOBPlusPoints GetPlusPointsFromJson(string request, MOBMPAccountValidationRequest req)
        {
            //TODO missing
            //Need to add package - not compatibility version for this United 2.0\packages\LoyaltyCommon.1.0.6.88\lib\Net45\United.TravelBank.Model.dll

            BalanceResponse PlusPointResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<BalanceResponse>(request);
            Balance plusPointsBalance;
            SubBalance subBalanceRequested;
            SubBalance subBalanceConfirmed;
            if (PlusPointResponse.Balances != null && (plusPointsBalance = PlusPointResponse.Balances.FirstOrDefault(ct => ct.ProgramCurrencyType == TravelBankConstants.ProgramCurrencyType.UGC)) != null && plusPointsBalance.SubBalances != null &&
                (subBalanceRequested = plusPointsBalance.SubBalances.FirstOrDefault(s => s.Type.ToUpper() == "REQUESTED")) != null &&
                (subBalanceConfirmed = plusPointsBalance.SubBalances.FirstOrDefault(s => s.Type.ToUpper() == "CONFIRMED")) != null &&
                !(plusPointsBalance.TotalBalance == 0 && subBalanceRequested.Amount == 0 && subBalanceConfirmed.Amount == 0))
            {
                List<MOBKVP> kvpList = new List<MOBKVP>();
                if (plusPointsBalance.BalanceDetails != null)
                {
                    foreach (BalanceDetail bd in plusPointsBalance.BalanceDetails)
                    {
                        kvpList.Add(new MOBKVP(bd.ProgramCurrencyAmount.ToString("#0"), bd.ExpirationDate.ToString("MMM dd, yyyy")));
                    }
                }
                MOBPlusPoints pluspoints = new MOBPlusPoints
                {
                    PlusPointsAvailableText = _configuration.GetValue<string>("PlusPointsAvailableText"),
                    PlusPointsAvailableValue = plusPointsBalance.TotalBalance.ToString("#0") +
                                                      " (" + subBalanceRequested.Amount.ToString("#0") + " requested)",
                    PlusPointsDeductedText = _configuration.GetValue<string>("PlusPointsDeductedText"),
                    PlusPointsDeductedValue = subBalanceConfirmed.Amount.ToString("#0"),
                    PlusPointsExpirationText = _configuration.GetValue<string>("PlusPointsExpirationText")
                };
                if (plusPointsBalance.EarliestExpirationDate != null && plusPointsBalance.EarliestExpirationDate.Value != null)
                {
                    pluspoints.PlusPointsExpirationValue = plusPointsBalance.EarliestExpirationDate.Value.ToString("MMM dd, yyyy");
                }
                else
                {
                    pluspoints.IsHidePlusPointsExpiration = true;
                }
                pluspoints.PlusPointsUpgradesText = _configuration.GetValue<string>("viewUpgradesText");
                pluspoints.PlusPointsUpgradesLink = _configuration.GetValue<string>("viewUpgradesLink");
                pluspoints.PlusPointsExpirationInfo = _configuration.GetValue<string>("PlusPointsExpirationInfo");
                pluspoints.PlusPointsExpirationInfoHeader = _configuration.GetValue<string>("PlusPointsExpirationInfoHeader");
                pluspoints.plusPointsExpirationPointsInfoSubHeader = _configuration.GetValue<string>("PlusPointsExpirationInfoPointsSubHeader");
                pluspoints.PlusPointsExpirationInfoDateSubHeader = _configuration.GetValue<string>("PlusPointsExpirationInfoDateSubHeader");
                pluspoints.ExpirationPointsAndDatesKVP = kvpList;
                pluspoints.RedirectToDotComMyTripsWithSSOCheck = _configuration.GetValue<bool>("EnablePlusPointsSSO");
                if (_configuration.GetValue<bool>("EnablePlusPointsSSO"))
                {
                    pluspoints.WebSessionShareUrl = _configuration.GetValue<string>("DotcomSSOUrl");
                    var ssoTokenObject = _dPService.GetSSOToken(req.Application.Id, req.MileagePlusNumber, _configuration).Result;
                    pluspoints.WebShareToken = $"{ssoTokenObject.TokenType} {ssoTokenObject.AccessToken}";
                }
                return pluspoints;
            }

            return null;
        }

        public bool IsPremierStatusTrackerSupportedVersion(int appId, string appVersion)
        {
            bool isPremierStatusTrackerSupportedVersion = false;
            if (!string.IsNullOrEmpty(appVersion) && appId != -1)
            {
                isPremierStatusTrackerSupportedVersion = GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidPremierStatusTrackerVersion", "iPhonePremierStatusTrackerVersion", "", "", true, _configuration);
            }
            return isPremierStatusTrackerSupportedVersion;
        }

        private MOBPremierActivity GetTrackingUpgradesActivity(ServiceResponse sResponse, int trackingUpgradeType)
        {
            MOBPremierActivity upgradeActivity = new MOBPremierActivity();
            MOBPremierQualifierTracker pqm1 = new MOBPremierQualifierTracker();
            MOBPremierQualifierTracker pqs1 = new MOBPremierQualifierTracker();
            MOBPremierQualifierTracker pqd1 = new MOBPremierQualifierTracker();
            bool isPresidentialPlusCard = false;
            bool isInternationalMember = false;
            bool isChaseSpendIndicator = false;
            if (sResponse != null && sResponse.CurrentYearActivity != null)
            {
                isPresidentialPlusCard = IsPresidentialPlusCard(sResponse.CurrentYearActivity.PresidentPlusIndicator);
                isInternationalMember = IsInternationalMember(sResponse.CurrentYearActivity.DomesticIndicator);
                isChaseSpendIndicator = IsChaseCardSpendIndicator(sResponse.MileagePlusCardIndicator, sResponse.CurrentYearActivity.ChaseCardSpendIndicator);
                List<MOBKVP> lUpgradeTrackingLevels = new List<MOBKVP>();
                List<MOBKVP> lFlexMiles = new List<MOBKVP>();

                upgradeActivity.PremierActivityTitle = _configuration.GetValue<string>("PremierActivityTitle");
                upgradeActivity.PremierActivityYear = _configuration.GetValue<string>("EarnedInText") + " " + sResponse.CurrentYearActivity.ActivityYear;
                upgradeActivity.PremierActivityStatus = _configuration.GetValue<bool>("EnablePointsActivity") ? _configuration.GetValue<string>("EarnPremierPlusPoints") : _configuration.GetValue<string>("EarnPremierupgrades");

                lUpgradeTrackingLevels = GetUpgradeTrackingLevels(trackingUpgradeType, sResponse.CurrentYearActivity.EliteQualifyingMiles, sResponse.CurrentYearActivity.EliteQualifyingPoints);
                upgradeActivity.PQM = GetPQM(trackingUpgradeType, lUpgradeTrackingLevels, isPresidentialPlusCard, sResponse.CurrentYearActivity.EliteQualifyingMiles, sResponse.CurrentYearActivity.FlexEliteQualifyingMiles);
                upgradeActivity.PQS = GetPQS(trackingUpgradeType, lUpgradeTrackingLevels, sResponse.CurrentYearActivity.EliteQualifyingPoints);
                if (trackingUpgradeType == 1 || trackingUpgradeType == 2)
                {
                    upgradeActivity.PQD = GetPQD(trackingUpgradeType, lUpgradeTrackingLevels, sResponse.CurrentYearActivity.TotalRevenue, isPresidentialPlusCard, isInternationalMember, isChaseSpendIndicator);
                }
                upgradeActivity.KeyValueList = GetUpgradeKeyValues(trackingUpgradeType, isPresidentialPlusCard, sResponse.MileagePlusCardIndicator, sResponse.CurrentYearActivity.ChaseCardSpendIndicator,
                                                    sResponse.CurrentYearActivity.MinimumSegmentsRequired, sResponse.CurrentYearActivity.RpcWaivedIndicator, isInternationalMember, sResponse.AccountNumber
                                                    );
            }
            return upgradeActivity;
        }

        private bool IsPresidentialPlusCard(string presidentPlusIndicator)
        {
            if (!string.IsNullOrEmpty(presidentPlusIndicator) && presidentPlusIndicator.ToUpper() == "Y")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsInternationalMember(Activity currentYearActivity)
        {
            bool isInternationalMember = false;
            if (currentYearActivity != null && currentYearActivity.DomesticIndicator != null && currentYearActivity.DomesticIndicator.ToUpper() == "N")
            {
                isInternationalMember = true;
            }
            return isInternationalMember;
        }

        private bool IsInternationalMember(string domesticIndicator)
        {
            bool isInternationalMember = false;
            if (!string.IsNullOrEmpty(domesticIndicator) && domesticIndicator.ToUpper() == "N")
            {
                isInternationalMember = true;
            }
            return isInternationalMember;
        }

        private bool IsChaseCardSpendIndicator(string mileagePlusCardIndicator, string chaseCardSpendIndicator)
        {
            if (!string.IsNullOrEmpty(mileagePlusCardIndicator) && mileagePlusCardIndicator.ToUpper() == "Y" &&
                !string.IsNullOrEmpty(chaseCardSpendIndicator) && chaseCardSpendIndicator.ToUpper() == "Y")
            {
                return true;
            }
            return false;
        }

        private List<MOBKVP> GetUpgradeTrackingLevels(int trackingUpgradeType, long miles, double segments)
        {
            string PremierLevelStatus = string.Empty;
            List<MOBKVP> list = new List<MOBKVP>();
            string premierActivityStatus = string.Empty;
            long thresholdMiles = 0;
            long thresholdSegments = 0;
            switch (trackingUpgradeType)
            {
                case 1://1=Platinum Upgrade; 
                    list.Add(new MOBKVP("TrackerThresholdMiles", _configuration.GetValue<string>("ThresholdMilesPlatinum")));
                    list.Add(new MOBKVP("TrackerThresholdSegments", _configuration.GetValue<string>("ThresholdSegmentsPlatinum")));
                    list.Add(new MOBKVP("TrackerThresholdDollars", _configuration.GetValue<string>("ThresholdDollarsPlatinum")));
                    break;
                case 2://2=1k Upgrade; 
                    list.Add(new MOBKVP("TrackerThresholdMiles", _configuration.GetValue<string>("ThresholdMiles1K")));
                    list.Add(new MOBKVP("TrackerThresholdSegments", _configuration.GetValue<string>("ThresholdSegments1K")));
                    list.Add(new MOBKVP("TrackerThresholdDollars", _configuration.GetValue<string>("ThresholdDollars1K")));
                    break;
                case 3://3= Incremental Upgrade
                    GetIncrementalThresholdMilesAndSegments(miles, segments, out thresholdMiles, out thresholdSegments);
                    list.Add(new MOBKVP("TrackerThresholdMiles", thresholdMiles.ToString()));
                    list.Add(new MOBKVP("TrackerThresholdSegments", thresholdSegments.ToString()));
                    break;
                default:
                    break;
            }
            return list;
        }

        private MOBPremierQualifierTracker GetPQM(int trackingUpgradeType, List<MOBKVP> lUpgradeTrackingLevels, bool isPresidentialPlusCard, long eliteQualifyingMiles, long flexEliteQualifyingMiles)
        {
            MOBPremierQualifierTracker pqm1 = new MOBPremierQualifierTracker();
            string thresholdMiles = string.Empty;
            pqm1.PremierQualifierTrackerTitle = _configuration.GetValue<string>("PremierQualifierTrackerMilesTitle");
            pqm1.PremierQualifierTrackerCurrentValue = Convert.ToString(eliteQualifyingMiles - flexEliteQualifyingMiles);
            if (isPresidentialPlusCard && trackingUpgradeType == 1 && flexEliteQualifyingMiles > 0)
            {
                pqm1.PremierQualifierTrackerCurrentText = string.Format("{0:###,##0}", eliteQualifyingMiles);
                pqm1.PremierQualifierTrackerCurrentFlexValue = Convert.ToString(flexEliteQualifyingMiles);
                pqm1.PremierQualifierTrackerCurrentFlexTitle = _configuration.GetValue<string>("flexPQMTitle") + " " + string.Format("{0:###,##0}", flexEliteQualifyingMiles);
            }
            else
            {
                pqm1.PremierQualifierTrackerCurrentText = string.Format("{0:###,##0}", eliteQualifyingMiles - flexEliteQualifyingMiles);
                pqm1.PremierQualifierTrackerCurrentFlexValue = "0";
            }
            thresholdMiles = GetValueFromList(lUpgradeTrackingLevels, "TrackerThresholdMiles");
            pqm1.PremierQualifierTrackerThresholdValue = thresholdMiles != string.Empty ? thresholdMiles : "0";
            pqm1.PremierQualifierTrackerThresholdText = thresholdMiles != string.Empty ? string.Format("{0:###,##0}", Convert.ToDecimal(thresholdMiles)) : "0";
            pqm1.PremierQualifierTrackerThresholdPrefix = "of";
            pqm1.Separator = "or";
            pqm1.IsWaived = false;

            return pqm1;
        }

        private string GetValueFromList(List<MOBKVP> list, string key)
        {
            string value = string.Empty;
            if (list != null && list.Count > 0 && list.Where(s => s.Key == key).SingleOrDefault() != null)
            {
                value = list.Where(s => s.Key == key).SingleOrDefault().Value ?? string.Empty;
            }
            return value;
        }

        private string GetValueFromList(List<MOBItem> list, string key)
        {
            string value = string.Empty;
            if (list != null && list.Count > 0 && list.Where(s => s.Id == key).SingleOrDefault() != null)
            {
                value = list.Where(s => s.Id == key).SingleOrDefault().CurrentValue ?? string.Empty;
            }
            return value;

        }

        private MOBPremierQualifierTracker GetPQS(int trackingUpgradeType, List<MOBKVP> lUpgradeTrackingLevels, double eliteQualifyingPoints)
        {
            MOBPremierQualifierTracker pqs1 = new MOBPremierQualifierTracker();
            string thresholdSegments = string.Empty;
            pqs1.PremierQualifierTrackerTitle = _configuration.GetValue<string>("PremierQualifierTrackerSegmentsTitle");
            pqs1.PremierQualifierTrackerCurrentValue = Convert.ToString(eliteQualifyingPoints);
            pqs1.PremierQualifierTrackerCurrentText = string.Format("{0:0.#}", eliteQualifyingPoints);
            thresholdSegments = GetValueFromList(lUpgradeTrackingLevels, "TrackerThresholdSegments");
            pqs1.PremierQualifierTrackerThresholdValue = thresholdSegments != string.Empty ? thresholdSegments : "0";
            pqs1.PremierQualifierTrackerThresholdText = string.Format("{0:###,##0}", thresholdSegments != string.Empty ? thresholdSegments : "0");
            pqs1.PremierQualifierTrackerThresholdPrefix = "of";
            pqs1.Separator = trackingUpgradeType == 3 ? string.Empty : "+"; //for incremental upgrades hide "+"
            pqs1.IsWaived = false;

            return pqs1;
        }

        private MOBPremierQualifierTracker GetPQD(int trackingUpgradeType, List<MOBKVP> lUpgradeTrackingLevels, double totalRevenue, bool isPresidentialPlusCard, bool isInternationalMember, bool isChaseCardSpendIndicator)
        {
            MOBPremierQualifierTracker pqd1 = new MOBPremierQualifierTracker();
            string thresholdDollars = string.Empty;
            pqd1.PremierQualifierTrackerTitle = _configuration.GetValue<string>("PremierQualifierTrackerDollarsTitle"); ;
            pqd1.PremierQualifierTrackerCurrentValue = Convert.ToString(totalRevenue);
            pqd1.PremierQualifierTrackerCurrentText = totalRevenue > 0 ? totalRevenue.ToString("C0") : "$0";
            pqd1.IsWaived = IsPQDWaived(trackingUpgradeType, isPresidentialPlusCard, isInternationalMember, isChaseCardSpendIndicator);
            thresholdDollars = GetValueFromList(lUpgradeTrackingLevels, "TrackerThresholdDollars");
            pqd1.PremierQualifierTrackerThresholdValue = thresholdDollars != string.Empty ? thresholdDollars : "0";
            pqd1.PremierQualifierTrackerThresholdText = pqd1.IsWaived == true ? _configuration.GetValue<string>("WaivedText") : (thresholdDollars != string.Empty ? Convert.ToDecimal(thresholdDollars).ToString("C0") : "$0");
            pqd1.PremierQualifierTrackerThresholdPrefix = pqd1.IsWaived == true ? string.Empty : "of";

            return pqd1;
        }

        private bool IsPQDWaived(int trackingUpgradeType, bool isPresidentialPlusCard, bool isInternationalMember, bool isChaseCardSpendIndicator)
        {
            bool isWaived = false;
            if (!_configuration.GetValue<bool>("EnableVBQII"))
            {
                if (trackingUpgradeType == 1 && (isInternationalMember || isPresidentialPlusCard || isChaseCardSpendIndicator))
                {
                    isWaived = true;
                }
                else if (trackingUpgradeType == 2 && isInternationalMember)
                {
                    isWaived = true;
                }
            }
            return isWaived;
        }

        private List<MOBKVP> GetUpgradeKeyValues(int trackingUpgradeType, bool isPresidentialPlusCard, string mileagePlusCardIndicator, string chaseCardSpendIndicator, double minimumSegmentsRequired, string rpcWaivedIndicator, bool isInternationalMember, string mpAccountNumber)
        {
            List<MOBKVP> lKeyValue = new List<MOBKVP>();
            string chaseOrPresCardKey = string.Empty;
            string chaseOrPresCardValue = string.Empty;
            if (!_configuration.GetValue<bool>("EnableVBQII"))
            {
                chaseOrPresCardKey = isPresidentialPlusCard ? _configuration.GetValue<string>("PresidentialPlusPQDWaiver") : _configuration.GetValue<string>("creditcardspendPQDWaiver");
                //For 1k Upgrade, always display "Not applicable"
                if (trackingUpgradeType == 2)
                {
                    chaseOrPresCardValue = _configuration.GetValue<string>("CreditCardSpendPQDNotapplicable");
                }
                else
                {
                    chaseOrPresCardValue = isPresidentialPlusCard ? !_configuration.GetValue<bool>("EnableVBQII") ? _configuration.GetValue<string>("PresidentialPlusPQDWaiverValue") : GetCreditCardSpendPQDWaiverText(mileagePlusCardIndicator, chaseCardSpendIndicator, trackingUpgradeType) : GetCreditCardSpendPQDWaiverText(mileagePlusCardIndicator, chaseCardSpendIndicator, trackingUpgradeType);
                }
            }
            switch (trackingUpgradeType)
            {
                case 1: //Platinum
                    {
                        lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("MyAccountNextMilestone"), GetNextMilesStoneRPUGPUValue(trackingUpgradeType, mpAccountNumber)));
                        lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("flightsegmentminimum"), Get4FlightSegmentMinimum(rpcWaivedIndicator, minimumSegmentsRequired)));
                        if (!_configuration.GetValue<bool>("EnableVBQII") && !isInternationalMember)
                        {
                            lKeyValue.Add(new MOBKVP(chaseOrPresCardKey, chaseOrPresCardValue));
                        }
                        lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("viewpremierstatustracker"), _configuration.GetValue<string>("viewpremierstatustrackerlink")));
                        break;
                    }
                case 2: //1K
                    {
                        lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("MyAccountNextMilestone"), GetNextMilesStoneRPUGPUValue(trackingUpgradeType, mpAccountNumber)));
                        lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("flightsegmentminimum"), Get4FlightSegmentMinimum(rpcWaivedIndicator, minimumSegmentsRequired)));
                        if (!_configuration.GetValue<bool>("EnableVBQII") && !isInternationalMember)
                        {
                            lKeyValue.Add(new MOBKVP(chaseOrPresCardKey, chaseOrPresCardValue));
                        }
                        //lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("creditcardspendPQDWaiver"), GetCreditCardSpendPQDWaiverText(sResponse, trackingUpgradeType)));
                        lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("viewpremierstatustracker"), _configuration.GetValue<string>("viewpremierstatustrackerlink")));
                        break;
                    }
                case 3://Incremental Upgrades
                    {
                        lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("MyAccountNextMilestone"), GetNextMilesStoneRPUGPUValue(trackingUpgradeType, mpAccountNumber)));
                        lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("viewpremierstatustracker"), _configuration.GetValue<string>("viewpremierstatustrackerlink")));
                        break;
                    }
                default:
                    break;
            }
            return lKeyValue;
        }

        private string Get4FlightSegmentMinimum(string rpcWaivedIndicator, double minSegmentRequired)
        {
            string value = string.Empty;
            if (rpcWaivedIndicator.ToUpper() == "N" || (_configuration.GetValue<bool>("EnableVBQII") && rpcWaivedIndicator.ToUpper() == "Y"))
            {
                value = minSegmentRequired >= 4 ? "4 of 4" : minSegmentRequired + " of 4";
            }
            else if (rpcWaivedIndicator.ToUpper() == "Y")
            {
                value = _configuration.GetValue<string>("WaivedText");
            }
            return value;
        }

        private string GetNextMilesStoneRPUGPUValue(int trackingUpgradeType, string mpAccountNumber)//EX: +2 RPU / +6 GPU or  +1 GPU or +2 RPU
        {
            switch (trackingUpgradeType)
            {
                case 1: //Platinum
                    {
                        return _configuration.GetValue<bool>("EnablePointsActivity") ? _configuration.GetValue<string>("PlatinumNextMilestonePlusPoints") : _configuration.GetValue<string>("PlatinumNextMilestone");
                    }
                case 2: //1K
                    {
                        return _configuration.GetValue<bool>("EnablePointsActivity") ? _configuration.GetValue<string>("1KNextMilestonePlusPoints") : _configuration.GetValue<string>("1KNextMilestone");
                    }
                case 3://Incremental Upgrades
                    {
                        return _configuration.GetValue<bool>("EnablePointsActivity") ? _configuration.GetValue<string>("IncrementalUpgradeNextMilestonePlusPoints") : _configuration.GetValue<string>("IncrementalUpgradeNextMilestone");
                    }
                default:
                    break;
            }
            return string.Empty;
        }

        private string GetCreditCardSpendPQDWaiverText(string mileagePlusCardIndicator, string chaseCardSpendIndicator, int trackingUpgradeType)
        {
            string value = string.Empty;
            if (trackingUpgradeType == 1)//Platinum Upgrade
            {
                if (mileagePlusCardIndicator != null && mileagePlusCardIndicator.ToUpper() == "Y")
                {
                    if (chaseCardSpendIndicator != null && chaseCardSpendIndicator.ToUpper() == "Y")
                    {
                        value = _configuration.GetValue<string>("CreditCardSpendPQDEligibleMet");
                    }
                    else
                    {
                        value = _configuration.GetValue<string>("CreditCardSpendPQDEligibleNotMet");
                    }
                }
                else
                {
                    value = _configuration.GetValue<string>("CreditCardSpendPQDNotEligible");
                }
            }
            return value;
        }

        private string GetCreditCardSpendPQDWaiverText(ServiceResponse serviceResponse)
        {
            string value = string.Empty;
            if (serviceResponse != null && serviceResponse.CurrentYearActivity != null)
            {
                if (serviceResponse.MileagePlusCardIndicator.ToUpper() == "N" &&
                    (serviceResponse.CurrentYearActivity.ChaseCardSpendIndicator == null || serviceResponse.CurrentYearActivity.ChaseCardSpendIndicator.ToUpper() == "N"))
                {
                    value = _configuration.GetValue<string>("CreditCardSpendPQDNotEligible");
                }
                else if (serviceResponse.MileagePlusCardIndicator.ToUpper() == "Y" &&
                    (serviceResponse.CurrentYearActivity.ChaseCardSpendIndicator == null || serviceResponse.CurrentYearActivity.ChaseCardSpendIndicator.ToUpper() == "N"))
                {
                    value = _configuration.GetValue<string>("CreditCardSpendPQDEligibleNotMet");
                }
                else if (serviceResponse.MileagePlusCardIndicator != null && serviceResponse.MileagePlusCardIndicator.ToUpper() == "Y" &&
                    serviceResponse.CurrentYearActivity.ChaseCardSpendIndicator != null && serviceResponse.CurrentYearActivity.ChaseCardSpendIndicator.ToUpper() == "Y")
                {
                    value = _configuration.GetValue<string>("CreditCardSpendPQDEligibleMet");
                }
            }
            return value;
        }

        private void GetIncrementalThresholdMilesAndSegments(long currentMiles, double segments, out long thresholdMiles, out long thresholdSegments)
        {
            thresholdSegments = 0;
            thresholdMiles = 0;
            long milesQuotient = 0;
            long segmentsQuotient = 0;
            try
            {
                long seg = Convert.ToInt64(segments);
                milesQuotient = currentMiles > 0 ? currentMiles / 25000 : 0;
                segmentsQuotient = seg > 0 ? seg / 30 : 0;
                if (milesQuotient >= segmentsQuotient)
                {
                    thresholdMiles = (milesQuotient * 25000) + 25000;
                    thresholdSegments = (milesQuotient * 30) + 30;
                }
                else
                {
                    thresholdMiles = (segmentsQuotient * 25000) + 25000;
                    thresholdSegments = (segmentsQuotient * 30) + 30;
                }
            }
            catch (Exception)
            {
            }
        }

        private int GetTrackingUpgradeType(ServiceResponse sResponse)
        {
            int trackingUpgradeType = 0;//1=Platinum Upgrade; 2=1k Upgrade; 3= Incremental Upgrade More Comments to understand whats this "trackingUpgradeType" use for
            if (sResponse != null && sResponse.CurrentYearActivity != null)
            {
                trackingUpgradeType = IsMPUpgradeEligible(sResponse.TrackingLevel, sResponse.CurrentPremierLevel, sResponse.InfiniteLevel, sResponse.LifetimeMiles,
                                        sResponse.CurrentYearActivity.EliteQualifyingMiles, sResponse.CurrentYearActivity.EliteQualifyingPoints, sResponse.CurrentYearActivity.TotalRevenue)
                                        ?
                    GetTrackingUpgradeType(sResponse.CurrentYearActivity.EliteQualifyingMiles, sResponse.CurrentYearActivity.FlexEliteQualifyingMiles, sResponse.CurrentYearActivity.EliteQualifyingPoints,
                        sResponse.CurrentYearActivity.TotalFlightRevenue, sResponse.CurrentYearActivity.MinimumSegmentsRequired, IsPresidentialPlusCard(sResponse.CurrentYearActivity.PresidentPlusIndicator),
                        IsInternationalMember(sResponse.CurrentYearActivity.DomesticIndicator), IsChaseCardSpendIndicator(sResponse.MileagePlusCardIndicator, sResponse.CurrentYearActivity.ChaseCardSpendIndicator),
                        Is4FlightSegmentWaived(sResponse.CurrentYearActivity.RpcWaivedIndicator)) : 0;
            }
            return trackingUpgradeType;
        }

        private bool IsMPUpgradeEligible(int trackingMPLevel, int currentPremierLevel, int infiniteLevel, long lifeTimeMiles, long pqm, double pqs, double pqd)
        {
            if (trackingMPLevel == 6 && (currentPremierLevel == 4 || currentPremierLevel == 5) &&
                (infiniteLevel == 4 || infiniteLevel == 5 || lifeTimeMiles >= 3000000 || ((pqm >= 100000 || pqs >= 120) && pqd >= 15000)))
            {
                return true;
            }
            return false;
        }

        private bool Is4FlightSegmentWaived(string rpcWaivedIndicator)
        {
            if (!_configuration.GetValue<bool>("EnableVBQII") && !string.IsNullOrEmpty(rpcWaivedIndicator) && rpcWaivedIndicator.ToUpper() == "Y")
            {
                return true;
            }
            return false;
        }

        private int GetTrackingUpgradeType(long eliteQualifyingMiles, long flexEliteQualifyingMiles, double pqs, double pqd, double minSegmentRequired, bool isPresidentialPlusCard, bool isInternationalMember, bool isChaseCardSpendIndicator, bool is4FlightSegmentWaived)
        {
            long pqm = eliteQualifyingMiles - flexEliteQualifyingMiles;

            if ((minSegmentRequired >= 4 || is4FlightSegmentWaived) && (pqm >= 100000 || pqs >= 120) && (pqd >= 15000 || isInternationalMember))
            {
                return 3;
            }
            else if ((minSegmentRequired >= 4 || is4FlightSegmentWaived) && ((pqm >= 75000 && pqm < 100000) || (pqm < 75000 && eliteQualifyingMiles >= 75000) || (pqs >= 90 && pqs < 120))
               && (pqd >= 9000 || IsPQDWaived(isPresidentialPlusCard, isInternationalMember, isChaseCardSpendIndicator))) //1K
            {
                return 2;
            }
            // When user has < 4 Minimum Segments Flown or user has <75K PQM AND (<$9,000 PQD OR PQD Waiver) or 
            // user has<90 PQS AND(<$9,000 PQD OR PQD Waiver) -- Falls in Platinum Upgrade
            else if (minSegmentRequired < 4 ||
                ((eliteQualifyingMiles < 75000 || pqs < 90) && (pqd < 9000 || IsPQDWaived(isPresidentialPlusCard, isInternationalMember, isChaseCardSpendIndicator)))
               ) // For Platinum upgrades consider Flex miles
            {
                return 1; //Platinum upgrade
            }

            return 0;
        }

        private bool IsPQDWaived(bool isPresidentialPlusCard, bool isInternationalMember, bool isChaseCardSpendIndicator)
        {
            bool isWaived = false;
            if (isInternationalMember || isPresidentialPlusCard || isChaseCardSpendIndicator)
            {
                isWaived = true;
            }
            return isWaived;
        }

        public async Task<bool> GetProfile_AllTravelerData(string mileagePlusNumber, string transactionId, string dpToken, int applicationId, string appVersion, string deviceId)
        {
            ProfileRequest profileRequest = new ProfileRequest();
            bool isTSAFlagON = false;
            profileRequest.LoyaltyId = mileagePlusNumber;
            profileRequest.RefreshCache = false;
            profileRequest.DataSetting = _configuration.GetValue<string>("CustomerDBDataSettingForCSLServices");
            profileRequest.LangCode = "en-US";
            List<string> requestStringList = new List<string>
            {
                "AllTravelerData",
                "SecureTravelers"
            };
            profileRequest.DataToLoad = requestStringList;
            try
            {
                string jsonRequest = DataContextJsonSerializer.Serialize<ProfileRequest>(profileRequest);

                var customerDataResponse = await _customerDataService.GetCustomerData<ProfileResponse>(dpToken, _headers.ContextValues.SessionId, jsonRequest).ConfigureAwait(false);

                var response = customerDataResponse.response;
                if (response != null)
                {
                    if (response?.Profiles?.FirstOrDefault()?.Travelers?.Count > 0)
                    {
                        foreach (var traveler in response?.Profiles?.FirstOrDefault()?.Travelers)
                        {
                            if (traveler.MileagePlusId?.ToUpper() == mileagePlusNumber?.ToUpper())
                            {
                                foreach (var secureTraveler in traveler.SecureTravelers)
                                {
                                    if (secureTraveler?.DocumentType?.Trim()?.ToUpper() == "U")
                                    {
                                        isTSAFlagON = true;
                                        return isTSAFlagON;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException wex)
            {
                if (wex.Response != null)
                {
                    throw new Exception(wex.Message);
                }
            }
            return isTSAFlagON;
        }

        private MOBErrorPremierActivity GetErrorPremierActivity(ServiceResponse sResponse)
        {
            MOBErrorPremierActivity errorPremierActivity = new MOBErrorPremierActivity();
            if (sResponse != null && sResponse.Status != null && !string.IsNullOrEmpty(sResponse.Status.Code))
            {
                if (sResponse.CurrentYearActivity != null)
                {
                    errorPremierActivity.ErrorPremierActivityTitle = _configuration.GetValue<string>("PremierActivityTitle");
                    if (sResponse.Status.Code.Trim().ToUpper() == "E8000")
                    {
                        errorPremierActivity.ShowErrorIcon = false;
                        errorPremierActivity.ErrorPremierActivityText = _configuration.GetValue<string>("InactiveErrorActivityText");
                    }
                    else
                    {
                        errorPremierActivity.ShowErrorIcon = true;
                        errorPremierActivity.ErrorPremierActivityText = _configuration.GetValue<string>("OtherErrorActivityText");
                    }
                }
            }
            return errorPremierActivity;
        }

        private MOBPremierActivity GetPremierActivity(ServiceResponse serviceResponse, ref string pdqchasewaiverLabel, ref string pdqchasewavier)
        {
            MOBPremierActivity premierActivity = new MOBPremierActivity();
            MOBPremierQualifierTracker pqm1 = new MOBPremierQualifierTracker();
            MOBPremierQualifierTracker pqs1 = new MOBPremierQualifierTracker();
            MOBPremierQualifierTracker pqd1 = new MOBPremierQualifierTracker();

            if (serviceResponse != null && serviceResponse.CurrentYearActivity != null)
            {
                List<MOBKVP> lPremierTrackingLevels = new List<MOBKVP>();
                List<MOBKVP> lFlexMiles = new List<MOBKVP>();
                bool isPresidentialPlusCard = false;
                lPremierTrackingLevels = GetPremierTrackingLevels(serviceResponse.CurrentPremierLevel, serviceResponse.TrackingLevel);

                premierActivity.PremierActivityTitle = _configuration.GetValue<string>("PremierActivityTitle");
                premierActivity.PremierActivityYear = _configuration.GetValue<string>("EarnedInText") + " " + serviceResponse.CurrentYearActivity.ActivityYear;
                premierActivity.PremierActivityStatus = (lPremierTrackingLevels != null && lPremierTrackingLevels.Count > 0) ? lPremierTrackingLevels.Where(s => s.Key == "PremierActivityStatus").SingleOrDefault().Value : "0";

                isPresidentialPlusCard = IsPresidentialPlusCard(serviceResponse.CurrentYearActivity);
                pqm1.PremierQualifierTrackerTitle = _configuration.GetValue<string>("PremierQualifierTrackerMilesTitle");
                pqm1.PremierQualifierTrackerCurrentValue = Convert.ToString(serviceResponse.CurrentYearActivity.EliteQualifyingMiles - serviceResponse.CurrentYearActivity.FlexEliteQualifyingMiles);
                if (isPresidentialPlusCard && serviceResponse.TrackingLevel < 4 && serviceResponse.CurrentYearActivity.FlexEliteQualifyingMiles > 0)
                {
                    pqm1.PremierQualifierTrackerCurrentText = string.Format("{0:###,##0}", serviceResponse.CurrentYearActivity.EliteQualifyingMiles);
                    pqm1.PremierQualifierTrackerCurrentFlexValue = Convert.ToString(serviceResponse.CurrentYearActivity.FlexEliteQualifyingMiles);
                    pqm1.PremierQualifierTrackerCurrentFlexTitle = _configuration.GetValue<string>("flexPQMTitle") + " " + string.Format("{0:###,##0}", serviceResponse.CurrentYearActivity.FlexEliteQualifyingMiles);
                }
                else
                {
                    pqm1.PremierQualifierTrackerCurrentText = string.Format("{0:###,##0}", serviceResponse.CurrentYearActivity.EliteQualifyingMiles - serviceResponse.CurrentYearActivity.FlexEliteQualifyingMiles);
                    pqm1.PremierQualifierTrackerCurrentFlexValue = "0";
                }
                pqm1.PremierQualifierTrackerThresholdValue = (lPremierTrackingLevels != null && lPremierTrackingLevels.Count > 0) ? lPremierTrackingLevels.Where(s => s.Key == "TrackerThresholdMiles").SingleOrDefault().Value : "0";
                pqm1.PremierQualifierTrackerThresholdText = string.Format("{0:###,##0}", (lPremierTrackingLevels != null && lPremierTrackingLevels.Count > 0) ? Convert.ToDecimal(lPremierTrackingLevels.Where(s => s.Key == "TrackerThresholdMiles").SingleOrDefault().Value) : 0);
                pqm1.PremierQualifierTrackerThresholdPrefix = "of";
                pqm1.Separator = "or";
                pqm1.IsWaived = false;
                premierActivity.PQM = pqm1;

                pqs1.PremierQualifierTrackerTitle = _configuration.GetValue<string>("PremierQualifierTrackerSegmentsTitle");
                pqs1.PremierQualifierTrackerCurrentValue = Convert.ToString(serviceResponse.CurrentYearActivity.EliteQualifyingPoints);
                pqs1.PremierQualifierTrackerCurrentText = string.Format("{0:0.#}", serviceResponse.CurrentYearActivity.EliteQualifyingPoints);
                pqs1.PremierQualifierTrackerThresholdValue = (lPremierTrackingLevels != null && lPremierTrackingLevels.Count > 0) ? lPremierTrackingLevels.Where(s => s.Key == "TrackerThresholdSegments").SingleOrDefault().Value : "0";
                pqs1.PremierQualifierTrackerThresholdText = string.Format("{0:###,##0}", (lPremierTrackingLevels != null && lPremierTrackingLevels.Count > 0) ? Convert.ToDecimal(lPremierTrackingLevels.Where(s => s.Key == "TrackerThresholdSegments").SingleOrDefault().Value) : 0);
                pqs1.PremierQualifierTrackerThresholdPrefix = "of";
                pqs1.Separator = "+";
                pqs1.IsWaived = false;
                premierActivity.PQS = pqs1;

                pqd1.PremierQualifierTrackerTitle = _configuration.GetValue<string>("PremierQualifierTrackerDollarsTitle"); ;
                pqd1.PremierQualifierTrackerCurrentValue = Convert.ToString(Convert.ToDecimal(serviceResponse.CurrentYearActivity.TotalRevenue));
                pqd1.PremierQualifierTrackerCurrentText = serviceResponse.CurrentYearActivity.TotalRevenue > 0 ? serviceResponse.CurrentYearActivity.TotalRevenue.ToString("C0") : "$0";
                pqd1.IsWaived = IsPQDWaived(serviceResponse);
                pqd1.PremierQualifierTrackerThresholdValue = (lPremierTrackingLevels != null && lPremierTrackingLevels.Count > 0) ? lPremierTrackingLevels.Where(s => s.Key == "TrackerThresholdDollars").SingleOrDefault().Value : "0";
                pqd1.PremierQualifierTrackerThresholdText = pqd1.IsWaived == true ? _configuration.GetValue<string>("WaivedText") : (lPremierTrackingLevels != null && lPremierTrackingLevels.Count > 0) ? Convert.ToDecimal(lPremierTrackingLevels.Where(s => s.Key == "TrackerThresholdDollars").SingleOrDefault().Value).ToString("C0") : "$0";
                pqd1.PremierQualifierTrackerThresholdPrefix = pqd1.IsWaived == true ? string.Empty : "of";
                premierActivity.PQD = pqd1;

                List<MOBKVP> lKeyValue = new List<MOBKVP>();

                MOBKVP premierActivityKey1 = new MOBKVP
                {
                    Key = _configuration.GetValue<string>("flightsegmentminimum"),
                    Value = Get4FlightSegmentMinimum(serviceResponse.CurrentYearActivity)
                };
                lKeyValue.Add(premierActivityKey1);
                if (!_configuration.GetValue<bool>("EnableVBQII") && !IsInternationalMember(serviceResponse.CurrentYearActivity))
                {
                    if (_configuration.GetValue<bool>("IsAvoidUAWSChaseMessagingSoapCall"))
                    {
                        MOBKVP premierActivityKey2 = new MOBKVP
                        {
                            Key = isPresidentialPlusCard ? _configuration.GetValue<string>("PresidentialPlusPQDWaiver") : _configuration.GetValue<string>("creditcardspendPQDWaiver")
                        };
                        if (serviceResponse.TrackingLevel == 4) //Loyal-3705
                        {
                            premierActivityKey2.Value = _configuration.GetValue<string>("CreditCardSpendPQDNotapplicable");
                        }
                        else
                        {
                            premierActivityKey2.Value = isPresidentialPlusCard ? !_configuration.GetValue<bool>("EnableVBQII") ? _configuration.GetValue<string>("PresidentialPlusPQDWaiverValue") : GetCreditCardSpendPQDWaiverText(serviceResponse) : GetCreditCardSpendPQDWaiverText(serviceResponse);
                        }

                        lKeyValue.Add(premierActivityKey2);
                        if (_configuration.GetValue<bool>("IsAvoidUAWSChaseMessagingSoapCall"))
                        {
                            pdqchasewaiverLabel = premierActivityKey2.Key;
                            pdqchasewavier = premierActivityKey2.Value;
                        }
                    }
                }
                MOBKVP premierActivityKey3 = new MOBKVP
                {
                    Key = _configuration.GetValue<string>("viewpremierstatustracker"),
                    Value = _configuration.GetValue<string>("viewpremierstatustrackerlink")
                };

                lKeyValue.Add(premierActivityKey3);
                premierActivity.KeyValueList = lKeyValue;
            }
            return premierActivity;

        }

        private string Get4FlightSegmentMinimum(Activity currentYearActivity)
        {
            string value = string.Empty;
            if (currentYearActivity != null)
            {
                if (currentYearActivity.RpcWaivedIndicator.ToUpper() == "N" || (_configuration.GetValue<bool>("EnableVBQII") && currentYearActivity.RpcWaivedIndicator.ToUpper() == "Y"))
                {
                    value = currentYearActivity.MinimumSegmentsRequired >= 4 ? "4 of 4" : currentYearActivity.MinimumSegmentsRequired + " of 4";
                }
                else if (currentYearActivity.RpcWaivedIndicator.ToUpper() == "Y")
                {
                    value = _configuration.GetValue<string>("WaivedText");
                }
            }
            return value;
        }

        private bool IsPQDWaived(ServiceResponse serviceResponse)
        {
            bool isWaived = false;
            if (!_configuration.GetValue<bool>("EnableVBQII") && serviceResponse != null && serviceResponse.CurrentYearActivity != null &&
                (
                (serviceResponse.CurrentYearActivity.DomesticIndicator != null && serviceResponse.CurrentYearActivity.DomesticIndicator.ToUpper() == "N") ||
                (serviceResponse.CurrentYearActivity.PresidentPlusIndicator != null && serviceResponse.CurrentYearActivity.PresidentPlusIndicator.ToUpper() == "Y" && serviceResponse.TrackingLevel < 4) ||
                (serviceResponse.MileagePlusCardIndicator != null && serviceResponse.MileagePlusCardIndicator.ToUpper() == "Y" &&
                serviceResponse.CurrentYearActivity.ChaseCardSpendIndicator != null && serviceResponse.CurrentYearActivity.ChaseCardSpendIndicator.ToUpper() == "Y" && serviceResponse.TrackingLevel < 4)
                )
                )
            {
                isWaived = true;
            }

            return isWaived;
        }

        private bool IsPresidentialPlusCard(Activity currentYearActivity)
        {
            if (currentYearActivity != null && !string.IsNullOrEmpty(currentYearActivity.PresidentPlusIndicator) && currentYearActivity.PresidentPlusIndicator.ToUpper() == "Y")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private List<MOBKVP> GetPremierTrackingLevels(int currentLevel, int trackingLevel)
        {
            string PremierLevelStatus = string.Empty;
            List<MOBKVP> list = new List<MOBKVP>();
            string premierStatusPrefix = string.Empty;
            if (currentLevel == trackingLevel)
            {
                premierStatusPrefix = _configuration.GetValue<string>("TorequalifyPrefix");
            }
            else
            {
                premierStatusPrefix = _configuration.GetValue<string>("ToreachPrefix");
            }
            switch (trackingLevel)
            {
                case 1:
                    list.Add(new MOBKVP("PremierActivityStatus", premierStatusPrefix + " " + _configuration.GetValue<string>("PremierStatusSilverText")));
                    list.Add(new MOBKVP("TrackerThresholdMiles", _configuration.GetValue<string>("ThresholdMilesSilver")));
                    list.Add(new MOBKVP("TrackerThresholdSegments", _configuration.GetValue<string>("ThresholdSegmentsSilver")));
                    list.Add(new MOBKVP("TrackerThresholdDollars", _configuration.GetValue<string>("ThresholdDollarsSilver")));
                    break;
                case 2:
                    list.Add(new MOBKVP("PremierActivityStatus", premierStatusPrefix + " " + _configuration.GetValue<string>("PremierStatusGoldText")));
                    list.Add(new MOBKVP("TrackerThresholdMiles", _configuration.GetValue<string>("ThresholdMilesGold")));
                    list.Add(new MOBKVP("TrackerThresholdSegments", _configuration.GetValue<string>("ThresholdSegmentsGold")));
                    list.Add(new MOBKVP("TrackerThresholdDollars", _configuration.GetValue<string>("ThresholdDollarsGold")));
                    break;
                case 3:
                    list.Add(new MOBKVP("PremierActivityStatus", premierStatusPrefix + " " + _configuration.GetValue<string>("PremierStatusPlatinumText")));
                    list.Add(new MOBKVP("TrackerThresholdMiles", _configuration.GetValue<string>("ThresholdMilesPlatinum")));
                    list.Add(new MOBKVP("TrackerThresholdSegments", _configuration.GetValue<string>("ThresholdSegmentsPlatinum")));
                    list.Add(new MOBKVP("TrackerThresholdDollars", _configuration.GetValue<string>("ThresholdDollarsPlatinum")));
                    break;
                case 4:
                case 6:
                    list.Add(new MOBKVP("PremierActivityStatus", GetPremierActivityStatus(currentLevel, trackingLevel)));
                    list.Add(new MOBKVP("TrackerThresholdMiles", _configuration.GetValue<string>("ThresholdMiles1K")));
                    list.Add(new MOBKVP("TrackerThresholdSegments", _configuration.GetValue<string>("ThresholdSegments1K")));
                    list.Add(new MOBKVP("TrackerThresholdDollars", _configuration.GetValue<string>("ThresholdDollars1K")));
                    break;
                default:
                    break;
            }

            return list;
        }

        private string GetPremierActivityStatus(int currentLevel, int trackingLevel)
        {
            string premierActivityStatus = string.Empty;
            if (currentLevel == 3 && trackingLevel == 4)
            {
                premierActivityStatus = _configuration.GetValue<string>("ToreachPrefix") + " " + string.Format(_configuration.GetValue<string>("Premier1KText"), (char)174);
            }
            else if (currentLevel == 4 && trackingLevel == 4)
            {
                premierActivityStatus = _configuration.GetValue<string>("TorequalifyPrefix") + " " + string.Format(_configuration.GetValue<string>("Premier1KText"), (char)174);
            }
            else if (currentLevel >= 4 && trackingLevel >= 4)
            {
                premierActivityStatus = _configuration.GetValue<bool>("EnablePointsActivity") ? _configuration.GetValue<string>
                    ("EarnPremierPlusPoints") : _configuration.GetValue<string>("EarnPremierupgrades");
            }
            return premierActivityStatus;
        }

        private bool IsPremierStatusTrackerUpgradesSupportedVersion(int appId, string appVersion)
        {
            bool isPremierStatusTrackerSupportedVersion = false;
            if (!string.IsNullOrEmpty(appVersion) && appId != -1)
            {
                isPremierStatusTrackerSupportedVersion = GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidPremierStatusTrackerUpgradesVersion", "iPhonePremierStatusTrackerUpgradesVersion", "", "", true, _configuration);
            }
            return isPremierStatusTrackerSupportedVersion;
        }

        private List<MOBKVP> GetVBQPremierTrackingLevels(int currentLevel, int trackingLevel, int infiniteLevel, long lifetimemiles)
        {
            string PremierLevelStatus = string.Empty;
            List<MOBKVP> list = new List<MOBKVP>();
            string premierStatusPrefix = string.Empty;
            if (currentLevel == trackingLevel)
            {
                premierStatusPrefix = _configuration.GetValue<string>("TorequalifyPrefix");
            }
            else
            {
                premierStatusPrefix = _configuration.GetValue<string>("ToreachPrefix");
            }
            switch (trackingLevel)
            {
                case 1:
                    list.Add(new MOBKVP("PremierActivityStatus", premierStatusPrefix + " " + _configuration.GetValue<string>("PremierStatusSilverText")));
                    list.Add(new MOBKVP("TrackerThresholdPQF", _configuration.GetValue<string>("ThresholdPQFSilver")));
                    list.Add(new MOBKVP("TrackerThresholdPQP", _configuration.GetValue<string>("ThresholdPQPSilver")));
                    list.Add(new MOBKVP("TrackerThresholdOutrightPQP", _configuration.GetValue<string>("ThresholdOutrightPQPSilver")));
                    break;
                case 2:
                    list.Add(new MOBKVP("PremierActivityStatus", premierStatusPrefix + " " + _configuration.GetValue<string>("PremierStatusGoldText")));
                    list.Add(new MOBKVP("TrackerThresholdPQF", _configuration.GetValue<string>("ThresholdPQFGold")));
                    list.Add(new MOBKVP("TrackerThresholdPQP", _configuration.GetValue<string>("ThresholdPQPGold")));
                    list.Add(new MOBKVP("TrackerThresholdOutrightPQP", _configuration.GetValue<string>("ThresholdOutrightPQPGold")));
                    break;
                case 3:
                    list.Add(new MOBKVP("PremierActivityStatus", premierStatusPrefix + " " + _configuration.GetValue<string>("PremierStatusPlatinumText")));
                    list.Add(new MOBKVP("TrackerThresholdPQF", _configuration.GetValue<string>("ThresholdPQFPlatinum")));
                    list.Add(new MOBKVP("TrackerThresholdPQP", _configuration.GetValue<string>("ThresholdPQPPlatinum")));
                    list.Add(new MOBKVP("TrackerThresholdOutrightPQP", _configuration.GetValue<string>("ThresholdOutrightPQPPlatinum")));
                    break;
                case 4:
                case 6:
                    list.Add(new MOBKVP("PremierActivityStatus", GetVBQPremierActivityStatus(currentLevel, trackingLevel, infiniteLevel, lifetimemiles)));
                    list.Add(new MOBKVP("TrackerThresholdPQF", _configuration.GetValue<string>("ThresholdPQF1K")));
                    list.Add(new MOBKVP("TrackerThresholdPQP", _configuration.GetValue<string>("ThresholdPQP1K")));
                    list.Add(new MOBKVP("TrackerThresholdOutrightPQP", _configuration.GetValue<string>("ThresholdOutrightPQP1K")));
                    break;
                default:
                    break;
            }
            return list;
        }

        private string GetVBQPremierActivityStatus(int currentLevel, int trackingLevel, int infiniteLevel, long lifetimemiles)
        {
            string premierActivityStatus = string.Empty;
            if (currentLevel == 3 && trackingLevel == 4)
            {
                premierActivityStatus = _configuration.GetValue<string>("ToreachPrefix") + " " + string.Format(_configuration.GetValue<string>("Premier1KText"), (char)174);
            }
            else if (currentLevel == 4 && trackingLevel == 4)
            {
                premierActivityStatus = _configuration.GetValue<string>("TorequalifyPrefix") + " " + string.Format(_configuration.GetValue<string>("Premier1KText"), (char)174);
            }
            else if (currentLevel == 5 && trackingLevel == 4)
            {
                premierActivityStatus = _configuration.GetValue<string>("ToreachPrefix") + " " + string.Format(_configuration.GetValue<string>("Premier1KText"), (char)174);
            }
            else if (trackingLevel == 6 && (currentLevel == 4 || currentLevel == 5) && ((infiniteLevel == 4 || infiniteLevel == 5) || lifetimemiles >= 3000000))
            {
                premierActivityStatus = _configuration.GetValue<string>("EarnPremierPlusPoints");
            }
            else if (currentLevel >= 4 && trackingLevel >= 4)
            {
                premierActivityStatus = _configuration.GetValue<string>("EarnPremierPlusPoints");
            }
            return premierActivityStatus;
        }

        private MOBVBQPremierActivity GetVBQPremierActivity(ServiceResponse sResponse)
        {
            MOBVBQPremierActivity premierActivity = new MOBVBQPremierActivity();
            MOBPremierQualifierTracker pqf = new MOBPremierQualifierTracker();
            MOBPremierQualifierTracker pqp = new MOBPremierQualifierTracker();
            MOBPremierQualifierTracker outrightPQP = new MOBPremierQualifierTracker();
            if (sResponse != null && sResponse.CurrentYearActivity != null)
            {
                List<MOBKVP> lPremierTrackingLevels = new List<MOBKVP>();
                List<MOBKVP> lFlexMiles = new List<MOBKVP>();
                lPremierTrackingLevels = GetVBQPremierTrackingLevels(sResponse.CurrentPremierLevel, sResponse.TrackingLevel, sResponse.InfiniteLevel, sResponse.LifetimeMiles);
                premierActivity.vBQPremierActivityTitle = _configuration.GetValue<string>("PremierActivityTitle");
                premierActivity.vBQPpremierActivityYear = _configuration.GetValue<string>("EarnedInText") + " " + sResponse.CurrentYearActivity.ActivityYear;
                pqf.PremierQualifierTrackerTitle = _configuration.GetValue<string>("PremierQualifierTrackerFlightsTitle");
                pqf.PremierQualifierTrackerCurrentValue = Convert.ToString(sResponse.CurrentYearActivity.PremierQualifyingFlightSegments);
                pqf.PremierQualifierTrackerCurrentText = string.Format("{0:###,##0}", sResponse.CurrentYearActivity.PremierQualifyingFlightSegments);
                pqf.PremierQualifierTrackerThresholdValue = (lPremierTrackingLevels != null && lPremierTrackingLevels.Count > 0) ? lPremierTrackingLevels.Where(s => s.Key == "TrackerThresholdPQF").SingleOrDefault().Value : "0";
                pqf.PremierQualifierTrackerThresholdText = string.Format("{0:###,##0}", (lPremierTrackingLevels != null && lPremierTrackingLevels.Count > 0) ? Convert.ToDecimal(lPremierTrackingLevels.Where(s => s.Key == "TrackerThresholdPQF").SingleOrDefault().Value) : 0);
                pqf.Separator = "+";
                pqf.PremierQualifierTrackerThresholdPrefix = "of";
                pqp.PremierQualifierTrackerTitle = _configuration.GetValue<string>("PremierQualifierTrackerPointsTitle");
                pqp.PremierQualifierTrackerCurrentValue = sResponse.TrackingLevel > 3 ? (sResponse.CurrentYearActivity.PremierQualifyingPoints - sResponse.CurrentYearActivity.FlexPremierQualifyingPoints).ToString() : sResponse.CurrentYearActivity.PremierQualifyingPoints.ToString();
                pqp.PremierQualifierTrackerCurrentText = string.Format("{0:###,##0}", Convert.ToInt32(pqp.PremierQualifierTrackerCurrentValue));

                pqp.PremierQualifierTrackerThresholdValue = (lPremierTrackingLevels != null && lPremierTrackingLevels.Count > 0) ? lPremierTrackingLevels.Where(s => s.Key == "TrackerThresholdPQP").SingleOrDefault().Value : "0";
                pqp.PremierQualifierTrackerThresholdText = string.Format("{0:###,##0}", (lPremierTrackingLevels != null && lPremierTrackingLevels.Count > 0) ? Convert.ToDecimal(lPremierTrackingLevels.Where(s => s.Key == "TrackerThresholdPQP").SingleOrDefault().Value) : 0);
                pqp.Separator = "or";
                pqp.PremierQualifierTrackerThresholdPrefix = "of";

                outrightPQP.PremierQualifierTrackerTitle = _configuration.GetValue<string>("PremierQualifierTrackerPointsTitle");
                outrightPQP.PremierQualifierTrackerCurrentValue = sResponse.TrackingLevel > 3 ? (sResponse.CurrentYearActivity.PremierQualifyingPoints - sResponse.CurrentYearActivity.FlexPremierQualifyingPoints).ToString() : sResponse.CurrentYearActivity.PremierQualifyingPoints.ToString(); ;
                outrightPQP.PremierQualifierTrackerCurrentText = string.Format("{0:###,##0}", Convert.ToInt32(outrightPQP.PremierQualifierTrackerCurrentValue));
                outrightPQP.PremierQualifierTrackerThresholdValue = (lPremierTrackingLevels != null && lPremierTrackingLevels.Count > 0) ? lPremierTrackingLevels.Where(s => s.Key == "TrackerThresholdOutrightPQP").SingleOrDefault().Value : "0";
                outrightPQP.PremierQualifierTrackerThresholdText = string.Format("{0:###,##0}", (lPremierTrackingLevels != null && lPremierTrackingLevels.Count > 0) ? Convert.ToDecimal(lPremierTrackingLevels.Where(s => s.Key == "TrackerThresholdOutrightPQP").SingleOrDefault().Value) : 0);
                premierActivity.vBQPremierActivityStatus = (lPremierTrackingLevels != null && lPremierTrackingLevels.Count > 0) ? lPremierTrackingLevels.Where(s => s.Key == "PremierActivityStatus").SingleOrDefault().Value : "0";
                outrightPQP.PremierQualifierTrackerThresholdPrefix = "of";
                premierActivity.pQF = pqf;
                premierActivity.pQP = pqp;
                premierActivity.OutrightPQP = outrightPQP;
                List<MOBKVP> lKeyValue = new List<MOBKVP>();
                MOBKVP premierActivityKey1 = new MOBKVP
                {
                    Key = _configuration.GetValue<string>("flightsegmentminimum"),
                    Value = Get4FlightSegmentMinimum(sResponse.CurrentYearActivity)
                };
                lKeyValue.Add(premierActivityKey1);
                if (sResponse.CurrentYearActivity.FlexPremierQualifyingPoints > 0)
                {
                    MOBKVP premierActivityChasePqp = new MOBKVP
                    {
                        Key = _configuration.GetValue<string>("ChasePQPText"),
                        Value = Convert.ToString(sResponse.CurrentYearActivity.FlexPremierQualifyingPoints)
                    };
                    lKeyValue.Add(premierActivityChasePqp);
                }
                MOBKVP premierActivityKey3 = new MOBKVP
                {
                    Key = _configuration.GetValue<string>("viewpremierstatustracker"),
                    Value = _configuration.GetValue<string>("viewpremierstatustrackerlink")
                };
                lKeyValue.Add(premierActivityKey3);
                premierActivity.KeyValueList = lKeyValue;
            }
            return premierActivity;
        }

        private MOBVBQPremierActivity GetVBQTrackingUpgradesActivity(ServiceResponse sResponse, int trackingUpgradeType)
        {
            MOBVBQPremierActivity upgradeActivity = new MOBVBQPremierActivity();
            MOBPremierQualifierTracker pqf = new MOBPremierQualifierTracker();
            MOBPremierQualifierTracker pqp = new MOBPremierQualifierTracker();
            MOBPremierQualifierTracker outrightPQP = new MOBPremierQualifierTracker();
            if (sResponse != null && sResponse.CurrentYearActivity != null)
            {
                List<MOBKVP> lUpgradeTrackingLevels = new List<MOBKVP>();
                string premierStatusPrefix = _configuration.GetValue<string>("TorequalifyPrefix");
                pqf.PremierQualifierTrackerCurrentValue = sResponse.CurrentYearActivity.PremierQualifyingFlightSegments.ToString();
                pqf.PremierQualifierTrackerCurrentText = string.Format("{0:###,##0}", sResponse.CurrentYearActivity.PremierQualifyingFlightSegments);
                pqf.PremierQualifierTrackerThresholdPrefix = "of";
                pqf.Separator = "+";
                pqf.PremierQualifierTrackerTitle = _configuration.GetValue<string>("PremierQualifierTrackerFlightsTitle");
                pqp.PremierQualifierTrackerCurrentValue = trackingUpgradeType == 1 ? sResponse.CurrentYearActivity.PremierQualifyingPoints.ToString() : (sResponse.CurrentYearActivity.PremierQualifyingPoints - sResponse.CurrentYearActivity.FlexPremierQualifyingPoints).ToString();
                pqp.PremierQualifierTrackerCurrentText = string.Format("{0:###,##0}", Convert.ToInt32(pqp.PremierQualifierTrackerCurrentValue));
                pqp.Separator = trackingUpgradeType != 3 ? "or" : string.Empty;
                pqp.PremierQualifierTrackerTitle = _configuration.GetValue<string>("PremierQualifierTrackerPointsTitle");
                outrightPQP.PremierQualifierTrackerCurrentValue = trackingUpgradeType == 1 ? sResponse.CurrentYearActivity.PremierQualifyingPoints.ToString() : (sResponse.CurrentYearActivity.PremierQualifyingPoints - sResponse.CurrentYearActivity.FlexPremierQualifyingPoints).ToString(); ;
                outrightPQP.PremierQualifierTrackerCurrentText = string.Format("{0:###,##0}", Convert.ToInt32(outrightPQP.PremierQualifierTrackerCurrentValue));
                // pqp.PremierQualifierTrackerCurrentChaseFlexValue = string.Format("{0:###,##0}", sResponse.CurrentYearActivity.FlexPremierQualifyingPoints);
                // outrightPQP.PremierQualifierTrackerCurrentChaseFlexValue = string.Format("{0:###,##0}", sResponse.CurrentYearActivity.FlexPremierQualifyingPoints);
                lUpgradeTrackingLevels = GetVBQUpgradeTrackingLevels(trackingUpgradeType, Convert.ToInt64(sResponse.CurrentYearActivity.PremierQualifyingFlightSegments), sResponse.CurrentYearActivity.PremierQualifyingPoints);
                pqf.PremierQualifierTrackerThresholdValue = trackingUpgradeType == 1 ? GetValueFromList(lUpgradeTrackingLevels, "TrackerThresholdPQF") : trackingUpgradeType == 2 ? GetValueFromList(lUpgradeTrackingLevels, "TrackerThresholdPQF") : string.Empty;
                pqp.PremierQualifierTrackerThresholdValue = trackingUpgradeType == 1 ? GetValueFromList(lUpgradeTrackingLevels, "TrackerThresholdPQP") : trackingUpgradeType == 2 ? GetValueFromList(lUpgradeTrackingLevels, "TrackerThresholdPQP") : trackingUpgradeType == 3 ? GetValueFromList(lUpgradeTrackingLevels, "TrackerThresholdPQP") : string.Empty;
                outrightPQP.PremierQualifierTrackerThresholdValue = trackingUpgradeType == 1 ? GetValueFromList(lUpgradeTrackingLevels, "TrackerThresholdOutrightPQP") : trackingUpgradeType == 2 ? GetValueFromList(lUpgradeTrackingLevels, "TrackerThresholdOutrightPQP") : string.Empty;
                pqf.PremierQualifierTrackerThresholdText = string.IsNullOrEmpty(pqf.PremierQualifierTrackerThresholdValue) ? string.Empty : string.Format("{0:###,##0}", Convert.ToDecimal(pqf.PremierQualifierTrackerThresholdValue));
                pqp.PremierQualifierTrackerThresholdText = string.IsNullOrEmpty(pqp.PremierQualifierTrackerThresholdValue) ? string.Empty : string.Format("{0:###,##0}", Convert.ToDecimal(pqp.PremierQualifierTrackerThresholdValue));
                outrightPQP.PremierQualifierTrackerThresholdText = string.IsNullOrEmpty(outrightPQP.PremierQualifierTrackerThresholdValue) ? string.Empty : string.Format("{0:###,##0}", Convert.ToDecimal(outrightPQP.PremierQualifierTrackerThresholdValue));
                upgradeActivity.vBQPremierActivityTitle = _configuration.GetValue<string>("PremierActivityTitle");
                upgradeActivity.vBQPpremierActivityYear = _configuration.GetValue<string>("EarnedInText") + " " + sResponse.CurrentYearActivity.ActivityYear;
                upgradeActivity.KeyValueList = GetVBQUpgradeKeyValues(trackingUpgradeType, sResponse.CurrentYearActivity.MinimumSegmentsRequired >= 4 ? 4 : sResponse.CurrentYearActivity.MinimumSegmentsRequired);
                pqp.PremierQualifierTrackerThresholdPrefix = "of";
                outrightPQP.PremierQualifierTrackerThresholdPrefix = "of";
                outrightPQP.PremierQualifierTrackerTitle = _configuration.GetValue<string>("PremierQualifierTrackerPointsTitle");
                upgradeActivity.KeyValueList = new List<MOBKVP>
                {
                    new MOBKVP()
                    {
                        Key = _configuration.GetValue<string>("MyAccountNextMilestone"),
                        Value = trackingUpgradeType == 1 ? _configuration.GetValue<string>("PlatinumNextMilestonePlusPoints") : trackingUpgradeType == 2 ? _configuration.GetValue<string>("1KNextMilestonePlusPoints") : trackingUpgradeType == 3 ? _configuration.GetValue<string>("IncrementalUpgradeNextMilestonePlusPoints") : string.Empty
                    }
                };
                MOBKVP premierActivityKey1 = new MOBKVP();
                if (trackingUpgradeType != 3)
                {
                    premierActivityKey1.Key = _configuration.GetValue<string>("flightsegmentminimum");
                    premierActivityKey1.Value = Get4FlightSegmentMinimum(sResponse.CurrentYearActivity);
                    upgradeActivity.KeyValueList.Add(premierActivityKey1);
                }
                if (sResponse.CurrentYearActivity.FlexPremierQualifyingPoints > 0)
                {
                    MOBKVP premierActivityChasePqp = new MOBKVP
                    {
                        Key = _configuration.GetValue<string>("ChasePQPText"),
                        Value = Convert.ToString(sResponse.CurrentYearActivity.FlexPremierQualifyingPoints)
                    };
                    upgradeActivity.KeyValueList.Add(premierActivityChasePqp);
                }
                upgradeActivity.KeyValueList.Add(
                    new MOBKVP()
                    {
                        Key = _configuration.GetValue<string>("viewpremierstatustracker"),
                        Value = _configuration.GetValue<string>("viewpremierstatustrackerlink")
                    });
                pqf.Separator = "+";
                pqp.Separator = "or";
            }
            upgradeActivity.pQP = pqp;
            upgradeActivity.pQF = trackingUpgradeType != 3 ? pqf : null;
            upgradeActivity.OutrightPQP = trackingUpgradeType != 3 ? outrightPQP : null;
            upgradeActivity.vBQPremierActivityStatus = _configuration.GetValue<string>("EarnPremierPlusPoints");
            return upgradeActivity;
        }

        private List<MOBKVP> GetVBQUpgradeKeyValues(int trackingUpgradeType, double minSegmentRequired)
        {
            List<MOBKVP> lKeyValue = new List<MOBKVP>();

            switch (trackingUpgradeType)
            {
                case 1: //Platinum
                    {
                        lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("MyAccountNextMilestone"), GetNextMilesStonePlusPointsValue(trackingUpgradeType)));
                        lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("flightsegmentminimum"), minSegmentRequired + " of 4"));

                        lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("viewpremierstatustracker"), _configuration.GetValue<string>("viewpremierstatustrackerlink")));
                        break;
                    }
                case 2: //1K
                    {
                        lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("MyAccountNextMilestone"), GetNextMilesStonePlusPointsValue(trackingUpgradeType)));
                        lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("flightsegmentminimum"), minSegmentRequired + " of 4"));
                        lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("viewpremierstatustracker"), _configuration.GetValue<string>("viewpremierstatustrackerlink")));
                        break;
                    }
                case 3://Incremental Upgrades
                    {
                        lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("MyAccountNextMilestone"), GetNextMilesStonePlusPointsValue(trackingUpgradeType)));
                        lKeyValue.Add(new MOBKVP(_configuration.GetValue<string>("viewpremierstatustracker"), _configuration.GetValue<string>("viewpremierstatustrackerlink")));
                        break;
                    }
                default:
                    break;
            }
            return lKeyValue;
        }

        private string GetNextMilesStonePlusPointsValue(int trackingUpgradeType)//EX: +2 RPU / +6 GPU or  +1 GPU or +2 RPU
        {
            switch (trackingUpgradeType)
            {
                case 1: //Platinum
                    {
                        return _configuration.GetValue<string>("PlatinumNextMilestonePlusPoints");
                    }
                case 2: //1K
                    {
                        return _configuration.GetValue<string>("1KNextMilestonePlusPoints");
                    }
                case 3://Incremental Upgrades
                    {
                        return _configuration.GetValue<string>("IncrementalUpgradeNextMilestoneVBQPlusPoints");
                    }
                default:
                    break;
            }
            return string.Empty;
        }

        private List<MOBKVP> GetVBQUpgradeTrackingLevels(int trackingUpgradeType, long PQF, long PQP)
        {
            string PremierLevelStatus = string.Empty;
            List<MOBKVP> list = new List<MOBKVP>();
            string premierActivityStatus = string.Empty;
            long thresholdPQP = 0;
            switch (trackingUpgradeType)
            {
                case 1://1=Platinum Upgrade; 
                    list.Add(new MOBKVP("TrackerThresholdPQF", _configuration.GetValue<string>("ThresholdPQFPlatinum")));
                    list.Add(new MOBKVP("TrackerThresholdPQP", _configuration.GetValue<string>("ThresholdPQPPlatinum")));
                    list.Add(new MOBKVP("TrackerThresholdOutrightPQP", _configuration.GetValue<string>("ThresholdOutrightPQPPlatinum")));
                    break;
                case 2://2=1k Upgrade; 
                    list.Add(new MOBKVP("TrackerThresholdPQF", _configuration.GetValue<string>("ThresholdPQF1K")));
                    list.Add(new MOBKVP("TrackerThresholdPQP", _configuration.GetValue<string>("ThresholdPQP1K")));
                    list.Add(new MOBKVP("TrackerThresholdOutrightPQP", _configuration.GetValue<string>("ThresholdOutrightPQP1K")));
                    break;
                case 3://3= Incremental Upgrade
                    GetVBQIncrementalThresholdPQFAndPQP(PQF, PQP, out thresholdPQP);
                    list.Add(new MOBKVP("TrackerThresholdPQP", thresholdPQP.ToString()));
                    break;
                default:
                    break;
            }
            return list;
        }

        private void GetVBQIncrementalThresholdPQFAndPQP(long PQF, long PQP, out long thresholdPQP)
        {
            bool isNewThresholdValues = _configuration.GetValue<bool>("EnableNewThresholdValues");
            long thresholdPQF1K = !string.IsNullOrEmpty(_configuration.GetValue<string>("ThresholdPQF1K")) ? Convert.ToInt64(_configuration.GetValue<string>("ThresholdPQF1K")) : 0;
            long thresholdPQP1K = !string.IsNullOrEmpty(_configuration.GetValue<string>("ThresholdPQP1K")) ? Convert.ToInt64(_configuration.GetValue<string>("ThresholdPQP1K")) : 0;
            long thresholdOutrightPQP1K = !string.IsNullOrEmpty(_configuration.GetValue<string>("ThresholdOutrightPQP1K")) ? Convert.ToInt64(_configuration.GetValue<string>("ThresholdOutrightPQP1K")) : 0;
            long thresholIncrementalPQP = !string.IsNullOrEmpty(_configuration.GetValue<string>("ThresholIncrementalPQP")) ? Convert.ToInt64(_configuration.GetValue<string>("ThresholIncrementalPQP")) : 0;

            long quotientPQP = 0;
            if (!isNewThresholdValues)
            {
                if (PQF >= 54)
                {
                    quotientPQP = (PQP - 18000) > 0 ? (PQP - 18000) / 3000 : 0;
                    thresholdPQP = (quotientPQP + 1) * 3000 + 18000;
                }
                else
                {
                    quotientPQP = (PQP - 24000) > 0 ? (PQP - 24000) / 3000 : 0;
                    thresholdPQP = (quotientPQP + 1) * 3000 + 24000;
                }
            }
            else
            {
                //LOYAL-5937 - Changes related to COVID-19 Loyalty MP Updates
                if (PQP < thresholIncrementalPQP)
                {
                    thresholdPQP = thresholIncrementalPQP;
                }
                else
                {
                    if (PQF >= thresholdPQF1K)
                    {
                        quotientPQP = (PQP - thresholdPQP1K) > 0 ? (PQP - thresholdPQP1K) / 3000 : 0;
                        thresholdPQP = (quotientPQP + 1) * 3000 + thresholdPQP1K;
                    }
                    else
                    {
                        quotientPQP = (PQP - thresholdOutrightPQP1K) > 0 ? (PQP - thresholdOutrightPQP1K) / 3000 : 0;
                        thresholdPQP = (quotientPQP + 1) * 3000 + thresholdOutrightPQP1K;
                    }
                }
            }
        }

        private int GetVBQTrackingUpgradeType(ServiceResponse sResponse)
        {
            int trackingUpgradeType = 0;//1=Platinum Upgrade; 2=1k Upgrade; 3= Incremental Upgrade More Comments to understand whats this "trackingUpgradeType" use for
            if (sResponse != null && sResponse.CurrentYearActivity != null)
            {
                trackingUpgradeType = IsVBQMPUpgradeEligible(sResponse.TrackingLevel, sResponse.CurrentPremierLevel, sResponse.InfiniteLevel, sResponse.LifetimeMiles,
                                        sResponse.CurrentYearActivity.PremierQualifyingFlightSegments, sResponse.CurrentYearActivity.FlexPremierQualifyingPoints)
                                        ?
                    GetVBQTrackingUpgradeType(sResponse.CurrentYearActivity.PremierQualifyingPoints, sResponse.CurrentYearActivity.FlexPremierQualifyingPoints, sResponse.CurrentYearActivity.PremierQualifyingFlightSegments,
                        sResponse.CurrentYearActivity.MinimumSegmentsRequired) : 0;
            }
            return trackingUpgradeType;
        }

        private int GetVBQTrackingUpgradeType(double elitePQP, double flexPQP, double pQF, double minSegmentRequired)
        {
            if (minSegmentRequired >= 4)
            {
                bool isNewThresholdValues = _configuration.GetValue<bool>("EnableNewThresholdValues");
                if (!isNewThresholdValues)
                {
                    if ((pQF >= 54 && elitePQP >= 18000) || (elitePQP >= 24000))  //Incremental 
                    {
                        return 3;
                    }
                    else if ((pQF >= 36 && (elitePQP + flexPQP) >= 12000) || ((elitePQP + flexPQP) >= 15000)) //1K
                    {
                        return 2;
                    }
                    // When user has < 4 Minimum Segments Flown or 1K threshouldNot Met -- Falls in Platinum Upgrade
                    else   // For Platinum upgrades consider Flex miles if ((pQF < 36 && pqp < 12000) && (pqp < 15000))
                    {
                        return 1; //Platinum upgrade
                    }
                }
                else
                {
                    //LOYAL - 5937 - Changes Related to this Story COVID-19 Loyalty MP updates
                    double thresholdPQF1K = !string.IsNullOrEmpty(_configuration.GetValue<string>("ThresholdPQF1K")) ? Convert.ToDouble(_configuration.GetValue<string>("ThresholdPQF1K")) : 0;
                    double thresholdPQP1K = !string.IsNullOrEmpty(_configuration.GetValue<string>("ThresholdPQP1K")) ? Convert.ToInt64(_configuration.GetValue<string>("ThresholdPQP1K")) : 0;
                    double thresholdOutrightPQP1K = !string.IsNullOrEmpty(_configuration.GetValue<string>("ThresholdOutrightPQP1K")) ? Convert.ToInt64(_configuration.GetValue<string>("ThresholdOutrightPQP1K")) : 0;

                    double thresholdPQFPlatinum = !string.IsNullOrEmpty(_configuration.GetValue<string>("ThresholdPQFPlatinum")) ? Convert.ToDouble(_configuration.GetValue<string>("ThresholdPQFPlatinum")) : 0;
                    double thresholdPQPPlatinum = !string.IsNullOrEmpty(_configuration.GetValue<string>("ThresholdPQPPlatinum")) ? Convert.ToDouble(_configuration.GetValue<string>("ThresholdPQPPlatinum")) : 0;
                    double thresholdOutrightPQPPlatinum = !string.IsNullOrEmpty(_configuration.GetValue<string>("ThresholdOutrightPQPPlatinum")) ? Convert.ToDouble(_configuration.GetValue<string>("ThresholdOutrightPQPPlatinum")) : 0;

                    if ((pQF >= thresholdPQF1K && elitePQP >= thresholdPQP1K) || (elitePQP >= thresholdOutrightPQP1K))  //Incremental       
                    {
                        return 3;
                    }
                    else if ((pQF >= thresholdPQFPlatinum && (elitePQP + flexPQP) >= thresholdPQPPlatinum) || ((elitePQP + flexPQP) >= thresholdOutrightPQPPlatinum)) //1K
                    {
                        return 2;
                    }
                    // When user has < 4 Minimum Segments Flown or 1K threshouldNot Met -- Falls in Platinum Upgrade
                    else   // For Platinum upgrades consider Flex miles if ((pQF < 36 && pqp < 12000) && (pqp < 15000))
                    {
                        return 1; //Platinum upgrade
                    }
                }
            }
            else if (minSegmentRequired < 4)
            {
                return 1;
            }

            return 0;
        }

        private bool IsVBQMPUpgradeEligible(int trackingMPLevel, int currentPremierLevel, int infiniteLevel, long lifeTimeMiles, double PQF, long PQP)
        {
            bool isNewThresholdValues = _configuration.GetValue<bool>("EnableNewThresholdValues");
            if (!isNewThresholdValues)
            {
                if (trackingMPLevel == 6 && (currentPremierLevel == 4 || currentPremierLevel == 5) &&
                    (infiniteLevel == 4 || infiniteLevel == 5 || lifeTimeMiles >= 3000000 || ((PQF >= 54 && PQP >= 18000) || PQP >= 24000)))
                {
                    return true;
                }
            }
            else
            {
                //LOYAL-5937 - Changes related to this Story COVID-19 Loyalty MP Updates
                double thresholdPQF1K = !string.IsNullOrEmpty(_configuration.GetValue<string>("ThresholdPQF1K")) ? Convert.ToDouble(_configuration.GetValue<string>("ThresholdPQF1K")) : 0;
                long thresholdPQP1K = !string.IsNullOrEmpty(_configuration.GetValue<string>("ThresholdPQP1K")) ? Convert.ToInt64(_configuration.GetValue<string>("ThresholdPQP1K")) : 0;
                long thresholdOutrightPQP1K = !string.IsNullOrEmpty(_configuration.GetValue<string>("ThresholdOutrightPQP1K")) ? Convert.ToInt64(_configuration.GetValue<string>("ThresholdOutrightPQP1K")) : 0;
                if (trackingMPLevel == 6 && (currentPremierLevel == 4 || currentPremierLevel == 5) &&
                    (infiniteLevel == 4 || infiniteLevel == 5 || lifeTimeMiles >= 3000000 || ((PQF >= thresholdPQF1K && PQP >= thresholdPQP1K) || PQP >= thresholdOutrightPQP1K)))
                {
                    return true;
                }
                else if (_configuration.GetValue<bool>("EnableMyAccountTrackingLevel5Changes") && trackingMPLevel == 5 && (currentPremierLevel == 4 || currentPremierLevel == 5)) //JIRA LOYAL-5994
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsVBQTrackerSupportedVersion(int appId, string appVersion)
        {
            bool isPremierStatusTrackerSupportedVersion = false;
            if (!string.IsNullOrEmpty(appVersion) && appId != -1)
            {
                isPremierStatusTrackerSupportedVersion = GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidVBQTracker", "iOSVBQTracker", "", "", true, _configuration);
            }
            return isPremierStatusTrackerSupportedVersion;
        }

        private MOBYearEndPremierActivity GetYearEndActivity(ServiceResponse response)
        {
            string sYear = DateTime.Now.ToString("yyyy");
            if (response.CurrentYearActivity.ActivityYear == DateTime.Now.Year)
            {
                sYear = DateTime.Now.AddYears(1).ToString("yyyy");
            }

            MOBYearEndPremierActivity yearEndPremierActivity = new MOBYearEndPremierActivity();
            if (response != null && response.CurrentYearActivity != null)
            {
                yearEndPremierActivity.YearEndPremierActivityTitle = _configuration.GetValue<string>("PremierActivityTitle");
                yearEndPremierActivity.YearEndPremierActivityYear = response.CurrentYearActivity.ActivityYear + " " + _configuration.GetValue<string>("YearendSnapshot");
                yearEndPremierActivity.YearEndPremierActivityStatus = string.Format(_configuration.GetValue<string>("YearendStatus"), sYear);
                yearEndPremierActivity.YearEndPQMTitle = _configuration.GetValue<string>("PremierQualifierTrackerMilesTitle");
                yearEndPremierActivity.YearEndPQMText = string.Format("{0:###,##0}", response.CurrentYearActivity.EliteQualifyingMiles);
                yearEndPremierActivity.YearEndPQSTitle = _configuration.GetValue<string>("PremierQualifierTrackerSegmentsTitle");
                yearEndPremierActivity.YearEndPQSText = string.Format("{0:0.#}", response.CurrentYearActivity.EliteQualifyingPoints);
                yearEndPremierActivity.YearEndPQDTitle = _configuration.GetValue<string>("PremierQualifierTrackerDollarsTitle");
                yearEndPremierActivity.YearEndPQDText = response.CurrentYearActivity.TotalRevenue > 0 ? response.CurrentYearActivity.TotalRevenue.ToString("C0") : "$0";
                yearEndPremierActivity.YearEnd4FlightSegmentMinimumText = _configuration.GetValue<string>("YearEnd4flightsegmentminimum");
                yearEndPremierActivity.YearEnd4FlightSegmentMinimumValue = GetYearEnd4FlightSegmentMinimum(response.CurrentYearActivity);
            }

            return yearEndPremierActivity;
        }

        private string GetYearEnd4FlightSegmentMinimum(Activity currentYearActivity)
        {
            string value = string.Empty;
            if (currentYearActivity != null)
            {
                value = currentYearActivity.MinimumSegmentsRequired >= 4 ? "4" : currentYearActivity.MinimumSegmentsRequired.ToString();
            }
            return value;
        }

        private async Task<List<MOBItem>> GetPremierActivityLearnAboutCaptions()
        {
            var captions = await GetCaptions("MYACCOUNT_PREMIERTRACKER_LEARNABOUTLINK").ConfigureAwait(false);
            return captions;
        }

        private async Task<List<MOBItem>> GetCaptions(string key)
        {
            return !string.IsNullOrEmpty(key) ? await GetCaptions(new List<string> { key }, true).ConfigureAwait(false) : null;
        }

        private async Task<List<MOBItem>> GetCaptions(List<string> keyList, bool isTnC)
        {
            var captions = new List<MOBItem>();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var title in keyList)
            {
                stringBuilder.Append("'");
                stringBuilder.Append(title);
                stringBuilder.Append("'");
                stringBuilder.Append(",");
            }
            string reqTitles = stringBuilder.ToString().Trim(',');
            try
            {
                string transactionId = !string.IsNullOrEmpty(_headers.ContextValues.TransactionId) ? _headers.ContextValues.TransactionId : "Leagaldocument01";
                var docs = await _dynamoDBUtility.GetNewLegalDocumentsForTitles(reqTitles, transactionId, true).ConfigureAwait(false);
                captions.AddRange(
                                 docs.Select(doc => new MOBItem
                                 {
                                     Id = doc.Title,
                                     CurrentValue = doc.LegalDocument
                                 }));
            }
            catch (Exception ex)
            {
                _logger.LogError("GetCaptions - GetLegalDocumentsForTitles Error {message} {exceptionStackTrace}", ex.Message, ex.StackTrace);
            }

            return captions;
        }

        public async Task<List<MOBCancelledFFCPNRDetails>> GetMPFutureFlightCreditFromCancelReservationService
           (string mileagePlusNumber, int applicationId, string version, string sessionId, string transactionId, string deviceId, string callsource)
        {
            var cancelledFFCPNRDetails = new List<MOBCancelledFFCPNRDetails>();
            _logger.LogInformation("GetMPFutureFlightCreditFromCancelReservationService-GetAnonymousToken {applicationId} {deviceId}", applicationId, deviceId);

            try
            {
                var sResponse = await _mPFutureFlightCredit.GetMPFutureFlightCredit<getResrvationResponse>(token: string.Empty, callsource, mileagePlusNumber, sessionId).ConfigureAwait(false);

                if (sResponse != null)
                {
                    if (string.Equals(sResponse.Status, "Success", StringComparison.OrdinalIgnoreCase))
                    {
                        if (sResponse.Reservation == null || !sResponse.Reservation.Any(x => string.IsNullOrEmpty(x.RecLoc) == false))
                        {
                            return null;
                        }

                        sResponse.Reservation?.ToList().ForEach(res =>
                        {
                            if (res.hasFFC)
                            {
                                var cancelledFFCPNRDetail = new MOBCancelledFFCPNRDetails
                                { RecordLocator = res.RecLoc, Passengers = new List<MOBName>() };

                                if (res.Traveller != null && res.Traveller.Any())
                                {
                                    cancelledFFCPNRDetail.PNRLastName = res.Traveller?.FirstOrDefault()?.LastName;

                                    res.Traveller.ToList().ForEach(pax =>
                                    {
                                        cancelledFFCPNRDetail.Passengers.Add(new MOBName
                                        {
                                            First = pax.FirstName,
                                            Last = pax.LastName
                                        });
                                    });
                                }
                                cancelledFFCPNRDetails.Add(cancelledFFCPNRDetail);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("GetMPFutureFlightCreditFromCancelReservationService {Exception} {StackTrace}",
                    string.Format(GeneralHelper.RemoveCarriageReturn(ex.Message) + " for mileage plus number - {0}", GeneralHelper.RemoveCarriageReturn(mileagePlusNumber)), GeneralHelper.RemoveCarriageReturn(ex.StackTrace));
                return null;
            }

            return cancelledFFCPNRDetails.Any() ? cancelledFFCPNRDetails : null;
        }

        private async Task<MOBMPAccountSummary> GetAccountSummaryV2(string transactionId, string mileagePlusNumber, string languageCode, bool includeMembershipCardBarCode, string sessionId = "")
        {
            MOBMPAccountSummary mpSummary = new MOBMPAccountSummary();
            transactionId = !String.IsNullOrEmpty(sessionId) ? sessionId : transactionId;

            if (_configuration.GetValue<string>("AddingExtraLogggingForAwardShopAccountSummary_Toggle") != null
                && _configuration.GetValue<bool>("AddingExtraLogggingForAwardShopAccountSummary_Toggle"))
            {
                if (string.IsNullOrWhiteSpace(mileagePlusNumber))
                {
                    _logger.LogError("GetAccountSummary - Empty MPNumber Passed");
                }
            }

            try
            {
                bool fourSegmentMinimunWaivedMember = false;
                bool noMileageExpiration = false;
                string balanceExpireDisclaimer = string.Empty;
                Session session = new Session();
                session = await _sessionHelperService.GetSession<Session>(sessionId, session.ObjectName, new List<string> { sessionId, session.ObjectName }).ConfigureAwait(false);

                Mobile.Model.CSLModels.ReadMemberInformation loyaltyProfileResponse = new ReadMemberInformation();               

                var jsonResponse = await _loyaltyAccountService.GetAccountProfile(session.Token, mileagePlusNumber, _headers.ContextValues.SessionId).ConfigureAwait(false);

                if (jsonResponse != null)
                {
                    var responseData = JsonConvert.DeserializeObject<Mobile.Model.CSLModels.CslResponse<ReadMemberInformation>>(jsonResponse);
                    if (responseData != null && responseData.Errors == null)
                    {
                        loyaltyProfileResponse = responseData.Data;

                        #region 55359, 81220 Bug Fix
                        //55359 and 81220 to check for closed, temporary closed and ClosedPermanently account-Alekhya 
                        if (loyaltyProfileResponse != null && (loyaltyProfileResponse.IsClosedTemporarily == true || loyaltyProfileResponse.IsClosedPermanently == true))
                        {
                            string exceptionMessage = _configuration.GetValue<string>("ErrorContactMileagePlus").ToString();
                            throw new MOBUnitedException(exceptionMessage);
                        }
                        //Changes end here
                        #endregion 55359, 81220 Bug Fix
                        if (loyaltyProfileResponse.BirthYear > 0 && loyaltyProfileResponse.BirthMonth > 0 && loyaltyProfileResponse.BirthDate > 0)
                        {
                            mpSummary.BirthDate = (new DateTime(loyaltyProfileResponse.BirthYear, loyaltyProfileResponse.BirthMonth, loyaltyProfileResponse.BirthDate)).ToString();
                        }

                        var balance = loyaltyProfileResponse.Balances?.FirstOrDefault(balnc => (int)balnc.Currency == 5);
                        mpSummary.Balance = balance != null ? balance.Amount.ToString() : "0";
                        var adjustHoursToLastActivityDate = string.IsNullOrEmpty(_configuration.GetValue<string>("AdjustHoursToLastActivityDate")) ? -12 : _configuration.GetValue<int>("AdjustHoursToLastActivityDate");
                        if (loyaltyProfileResponse.LastActivityDate.Year <= 1) adjustHoursToLastActivityDate = 0;
                        mpSummary.LastActivityDate = loyaltyProfileResponse.LastActivityDate != null ? loyaltyProfileResponse.LastActivityDate.AddHours(adjustHoursToLastActivityDate).ToString("MM/dd/yyyy") : "";
                        mpSummary.LearnMoreTitle = _configuration.GetValue<string>("MilagePluslearnMoreText");
                        mpSummary.LearnMoreHeader = _configuration.GetValue<string>("MilagePluslearnMoreDesc");
                        mpSummary.MilesNeverExpireText = _configuration.GetValue<string>("MilagePlusMilesNeverExpire");
                        if (_configuration.GetValue<bool>("HideMileageBalanceExpireDate") == true)
                        {
                            mpSummary.BalanceExpireDate = "";
                            mpSummary.IsHideMileageBalanceExpireDate = true;
                        }

                        mpSummary.BalanceExpireDisclaimer = _configuration.GetValue<bool>("HideMileageBalanceExpireDate") ? _configuration.GetValue<string>("BalanceExpireDisclaimerNeverExpire") : _configuration.GetValue<string>("BalanceExpireDisclaimer");
                        mpSummary.CustomerId = loyaltyProfileResponse.CustomerId;
                        var premierQualifyingSegmentBalance = loyaltyProfileResponse.PremierQualifyingMetrics?.FirstOrDefault(pqm => pqm.ProgramCurrency == "PQP");
                        if (premierQualifyingSegmentBalance != null)
                        {
                            mpSummary.EliteMileage = string.Format("{0:###,##0}", premierQualifyingSegmentBalance.Balance);
                        }

                        if (premierQualifyingSegmentBalance != null)
                        {
                            if (premierQualifyingSegmentBalance.Balance == 0)
                            {
                                mpSummary.EliteSegment = "0";
                            }
                            else
                            {
                                mpSummary.EliteSegment = string.Format("{0:0.#}", premierQualifyingSegmentBalance);
                            }
                        }
                        // test comments
                        mpSummary.EliteStatus = new MOBEliteStatus(_configuration)
                        {
                            Code = loyaltyProfileResponse.MPTierLevel.ToString()
                        };
                        if (mpSummary.EnrollDate != null)
                        {
                            mpSummary.EnrollDate = string.IsNullOrEmpty(_configuration.GetValue<string>("AdjustHoursToEnrollDate")) ? loyaltyProfileResponse.EnrollDate.AddHours(-12).ToString("MM/dd/yyyy") : loyaltyProfileResponse.EnrollDate.AddHours(_configuration.GetValue<int>("AdjustHoursToEnrollDate")).ToString("MM/dd/yyyy");
                            //mpSummary.EnrollDate = loyaltyProfileResponse.EnrollDate.ToString("MM/dd/yyyy");
                        }
                        //mpSummary.HasUAClubMemberShip = loyaltyProfileResponse.AccountProfileInfo.IsUnitedClubMember;
                        //mpSummary.LastExpiredMileDate = DateTime.ParseExact(loyaltyProfileResponse.AccountProfileInfo.MilesExpireLastActivityDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy");
                        mpSummary.LastFlightDate = loyaltyProfileResponse.LastFlightDate != null ? loyaltyProfileResponse.LastFlightDate.ToString("MM/dd/yyyy") : "";
                        mpSummary.MileagePlusNumber = loyaltyProfileResponse.MileageplusId;
                        mpSummary.Name = new MOBName
                        {
                            First = loyaltyProfileResponse.FirstName,
                            Last = loyaltyProfileResponse.LastName,
                            Middle = loyaltyProfileResponse.MiddleName,
                            Suffix = loyaltyProfileResponse.Suffix,
                            Title = loyaltyProfileResponse.Title
                        };

                        mpSummary.IsCEO = loyaltyProfileResponse.CEO == true;

                        mpSummary.LifetimeMiles = Convert.ToInt32(loyaltyProfileResponse.LifetimeMiles);

                        if (loyaltyProfileResponse.MillionMilerLevel == 0)
                        {
                            mpSummary.MillionMilerIndicator = string.Empty;
                        }
                        else
                        {
                            mpSummary.MillionMilerIndicator = loyaltyProfileResponse.MillionMilerLevel.ToString();
                        }

                        if (_configuration.GetValue<DateTime>("MP2014EnableDate") < DateTime.Now)
                        {
                            if (loyaltyProfileResponse != null)
                            {
                                bool isValidPqdAddress = false;
                                bool activeNonPresidentialPlusCardMember = false;
                                bool activePresidentialPlusCardMembe = false;
                                bool chaseSpending = false;
                                bool presidentialPlus = false;
                                bool showChaseBonusTile = false;
                                if (_configuration.GetValue<bool>("EnableCallingPQDService"))
                                {
                                    //Migrate XML to CSL service call
                                    if (_configuration.GetValue<bool>("NewServieCall_GetProfile_PaymentInfos"))
                                    {
                                        var tupleRes = await IsValidPQDAddressV2("GetAccountSummary", transactionId, session.Token, mpSummary.MileagePlusNumber, isValidPqdAddress, activeNonPresidentialPlusCardMember, activePresidentialPlusCardMembe, fourSegmentMinimunWaivedMember, showChaseBonusTile).ConfigureAwait(false);
                                        isValidPqdAddress = tupleRes.isValidPqdAddress;
                                        activeNonPresidentialPlusCardMember = tupleRes.activeNonPresidentialPlusCardMember;
                                        activePresidentialPlusCardMembe = tupleRes.activePresidentialPlusCardMembe;
                                        fourSegmentMinimunWaivedMember = tupleRes.fourSegmentMinimunWaivedMember;
                                        showChaseBonusTile = tupleRes.chaseBonusTile;
                                    }
                                }

                                mpSummary.ShowChaseBonusTile = showChaseBonusTile;
                                AssignCustomerChasePromoType(mpSummary, showChaseBonusTile);

                                noMileageExpiration = activeNonPresidentialPlusCardMember || activePresidentialPlusCardMembe;

                                if (fourSegmentMinimunWaivedMember)
                                {
                                    mpSummary.FourSegmentMinimun = "Waived";
                                }
                                else if (loyaltyProfileResponse.MinimumSegments >= 4)
                                {
                                    mpSummary.FourSegmentMinimun = "4 of 4";
                                }
                                else
                                {
                                    mpSummary.FourSegmentMinimun = string.Format("{0} of 4", loyaltyProfileResponse.MinimumSegments);
                                }
                                //TODO : UCB
                                //if (loyaltyProfileResponse.AccountProfileInfo.PremierQualifyingDollars == 0 || !isValidPqdAddress)
                                //{
                                //    if (!isValidPqdAddress)
                                //    {
                                //        mpSummary.PremierQualifyingDollars = string.Empty;
                                //    }
                                //    else
                                //    {
                                //        mpSummary.PremierQualifyingDollars = "0";
                                //    }
                                //}
                                //else
                                //{
                                //    decimal pqd = 0;
                                //    try
                                //    {
                                //        pqd = Convert.ToDecimal(loyaltyProfileResponse.AccountProfileInfo.PremierQualifyingDollars);
                                //    }
                                //    catch (System.Exception) { }
                                //    //Below are the two toggles used in Appsettings 
                                //    //< add key = "PqdAmount" value = "12000" /> < add key = "PqdText" value = "Over $12,000" />
                                //    //Work Items LOYAL-3236, LOYAL-3241
                                //    if (!string.IsNullOrEmpty(_configuration.GetValue<string>["PqdAmount")) && !string.IsNullOrEmpty(_configuration.GetValue<string>["PqdText")))
                                //    {
                                //        if (pqd > Convert.ToDecimal(_configuration.GetValue<string>"PqdAmount")))
                                //        {
                                //            mpSummary.PremierQualifyingDollars = ConfigurationManager.AppSettings["PqdText");
                                //        }
                                //    }
                                //    else
                                //    {
                                //        mpSummary.PremierQualifyingDollars = pqd.ToString("C0");
                                //    }
                                //}


                                string pdqchasewaiverLabel = string.Empty;
                                string pdqchasewavier = string.Empty;
                                if (isValidPqdAddress)
                                {
                                    if (loyaltyProfileResponse.IsChaseSpend)
                                    {
                                        chaseSpending = true;
                                    }
                                    if (!string.IsNullOrEmpty(balanceExpireDisclaimer) && !_configuration.GetValue<bool>("HideMileageBalanceExpireDate"))
                                    {
                                        mpSummary.BalanceExpireDisclaimer = mpSummary.BalanceExpireDisclaimer + " " + balanceExpireDisclaimer;
                                    }
                                }
                                if (!fourSegmentMinimunWaivedMember && !_configuration.GetValue<bool>("HideMileageBalanceExpireDate"))
                                {
                                    mpSummary.BalanceExpireDisclaimer = mpSummary.BalanceExpireDisclaimer + " " + _configuration.GetValue<string>("FouSegmentMessage");
                                }
                            }
                        }
                        // MOBILE-24412
                        if (_configuration.GetValue<bool>("EnableShopChaseCardPaxInfoFix"))
                        {
                            var chaseCardTypes = _configuration.GetValue<string>("ChaseCardTypesForStrikeThrough") ?? "";
                            if ((loyaltyProfileResponse?.PartnerCards?.Count ?? 0) > 0)
                            {
                                mpSummary.IsChaseCardHolder = loyaltyProfileResponse.PartnerCards.Exists(p => p.PartnerCode == "CH" && chaseCardTypes.Contains(p.CardType));
                            }
                        }

                    }
                }

                if (includeMembershipCardBarCode)
                {
                    //%B[\w]{2,3}\d{5,6}\s{7}\d{4}(GL|1K|GS|PL|SL|\s\s)\s\s[\s\w\<\-\'\.]{35}\sUA
                    string eliteLevel = "";
                    switch (mpSummary.EliteStatus.Level)
                    {
                        case 0:
                            eliteLevel = "  ";
                            break;
                        case 1:
                            eliteLevel = "SL";
                            break;
                        case 2:
                            eliteLevel = "GL";
                            break;
                        case 3:
                            eliteLevel = "PL";
                            break;
                        case 4:
                            eliteLevel = "1K";
                            break;
                        case 5:
                            eliteLevel = "GS";
                            break;
                    }
                    string name = string.Format("{0}<{1}", mpSummary.Name.Last, mpSummary.Name.First);
                    if (!string.IsNullOrEmpty(mpSummary.Name.Middle))
                    {
                        name = name + " " + mpSummary.Name.Middle.Substring(0, 1);
                    }
                    name = String.Format("{0, -36}", name);

                    bool hasUnitedClubMemberShip = false;
                    var tupleResponse = await GetUnitedClubMembershipDetails(mpSummary.MileagePlusNumber, session.SessionId, session.Token).ConfigureAwait(false);
                    mpSummary.uAClubMemberShipDetails = tupleResponse.clubMemberShipDetails;
                    hasUnitedClubMemberShip = tupleResponse.hasUnitedClubMemberShip;
                    mpSummary.HasUAClubMemberShip = hasUnitedClubMemberShip;
                    //if (hasUnitedClubMemberShip)
                    //{
                    mpSummary.MembershipCardExpirationDate = await this.GetMembershipCardExpirationDate(mpSummary.MileagePlusNumber, mpSummary.EliteStatus.Level).ConfigureAwait(false);
                    string expirationDate = _configuration.GetValue<string>("MPCardExpirationDate");
                    try
                    {
                        if (mpSummary.MembershipCardExpirationDate.Equals("Trial Status"))
                        {
                            expirationDate = string.Format("12/{0}", DateTime.Today.Year);
                        }
                        else
                        {
                            expirationDate = string.IsNullOrEmpty(mpSummary.MembershipCardExpirationDate) ? _configuration.GetValue<string>("MPCardExpirationDate") : mpSummary.MembershipCardExpirationDate.Substring(11, 2) + mpSummary.MembershipCardExpirationDate.Substring(16, 2);
                        }
                    }
                    catch (System.Exception) { }

                    //string expirationDate = ConfigurationManager.AppSettings["MPCardExpirationDate");
                    string barCodeData = string.Format("%B{0}       {1}{2}  {3}UA              ?", mpSummary.MileagePlusNumber, expirationDate, eliteLevel, name);
                    string allianceTierLevel = "   ";
                    switch (mpSummary.EliteStatus.Level)
                    {
                        case 0:
                            allianceTierLevel = "   ";
                            break;
                        case 1:
                            allianceTierLevel = "UAS";
                            break;
                        case 2:
                            allianceTierLevel = "UAG";
                            break;
                        case 3:
                            allianceTierLevel = "UAG";
                            break;
                        case 4:
                            allianceTierLevel = "UAG";
                            break;
                        case 5:
                            allianceTierLevel = "UAG";
                            break;
                    }
                    string allianceTierLevelExpirationDate = "    ";
                    if (!allianceTierLevel.Equals("   "))
                    {
                        if (mpSummary.MembershipCardExpirationDate.Equals("Trial Status"))
                        {
                            allianceTierLevelExpirationDate = string.Format("12/{0}", DateTime.Today.Year);
                        }
                        else
                        {
                            allianceTierLevelExpirationDate = string.IsNullOrEmpty(mpSummary.MembershipCardExpirationDate) ? _configuration.GetValue<string>("MPCardExpirationDate") : mpSummary.MembershipCardExpirationDate.Substring(11, 2) + mpSummary.MembershipCardExpirationDate.Substring(16, 2);
                        }
                    }

                    string paidLoungeIndicator = "N";
                    string paidLoungeExpireationDate = "      ";
                    if (mpSummary.uAClubMemberShipDetails != null)
                    {
                        if (string.IsNullOrEmpty(mpSummary.uAClubMemberShipDetails.PrimaryOrCompanion))
                        {
                            paidLoungeIndicator = "P";
                        }
                        else
                        {
                            paidLoungeIndicator = mpSummary.uAClubMemberShipDetails.PrimaryOrCompanion;
                        }
                        paidLoungeExpireationDate = Convert.ToDateTime(mpSummary.uAClubMemberShipDetails.DiscontinueDate).ToString("ddMMyyyy");
                    }

                    string starBarCodeData = string.Format("FFPC001UA{0}{1}{2}{3}{4}{5}{6}{7}^001", mpSummary.MileagePlusNumber.PadRight(16), mpSummary.Name.Last.PadRight(20), mpSummary.Name.First.PadRight(20), allianceTierLevel, allianceTierLevelExpirationDate, eliteLevel, paidLoungeIndicator, paidLoungeExpireationDate);

                    if (!string.IsNullOrEmpty(_configuration.GetValue<string>("ReturnMPMembershipBarcode")) && _configuration.GetValue<bool>("ReturnMPMembershipBarcode"))
                    {
                        if (!string.IsNullOrEmpty(_configuration.GetValue<string>("UseStarMembershipCardFormatDateTime")) && _configuration.GetValue<DateTime>("UseStarMembershipCardFormatDateTime") <= DateTime.Now)
                        {
                            mpSummary.MembershipCardBarCodeString = starBarCodeData.ToUpper();
                            mpSummary.MembershipCardBarCode = GetBarCode(starBarCodeData);
                        }
                        else
                        {
                            mpSummary.MembershipCardBarCodeString = barCodeData.ToUpper();
                            mpSummary.MembershipCardBarCode = GetBarCode(barCodeData);
                        }
                    }
                    else
                    {
                        mpSummary.MembershipCardBarCodeString = null;
                        mpSummary.MembershipCardBarCode = null;
                    }
                    //}
                }

                //bool noMileageExpiration = HasChaseNoMileageExpirationCard(mpSummary.MileagePlusNumber);
                mpSummary.NoMileageExpiration = noMileageExpiration.ToString();

                if (_configuration.GetValue<bool>("HideMileageBalanceExpireDate"))

                {
                    mpSummary.NoMileageExpirationMessage = _configuration.GetValue<string>("BalanceExpireDisclaimerNeverExpire");
                }
                else if (noMileageExpiration)
                {
                    mpSummary.NoMileageExpirationMessage = _configuration.GetValue<string>("ChaseNoMileageExpirationMessage") + " " + balanceExpireDisclaimer;
                    if (!fourSegmentMinimunWaivedMember)
                    {
                        mpSummary.NoMileageExpirationMessage = mpSummary.NoMileageExpirationMessage + " " + _configuration.GetValue<string>("FouSegmentMessage");
                    }
                }
            }
            catch (MOBUnitedException)
            {
                throw;
            }
            catch (System.Exception)
            {
                throw;
            }

            _logger.LogInformation("Loyalty Get Profile Response to client {@MpSummary}", JsonConvert.SerializeObject(mpSummary));


            if (_configuration.GetValue<string>("StartDateTimeToReturnEmptyMPExpirationDate") != null && _configuration.GetValue<String>("EndtDateTimeToReturnEmptyMPExpirationDate") != null && DateTime.ParseExact(_configuration.GetValue<String>("StartDateTimeToReturnEmptyMPExpirationDate").ToString().ToUpper().Trim(), "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture) <= DateTime.Now && DateTime.ParseExact(_configuration.GetValue<String>("EndtDateTimeToReturnEmptyMPExpirationDate").ToString().ToUpper().Trim(), "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture) >= DateTime.Now)
            {
                mpSummary.MembershipCardExpirationDate = string.Empty;
            }
            return mpSummary;
        }

        private async Task<MOBClubMembership> GetCurrentMembershipInfo(string mpNumber)
        {
            MOBClubMembership mobClubMembership = null;
            string jsonResponse = await _loyaltyAccountService.GetCurrentMembershipInfo(mpNumber, string.Empty).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                List<UClubMembershipInfo> uClubMembershipInfoList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<UClubMembershipInfo>>(jsonResponse);
                if (uClubMembershipInfoList != null && uClubMembershipInfoList.Count > 0)
                {
                    foreach (UClubMembershipInfo uClubMembershipInfo in uClubMembershipInfoList)
                    {
                        if (uClubMembershipInfo.DiscontinueDate >= DateTime.Now && string.IsNullOrEmpty(uClubMembershipInfo.ClubStatusCode))
                        {
                            mobClubMembership = new MOBClubMembership
                            {
                                CompanionMPNumber = string.IsNullOrEmpty(uClubMembershipInfo.CompanionMpNumber) ? string.Empty : uClubMembershipInfo.CompanionMpNumber,
                                EffectiveDate = uClubMembershipInfo.EffectiveDate.ToString("MM/dd/yyyy"),
                                ExpirationDate = uClubMembershipInfo.DiscontinueDate.ToString("MM/dd/yyyy"),
                                IsPrimary = string.IsNullOrEmpty(uClubMembershipInfo.PrimaryOrCompanion) ? true : false,
                                MembershipTypeCode = string.IsNullOrEmpty(uClubMembershipInfo.MemberTypeCode) ? string.Empty : uClubMembershipInfo.MemberTypeCode,
                                MembershipTypeDescription = string.IsNullOrEmpty(uClubMembershipInfo.MemberTypeDescription) ? string.Empty : uClubMembershipInfo.MemberTypeDescription
                            };
                        }
                    }
                }
            }
            return mobClubMembership;
        }

        private async Task<(MOBClubMembership clubMemberShip, Service.Presentation.ProductResponseModel.Subscription merchOut)> GetCurrentMembershipInfoV2(string mpAccountNumber, string sessionID, string token)
        {
            string channelId = string.Empty;
            string channelName = string.Empty;
            if (_configuration.GetValue<bool>("EnabledMERCHChannels"))
            {
                _merchandizingServices.SetMerchandizeChannelValues("MOBBE", ref channelId, ref channelName);
            }
            else
            {
                channelId = _configuration.GetValue<string>("MerchandizeOffersServiceChannelID").Trim();// "401";  //Changed per Praveen Vemulapalli email
                channelName = _configuration.GetValue<string>("MerchandizeOffersServiceChannelName").Trim();//"MBE";  //Changed per Praveen Vemulapalli emai
            }

            return await _merchandizingServices.GetUAClubSubscriptions(mpAccountNumber, sessionID, channelId, channelName, token).ConfigureAwait(false);
        }

        //TO-DO: Rajesh K need to visit
        public async Task<(MOBMPAccountSummary mpSummary, Service.Presentation.ProductResponseModel.Subscription merchOut)> GetAccountSummaryWithPremierActivityV2(MOBMPAccountValidationRequest req, bool includeMembershipCardBarCode, string dpToken)
        {
            MOBMPAccountSummary mpSummary = new MOBMPAccountSummary();
            Service.Presentation.ProductResponseModel.Subscription merchOut = null;
            try
            {
                bool fourSegmentMinimunWaivedMember = false;
                ReadMemberInformation loyaltyProfileResponse = new ReadMemberInformation();
                ServiceResponse sResponse = new ServiceResponse();
                string balanceExpireDisclaimer = string.Empty;
                bool noMileageExpiration = false;
                var jsonResponse = await _loyaltyAccountService.GetAccountProfile(dpToken, req.MileagePlusNumber, _headers.ContextValues.SessionId).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    var responseData = Newtonsoft.Json.JsonConvert.DeserializeObject<Mobile.Model.CSLModels.CslResponse<ReadMemberInformation>>(jsonResponse);
                    if (responseData.Data != null && responseData.Errors == null)
                    {
                        loyaltyProfileResponse = responseData.Data;
                        #region 55359, 81220 Bug Fix
                        if (loyaltyProfileResponse != null && (loyaltyProfileResponse.IsClosedTemporarily == true || loyaltyProfileResponse.IsClosedPermanently == true))
                        {
                            string exceptionMessage = _configuration.GetValue<string>("ErrorContactMileagePlus").ToString();
                            throw new MOBUnitedException(exceptionMessage);
                        }
                        #endregion 55359, 81220 Bug Fix
                        var balance = loyaltyProfileResponse.Balances?.FirstOrDefault(balnc => (int)balnc.Currency == 5);
                        mpSummary.Balance = balance != null ? balance.Amount.ToString() : "0";
                        var adjustHoursToLastActivityDate = string.IsNullOrEmpty(_configuration.GetValue<string>("AdjustHoursToLastActivityDate")) ? -12 : _configuration.GetValue<int>("AdjustHoursToLastActivityDate");
                        if (loyaltyProfileResponse.LastActivityDate.Year <= 1) adjustHoursToLastActivityDate = 0;
                        mpSummary.LastActivityDate = loyaltyProfileResponse.LastActivityDate != null ? loyaltyProfileResponse.LastActivityDate.AddHours(adjustHoursToLastActivityDate).ToString("MM/dd/yyyy") : "";
                        mpSummary.LearnMoreTitle = _configuration.GetValue<string>("MileagePluslearnMoreText");
                        mpSummary.LearnMoreHeader = _configuration.GetValue<string>("MileagePluslearnMoreDesc");
                        mpSummary.MilesNeverExpireText = _configuration.GetValue<string>("MileagePlusMilesNeverExpire");
                        if (_configuration.GetValue<bool>("HideMileageBalanceExpireDate") == true)
                        {
                            mpSummary.BalanceExpireDate = "";
                            mpSummary.IsHideMileageBalanceExpireDate = true;
                        }

                        mpSummary.BalanceExpireDisclaimer = _configuration.GetValue<bool>("HideMileageBalanceExpireDate") ? _configuration.GetValue<string>("BalanceExpireDisclaimerNeverExpire") : _configuration.GetValue<string>("BalanceExpireDisclaimer");
                        mpSummary.CustomerId = loyaltyProfileResponse.CustomerId;
                        var premierQualifyingSegmentBalance = loyaltyProfileResponse.PremierQualifyingMetrics?.FirstOrDefault(pqm => pqm.ProgramCurrency == "PQP");
                        if (premierQualifyingSegmentBalance != null)
                        {
                            mpSummary.EliteMileage = string.Format("{0:###,##0}", premierQualifyingSegmentBalance.Balance);
                        }

                        if (premierQualifyingSegmentBalance != null)
                        {
                            if (premierQualifyingSegmentBalance.Balance == 0)
                            {
                                mpSummary.EliteSegment = "0";
                            }
                            else
                            {
                                mpSummary.EliteSegment = string.Format("{0:0.#}", premierQualifyingSegmentBalance);
                            }
                        }

                        mpSummary.EliteStatus = new MOBEliteStatus(_configuration)
                        {
                            Code = loyaltyProfileResponse.MPTierLevel.ToString()
                        };
                        if (mpSummary.EnrollDate != null)
                        {
                            mpSummary.EnrollDate = string.IsNullOrEmpty(_configuration.GetValue<string>("AdjustHoursToEnrollDate")) ? loyaltyProfileResponse.EnrollDate.AddHours(-12).ToString("MM/dd/yyyy") : loyaltyProfileResponse.EnrollDate.AddHours(_configuration.GetValue<int>("AdjustHoursToEnrollDate")).ToString("MM/dd/yyyy");
                        }

                        var adjustHoursToLastFlightDate = string.IsNullOrEmpty(_configuration.GetValue<string>("AdjustHoursToLastActivityDate")) ? -12 : _configuration.GetValue<int>("AdjustHoursToLastActivityDate");
                        if (loyaltyProfileResponse.LastFlightDate.Year <= 1) adjustHoursToLastFlightDate = 0;
                        mpSummary.LastFlightDate = loyaltyProfileResponse.LastFlightDate != null ? loyaltyProfileResponse.LastFlightDate.AddHours(adjustHoursToLastFlightDate).ToString("MM/dd/yyyy") : "";
                        mpSummary.MileagePlusNumber = loyaltyProfileResponse.MileageplusId;
                        mpSummary.Name = new MOBName
                        {
                            First = loyaltyProfileResponse.FirstName,
                            Last = loyaltyProfileResponse.LastName,
                            Middle = loyaltyProfileResponse.MiddleName,
                            Suffix = loyaltyProfileResponse.Suffix,
                            Title = loyaltyProfileResponse.Title
                        };
                        mpSummary.IsCEO = loyaltyProfileResponse.CEO == true;
                        mpSummary.LifetimeMiles = Convert.ToInt32(loyaltyProfileResponse.LifetimeMiles);

                        if (loyaltyProfileResponse.MillionMilerLevel == 0)
                        {
                            mpSummary.MillionMilerIndicator = string.Empty;
                        }
                        else
                        {
                            mpSummary.MillionMilerIndicator = loyaltyProfileResponse.MillionMilerLevel.ToString();
                        }

                        if (_configuration.GetValue<DateTime>("MP2014EnableDate") < DateTime.Now)
                        {
                            if (loyaltyProfileResponse != null)
                            {
                                bool isValidPqdAddress = false;
                                bool activeNonPresidentialPlusCardMember = false;
                                bool activePresidentialPlusCardMembe = false;
                                bool showChaseBonusTile = false;
                                if (_configuration.GetValue<bool>("EnableCallingPQDService"))
                                {
                                    //Migrate XML to CSL service call
                                    if (_configuration.GetValue<bool>("NewServieCall_GetProfile_PaymentInfos"))
                                    {
                                        var tupleRes = await IsValidPQDAddressV2("GetAccountSummaryWithPremierActivity", req.TransactionId, dpToken, mpSummary.MileagePlusNumber, isValidPqdAddress, activeNonPresidentialPlusCardMember, activePresidentialPlusCardMembe, fourSegmentMinimunWaivedMember, showChaseBonusTile).ConfigureAwait(false);
                                        isValidPqdAddress = tupleRes.isValidPqdAddress;
                                        activeNonPresidentialPlusCardMember = tupleRes.activeNonPresidentialPlusCardMember;
                                        activePresidentialPlusCardMembe = tupleRes.activePresidentialPlusCardMembe;
                                        showChaseBonusTile = tupleRes.chaseBonusTile;
                                        fourSegmentMinimunWaivedMember = tupleRes.fourSegmentMinimunWaivedMember;
                                    }
                                }
                                mpSummary.ShowChaseBonusTile = showChaseBonusTile;
                                AssignCustomerChasePromoType(mpSummary, showChaseBonusTile);

                                noMileageExpiration = activeNonPresidentialPlusCardMember || activePresidentialPlusCardMembe;

                                if (fourSegmentMinimunWaivedMember)
                                {
                                    mpSummary.FourSegmentMinimun = "Waived";
                                }
                                else if (loyaltyProfileResponse.MinimumSegments >= 4)
                                {
                                    mpSummary.FourSegmentMinimun = "4 of 4";
                                }
                                else
                                {
                                    mpSummary.FourSegmentMinimun = string.Format("{0} of 4", loyaltyProfileResponse.MinimumSegments);
                                }
                                //TODO : UCB
                                //if (loyaltyProfileResponse.AccountProfileInfo.PremierQualifyingDollars == 0 || !isValidPqdAddress)
                                //{
                                //    if (!isValidPqdAddress)
                                //    {
                                //        mpSummary.PremierQualifyingDollars = string.Empty;
                                //    }
                                //    else
                                //    {
                                //        mpSummary.PremierQualifyingDollars = "0";
                                //    }
                                //}
                                //else
                                //{
                                //    decimal pqd = 0;
                                //    try
                                //    {
                                //        pqd = Convert.ToDecimal(loyaltyProfileResponse.AccountProfileInfo.PremierQualifyingDollars);
                                //    }
                                //    catch (System.Exception) { }

                                //    if (!string.IsNullOrEmpty(_configuration.GetValue<string>"PqdAmount"]) && !string.IsNullOrEmpty(_configuration.GetValue<string>"PqdText"]))
                                //    {
                                //        if (pqd > Convert.ToDecimal(_configuration.GetValue<string>"PqdAmount"]))
                                //        {
                                //            mpSummary.PremierQualifyingDollars = _configuration.GetValue<string>"PqdText");
                                //        }
                                //    }
                                //    else
                                //    {
                                //        mpSummary.PremierQualifyingDollars = pqd.ToString("C0");
                                //    }
                                //}
                                //to be removed
                                string pdqchasewaiverLabel = string.Empty;
                                string pdqchasewavier = string.Empty;
                                if (isValidPqdAddress)
                                {
                                    if (loyaltyProfileResponse.IsChaseSpend)
                                    {
                                    }
                                    //No longer been use as this property is deprecated
                                    //if (!string.IsNullOrEmpty(loyaltyProfileResponse.AccountProfileInfo.PresidentialPlusIndicator) && loyaltyProfileResponse.AccountProfileInfo.PresidentialPlusIndicator.Equals("Y"))
                                    //{
                                    //    presidentialPlus = true;
                                    //}

                                    if (!_configuration.GetValue<bool>("IsAvoidUAWSChaseMessagingSoapCall"))
                                    {
                                        //GetChaseMessage(activeNonPresidentialPlusCardMember, activePresidentialPlusCardMembe, chaseSpending, presidentialPlus, ref pdqchasewaiverLabel, ref pdqchasewavier, ref balanceExpireDisclaimer);
                                        //mpSummary.PDQchasewaiverLabel = pdqchasewaiverLabel;
                                        //mpSummary.PDQchasewavier = pdqchasewavier;
                                    }

                                    if (!string.IsNullOrEmpty(balanceExpireDisclaimer) && !_configuration.GetValue<bool>("HideMileageBalanceExpireDate"))
                                    {
                                        mpSummary.BalanceExpireDisclaimer = mpSummary.BalanceExpireDisclaimer + " " + balanceExpireDisclaimer;
                                    }
                                }
                                //End to be removed
                                //GetPremierActivity
                                //long l = 0;
                                //l = GetIncrementalThresholdMiles(121000);
                                //This endpoint is not being used. 
                                //sResponse = await GetActivityFromQualificationService(req, dpToken).ConfigureAwait(false);
                                sResponse = null;
                                //if (sResponse != null && sResponse.Status != null && !string.IsNullOrEmpty(sResponse.Status.Code) && sResponse.Status.Code.Trim().ToUpper() == "E0000")
                                //{
                                //    //if ((!string.IsNullOrEmpty(sResponse.RunType) && sResponse.RunType.Trim().ToUpper() == "QL") || ShowActivity(req.MileagePlusNumber))
                                //    //if (!string.IsNullOrEmpty(sResponse.RunType) && sResponse.RunType.Trim().ToUpper() == "QL")
                                //    if (!string.IsNullOrEmpty(sResponse.YearEndIndicator) && sResponse.YearEndIndicator.Trim().ToUpper() == "Y")
                                //    {
                                //        mpSummary.YearEndPremierActivity = GetYearEndActivity(sResponse);
                                //        mpSummary.PremierActivityType = 2;//YearEndActivity
                                //    }
                                //    else
                                //    {
                                //        if (_configuration.GetValue<bool>("ImplementForceUpdateLogicToggleAccountSummary") && !GeneralHelper.IsApplicationVersionGreater(req.Application.Id, req.Application.Version.Major, "ImplementForceUpdateLogicToggleAccountSummaryIOS", "ImplementForceUpdateLogicToggleAccountSummaryAndroid", "", "", true, _configuration))
                                //        {
                                //            mpSummary.PremierActivityType = 3;//ErrorActivity
                                //            mpSummary.ErrorPremierActivity = new MOBErrorPremierActivity()
                                //            {
                                //                ShowErrorIcon = true,
                                //                ErrorPremierActivityTitle = _configuration.GetValue<string>("PremierActivityTitle"),
                                //                ErrorPremierActivityText = _configuration.GetValue<string>("MyAccountForceUpdateMessage")
                                //            };
                                //            mpSummary.PremierStatusTrackerText = _configuration.GetValue<string>("viewpremierstatustracker");
                                //            mpSummary.PremierStatusTrackerLink = _configuration.GetValue<string>("viewpremierstatustrackerlink");
                                //            var captionsForError = await GetPremierActivityLearnAboutCaptions().ConfigureAwait(false);
                                //            if (captionsForError != null && captionsForError.Count > 0)
                                //            {
                                //                mpSummary.PremierTrackerLearnAboutTitle = GetValueFromList(captionsForError, "PremierTrackerLearnAboutTitle");
                                //                mpSummary.PremierTrackerLearnAboutHeader = GetValueFromList(captionsForError, "PremierTrackerLearnAboutHeader");
                                //                mpSummary.PremierTrackerLearnAboutText = GetValueFromList(captionsForError, "PremierTrackerLearnAboutText");
                                //                mpSummary.PremierTrackerLearnAboutText = mpSummary.PremierTrackerLearnAboutText + GetValueFromList(captionsForError, "FullTermsAndConditions");
                                //            }
                                //            //if (!string.IsNullOrEmpty(Utility.GetConfigEntries("ShowWelcomeModel")) && Utility.GetBooleanConfigValue("ShowWelcomeModel") && !Utility.IsVBQWelcomeModelDisplayed(req.MileagePlusNumber, req.Application.Id, req.DeviceId))
                                //            //{
                                //            //    mpSummary.VBQWelcomeModel = new MOBVBQWelcomeModel();
                                //            //}
                                //            mpSummary.NoMileageExpiration = noMileageExpiration.ToString();
                                //            if (_configuration.GetValue<bool>("HideMileageBalanceExpireDate"))
                                //            {
                                //                mpSummary.NoMileageExpirationMessage = _configuration.GetValue<string>("BalanceExpireDisclaimerNeverExpire");
                                //            }
                                //            else if (noMileageExpiration)
                                //            {
                                //                mpSummary.NoMileageExpirationMessage = _configuration.GetValue<string>("ChaseNoMileageExpirationMessage") + " " + balanceExpireDisclaimer;
                                //                if (!fourSegmentMinimunWaivedMember)
                                //                {
                                //                    mpSummary.NoMileageExpirationMessage = mpSummary.NoMileageExpirationMessage + " " + _configuration.GetValue<string>("FouSegmentMessage");
                                //                }
                                //            }
                                //            return mpSummary;
                                //        }
                                //        int trackingUpgradeType = 0;
                                //        if (_configuration.GetValue<bool>("EnableVBQII") && IsVBQTrackerSupportedVersion(req.Application.Id, req.Application.Version.Major))
                                //        {
                                //            trackingUpgradeType = GetVBQTrackingUpgradeType(sResponse);
                                //            if (trackingUpgradeType > 0)
                                //            {
                                //                mpSummary.vBQPremierActivity = GetVBQTrackingUpgradesActivity(sResponse, trackingUpgradeType);
                                //            }
                                //            else
                                //            {
                                //                mpSummary.vBQPremierActivity = GetVBQPremierActivity(sResponse);
                                //            }
                                //        }
                                //        else
                                //        {
                                //            if (_configuration.GetValue<bool>("MyAccountEnableUpgrades") &&
                                //             IsPremierStatusTrackerUpgradesSupportedVersion(req.Application.Id, req.Application.Version.Major))
                                //            {
                                //                trackingUpgradeType = GetTrackingUpgradeType(sResponse);
                                //                if (trackingUpgradeType > 0)
                                //                {
                                //                    mpSummary.PremierActivity = GetTrackingUpgradesActivity(sResponse, trackingUpgradeType);
                                //                }
                                //                else
                                //                {
                                //                    mpSummary.PremierActivity = GetPremierActivity(sResponse, ref pdqchasewaiverLabel, ref pdqchasewavier);
                                //                }
                                //            }
                                //        }
                                //        if (trackingUpgradeType == 3)
                                //        {
                                //            mpSummary.IsIncrementalUpgrade = true;
                                //        }

                                //        mpSummary.PremierActivityType = 1;//PremierActivity
                                //    }
                                //}
                                //else
                                //{
                                mpSummary.PremierActivityType = 3;//ErrorActivity
                                                                    //if (!string.IsNullOrEmpty(Utility.GetConfigEntries("ShowWelcomeModel")) && Utility.GetBooleanConfigValue("ShowWelcomeModel") && !Utility.IsVBQWelcomeModelDisplayed(req.MileagePlusNumber, req.Application.Id, req.DeviceId))
                                                                    //{
                                                                    //    mpSummary.VBQWelcomeModel = new MOBVBQWelcomeModel();
                                                                    //}
                                mpSummary.ErrorPremierActivity = GetErrorPremierActivity(sResponse);
                                //}
                                //Display PremierActivity for older versions all the time
                                if ((mpSummary.PremierActivityType == 2 || mpSummary.PremierActivityType == 3) && IsPremierStatusTrackerSupportedVersion(req.Application.Id, req.Application.Version.Major) == false)
                                {
                                    mpSummary.PremierActivity = GetPremierActivity(sResponse, ref pdqchasewaiverLabel, ref pdqchasewavier);
                                }
                                var captions = await GetPremierActivityLearnAboutCaptions().ConfigureAwait(false);
                                if (captions != null && captions.Count > 0)
                                {
                                    mpSummary.PremierTrackerLearnAboutTitle = GetValueFromList(captions, "PremierTrackerLearnAboutTitle");
                                    mpSummary.PremierTrackerLearnAboutHeader = GetValueFromList(captions, "PremierTrackerLearnAboutHeader");
                                    mpSummary.PremierTrackerLearnAboutText = GetValueFromList(captions, "PremierTrackerLearnAboutText");
                                    if (sResponse?.CurrentYearActivity.FlexPremierQualifyingPoints > 0)
                                    {
                                        mpSummary.PremierTrackerLearnAboutText = mpSummary.PremierTrackerLearnAboutText + GetValueFromList(captions, "ChasePQPDescription");

                                    }
                                    mpSummary.PremierTrackerLearnAboutText = mpSummary.PremierTrackerLearnAboutText + GetValueFromList(captions, "FullTermsAndConditions");
                                }

                                mpSummary.PremierStatusTrackerText = _configuration.GetValue<string>("viewpremierstatustracker");
                                mpSummary.PremierStatusTrackerLink = _configuration.GetValue<string>("viewpremierstatustrackerlink");
                                mpSummary.PlusPoints = _configuration.GetValue<bool>("EnablePlusPointsSummary") ? await GetPlusPointsFromLoyaltyBalanceService(req, dpToken).ConfigureAwait(false) : null;
                                if (!fourSegmentMinimunWaivedMember && !_configuration.GetValue<bool>("HideMileageBalanceExpireDate"))
                                {
                                    mpSummary.BalanceExpireDisclaimer = mpSummary.BalanceExpireDisclaimer + " " + _configuration.GetValue<string>("FouSegmentMessage");
                                }
                            }
                        }
                    }
                }


                if (includeMembershipCardBarCode)
                {
                    //%B[\w]{2,3}\d{5,6}\s{7}\d{4}(GL|1K|GS|PL|SL|\s\s)\s\s[\s\w\<\-\'\.]{35}\sUA
                    string eliteLevel = "";
                    switch (mpSummary.EliteStatus.Level)
                    {
                        case 0:
                            eliteLevel = "  ";
                            break;
                        case 1:
                            eliteLevel = "SL";
                            break;
                        case 2:
                            eliteLevel = "GL";
                            break;
                        case 3:
                            eliteLevel = "PL";
                            break;
                        case 4:
                            eliteLevel = "1K";
                            break;
                        case 5:
                            eliteLevel = "GS";
                            break;
                    }
                    string name = string.Format("{0}<{1}", mpSummary.Name.Last, mpSummary.Name.First);
                    if (!string.IsNullOrEmpty(mpSummary.Name.Middle))
                    {
                        name = name + " " + mpSummary.Name.Middle.Substring(0, 1);
                    }
                    name = String.Format("{0, -36}", name);

                    bool hasUnitedClubMemberShip = false;
                    var tupleResponse = await GetUnitedClubMembershipDetails(mpSummary.MileagePlusNumber, _headers.ContextValues.SessionId, dpToken).ConfigureAwait(false);
                    mpSummary.uAClubMemberShipDetails = tupleResponse.clubMemberShipDetails;
                    hasUnitedClubMemberShip = tupleResponse.hasUnitedClubMemberShip;
                    mpSummary.HasUAClubMemberShip = hasUnitedClubMemberShip;
                    merchOut = tupleResponse.merchOut;
                    mpSummary.MembershipCardExpirationDate = await this.GetMembershipCardExpirationDate(mpSummary.MileagePlusNumber, mpSummary.EliteStatus.Level).ConfigureAwait(false);

                    bool Keep_MREST_MP_EliteLevel_Expiration_Logic = _configuration.GetValue<bool>("Keep_MREST_MP_EliteLevel_Expiration_Logic");
                    string expirationDate = _configuration.GetValue<string>("MPCardExpirationDate");
                    try
                    {
                        if (!Keep_MREST_MP_EliteLevel_Expiration_Logic && sResponse != null)
                        {
                            #region
                            if (!_configuration.GetValue<bool>("Reset_MembershipCardExpirationDate"))
                            {
                                mpSummary.MembershipCardExpirationDate = string.Empty; // Always return the expiration date based on Premier QUAL service #LOYAL-6376
                            }
                            if (sResponse != null && sResponse.CurrentPremierLevel > 0)
                            {
                                DateTime dDate;
                                DateTime.TryParse(_configuration.GetValue<string>("DefaultMileagePlusCardExpirationDateReturnedByQaulSerivceForGeneralMember"), out dDate); //  <add key="DefaultMileagePlusCardExpirationDateReturnedByQaulSerivceForGeneralMember" value="1753-01-01"/>
                                if (sResponse.CurrentPremierLevelExpirationDate.Equals(dDate))
                                {
                                    if (sResponse.CurrentYearInstantElite != null &&
                                        !string.IsNullOrEmpty(sResponse.CurrentYearInstantElite.ConsolidatedCode) &&
                                       sResponse.CurrentYearInstantElite.ExpirationDate != null &&
                                     (sResponse.CurrentYearInstantElite.ConsolidatedCode.ToUpper().Trim() == "TRIAL"))
                                    {
                                        expirationDate = GetMPExpirationDateFromProfileAndQualService(mpSummary, string.Empty, sResponse.CurrentYearInstantElite.ConsolidatedCode, Convert.ToString(sResponse.CurrentYearInstantElite.ExpirationDate), expirationDate);
                                    }
                                }
                                else
                                {
                                    string currentPremierLevelExpirationDate = sResponse.CurrentPremierLevelExpirationDate != null ? Convert.ToString(sResponse.CurrentPremierLevelExpirationDate) : string.Empty;
                                    string consolidatedCode = sResponse.CurrentYearInstantElite != null && !string.IsNullOrEmpty(sResponse.CurrentYearInstantElite.ConsolidatedCode) ? sResponse.CurrentYearInstantElite.ConsolidatedCode : string.Empty;
                                    string currentYearInstantEliteExpirationDate = sResponse.CurrentYearInstantElite != null && sResponse.CurrentYearInstantElite.ExpirationDate != null ? Convert.ToString(sResponse.CurrentYearInstantElite.ExpirationDate) : string.Empty;
                                    bool isPermierLevelExpirationDateGreaterThanCurrentDate = !string.IsNullOrEmpty(currentPremierLevelExpirationDate) ? sResponse.CurrentPremierLevelExpirationDate > DateTime.Now : false;
                                    if (!isPermierLevelExpirationDateGreaterThanCurrentDate)
                                    {
                                        currentPremierLevelExpirationDate = string.Empty;
                                    }
                                    expirationDate = GetMPExpirationDateFromProfileAndQualService(mpSummary, currentPremierLevelExpirationDate, consolidatedCode, currentYearInstantEliteExpirationDate, expirationDate);
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            mpSummary.MembershipCardExpirationDate = await this.GetMembershipCardExpirationDate(mpSummary.MileagePlusNumber, mpSummary.EliteStatus.Level).ConfigureAwait(false);
                            if (mpSummary.MembershipCardExpirationDate.Equals("Trial Status"))
                            {
                                expirationDate = string.Format("12/{0}", DateTime.Today.Year);
                            }
                            else
                            {
                                expirationDate = string.IsNullOrEmpty(mpSummary.MembershipCardExpirationDate) ? _configuration.GetValue<string>("MPCardExpirationDate") : mpSummary.MembershipCardExpirationDate.Substring(11, 2) + mpSummary.MembershipCardExpirationDate.Substring(16, 2);
                            }
                        }
                    }
                    catch (System.Exception) { }

                    string barCodeData = string.Format("%B{0}       {1}{2}  {3}UA              ?", mpSummary.MileagePlusNumber, expirationDate, eliteLevel, name);
                    string allianceTierLevel = "   ";
                    switch (mpSummary.EliteStatus.Level)
                    {
                        case 0:
                            allianceTierLevel = "   ";
                            break;
                        case 1:
                            allianceTierLevel = "UAS";
                            break;
                        case 2:
                            allianceTierLevel = "UAG";
                            break;
                        case 3:
                            allianceTierLevel = "UAG";
                            break;
                        case 4:
                            allianceTierLevel = "UAG";
                            break;
                        case 5:
                            allianceTierLevel = "UAG";
                            break;
                    }
                    string allianceTierLevelExpirationDate = "    ";
                    if (!allianceTierLevel.Equals("   "))
                    {
                        if (!Keep_MREST_MP_EliteLevel_Expiration_Logic && sResponse != null)
                        {
                            allianceTierLevelExpirationDate = expirationDate;
                        }
                        else
                        {
                            if (mpSummary.MembershipCardExpirationDate.Equals("Trial Status"))
                            {
                                allianceTierLevelExpirationDate = string.Format("12/{0}", DateTime.Today.Year);
                            }
                            else
                            {
                                allianceTierLevelExpirationDate = string.IsNullOrEmpty(mpSummary.MembershipCardExpirationDate) ? _configuration.GetValue<string>("MPCardExpirationDate") : mpSummary.MembershipCardExpirationDate.Substring(11, 2) + mpSummary.MembershipCardExpirationDate.Substring(16, 2);
                            }
                        }
                    }

                    string paidLoungeIndicator = "N";
                    string paidLoungeExpireationDate = "      ";
                    if (mpSummary.uAClubMemberShipDetails != null)
                    {
                        if (string.IsNullOrEmpty(mpSummary.uAClubMemberShipDetails.PrimaryOrCompanion))
                        {
                            paidLoungeIndicator = "P";
                        }
                        else
                        {
                            paidLoungeIndicator = mpSummary.uAClubMemberShipDetails.PrimaryOrCompanion;
                        }
                        paidLoungeExpireationDate = Convert.ToDateTime(mpSummary.uAClubMemberShipDetails.DiscontinueDate).ToString("ddMMyyyy");
                    }

                    string starBarCodeData = string.Format("FFPC001UA{0}{1}{2}{3}{4}{5}{6}{7}^001", mpSummary.MileagePlusNumber.PadRight(16), mpSummary.Name.Last.PadRight(20), mpSummary.Name.First.PadRight(20), allianceTierLevel, allianceTierLevelExpirationDate, eliteLevel, paidLoungeIndicator, paidLoungeExpireationDate);

                    if (!string.IsNullOrEmpty(_configuration.GetValue<string>("ReturnMPMembershipBarcode")) && _configuration.GetValue<bool>("ReturnMPMembershipBarcode"))
                    {
                        if (!string.IsNullOrEmpty(_configuration.GetValue<string>("UseStarMembershipCardFormatDateTime")) && _configuration.GetValue<DateTime>("UseStarMembershipCardFormatDateTime") <= DateTime.Now)
                        {
                            mpSummary.MembershipCardBarCodeString = starBarCodeData.ToUpper();
                            mpSummary.MembershipCardBarCode = GetBarCode(starBarCodeData);
                        }
                        else
                        {
                            mpSummary.MembershipCardBarCodeString = barCodeData.ToUpper();
                            mpSummary.MembershipCardBarCode = GetBarCode(barCodeData);
                        }
                    }
                    else
                    {
                        mpSummary.MembershipCardBarCodeString = null;
                        mpSummary.MembershipCardBarCode = null;
                    }
                }

                mpSummary.NoMileageExpiration = noMileageExpiration.ToString();
                if (_configuration.GetValue<bool>("HideMileageBalanceExpireDate"))
                {
                    mpSummary.NoMileageExpirationMessage = _configuration.GetValue<string>("BalanceExpireDisclaimerNeverExpire");
                }
                else if (noMileageExpiration)
                {
                    mpSummary.NoMileageExpirationMessage = _configuration.GetValue<string>("ChaseNoMileageExpirationMessage") + " " + balanceExpireDisclaimer;
                    if (!fourSegmentMinimunWaivedMember)
                    {
                        mpSummary.NoMileageExpirationMessage = mpSummary.NoMileageExpirationMessage + " " + _configuration.GetValue<string>("FouSegmentMessage");
                    }
                }
                //Client is no longer using travelcreditinfo after RAMP 
                //  mpSummary.TravelCreditInfo = GetTravelCreditDetail(req.MileagePlusNumber, req.Application, req.SessionId, req.TransactionId, req.DeviceId);
            }
            catch (MOBUnitedException ex)
            {
                throw new MOBUnitedException(ex.Message);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message);
            }


            if (_configuration.GetValue<string>("StartDateTimeToReturnEmptyMPExpirationDate") != null && _configuration.GetValue<string>("EndtDateTimeToReturnEmptyMPExpirationDate") != null && DateTime.ParseExact(_configuration.GetValue<string>("StartDateTimeToReturnEmptyMPExpirationDate").ToString().ToUpper().Trim(), "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture) <= DateTime.Now && DateTime.ParseExact(_configuration.GetValue<string>("EndtDateTimeToReturnEmptyMPExpirationDate").ToString().ToUpper().Trim(), "MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture) >= DateTime.Now)
            {
                mpSummary.MembershipCardExpirationDate = string.Empty;
            }
            if (!string.IsNullOrEmpty(_configuration.GetValue<string>("ShowWelcomeModel")) && _configuration.GetValue<bool>("ShowWelcomeModel") &&
                await _dynamoDBUtility.IsVBQWelcomeModelDisplayed(req.Application.Id, req.DeviceId, req.MileagePlusNumber, _headers.ContextValues.SessionId).ConfigureAwait(false))
            {
                mpSummary.vBQWelcomeModel = new MOBVBQWelcomeModel();
            }
            return (mpSummary, merchOut);
        }

    }
}
