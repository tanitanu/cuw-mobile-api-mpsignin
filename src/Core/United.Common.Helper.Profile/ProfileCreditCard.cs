using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.CorporateDirect.Models.CustomerProfile;
using United.Definition.CSLModels.CustomerProfile;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.CSLSerivce;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.Internal.Exception;
using United.Utility.Helper;
using CslDataVaultResponse = United.Service.Presentation.PaymentResponseModel.DataVault<United.Service.Presentation.PaymentModel.Payment>;
using Reservation = United.Mobile.Model.Shopping.Reservation;

namespace United.Common.Helper.Profile
{
    public class ProfileCreditCard : IProfileCreditCard
    {
        private readonly ICacheLog<ProfileCreditCard> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IDataVaultService _dataVaultService;
        private string _deviceId = string.Empty;

        private MOBApplication _application = new MOBApplication() { Version = new MOBVersion() };

        public ProfileCreditCard(ICacheLog<ProfileCreditCard> logger
            , IConfiguration configuration
            , ISessionHelperService sessionHelperService
            , IDataVaultService dataVaultService
            )
        {
            _logger = logger;
            _configuration = configuration;
            _sessionHelperService = sessionHelperService;
            _dataVaultService = dataVaultService;
        }

        #region Methods

        public async Task<List<MOBCreditCard>> PopulateCorporateCreditCards(List<Services.Customer.Common.CreditCard> creditCards, bool isGetCreditCardDetailsCall, List<MOBAddress> addresses, Reservation persistedReservation, string sessionId)
        {
            #region

            List<MOBCreditCard> mobCreditCards = new List<MOBCreditCard>();
            var mobGhostCardFirstInList = new List<MOBCreditCard>();
            MOBCreditCard ghostCreditCard = null;
            bool isGhostCard = false;
            bool isValidForTPI = false;
            if (creditCards != null && creditCards.Count > 0)
            {
                #region
                foreach (Services.Customer.Common.CreditCard creditCard in creditCards)
                {
                    MOBCreditCard cc = new MOBCreditCard
                    {
                        Message = IsValidCreditCardMessage(creditCard),
                        AddressKey = creditCard.AddressKey,
                        Key = creditCard.Key
                    };
                    if (_configuration.GetValue<bool>("WorkAround4DataVaultUntilClientChange"))
                    {
                        cc.Key = creditCard.Key + "~" + creditCard.AccountNumberToken;
                    }
                    cc.CardType = creditCard.Code;
                    //switch (creditCard.CCTypeDescription.ToLower())
                    //{
                    //    case "diners club":
                    //        cc.CardTypeDescription = "Diners Club Card";
                    //        break;
                    //    case "uatp (formerly Air Travel Card)":
                    //        cc.CardTypeDescription = "UATP";
                    //        break;
                    //    default:
                    //        cc.CardTypeDescription = creditCard.CCTypeDescription;
                    //        break;
                    //}
                    cc.CardTypeDescription = creditCard.CCTypeDescription;
                    cc.Description = creditCard.CustomDescription;
                    cc.ExpireMonth = creditCard.ExpMonth.ToString();
                    cc.ExpireYear = creditCard.ExpYear.ToString();
                    cc.IsPrimary = creditCard.IsPrimary;

                    //Wade 11/03/2014
                    cc.UnencryptedCardNumber = "XXXXXXXXXXXX" + creditCard.AccountNumberLastFourDigits;
                    //**NOTE**: IF "XXXXXXXXXXXX" is updated to any other format need to fix this same at GetUpdateCreditCardRequest() as we trying to check if CC updated by checking "XXXXXXXXXXXX" exists in the UnencryptedCardNumber if exists CC Number not updated.

                    cc.DisplayCardNumber = cc.UnencryptedCardNumber;

                    cc.cIDCVV2 = creditCard.SecurityCode;

                    if (creditCard.Payor != null)
                    {
                        cc.cCName = creditCard.Payor.GivenName;
                    }
                    if (isGetCreditCardDetailsCall)
                    {
                        cc.UnencryptedCardNumber = creditCard.AccountNumber;
                    }
                    cc.AccountNumberToken = creditCard.AccountNumberToken;
                    MOBVormetricKeys vormetricKeys = await AssignPersistentTokenToCC(creditCard.AccountNumberToken, creditCard.PersistentToken, creditCard.SecurityCodeToken, creditCard.Code, sessionId, "PopulateCorporateCreditCards", 0, "").ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(vormetricKeys.PersistentToken))
                    {
                        cc.PersistentToken = vormetricKeys.PersistentToken;
                        cc.SecurityCodeToken = vormetricKeys.SecurityCodeToken;
                    }
                    if (!string.IsNullOrEmpty(vormetricKeys.CardType) && string.IsNullOrEmpty(cc.CardType))
                    {
                        cc.CardType = vormetricKeys.CardType;
                    }
                    cc.IsCorporate = creditCard.IsCorporate;
                    cc.IsMandatory = creditCard.IsMandatory;
                    if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                    {
                        cc.IsValidForTPIPurchase = IsValidFOPForTPIpayment(creditCard.Code);
                    }
                    //Not assigning the cc.EncryptedCardNumber = creditCard.AccountNumberEncrypted; because client will send back to us and while updating we will call DataVault and it fails with AppId
                    if (addresses != null)
                    {
                        foreach (var address in addresses)
                        {
                            if (address.Key.ToUpper().Trim() == cc.AddressKey.ToUpper().Trim() && !cc.IsCorporate && !cc.IsMandatory)
                            {
                                mobCreditCards.Add(cc);
                            }
                        }
                    }
                    //Mandatory Ghost Cards - If Present then only one card should be displayed to the client and no option to add CC / select other FOPs
                    if (creditCard.IsCorporate && creditCard.IsMandatory)
                    {
                        ghostCreditCard = cc;
                        isGhostCard = true;
                        if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                        {
                            isValidForTPI = cc.IsValidForTPIPurchase;
                        }
                        break;
                    }
                    //Non Mandatory Ghost cards - If Present client can select/Add/Edit other cards and will be first in the list
                    if (cc.IsCorporate && !cc.IsMandatory)
                    {
                        mobGhostCardFirstInList.Add(cc);
                        isGhostCard = true;
                        if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                        {
                            isValidForTPI = cc.IsValidForTPIPurchase;
                        }
                    }
                }
                #endregion
            }
            if (ghostCreditCard != null)
            {
                //In this case only Ghost card will be in the list
                mobGhostCardFirstInList.Add(ghostCreditCard);
            }
            else
            {
                mobGhostCardFirstInList.AddRange(mobCreditCards);
            }
            await GhostCardValidationForTPI(persistedReservation, ghostCreditCard, isGhostCard, isValidForTPI).ConfigureAwait(false);
            #endregion

