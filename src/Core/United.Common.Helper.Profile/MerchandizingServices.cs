using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using United.Mobile.DataAccess.MerchandizeService;
using United.Mobile.Model.Common;
using United.Mobile.Model.MPSignIn.Subscription;
using United.Utility.Helper;

namespace United.Common.Helper.Profile
{
    public class MerchandizingServices : IMerchandizingServices
    {
        private readonly ICacheLog<MerchandizingServices> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMerchOffersService _merchOffersService;

        public MerchandizingServices(
            ICacheLog<MerchandizingServices> logger
            , IConfiguration configuration
            , IMerchOffersService merchOffersService
           )
        {
            _logger = logger;
            _configuration = configuration;
            _merchOffersService = merchOffersService;
        }

        public void SetMerchandizeChannelValues(string merchChannel, ref string channelId, ref string channelName)
        {
            channelId = string.Empty;
            channelName = string.Empty;

            if (merchChannel != null)
            {
                switch (merchChannel)
                {
                    case "MOBBE":
                        channelId = _configuration.GetValue<string>("MerchandizeOffersServiceMOBBEChannelID").Trim();
                        channelName = _configuration.GetValue<string>("MerchandizeOffersServiceMOBBEChannelName").Trim();
                        break;
                    case "MOBMYRES":
                        channelId = _configuration.GetValue<string>("MerchandizeOffersServiceMOBMYRESChannelID").Trim();
                        channelName = _configuration.GetValue<string>("MerchandizeOffersServiceMOBMYRESChannelName").Trim();
                        break;
                    case "MOBWLT":
                        channelId = _configuration.GetValue<string>("MerchandizeOffersServiceMOBWLTChannelID").Trim();
                        channelName = _configuration.GetValue<string>("MerchandizeOffersServiceMOBWLTChannelName").Trim();
                        break;
                    default:
                        break;
                }
            }
        }

