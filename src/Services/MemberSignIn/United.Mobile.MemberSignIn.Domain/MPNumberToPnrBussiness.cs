using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.CSLSerivce;
using United.Mobile.DataAccess.MPSignIn;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.MPSignIn.Common;
using United.Mobile.Model.MPSignIn.MPNumberToPNR;
using United.Mobile.Model.OneClickEnrollment;
using United.Service.Presentation.CommonModel;
using United.Utility.Extensions;
using United.Utility.Helper;
using United.Service.Presentation.ReservationResponseModel;
using United.Mobile.Model.Internal.Exception;

namespace United.Mobile.MemberSignIn.Domain
{
    public class MPNumberToPnrBussiness : IMPNumberToPnrBussiness
    {
        private readonly ICacheLog<MPNumberToPnrBussiness> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICustomerProfileService _customerProfileService;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IUCBProfileService _ucbProfileService;
        private readonly IMemberProfileService _memberProfileService;
        private readonly IPNRRetrievalService _pNRRetrievalService;
        private readonly IDPService _tokenService;

        public MPNumberToPnrBussiness(
            ICacheLog<MPNumberToPnrBussiness> logger,
            IConfiguration configuration,
            ICustomerProfileService customerProfileService,
            ISessionHelperService sessionHelperService,
            IUCBProfileService ucbProfileService,
            IMemberProfileService memberProfileService,
            IPNRRetrievalService pNRRetrievalService,
            IDPService tokenService)
        {
            _logger = logger;
            _configuration = configuration;
            _customerProfileService = customerProfileService;
            _sessionHelperService = sessionHelperService;
            _ucbProfileService = ucbProfileService;
            _memberProfileService = memberProfileService;
            _pNRRetrievalService = pNRRetrievalService;
            _tokenService = tokenService;
        }

        public async Task<MOBAddMpToPnrEligibilityResponse> AddMpToPnrEligibilityCheck(MPSearchRequest request)
        {
            var response = new MOBAddMpToPnrEligibilityResponse();
            try
            {
                if (string.IsNullOrEmpty(request.SessionId))
                {
                    _logger.LogInformation("SessionId is Null from clientRequest");
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }

                response.SessionId = request.SessionId;
                var reservationdetail = new ReservationDetail();
                var cslReservation = await _sessionHelperService.GetSession<ReservationDetail>(request.SessionId, reservationdetail.GetType().FullName, new List<string> { request.SessionId, reservationdetail.GetType().FullName }).ConfigureAwait(false);
                if (_configuration.GetValue<bool>("addMpNumbersToPnrEnabled") && cslReservation?.Detail != null)
                {
                    response.ShowAddMpToPnr = await ValidateAddMpToPnrEligibility(request, cslReservation.Detail);
                    //response.ShowAddMpToPnr = true;
                    if (response.ShowAddMpToPnr)
                    {
                        response.AddMpToPnrHeader = _configuration.GetSection("AddMpToPnrEligibilityConfig").GetValue<string>("AddMpToPnrHeader");
                        response.AddMpToPnrLinkText = _configuration.GetSection("AddMpToPnrEligibilityConfig").GetValue<string>("AddMpToPnrLinkText");
                        response.AddMpToPnrDescription = _configuration.GetSection("AddMpToPnrEligibilityConfig").GetValue<string>("AddMpToPnrDescription");

                    }
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("AddMpToPnrEligibilityCheck error {@UnitedException}, {@stackTrack}", uaex.Message, uaex.StackTrace);

                if (uaex != null && !string.IsNullOrEmpty(uaex.Message.Trim()))
                {
                    response.Exception = new MOBException
                    {
                        Message = uaex.Message
                    };
                }
                else
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
            }
            catch (System.Exception ex)
            {
                MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);
                _logger.LogError("AddMpToPnrEligibilityCheck error {@errormessage} {@PNR}", exceptionWrapper, request?.PNR);

                if (!_configuration.GetValue<bool>("SurfaceErrorToClient"))
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    response.Exception = new MOBException("9999", "SessionId :" + request.SessionId + ex.Message);
                }
            }

            return response;
        }