            return mobGhostCardFirstInList;
        }

        public bool IsValidFOPForTPIpayment(string cardType)
        {
            return !string.IsNullOrEmpty(cardType) &&
                (cardType.ToUpper().Trim() == "VI" || cardType.ToUpper().Trim() == "MC" || cardType.ToUpper().Trim() == "AX" || cardType.ToUpper().Trim() == "DS");
        }

        private async Task GhostCardValidationForTPI(Reservation persistedReservation, MOBCreditCard ghostCreditCard, bool isGhostCard, bool isValidForTPI)
        {
            if (isGhostCard)
            {
                if (persistedReservation != null && persistedReservation.ShopReservationInfo != null)
                {
                    //If ghost card has invalid FOP for TPI purchase, we should not show TPI 
                    if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch") && !isValidForTPI)
                    {
                        persistedReservation.ShopReservationInfo.IsGhostCardValidForTPIPurchase = false;
                    }

                    if (ghostCreditCard != null)
                    {
                        persistedReservation.ShopReservationInfo.CanHideSelectFOPOptionsAndAddCreditCard = true;
                    }
                    await SavePersistedReservation(persistedReservation).ConfigureAwait(false);
                }
            }
        }

        public async Task<List<MOBCreditCard>> PopulateCreditCards(List<Services.Customer.Common.CreditCard> creditCards, bool isGetCreditCardDetailsCall, List<Mobile.Model.Common.MOBAddress> addresses, string sessionId)
        {
            #region

            List<MOBCreditCard> mobCreditCards = new List<MOBCreditCard>();
            if (creditCards != null && creditCards.Count > 0)
            {
                #region
                foreach (Services.Customer.Common.CreditCard creditCard in creditCards)
                {
                    //if(!IsValidCreditCard(creditCard))
                    //{
                    //    continue;
                    //}
                    if (creditCard.IsCorporate)
                    {
                        continue;
                    }

                    MOBCreditCard cc = new MOBCreditCard
                    {
                        Message = IsValidCreditCardMessage(creditCard),
                        AddressKey = creditCard.AddressKey,
                        Key = creditCard.Key
                    };
                    if (_configuration.GetValue<bool>("WorkAround4DataVaultUntilClientChange"))
                    {
                        cc.Key = creditCard.Key + "~" + creditCard.AccountNumberToken;
                    }
                    cc.CardType = creditCard.Code;
                    //switch (creditCard.CCTypeDescription.ToLower())
                    //{
                    //    case "diners club":
                    //        cc.CardTypeDescription = "Diners Club Card";
                    //        break;
                    //    case "uatp (formerly air travel card)":
                    //        cc.CardTypeDescription = "UATP";
                    //        break;
                    //    default:
                    //        cc.CardTypeDescription = creditCard.CCTypeDescription;
                    //        break;
                    //}
                    cc.CardTypeDescription = creditCard.CCTypeDescription;
                    cc.Description = creditCard.CustomDescription;
                    cc.ExpireMonth = creditCard.ExpMonth.ToString();
                    cc.ExpireYear = creditCard.ExpYear.ToString();
                    cc.IsPrimary = creditCard.IsPrimary;
                    //if (creditCard.AccountNumber.Length == 15)
                    //    cc.UnencryptedCardNumber = "XXXXXXXXXXX" + creditCard.AccountNumber.Substring(creditCard.AccountNumber.Length - 4, 4);
                    //else
                    //cc.UnencryptedCardNumber = "XXXXXXXXXXXX" + creditCard.AccountNumber.Substring(creditCard.AccountNumber.Length - 4, 4);
                    //updated due to CSL no longer providing the account number.
                    //Wade 11/03/2014
                    cc.UnencryptedCardNumber = "XXXXXXXXXXXX" + creditCard.AccountNumberLastFourDigits;
                    //**NOTE**: IF "XXXXXXXXXXXX" is updated to any other format need to fix this same at GetUpdateCreditCardRequest() as we trying to check if CC updated by checking "XXXXXXXXXXXX" exists in the UnencryptedCardNumber if exists CC Number not updated.

                    cc.DisplayCardNumber = cc.UnencryptedCardNumber;
                    cc.EncryptedCardNumber = creditCard.AccountNumberEncrypted;
                    cc.cIDCVV2 = creditCard.SecurityCode;
                    //cc.CCName = creditCard.Name;
                    if (creditCard.Payor != null)
                    {
                        cc.cCName = creditCard.Payor.GivenName;
                    }
                    if (isGetCreditCardDetailsCall)
                    {
                        cc.UnencryptedCardNumber = creditCard.AccountNumber;
                    }
                    cc.AccountNumberToken = creditCard.AccountNumberToken;
                    MOBVormetricKeys vormetricKeys = await AssignPersistentTokenToCC(creditCard.AccountNumberToken, creditCard.PersistentToken, creditCard.SecurityCodeToken, creditCard.Code, sessionId, "PopulateCreditCards", 0, "").ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(vormetricKeys.PersistentToken))
                    {
                        cc.PersistentToken = vormetricKeys.PersistentToken;
                        //cc.SecurityCodeToken = vormetricKeys.SecurityCodeToken;
                    }

                    if (!string.IsNullOrEmpty(vormetricKeys.CardType) && string.IsNullOrEmpty(cc.CardType))
                    {
                        cc.CardType = vormetricKeys.CardType;
                    }

                    if (_configuration.GetValue<bool>("CFOPViewRes_ExcludeCorporateCard"))
                    {
                        cc.IsCorporate = creditCard.IsCorporate;
                    }

                    if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                    {
                        cc.IsValidForTPIPurchase = IsValidFOPForTPIpayment(creditCard.Code);
                    }
                    if (addresses != null)
                    {
                        foreach (var address in addresses)
                        {
                            if (address.Key.ToUpper().Trim() == cc.AddressKey.ToUpper().Trim())
                            {
                                mobCreditCards.Add(cc);
                            }
                        }
                    }
                }
                #endregion
            }
            #endregion