        public async Task<MOBUASubscriptions> GetUASubscriptions(string mpAccountNumber, int applicationID, string sessionID, string channelId, string channelName, string token)
        {
            MOBUASubscriptions objUASubscriptionsList = new MOBUASubscriptions
            {
                MPAccountNumber = mpAccountNumber
            };
            try
            {
                try
                {
                    Requester requester = new Requester()
                    {
                        Requestor = new Requestor()
                        {
                            ChannelID = channelId,
                            ChannelName = channelName,
                            LanguageCode = "en-US"
                        }
                    };
                    CSLSubscriptionRequest request = new CSLSubscriptionRequest()
                    {
                        CountryCode = "US",
                        CurrencyCode = "USD",
                        LoyaltyProgramMemberID = mpAccountNumber,
                        Requester = requester,
                        TicketingCountryCode = "USD"
                    };

                    var payload = JsonConvert.SerializeObject(request);

                    var merchOut = await _merchOffersService.GetSubscriptions<United.Service.Presentation.ProductResponseModel.Subscription>(payload, token, sessionID).ConfigureAwait(false);

                    if (merchOut.Subscriptions != null)
                    {
                        objUASubscriptionsList.SubscriptionTypes = new List<MOBUASubscription>();
                        foreach (var objSubscriptionType in merchOut.Subscriptions)
                        {
                            if (objSubscriptionType.Subscription.Status != null && objSubscriptionType.Subscription.Status.Trim().ToUpper() == "ACTIVE")
                            {
                                objUASubscriptionsList.SubscriptionTypes.Add(GetEachSubscriptionTypeDetails(objSubscriptionType));
                            }

                        }
                    }
                }
                catch (System.ServiceModel.FaultException ex)
                {
                    if (ex != null && ex.Message != null)
                    {
                        string responseErrorXML = ex.Message;
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (ex != null && ex.Message != null)
                {
                    string responseErrorXML = ex.Message;
                }
            }

            return objUASubscriptionsList;
        }

         public MOBUASubscriptions GetUASubscriptions(string mpAccountNumber, Service.Presentation.ProductResponseModel.Subscription merchOut)
        {
            MOBUASubscriptions objUASubscriptionsList = new MOBUASubscriptions
            {
                MPAccountNumber = mpAccountNumber
            };

            try
            {
                try
                {
                    if (merchOut.Subscriptions != null)
                    {
                        objUASubscriptionsList.SubscriptionTypes = new List<MOBUASubscription>();

                        foreach (var objSubscriptionType in merchOut.Subscriptions)
                        {
                            if (objSubscriptionType.Subscription.Status != null && objSubscriptionType.Subscription.Status.Trim().ToUpper() == "ACTIVE")
                            {
                                objUASubscriptionsList.SubscriptionTypes.Add(GetEachSubscriptionTypeDetails(objSubscriptionType));
                            }
                        }
                    }
                }
                catch (System.ServiceModel.FaultException ex)
                {
                    if (ex != null && ex.Message != null)
                    {
                        string responseErrorXML = ex.Message;
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (ex != null && ex.Message != null)
                {
                    string responseErrorXML = ex.Message;
                }
            }

            return objUASubscriptionsList;
        }

        public async Task<(MOBClubMembership clubMemberShip, Service.Presentation.ProductResponseModel.Subscription merchOut)>
            GetUAClubSubscriptions(string mpAccountNumber, string sessionID, string channelId,  string channelName, string token)
        {
            MOBClubMembership mobClubMembership = null;
            Service.Presentation.ProductResponseModel.Subscription merchOut = null;
            try
            {
                try
                {
                    Requester requester = new Requester()
                    {
                        Requestor = new Requestor()
                        {
                            ChannelID = channelId,
                            ChannelName = channelName,
                            LanguageCode = "en-US"
                        }
                    };
                    CSLSubscriptionRequest request = new CSLSubscriptionRequest()
                    {
                        CountryCode = "US",
                        CurrencyCode = "USD",
                        LoyaltyProgramMemberID = mpAccountNumber,
                        Requester = requester,
                        TicketingCountryCode = "USD"
                    };

                    var payload = JsonConvert.SerializeObject(request);

                    merchOut = await _merchOffersService.GetSubscriptions<United.Service.Presentation.ProductResponseModel.Subscription>(
                        payload, token, sessionID).ConfigureAwait(false);

                    if (merchOut?.Subscriptions != null && merchOut?.Subscriptions?.Count > 0)
                    {
                        var subscriptionResponse = merchOut.Subscriptions.Where(x => x.Subscription.Type == "SCL" &&
                                                    ValidateSubscription(x?.Subscription?.ExpirationDate)).FirstOrDefault();
                        if (subscriptionResponse?.Subscription != null)
                        {
                            mobClubMembership = new MOBClubMembership
                            {
                                CompanionMPNumber = string.IsNullOrEmpty(subscriptionResponse?.Subscription?.LoyaltyProgramMemberID)
                                            ? string.Empty : !string.IsNullOrEmpty(subscriptionResponse?.Subscription?.HolderType)
                                            && subscriptionResponse?.Subscription?.HolderType?.ToUpper() == "PRIMARY"
                                            ? string.Empty : subscriptionResponse?.Subscription?.LoyaltyProgramMemberID,
                                EffectiveDate = subscriptionResponse?.Subscription?.EffectiveDate,
                                ExpirationDate = subscriptionResponse?.Subscription?.ExpirationDate,
                                IsPrimary = !string.IsNullOrEmpty(subscriptionResponse?.Subscription?.HolderType)
                                            && subscriptionResponse?.Subscription?.HolderType?.ToUpper() == "PRIMARY",
                                MembershipTypeCode = string.IsNullOrEmpty(subscriptionResponse?.Subscription?.Code)
                                            ? string.Empty : subscriptionResponse?.Subscription?.Code,
                                MembershipTypeDescription = string.IsNullOrEmpty(subscriptionResponse?.Subscription?.Name)
                                            ? string.Empty : subscriptionResponse?.Subscription?.Name
                            };
                        }
                    }
                }
                catch (System.ServiceModel.FaultException ex)
                {
                    if (ex != null && ex.Message != null)
                    {
                        string responseErrorXML = ex.Message;
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (ex != null && ex.Message != null)
                {
                    string responseErrorXML = ex.Message;
                }
            }

            return (mobClubMembership, merchOut);
        }

        private bool ValidateSubscription(string expirationDate)
        {
            DateTime subscriptionExpiryDate;
            if (DateTime.TryParse(expirationDate, out subscriptionExpiryDate))
            {
                return subscriptionExpiryDate >= DateTime.Now.Date;
            }
            return false;
        }

        private MOBUASubscription GetEachSubscriptionTypeDetails(Service.Presentation.ProductModel.TravelerSubscription objSubscriptionType)
        {
            #region objUASubscription
            MOBUASubscription objUASubscription = new MOBUASubscription
            {
                Items = new List<MOBItem>()
            };

            MOBItem item2 = new MOBItem
            {
                Id = "Expiration",
                CurrentValue = objSubscriptionType.Subscription.ExpirationDate
            };

            MOBItem item3 = new MOBItem
            {
                Id = "Auto-renew",
                CurrentValue = "Off"
            };
            if (objSubscriptionType.AutoRenewPayments != null && objSubscriptionType.IsAutoRenew.Trim().ToUpper() == "TRUE")
            {
                item3.CurrentValue = "On";
            }
            MOBItem item4 = null;
            MOBItem item5 = null;
            MOBItem item6 = null;
            MOBItem item7 = null;
            if (objSubscriptionType.Subscription.Features != null)
            {
                #region
                foreach (var objFeature in objSubscriptionType.Subscription.Features)
                {
                    if (objFeature.Type == Service.Presentation.CommonEnumModel.ProductFeatureType.Region)
                    {
                        item4 = new MOBItem
                        {
                            Id = "Region",
                            CurrentValue = objFeature.DisplayName//"Global";
                        };
                    }
                    else if (objFeature.Type == Service.Presentation.CommonEnumModel.ProductFeatureType.Beneficiary)
                    {
                        item5 = new MOBItem
                        {
                            Id = "Included travelers",
                            CurrentValue = objFeature.DisplayName//"Subscriber";
                        };
                    }
                    else if (objFeature.Type == Service.Presentation.CommonEnumModel.ProductFeatureType.BaggageFeeWaiver)
                    {
                        item6 = new MOBItem
                        {
                            Id = "Baggage subscription"
                        };
                        string baggageSubscription1st2ndBagDescriptions = "1st checked bag|1st and 2nd checked bags";
                        if (_configuration.GetValue<string>("BaggageSubscription1st2ndBagDescriptions") != null)
                        {
                            baggageSubscription1st2ndBagDescriptions = _configuration.GetValue<string>("BaggageSubscription1st2ndBagDescriptions").ToString();
                        }
                        item6.CurrentValue = baggageSubscription1st2ndBagDescriptions.Split('|')[0].ToString();
                        if (objFeature.Value.Trim() == "2")
                        {
                            item6.CurrentValue = baggageSubscription1st2ndBagDescriptions.Split('|')[1].ToString();
                        }
                    }
                    else if (objFeature.Type == Service.Presentation.CommonEnumModel.ProductFeatureType.Handling)
                    {
                        item7 = new MOBItem
                        {
                            Id = "Baggage handling"
                        };
                        System.Globalization.TextInfo myTI = new System.Globalization.CultureInfo("en-US", false).TextInfo;
                        item7.CurrentValue = myTI.ToTitleCase(objFeature.Value.Trim().ToLower());//"Priority";
                    }
                }
                #endregion
            }
            MOBItem item1 = new MOBItem
            {
                Id = "Subscription"
            };
            if (objSubscriptionType.Subscription.Type.Trim().ToUpper() == "SBG")
            {
                #region
                item1.CurrentValue = "Baggage subscription";
                objUASubscription.Items.Add(item1);
                objUASubscription.Items.Add(item2);
                objUASubscription.Items.Add(item3);
                if (item4 != null)
                {
                    objUASubscription.Items.Add(item4);
                }
                if (item5 != null)
                {
                    objUASubscription.Items.Add(item5);
                }
                if (item6 != null)
                {
                    objUASubscription.Items.Add(item6);
                }
                if (item7 != null)
                {
                    objUASubscription.Items.Add(item7);
                }
                #endregion
            }
            else if (objSubscriptionType.Subscription.Type.Trim().ToUpper() == "SEP")
            {
                #region
                item1.CurrentValue = _configuration.GetValue<string>("EPlusSubscriberMessageTitle").ToString();
                objUASubscription.Items.Add(item1);
                objUASubscription.Items.Add(item2);
                objUASubscription.Items.Add(item3);
                if (item4 != null)
                {
                    objUASubscription.Items.Add(item4);
                }
                if (item5 != null)
                {
                    objUASubscription.Items.Add(item5);
                }
                #endregion
            }
            else if (objSubscriptionType.Subscription.Type.Trim().ToUpper() == "SCL")
            {
                #region
                item1.CurrentValue = "United Club SM membership";
                objUASubscription.Items.Add(item1);
                objUASubscription.Items.Add(item2);
                objUASubscription.Items.Add(item3);
                if (item5 != null)
                {
                    objUASubscription.Items.Add(item5);
                }
                #endregion
            }
            #endregion
            return objUASubscription;
        }
    }
}