        public async Task<MPSearchResponse> SearchMPNumber(MPSearchRequest request)
        {
            var response = new MPSearchResponse();
            try
            {
                if (string.IsNullOrEmpty(request.SessionId))
                {
                    _logger.LogInformation("SessionId is Null from clientRequest");
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                response.SessionId = request.SessionId;
                var reservationdetail = new ReservationDetail();
                var cslReservation = await _sessionHelperService.GetSession<ReservationDetail>(request.SessionId, reservationdetail.GetType().FullName, new List<string> { request.SessionId, reservationdetail.GetType().FullName }).ConfigureAwait(false);
                response.TravelerInfo = new List<TravelerInformation>();

                if (_configuration.GetValue<bool>("addMpNumbersToPnrEnabled") && cslReservation?.Detail != null)
                {
                    response.TravelerInfo = await SearchMemberProfiles(request, cslReservation.Detail);
                    if (response.TravelerInfo?.Count == 0)
                    {
                        response.AlertInfo = new Alert();
                        response.AlertInfo.Title = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("ErrorHeaderText");
                        response.AlertInfo.Content = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("ErrorDescriptionText");
                    }
                    else
                    {
                        response.Title = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("Title");
                        response.Header = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("Header");
                        response.Image = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("Image");
                        response.Description = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("Description");
                        response.AddMileagePlusButtonText = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("AddMilegePlusButtonText");
                        response.TravelerNameLabel = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("TravelerNameLabel");
                        response.MileagePlusNumberLabel = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("MilegePlusNumberLabel");

                        if (response.TravelerInfo?.Count >= 2)
                        {
                            response.Description = response.Description.Replace("number", "numbers");
                            response.AddMileagePlusButtonText = response.AddMileagePlusButtonText.Replace("number", "numbers");
                        }
                        
                    }
                }

            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("SearchMPNumber error {@UnitedException}, {@stackTrack}", uaex.Message, uaex.StackTrace);

                if (uaex != null && !string.IsNullOrEmpty(uaex.Message.Trim()))
                {
                    response.Exception = new MOBException
                    {
                        Message = uaex.Message
                    };
                }
                else
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
            }
            catch (System.Exception ex)
            {
                MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);
                _logger.LogError("SearchMPNumber error {@errormessage} {@PNR}", exceptionWrapper, request?.PNR);

                if (!_configuration.GetValue<bool>("SurfaceErrorToClient"))
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    response.Exception = new MOBException("9999", "SessionId :" + request.SessionId + ex.Message);
                }
                response.AlertInfo = new Alert();
                response.AlertInfo.Title = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("ErrorHeaderText");
                response.AlertInfo.Content = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("ErrorDescriptionText");
            }