            return mobCreditCards;
        }

        private string IsValidCreditCardMessage(Services.Customer.Common.CreditCard creditCard)
        {
            string message = string.Empty;
            if (string.IsNullOrEmpty(creditCard.AddressKey))
            {
                message = _configuration.GetValue<string>("NoAddressAssociatedWithTheSavedCreditCardMessage");
            }
            if (creditCard.ExpYear < DateTime.Today.Year)
            {
                message = message + _configuration.GetValue<string>("CreditCardDateExpiredMessage");
            }
            else if (creditCard.ExpYear == DateTime.Today.Year)
            {
                if (creditCard.ExpMonth < DateTime.Today.Month)
                {
                    message = message + _configuration.GetValue<string>("CreditCardDateExpiredMessage");
                }
            }
            return message;
        }

        private async Task SavePersistedReservation(Reservation persistedReservation)
        {
            await _sessionHelperService.SaveSession<Reservation>(persistedReservation, persistedReservation.SessionId, new List<string> { persistedReservation.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);
        }

        private async Task<MOBVormetricKeys> AssignPersistentTokenToCC(string accountNumberToken, string persistentToken, string securityCodeToken, string cardType, string sessionId, string action, int appId, string deviceID)
        {
            MOBVormetricKeys vormetricKeys = new MOBVormetricKeys();
            if (_configuration.GetValue<bool>("VormetricTokenMigration"))
            {
                Session session = new Session();
                session = await _sessionHelperService.GetSession<Session>(sessionId, session.ObjectName, new List<string>() { sessionId, session.ObjectName }).ConfigureAwait(false);
                if ((string.IsNullOrEmpty(persistentToken) || string.IsNullOrEmpty(cardType)) && !string.IsNullOrEmpty(accountNumberToken) && !string.IsNullOrEmpty(sessionId) && !string.IsNullOrEmpty(session?.Token))
                {
                    vormetricKeys = await GetPersistentTokenUsingAccountNumberToken(accountNumberToken, sessionId, session.Token).ConfigureAwait(false);
                    persistentToken = vormetricKeys.PersistentToken;
                }

                if (!string.IsNullOrEmpty(persistentToken))
                {
                    vormetricKeys.PersistentToken = persistentToken;
                    vormetricKeys.SecurityCodeToken = securityCodeToken;
                    vormetricKeys.CardType = cardType;
                }
                else
                {
                    LogNoPersistentTokenInCSLResponseForVormetricPayment(sessionId);
                }
            }
            else
            {
                persistentToken = string.Empty;
            }

            return vormetricKeys;
        }

        private void LogNoPersistentTokenInCSLResponseForVormetricPayment(string sessionId, string Message = "Unable to retieve PersistentToken")
        {
            _logger.LogWarning("{PERSISTENTTOKENNOTFOUND}", Message);
        }

        private async Task<MOBVormetricKeys> GetPersistentTokenUsingAccountNumberToken(string accountNumberToke, string sessionId, string token)
        {
            string url = string.Format("/{0}/RSA", accountNumberToke);

            var cslResponse = await MakeHTTPCallAndLogIt(sessionId, _deviceId, "CSL-ChangeEligibleCheck", _application, token, url, string.Empty, true, false).ConfigureAwait(false);

            return GetPersistentTokenFromCSLDatavaultResponse(cslResponse, sessionId);
        }

        private MOBVormetricKeys GetPersistentTokenFromCSLDatavaultResponse(string jsonResponseFromCSL, string sessionID)
        {
            MOBVormetricKeys vormetricKeys = new MOBVormetricKeys();
            if (!string.IsNullOrEmpty(jsonResponseFromCSL))
            {
                CslDataVaultResponse response = DataContextJsonSerializer.DeserializeJsonDataContract<CslDataVaultResponse>(jsonResponseFromCSL);
                if (response != null && response.Responses != null && response.Responses[0].Error == null && response.Responses[0].Message != null && response.Responses[0].Message.Count > 0 && response.Responses[0].Message[0].Code.Trim() == "0")
                {
                    var creditCard = response.Items[0] as Service.Presentation.PaymentModel.CreditCard;
                    vormetricKeys.PersistentToken = creditCard.PersistentToken;
                    vormetricKeys.SecurityCodeToken = creditCard.SecurityCodeToken;
                    vormetricKeys.CardType = creditCard.Code;
                }
                else
                {
                    if (response.Responses[0].Error != null && response.Responses[0].Error.Count > 0)
                    {
                        string errorMessage = string.Empty;
                        foreach (var error in response.Responses[0].Error)
                        {
                            errorMessage = errorMessage + " " + error.Text;
                        }
                        if (!_configuration.GetValue<bool>("DisableSoftErrorLogForCCDataVaultRSA_MOBILE20913"))
                        {
                            _logger.LogWarning("GetPersistentTokenUsingAccountNumberToken response ", response);
                        }
                        else
                        {
                            throw new MOBUnitedException(errorMessage);
                        }
                    }
                    else
                    {
                        string exceptionMessage = _configuration.GetValue<string>("UnableToInsertCreditCardToProfileErrorMessage").ToString();
                        if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && _configuration.GetValue<bool>("ReturnActualExceptionMessageBackForTesting"))
                        {
                            exceptionMessage = exceptionMessage + " response.Status not success and response.Errors.Count == 0 - at DAL InsertTravelerCreditCard(MOBUpdateTravelerRequest request)";
                        }
                        throw new MOBUnitedException(exceptionMessage);
                    }
                }
            }

            return vormetricKeys;
        }

        private async Task<string> MakeHTTPCallAndLogIt(string sessionId, string deviceId, string action, MOBApplication application, string token, string url, string jsonRequest, bool isGetCall, bool isXMLRequest = false)
        {
            string jsonResponse = string.Empty;

            string paypalCSLCallDurations = string.Empty;
            string callTime4Tuning = string.Empty;

            string applicationRequestType = isXMLRequest ? "xml" : "json";
            if (isGetCall)
            {
                jsonResponse = await _dataVaultService.GetPersistentToken(token, jsonRequest, url, sessionId).ConfigureAwait(false);

            }
            else
            {
                jsonResponse = await _dataVaultService.PersistentToken(token, jsonRequest, url, sessionId).ConfigureAwait(false);

            }

            return jsonResponse;
        }

        #endregion

        #region UCB Changes

        public async Task<List<MOBCreditCard>> PopulateCorporateCreditCards(bool isGetCreditCardDetailsCall, List<MOBAddress> addresses, Reservation persistedReservation, MOBCPProfileRequest request)
        {

            United.CorporateDirect.Models.CustomerProfile.CorpFopResponse corpFopResponse = new CorpFopResponse();
            var response = await _sessionHelperService.GetSession<United.CorporateDirect.Models.CustomerProfile.CorpFopResponse>(request.SessionId, corpFopResponse.GetType().FullName, new List<string> { request.SessionId, corpFopResponse.GetType().FullName }).ConfigureAwait(false);
            #region     
            if (response != null)
            {
                List<MOBCreditCard> mobCreditCards = new List<MOBCreditCard>();
                var mobGhostCardFirstInList = new List<MOBCreditCard>();
                MOBCreditCard ghostCreditCard = null;
                bool isGhostCard = false;
                bool isValidForTPI = false;
                var creditCards = response.CreditCards;
                if (creditCards != null && creditCards.Count > 0)
                {
                    #region
                    foreach (United.CorporateDirect.Models.CustomerProfile.CorporateCreditCard creditCard in creditCards)
                    {
                        string addressKey = addresses != null ? addresses[0].Key : string.Empty;
                        MOBCreditCard cc = new MOBCreditCard
                        {
                            Message = IsValidCreditCardMessage(addressKey, creditCard.ExpYear, creditCard.ExpMonth),
                            AddressKey = addresses != null ? addresses[0].Key : string.Empty,//Getprofile service was assigning the first address(Confirmed with service team) ..Corporate direct service wont have addresskey..So,implemented how getprofile is doing
                            Key = !_configuration.GetValue<bool>("DisablePassingCreditcardKeyForCorporateCreditcards") ? (new Guid()).ToString() : string.Empty
                        };
                        if (_configuration.GetValue<bool>("WorkAround4DataVaultUntilClientChange"))
                        {
                            cc.Key = "~" + creditCard.AccountNumberToken;
                        }
                        cc.CardType = creditCard.Code;
                        cc.CardTypeDescription = creditCard.CCTypeDescription;
                        cc.Description = creditCard.CustomDescription;
                        cc.ExpireMonth = creditCard.ExpMonth.ToString();
                        cc.ExpireYear = creditCard.ExpYear.ToString();
                        //cc.IsPrimary = creditCard.IsPrimary;

                        //Wade 11/03/2014
                        cc.UnencryptedCardNumber = "XXXXXXXXXXXX" + creditCard.AccountNumberLastFourDigits;
                        //**NOTE**: IF "XXXXXXXXXXXX" is updated to any other format need to fix this same at GetUpdateCreditCardRequest() as we trying to check if CC updated by checking "XXXXXXXXXXXX" exists in the UnencryptedCardNumber if exists CC Number not updated.

                        cc.DisplayCardNumber = cc.UnencryptedCardNumber;

                        // cc.cIDCVV2 = creditCard.SecurityCode; Service wil never send this confirmed with service team

                        if (creditCard.Payor != null)
                        {
                            cc.cCName = creditCard.Payor.GivenName;
                        }
                        //if (isGetCreditCardDetailsCall)
                        //{
                        //    cc.UnencryptedCardNumber = creditCard.AccountNumber;
                        //}
                        cc.AccountNumberToken = creditCard.AccountNumberToken;
                        MOBVormetricKeys vormetricKeys = await AssignPersistentTokenToCC(creditCard.AccountNumberToken, creditCard.PersistentToken, /*creditCard.SecurityCodeToken*/"", creditCard.Code, "", "PopulateCorporateCreditCards", 0, "").ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(vormetricKeys.PersistentToken))
                        {
                            cc.PersistentToken = vormetricKeys.PersistentToken;
                            cc.SecurityCodeToken = vormetricKeys.SecurityCodeToken;
                        }
                        if (!string.IsNullOrEmpty(vormetricKeys.CardType) && string.IsNullOrEmpty(cc.CardType))
                        {
                            cc.CardType = vormetricKeys.CardType;
                        }
                        cc.IsCorporate = creditCard.IsCorporate;
                        cc.IsMandatory = creditCard.IsMandatory;
                        if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                        {
                            cc.IsValidForTPIPurchase = IsValidFOPForTPIpayment(creditCard.Code);
                        }
                        //Not assigning the cc.EncryptedCardNumber = creditCard.AccountNumberEncrypted; because client will send back to us and while updating we will call DataVault and it fails with AppId
                        if (addresses != null)
                        {
                            foreach (var address in addresses)
                            {
                                if (address.Key.ToUpper().Trim() == cc.AddressKey.ToUpper().Trim() && !cc.IsCorporate && !cc.IsMandatory)
                                {
                                    mobCreditCards.Add(cc);
                                }
                            }
                        }
                        //Mandatory Ghost Cards - If Present then only one card should be displayed to the client and no option to add CC / select other FOPs
                        if (creditCard.IsCorporate && creditCard.IsMandatory)
                        {
                            ghostCreditCard = cc;
                            isGhostCard = true;
                            if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                            {
                                isValidForTPI = cc.IsValidForTPIPurchase;
                            }
                            break;
                        }
                        //Non Mandatory Ghost cards - If Present client can select/Add/Edit other cards and will be first in the list
                        if (cc.IsCorporate && !cc.IsMandatory)
                        {
                            mobGhostCardFirstInList.Add(cc);
                            isGhostCard = true;
                            if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                            {
                                isValidForTPI = cc.IsValidForTPIPurchase;
                            }
                        }
                    }
                    #endregion
                }
                if (ghostCreditCard != null)
                {
                    //In this case only Ghost card will be in the list
                    mobGhostCardFirstInList.Add(ghostCreditCard);
                }
                else
                {
                    mobGhostCardFirstInList.AddRange(mobCreditCards);
                }
                await GhostCardValidationForTPI(persistedReservation, ghostCreditCard, isGhostCard, isValidForTPI).ConfigureAwait(false);
                return mobGhostCardFirstInList;
            }
            #endregion
            return null;

        }

        private string IsValidCreditCardMessage(string addressKey, int expYear, int expMonth)
        {
            string message = string.Empty;
            if (string.IsNullOrEmpty(addressKey))
            {
                message = _configuration.GetValue<string>("NoAddressAssociatedWithTheSavedCreditCardMessage");
            }
            if (expYear < DateTime.Today.Year)
            {
                message = message + _configuration.GetValue<string>("CreditCardDateExpiredMessage");
            }
            else if (expYear == DateTime.Today.Year)
            {
                if (expMonth < DateTime.Today.Month)
                {
                    message = message + _configuration.GetValue<string>("CreditCardDateExpiredMessage");
                }
            }
            return message;
        }

        public async Task<List<MOBCreditCard>> PopulateCreditCards(bool isGetCreditCardDetailsCall, List<MOBAddress> addresses, MOBCPProfileRequest request)
        {
            var response = await _sessionHelperService.GetSession<CreditCardDataReponseModel>(request.SessionId, new CreditCardDataReponseModel().GetType().FullName, new List<string> { request.SessionId, new CreditCardDataReponseModel().GetType().FullName }).ConfigureAwait(false);
            if (response != null)
            {
                var creditCards = response.CreditCards;
                List<MOBCreditCard> mobCreditCards = new List<MOBCreditCard>();
                if (creditCards != null && creditCards.Count > 0)
                {
                    #region
                    foreach (ProfileCreditCardItem creditCard in creditCards)
                    {
                        MOBCreditCard cc = new MOBCreditCard();
                        cc.Message = IsValidCreditCardMessage(creditCard.AddressKey, creditCard.ExpYear, creditCard.ExpMonth);
                        cc.AddressKey = creditCard.AddressKey;
                        cc.Key = creditCard.Key;
                        if (_configuration.GetValue<bool>("WorkAround4DataVaultUntilClientChange"))
                        {
                            cc.Key = creditCard.Key + "~" + creditCard.AccountNumberToken;
                        }
                        cc.CardType = creditCard.Code;

                        cc.CardTypeDescription = creditCard.CCTypeDescription;
                        cc.Description = creditCard.CustomDescription;
                        cc.ExpireMonth = creditCard.ExpMonth.ToString();
                        cc.ExpireYear = creditCard.ExpYear.ToString();
                        cc.IsPrimary = creditCard.IsPrimary;

                        //updated due to CSL no longer providing the account number.
                        //Wade 11/03/2014
                        cc.UnencryptedCardNumber = "XXXXXXXXXXXX" + creditCard.AccountNumberLastFourDigits;
                        //**NOTE**: IF "XXXXXXXXXXXX" is updated to any other format need to fix this same at GetUpdateCreditCardRequest() as we trying to check if CC updated by checking "XXXXXXXXXXXX" exists in the UnencryptedCardNumber if exists CC Number not updated.

                        cc.DisplayCardNumber = cc.UnencryptedCardNumber;
                        // cc.EncryptedCardNumber = creditCard.AccountNumberEncrypted;Service usually never send this property confirmed with them
                        // cc.cIDCVV2 = creditCard.SecurityCode;Service usually never send this property confirmed with them
                        //cc.CCName = creditCard.Name;
                        if (creditCard.Payor != null)
                        {
                            cc.cCName = creditCard.Payor.GivenName;
                        }
                        //if (isGetCreditCardDetailsCall)
                        //{
                        //    cc.UnencryptedCardNumber = creditCard.AccountNumber;
                        //}
                        cc.AccountNumberToken = creditCard.AccountNumberToken;
                        MOBVormetricKeys vormetricKeys = await AssignPersistentTokenToCC(creditCard.AccountNumberToken, creditCard.PersistentToken, /*creditCard.SecurityCodeToken*/"", creditCard.Code, "", "PopulateCreditCards", 0, "").ConfigureAwait(false);//SecurityCodeToken will never be sent by service confirmed with service team
                        if (!string.IsNullOrEmpty(vormetricKeys.PersistentToken))
                        {
                            cc.PersistentToken = vormetricKeys.PersistentToken;
                            //cc.SecurityCodeToken = vormetricKeys.SecurityCodeToken;
                        }

                        if (!string.IsNullOrEmpty(vormetricKeys.CardType) && string.IsNullOrEmpty(cc.CardType))
                        {
                            cc.CardType = vormetricKeys.CardType;
                        }

                        //if (_configuration.GetValue<bool>("CFOPViewRes_ExcludeCorporateCard")) //No longer needed to check as new service wont return the corporate credit cards
                        //    cc.IsCorporate = creditCard.IsCorporate;

                        if (_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch"))
                        {
                            cc.IsValidForTPIPurchase = IsValidFOPForTPIpayment(creditCard.Code);
                        }
                        if (addresses != null)
                        {
                            foreach (var address in addresses)
                            {
                                if (address.Key.ToUpper().Trim() == cc.AddressKey.ToUpper().Trim())
                                {
                                    mobCreditCards.Add(cc);
                                }
                            }
                        }
                    }
                    #endregion
                }
                return mobCreditCards;
            }
            else
            {
                throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
        }

        #endregion
    }
}