            return response;
        }

        public async Task<AddMPNumberToPnrResponse> AddMPNumberToPnr(AddMPNumberToPnrRequest request)
        {
            var response = new AddMPNumberToPnrResponse();
            Session session = null;
            try
            {
                if (string.IsNullOrEmpty(request.SessionId))
                {
                    _logger.LogInformation("SessionId is Null from clientRequest");
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                response.SessionId = request.SessionId;
                var reservationdetail = new ReservationDetail();
                var cslReservation = await _sessionHelperService.GetSession<ReservationDetail>(request.SessionId, reservationdetail.GetType().FullName, new List<string> { request.SessionId, reservationdetail.GetType().FullName }).ConfigureAwait(false);

                List<United.Service.Presentation.ReservationModel.Traveler> travelerInfoMPRequest = AddMPNumberToPnrInfoRequest(request, cslReservation.Detail);
                string jsonRequest = JsonConvert.SerializeObject(travelerInfoMPRequest);

                await AddMPNumberToPnrInfoV2(response, request, jsonRequest, cslReservation);

                string[] dNWithoutAirportCode = cslReservation?.Detail?.FlightSegments?.LastOrDefault()?.FlightSegment?.ArrivalAirport?.Name?.Split('(');
                string AddMpNumberToPnrSuccessContent = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("AddMpNumberToPnrSuccessContent");
                AddMpNumberToPnrSuccessContent = AddMpNumberToPnrSuccessContent.Replace("[Destination]", dNWithoutAirportCode[0]);
                if (response?.AlertInfo == null)
                {
                    response.Title = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("Title");
                    response.Header = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("AddMpNumberToPnrSuccessHeader");
                    response.Description = AddMpNumberToPnrSuccessContent;
                    response.ViewReservationDetailsButtonText = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("ViewReservationDetailsButtonText");
                }
                if (request?.Traveler?.Count >= 2)
                {
                    response.Description = response.Description.Replace("number", "numbers");
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("AddMPNumberToPnr error {@UnitedException}, {@stackTrack}", uaex.Message, uaex.StackTrace);

                if (uaex != null && !string.IsNullOrEmpty(uaex.Message.Trim()))
                {
                    response.Exception = new MOBException
                    {
                        Message = uaex.Message
                    };
                }
                else
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
            }
            catch (System.Exception ex)
            {
                if (session == null || string.IsNullOrEmpty(request.SessionId))
                {
                    session = new Session
                    {
                        SessionId = request.TransactionId + "_" + request.PNR
                    };
                }

                MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);
                _logger.LogError("AddMPNumberToPnr error {@errormessage} {@PNR}", exceptionWrapper, request?.PNR);

                if (!_configuration.GetValue<bool>("SurfaceErrorToClient"))
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    response.Exception = new MOBException("9999", "SessionId :" + request.SessionId + ex.Message);
                }
                response.AlertInfo = new Alert();
                response.AlertInfo.Title = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("AddMpNumberToPnrErrorHeader");
                response.AlertInfo.Content = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("AddMpNumberToPnrErrorContent");
            }
            return response;
        }

        private async Task<bool> ValidateAddMpToPnrEligibility(MPSearchRequest request, Service.Presentation.ReservationModel.Reservation reservationdetail)
        {
            try
            {
                int profileSearchCount = 0;
                /*
                1. TCD email, and DOB that matches one existing MileagePlus account =>done
                2. customer has the same name using the matching logic from SHARES  =>done
                3. email is verified on the MileagePlus account  =>done
                4. itinerary is a revenue or award ticket =>done
                5.  itinerary is not a group booking  =>done
                6. itinerary is not a Space Available or Positive Space ticket =>done
                7. traveler is not an extra seat  =>done
                8. traveler is not an infant on lap =>done

                9. if doesn't have a TCD email, then traveler has a payment email,  (Pending from PNR team)
                 */
                var isGroupBooking = reservationdetail?.FlightSegments?.FirstOrDefault().FlightSegment?.IsGroupBooking;
                if (isGroupBooking != "False")
                {
                    return false;
                }
                if (reservationdetail?.Prices != null)
                {
                    Collection<Service.Presentation.PriceModel.Price> prices = reservationdetail?.Prices;
                    var fareTypeRev = prices.Any(p => p.FareType == Service.Presentation.CommonEnumModel.FareType.Revenue);
                    var fareTypeAw = prices.Any(p => p.FareType == Service.Presentation.CommonEnumModel.FareType.Award);

                    if (fareTypeRev || fareTypeAw)
                    {
                        if (reservationdetail?.Travelers?.Count > 0)
                        {
                            bool isPassrider = Convert.ToBoolean(reservationdetail?.BookingIndicators?.IsPassrider);
                            foreach (var currentTraveler in reservationdetail?.Travelers)
                            {

                                bool currIsSAOrPS = false;
                                if (currentTraveler.Tickets != null && isPassrider)
                                {
                                    var currentIsSAOrPS = currentTraveler.Tickets.FirstOrDefault().Characteristic.Any(s => s.Code == "PassengerIndicator"
                                    && (s.Value == "SA" || s.Value == "PS"));
                                    if (currentIsSAOrPS)
                                    {
                                        currIsSAOrPS = true;
                                    }
                                }

                                string initialChars = "";
                                string firstName = currentTraveler?.Person?.GivenName;
                                initialChars = (firstName?.Length > 4) ? firstName.Substring(0, 4) : "";
                                string loyaltyProgramMemberID = currentTraveler?.LoyaltyProgramProfile?.LoyaltyProgramMemberID;
                                if (string.IsNullOrEmpty(loyaltyProgramMemberID) && currentTraveler?.Person?.Type != "INF" && initialChars != "EXST" && !currIsSAOrPS)
                                {
                                    var searchMemberRequest = BuildMemberSearchRequest(request, currentTraveler);
                                    var token = await _tokenService.GetAnonymousToken(request.Application.Id, request.DeviceId, _configuration).ConfigureAwait(false);
                                    var resultMemberProfile = await _memberProfileService.SearchMemberInfo<SearchMemberInfoResponse>(token, JsonConvert.SerializeObject(searchMemberRequest), request.SessionId).ConfigureAwait(false);

                                    if (!(resultMemberProfile?.Data?.SearchCount >= 2))
                                    {
                                        string mpNumber = resultMemberProfile?.Data?.SearchMembers?.FirstOrDefault().LoyaltyId;
                                        if (!string.IsNullOrEmpty(mpNumber))
                                        {
                                            var emailAddressTask = await _ucbProfileService.GetEmail(token, mpNumber);

                                            var emailAddressResponse = JsonConvert.DeserializeObject<CslResponse<CslEmailData>>(emailAddressTask);
                                            if (emailAddressResponse.Errors != null && emailAddressResponse.Errors.Any())
                                            {
                                                var error = emailAddressResponse.Errors.ToList().FirstOrDefault();
                                                _logger.LogError("AddMPNumberToPnr-GetEmailVerification Failed {@Errors} and {mileagePlusNumber}", emailAddressResponse.Errors, mpNumber);
                                            }
                                            else
                                            {

                                                if (emailAddressResponse?.Data != null && emailAddressResponse.Data.Emails.Any())
                                                {
                                                    bool isEmailVerified = emailAddressResponse.Data.Emails.Any(x => x.IsVerified);
                                                    _logger.LogInformation("AddMPNumberToPnr-isEmailVerified  {@emailVerificationResponse} and {isEmailVerified}", emailAddressResponse, isEmailVerified);

                                                    if (isEmailVerified)
                                                    {
                                                        profileSearchCount = profileSearchCount + 1;
                                                    }

                                                }
                                            }

                                        }
                                    }
                                }

                            }
                            if (profileSearchCount > 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);
                _logger.LogError("AddMPNumberToPnr ValidateAddMpToPnrEligibility error {@errormessage}", exceptionWrapper);

            }
            return false;
        }

        private SeachMemberInfoRequest BuildMemberSearchRequest(MPSearchRequest request, Service.Presentation.ReservationModel.Traveler currentTraveler)
        {
            SeachMemberInfoRequest memberSearchRequest = new SeachMemberInfoRequest();
            string travelerEmail = "";
            if (currentTraveler?.Person?.Contact?.Emails?.Count > 0)
            {
                travelerEmail = currentTraveler?.Person?.Contact?.Emails?.FirstOrDefault().Address;
            }
            memberSearchRequest.FirstName = currentTraveler?.Person?.GivenName;
            memberSearchRequest.LastName = currentTraveler?.Person?.Surname;
            DateTime DateOfBirth = Convert.ToDateTime(currentTraveler?.Person?.DateOfBirth);
            memberSearchRequest.DateOfBirth = DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            memberSearchRequest.EmailAddress = string.IsNullOrEmpty(request.EmailAddress) ? travelerEmail : request.EmailAddress;
            return memberSearchRequest;
        }

        private List<United.Service.Presentation.ReservationModel.Traveler> AddMPNumberToPnrInfoRequest(AddMPNumberToPnrRequest request, Service.Presentation.ReservationModel.Reservation reservationdetail)
        {
            List<United.Service.Presentation.ReservationModel.Traveler>
                _TravellerList = new List<United.Service.Presentation.ReservationModel.Traveler>();
            //Collection<Service.Presentation.ReservationModel.Traveler> csltravelers = reservationdetail?.Travelers;

            request?.Traveler?.ForEach(traveler =>
            {
                Service.Presentation.PersonModel.Person person;
                LoyaltyProgramProfile loyaltyProgramProfile;
                if (!string.IsNullOrEmpty(traveler?.Person?.MileagePlusNumber))
                {
                    person = new Service.Presentation.PersonModel.Person
                    {
                        GivenName = traveler?.Person?.GivenName,
                        Surname = traveler?.Person?.LastName,
                        InfantIndicator = "FALSE",
                        Key = traveler?.Person?.SharesPosition
                    };

                    loyaltyProgramProfile = new LoyaltyProgramProfile
                    {
                        LoyaltyProgramCarrierCode = "UA",
                        LoyaltyProgramMemberID = traveler?.Person?.MileagePlusNumber,
                    };

                    _TravellerList.Add(new United.Service.Presentation.ReservationModel.Traveler
                    {
                        Person = person,
                        LoyaltyProgramProfile = loyaltyProgramProfile
                    });
                }
            });

            return _TravellerList;
        }

        private async Task<List<TravelerInformation>> SearchMemberProfiles(MPSearchRequest request, Service.Presentation.ReservationModel.Reservation reservationdetail)
        {
            var response = new List<TravelerInformation>();
            try
            {
                var isGroupBooking = reservationdetail?.FlightSegments?.FirstOrDefault().FlightSegment?.IsGroupBooking;

                if (isGroupBooking != "False")
                {
                    return response;
                }
                if (reservationdetail?.Prices != null)
                {
                    Collection<Service.Presentation.PriceModel.Price> prices = reservationdetail?.Prices;
                    var fareTypeRev = prices.Any(p => p.FareType == Service.Presentation.CommonEnumModel.FareType.Revenue);
                    var fareTypeAw = prices.Any(p => p.FareType == Service.Presentation.CommonEnumModel.FareType.Award);

                    if (fareTypeRev || fareTypeAw)
                    {
                        if (reservationdetail?.Travelers?.Count > 0)
                        {
                            foreach (var currentTraveler in reservationdetail?.Travelers)
                            {
                                bool isPassrider = Convert.ToBoolean(reservationdetail?.BookingIndicators?.IsPassrider);
                                bool currIsSAOrPS = false;
                                if (currentTraveler.Tickets != null && isPassrider)
                                {
                                    var currentIsSAOrPS = currentTraveler.Tickets.FirstOrDefault().Characteristic.Any(s => s.Code == "PassengerIndicator"
                                    && (s.Value == "SA" || s.Value == "PS"));
                                    if (currentIsSAOrPS)
                                    {
                                        currIsSAOrPS = true;
                                    }
                                }
                                string isInfant = currentTraveler?.Person?.Type;
                                string firstName = currentTraveler?.Person?.GivenName;
                                string initialChars = "";
                                initialChars = (firstName?.Length > 4) ? firstName.Substring(0, 4) : "";
                                string loyaltyProgramMemberID = currentTraveler?.LoyaltyProgramProfile?.LoyaltyProgramMemberID;

                                if (string.IsNullOrEmpty(loyaltyProgramMemberID) && isInfant != "INF" && initialChars != "EXST" && !currIsSAOrPS)
                                {
                                    var searchMemberRequest = BuildMemberSearchRequest(request, currentTraveler);
                                    var token = await _tokenService.GetAnonymousToken(request.Application.Id, request.DeviceId, _configuration).ConfigureAwait(false);
                                    var resultMemberProfile = await _memberProfileService.SearchMemberInfo<SearchMemberInfoResponse>(token, JsonConvert.SerializeObject(searchMemberRequest), request.SessionId).ConfigureAwait(false);

                                    if (resultMemberProfile?.Data?.SearchCount == 1)
                                    {
                                        string mpNumber = resultMemberProfile?.Data?.SearchMembers?.FirstOrDefault().LoyaltyId;
                                        var emailAddressTask = await _ucbProfileService.GetEmail(token, mpNumber);
                                        var emailAddressResponse = JsonConvert.DeserializeObject<CslResponse<CslEmailData>>(emailAddressTask);
                                        if (emailAddressResponse.Errors != null && emailAddressResponse.Errors.Any())
                                        {
                                            var error = emailAddressResponse.Errors.ToList().FirstOrDefault();
                                            _logger.LogError("GetEmail cslService error {@Errors} and {@mileagePlusNumber}", emailAddressResponse.Errors, mpNumber);
                                        }
                                        else
                                        {
                                            if (emailAddressResponse?.Data != null && emailAddressResponse.Data.Emails.Any())
                                            {
                                                bool isEmailVerified = emailAddressResponse.Data.Emails.Any(x => x.IsVerified);
                                                if (isEmailVerified)
                                                {
                                                    int mpNumLength = 0;
                                                    mpNumLength = (!string.IsNullOrEmpty(mpNumber)) ? mpNumber.Length : 0;
                                                    response.Add(new TravelerInformation
                                                    {
                                                        MileagePlusNumber = mpNumber,
                                                        TravelerName = resultMemberProfile?.Data?.SearchMembers?.FirstOrDefault().FirstName + " " + resultMemberProfile?.Data?.SearchMembers?.FirstOrDefault().LastName,
                                                        MaskMileagePlusNumber = mpNumber.Mask(0, mpNumLength - 3),
                                                        SharesPosition = currentTraveler?.Person?.Key
                                                    });
                                                }

                                            }
                                        }

                                    }

                                }

                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);
                _logger.LogError("UpdateTravelerInfo error {@errormessage}", exceptionWrapper);
            }
            return response;
        }

        private async Task<AddMPNumberToPnrResponse> AddMPNumberToPnrInfoV2(AddMPNumberToPnrResponse response, AddMPNumberToPnrRequest request, string cslJsonRequest, ReservationDetail cslReservation)
        {
            var urlLoyaltyId = string.Empty;
            var jsonResponseLoyaltyId = string.Empty;

            try
            {
                var token = await _tokenService.GetAnonymousToken(request.Application.Id, request.DeviceId, _configuration).ConfigureAwait(false);
                urlLoyaltyId = string.Format("/{0}/Passengers/Loyalty?RetrievePNR=true&EndTransaction=true", request.PNR);
                jsonResponseLoyaltyId = await _pNRRetrievalService.UpdateTravelerInfo(token, cslJsonRequest, urlLoyaltyId, request.SessionId).ConfigureAwait(false);

                //If Response NOT-NULL Check for MP Number / FREQ Flyer Number 
                if (!string.IsNullOrEmpty(jsonResponseLoyaltyId))
                {
                    List<United.Service.Presentation.CommonModel.Message>
                       msgResponseLoyaltyId = JsonConvert.DeserializeObject<List<United.Service.Presentation.CommonModel.Message>>(jsonResponseLoyaltyId);

                    if (msgResponseLoyaltyId?[0].Type.ToLower() == "failed")
                    {
                        response.AlertInfo = new Alert();
                        response.AlertInfo.Title = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("AddMpNumberToPnrErrorHeader");
                        response.AlertInfo.Content = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("AddMpNumberToPnrErrorContent");
                    }

                    if (msgResponseLoyaltyId != null)
                    {
                        foreach (var reqTraverler in request?.Traveler)
                        {
                            var travelerInfo = cslReservation?.Detail?.Travelers?.FirstOrDefault(x => string.Equals(x.Person?.Key, reqTraverler.Person?.SharesPosition, StringComparison.OrdinalIgnoreCase) == true);
                            for (int i = 0; i < msgResponseLoyaltyId.Count; i++)
                            {
                                if (string.Equals(msgResponseLoyaltyId[i].Status, "SHARE_RESPONSE", StringComparison.OrdinalIgnoreCase) && msgResponseLoyaltyId[i].Text.IndexOf(reqTraverler?.Person?.MileagePlusNumber) != -1)
                                {
                                    travelerInfo.LoyaltyProgramProfile = new LoyaltyProgramProfile()
                                    {
                                        LoyaltyProgramMemberID = reqTraverler?.Person?.MileagePlusNumber
                                    };
                                    cslReservation.Detail.Travelers[cslReservation.Detail.Travelers.IndexOf(travelerInfo)] = travelerInfo;

                                }
                            }
                        }

                        await _sessionHelperService.SaveSession<ReservationDetail>(cslReservation, request.SessionId, new List<string> { request.SessionId, new ReservationDetail().GetType().FullName },
                            new ReservationDetail().GetType().FullName, 5400, _configuration.GetValue<bool>("SaveOneClickSessionToCouchbase")).ConfigureAwait(false);

                    }

                }
            }
            catch (Exception ex)
            {
                MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);
                _logger.LogError("AddMPNumberToPnr UpdateTravelerInfo error {@errormessage}", exceptionWrapper);

                response.AlertInfo = new Alert();
                response.AlertInfo.Title = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("AddMpNumberToPnrErrorHeader");
                response.AlertInfo.Content = _configuration.GetSection("frequentFlyerProgramConfig").GetValue<string>("AddMpNumberToPnrErrorContent");
            }
            return response;
        }

    }
}
