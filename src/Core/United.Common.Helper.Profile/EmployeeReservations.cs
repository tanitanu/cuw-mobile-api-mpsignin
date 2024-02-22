using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Common;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Common.DynamoDB;
using United.Mobile.Model.Internal.Exception;
using United.Mobile.Model.MPSignIn;
using United.Service.Presentation.PersonModel;
using United.Utility.Helper;
using United.Mobile.DataAccess.CSLSerivce;


namespace United.Common.Helper.EmployeeReservation
{
    public class EmployeeReservations : IEmployeeReservations
    {
        private readonly ICacheLog<EmployeeReservations> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly ICachingService _cachingService;
        private readonly IEmployeeProfileService _employeeProfileService;


        public EmployeeReservations(ICacheLog<EmployeeReservations> logger
            , IConfiguration configuration
            , IHeaders headers
            , ICachingService cachingService
            , IEmployeeProfileService employeeProfileService)
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _cachingService = cachingService;
            _employeeProfileService = employeeProfileService;
        }
        public MOBEmpTravelTypeAndJAProfileResponse GetTravelTypesAndJAProfile(MOBEmpTravelTypeAndJAProfileRequest request)
        {
            string strServerUpdateTimes = _configuration.GetValue<string>("empJAReloadTimes");
            int intWaitTime = _configuration.GetValue<int>("empJAReloadWaitMinutes");
            int intExpireCacheTime = _configuration.GetValue<int>("empJAReladExpireHours");
            MOBCacheDataResponse cacheData = new MOBCacheDataResponse();
            cacheData = GetCacheDataID(strServerUpdateTimes, intWaitTime, intExpireCacheTime, request.SessionId, request.DeviceId, request.MPNumber, request.Application.Id, request.Application.Version.Major, "MOBEmpTravelTypeAndJAProfileResponse");
            //Check if Data is current
            //Utility.InsertCacheData
            MOBEmpTravelTypeAndJAProfileResponse empTravelTypeAndJAProfileResponse = new MOBEmpTravelTypeAndJAProfileResponse();
            //empJAReladExpireHours
            if (!cacheData.BlnRefresh || (cacheData.CacheData != "" && cacheData.CacheData != null))
            {
                empTravelTypeAndJAProfileResponse = JsonConvert.DeserializeObject<MOBEmpTravelTypeAndJAProfileResponse>(cacheData.CacheData);
                //Hashing the employee ID for Stephen Copley's test account for his demo/video that he's making.
                //This account belongs to Gemma Egana and she is aware and agreed to let us use it for this purpose.
                if (!string.IsNullOrEmpty(_configuration.GetValue<string>("empHashEmployeeNumber")))
                {
                    bool blnHashEmployeeID = false;
                    if (_configuration.GetValue<bool>("empHashEmployeeNumber"))
                    {
                        blnHashEmployeeID = true;
                    }
                    if (blnHashEmployeeID)
                    {
                        if (!string.IsNullOrEmpty(_configuration.GetValue<string>("empEmployeeNumberToHash")))
                        {
                            string strEmployeeIDsToHash = _configuration.GetValue<string>("empEmployeeNumberToHash");
                            if (!string.IsNullOrEmpty(empTravelTypeAndJAProfileResponse.MOBEmpTravelTypeResponse.DisplayEmployeeId) && strEmployeeIDsToHash.Length > 0)
                            {
                                if (strEmployeeIDsToHash.Trim().ToUpper().Contains(request.MPNumber.Trim().ToUpper()))
                                {
                                    //empTravelTypeAndJAProfileResponse.DisplayEmployeeId = "U000000";
                                    empTravelTypeAndJAProfileResponse.MOBEmpTravelTypeResponse.DisplayEmployeeId = "U000000";
                                }
                            }
                        }
                    }
                }
            }
            if (cacheData.BlnRefresh)
            {
                try
                {
                    empTravelTypeAndJAProfileResponse.TransactionId = request.TransactionId;
                    MOBEmpTravelType empTravelType = new MOBEmpTravelType();
                    employeeRes.Models.BookingTypesResponse bookingTypes = new employeeRes.Models.BookingTypesResponse();

                    //to do
                    //Eres Wrapper Migration
                    //Starts Here
                    string url = string.Empty;
                    EmployeeRes.ClientProxy.EmployeeResProxyClient proxy;

                    if (!_configuration.GetValue<bool>("eResMigrationToggle"))
                    {
                        url = _configuration.GetValue<string>("eResApiURL").ToString().Trim();
                        proxy = new EmployeeRes.ClientProxy.EmployeeResProxyClient(url);
                        proxy.RequiredAudits = new employeeRes.Models.RequiredAudits
                        {
                            DeviceId = request.DeviceId,
                            MPNumber = request.MPNumber,
                            EmployeeId = request.EmployeeID
                        };
                    }
                    else
                    {
                        proxy = SetURLDpTokenToEresProxy(request.DeviceId, request.MPNumber, request.EmployeeID, request.TokenId);
                    }

                    bookingTypes = proxy.GetEmployeeEligibleBookingTypes(request.EmployeeID);

                    List<MOBEmpTravelTypeItem> empTravelTypeObjs = new List<MOBEmpTravelTypeItem>();
                    if (bookingTypes.BookingTypes != null && bookingTypes.BookingTypes.Count != 0)
                    {
                        foreach (var bookingType in bookingTypes.BookingTypes)
                        {

                            MOBEmpTravelTypeItem empTravelTypeObj = new MOBEmpTravelTypeItem
                            {
                                Advisory = "",
                                IsAuthorizationRequired = false,
                                IsEligible = true,
                                NumberOfTravelers = 1
                            };
                            if (bookingTypes.NumberOfPassengersInJA > 1)
                            {
                                empTravelTypeObj.NumberOfTravelers = bookingTypes.NumberOfPassengersInJA;
                            }

                            empTravelTypeObj.TravelType = bookingType.DisplayCode;

                            if (bookingType.DisplayCode == "RA")
                            {
                                bookingType.Display = _configuration.GetValue<string>("RevenueAwardUILabel");
                            }
                            else if (bookingType.DisplayCode == "E20")
                            {
                                bookingType.Display = _configuration.GetValue<string>("Employee20UILabel");
                            }
                            else if (bookingType.DisplayCode == "P")
                            {
                                bookingType.Display = _configuration.GetValue<string>("PersonalLeisureUILabel");
                            }
                            else if (bookingType.DisplayCode == "B")
                            {
                                if (bookingType.Display.ToLower().Trim().Contains("authorization"))
                                {
                                    empTravelTypeObj.IsAuthorizationRequired = true;
                                }
                                bookingType.Display = _configuration.GetValue<string>("BusinessUILabel");
                                if (empTravelTypeObj.IsAuthorizationRequired)
                                {
                                    bookingType.Display = _configuration.GetValue<string>("BusinessUILabelAuthRequired");
                                }
                                empTravelTypeObj.Advisory = bookingTypes.PositiveSpaceAlertMessage;
                            }
                            empTravelTypeObj.TravelTypeDescription = bookingType.Display;
                            empTravelTypeObjs.Add(empTravelTypeObj);
                        }
                        empTravelType.EmpTravelTypes = empTravelTypeObjs;

                        empTravelType.IsTermsAndConditionsAccepted = bookingTypes.IsTermsAndConditionsAccepted;
                        empTravelType.NumberOfPassengersInJA = bookingTypes.NumberOfPassengersInJA;

                        MOBEmpTravelTypeResponse mobEmpTravelTypeResponse = new MOBEmpTravelTypeResponse
                        {
                            EResTransactionId = bookingTypes.TransactionId
                        };
                        empTravelTypeAndJAProfileResponse.MOBEmpTravelTypeResponse = mobEmpTravelTypeResponse;
                        empTravelTypeAndJAProfileResponse.MOBEmpTravelTypeResponse.EmpTravelType = empTravelType;
                    }
                    else
                    {
                        throw new MOBUnitedException("BookingTypes are Empty");
                    }

                    empTravelTypeAndJAProfileResponse.MOBEmpJAResponse = LoadEmployeeJA(bookingTypes);
                    //JD - Populating IsPayrollDeduct from TravelElligibilities instead of GetEmployeeProfile (moved from MileagePlusCSLController)
                    if (bookingTypes.EmployeeJA != null && bookingTypes.EmployeeJA.Airlines != null && bookingTypes.EmployeeJA.Airlines.Count > 0)
                    {
                        foreach (var airline in bookingTypes.EmployeeJA.Airlines)
                        {
                            if (airline.PaymentDetails != null)
                            {
                                if (!string.IsNullOrEmpty(airline.PaymentDetails.PayrollDeduct) && (airline.PaymentDetails.PayrollDeduct.Trim().ToUpper() == "Y" || airline.PaymentDetails.PayrollDeduct.Trim().ToUpper() == "YES"))
                                {
                                    empTravelTypeAndJAProfileResponse.MOBEmpTravelTypeResponse.IsPayrollDeduct = true;
                                }
                            }
                        }
                    }
                    string commonRequestJSON = JsonConvert.SerializeObject(empTravelTypeAndJAProfileResponse);
                    MOBInsertCacheDataRequest mobRequest = new MOBInsertCacheDataRequest
                    {
                        IntID = cacheData.Id,
                        StrGUID = request.SessionId,
                        StrDeviceID = request.DeviceId,
                        StrMPNumber = request.MPNumber,
                        IntAppId = request.Application.Id,
                        StrAppVersion = request.Application.Version.Major,
                        StrCacheData = commonRequestJSON,
                        StrDataDescription = "MOBEmpTravelTypeAndJAProfileResponse"
                    };
                    InsertCacheData(mobRequest);
                }
                catch (System.Exception)
                {
                    //Do something or just let it fall through?
                }
            }
            return empTravelTypeAndJAProfileResponse;
        }

        private EmployeeRes.ClientProxy.EmployeeResProxyClient SetURLDpTokenToEresProxy(string deviceId, string mpNumber, string employeeId, string tokenId = "")
        {
            var url = _configuration.GetValue<string>("eResApiWrapperURL").ToString().Trim();
            var proxy = new EmployeeRes.ClientProxy.EmployeeResProxyClient(url);

            proxy.RequiredAudits = new employeeRes.Models.RequiredAudits
            {
                DeviceId = deviceId,
                MPNumber = mpNumber,
                EmployeeId = employeeId,
                Authorization = tokenId
            };

            return proxy;
        }

        public Task InsertCacheData(MOBInsertCacheDataRequest request)
        {
            try
            {
                var data = new CacheData();
                return _cachingService.SaveCache<CacheData>("TravelTypeCacheData", data, _headers.ContextValues.TransactionId, new System.TimeSpan(3, 0, 0, 0));
            }
            catch (System.Exception) { }
            return default;
        }
        public MOBEmpJAResponse LoadEmployeeJA(employeeRes.Models.BookingTypesResponse bookingTypes)
        {
            MOBEmpJA empJA = new MOBEmpJA();
            MOBEmpJAResponse empJAResponse = new MOBEmpJAResponse();
            MOBEmpPassRiderExtended empPassRiderExtended = new MOBEmpPassRiderExtended();
            MOBEmployeeProfileExtended empProfileExtended = new MOBEmployeeProfileExtended();

            List<MOBEmpBuddy> empBuddies = new List<MOBEmpBuddy>();
            List<MOBEmpPassRider> empPassRiders = new List<MOBEmpPassRider>();
            List<MOBEmpPassRider> empSuspendedPassRiders = new List<MOBEmpPassRider>();
            List<MOBEmpJAByAirline> empJAByAirlines = new List<MOBEmpJAByAirline>();

            List<MOBEmpBuddy> empLoggedInBuddies = new List<MOBEmpBuddy>();
            List<MOBEmpPassRider> empLoggedInPassRiders = new List<MOBEmpPassRider>();
            List<MOBEmpPassRider> empLoggedInSuspendedPassRiders = new List<MOBEmpPassRider>();
            List<MOBEmpJAByAirline> empLoggedInJAByAirlines = new List<MOBEmpJAByAirline>();

            empJAResponse.EmpJA = empJA;

            if (bookingTypes != null)
            {
                #region Buddies
                if (bookingTypes != null && bookingTypes.EmployeeJA != null && bookingTypes.EmployeeJA.Buddies != null && bookingTypes.EmployeeJA.Buddies.Count != 0)
                {
                    foreach (var buddy in bookingTypes.EmployeeJA.Buddies)
                    {
                        MOBEmpBuddy empBuddy = new MOBEmpBuddy
                        {
                            BirthDate = buddy.BirthDate,
                            Email = buddy.DayOfEmail,
                            Gender = buddy.Gender
                        };
                        empBuddy.Name.First = buddy.FirstName;
                        empBuddy.Name.Last = buddy.LastName;
                        empBuddy.Name.Middle = buddy.MiddleName;
                        empBuddy.Name.Suffix = buddy.NameSuffix;
                        empBuddy.OwnerCarrier = buddy.OwnerCarrier;
                        empBuddy.Phone = buddy.DayOfPhone;
                        empBuddy.Redress = buddy.Redress;
                        empBuddy.EmpRelationship.Relationship = buddy.Relationship.Relationship;
                        empBuddy.EmpRelationship.RelationshipDescription = buddy.Relationship.RelationshipDescription;
                        empBuddy.EmpRelationship.RelationshipSubType = buddy.Relationship.RelationshipSubType;
                        empBuddy.EmpRelationship.RelationshipSubTypeDescription = buddy.Relationship.RelationshipSubTypeDescription;
                        empBuddies.Add(empBuddy);
                    }
                    empJAResponse.EmpJA.EmpBuddies = empBuddies;
                }
                #endregion

                #region PassRiders
                if (bookingTypes != null && bookingTypes.EmployeeJA != null && bookingTypes.EmployeeJA.PassRiders != null && bookingTypes.EmployeeJA.PassRiders.Count != 0)
                {
                    foreach (var passrider in bookingTypes.EmployeeJA.PassRiders)
                    {
                        MOBEmpPassRider empPassRider = new MOBEmpPassRider
                        {
                            Age = passrider.Age,
                            BirthDate = passrider.BirthDate.ToString(),
                            DependantID = passrider.DependantID,

                            FirstBookingBuckets = passrider.FirstBookingBuckets,
                            Gender = passrider.Gender,
                            MustUseCurrentYearPasses = passrider.MustUseCurrentYearPasses
                        };

                        empPassRiders.Add(empPassRider);
                    }
                    empJAResponse.EmpJA.EmpPassRiders = empPassRiders;
                }
                #endregion

                #region Suspended PassRiders
                if (bookingTypes != null && bookingTypes.EmployeeJA != null && bookingTypes.EmployeeJA.SuspendedPassRiders != null && bookingTypes.EmployeeJA.SuspendedPassRiders.Count != 0)
                {
                    foreach (var spassrider in bookingTypes.EmployeeJA.SuspendedPassRiders)
                    {
                        MOBEmpPassRider empSuspendedPassRider = new MOBEmpPassRider
                        {
                            Age = spassrider.Age,
                            BirthDate = spassrider.BirthDate.ToString(),
                            DependantID = spassrider.DependantID,
                            FirstBookingBuckets = spassrider.FirstBookingBuckets,
                            Gender = spassrider.Gender,
                            MustUseCurrentYearPasses = spassrider.MustUseCurrentYearPasses,
                            PrimaryFriend = spassrider.PrimaryFriend,
                            UnaccompaniedFirst = spassrider.UnaccompaniedFirst
                        };
                        empSuspendedPassRiders.Add(empSuspendedPassRider);
                    }
                    empJAResponse.EmpJA.EmpSuspendedPassRiders = empSuspendedPassRiders;
                }
                #endregion

                #region Airlines
                if (bookingTypes.EmployeeJA.Airlines.Count != 0)
                {
                    foreach (var airline in bookingTypes.EmployeeJA.Airlines)
                    {
                        MOBEmpJAByAirline empJAByAirline = new MOBEmpJAByAirline();
                        empJAByAirline.AirlineCode = airline.AirlineCode;
                        empJAByAirline.AirlineDescription = airline.AirlineDescription;
                        empJAByAirline.BoardDate = airline.BoardDate;
                        empJAByAirline.BuddyPassClass = airline.BuddyPassClass;
                        empJAByAirline.BusinessPassClass = airline.BusinessPassClass;
                        empJAByAirline.CanBookFirstOnBusiness = airline.CanBookFirstOnBusiness;
                        empJAByAirline.DeviationPassClass = airline.DeviationPassClass;
                        empJAByAirline.Display = airline.Display;
                        empJAByAirline.ETicketIndicator = airline.ETicketIndicator;
                        empJAByAirline.ExtendedFamilyPassClass = airline.ExtendedFamilyPassClass;
                        empJAByAirline.FamilyPassClass = airline.FamilyPassClass;
                        empJAByAirline.FamilyVacationPassClass = airline.FamilyVacationPassClass;
                        empJAByAirline.FeeWaivedCoach = airline.FeeWaivedCoach;
                        empJAByAirline.FeeWaivedFirst = airline.FeeWaivedFirst;
                        empJAByAirline.JumpSeatPassClass = airline.JumpSeatPassClass;
                        empJAByAirline.PaymentIndicator = airline.PaymentIndicator;
                        empJAByAirline.PersonalPassClass = airline.PersonalPassClass;
                        empJAByAirline.ScheduleEngineCode = airline.ScheduleEngineCode;
                        empJAByAirline.Seniority = airline.Seniority;
                        empJAByAirline.SeniorityDate = airline.SeniorityDate;
                        empJAByAirline.SuspendEndDate = airline.SuspendEndDate;
                        empJAByAirline.SuspendStartDate = airline.SuspendStartDate;
                        empJAByAirline.TrainingPassClass = airline.TrainingPassClass;
                        empJAByAirline.VacationPassClass = airline.VacationPassClass;
                        empJAByAirlines.Add(empJAByAirline);

                    }
                    empJAResponse.EmpJA.EmpJAByAirlines = empJAByAirlines;
                }
                #endregion

                #region EmployeeProfileExtended
                if (bookingTypes.EmployeeProfile != null && bookingTypes.EmployeeProfile.ExtendedProfile != null)
                {

                    empProfileExtended.Email = bookingTypes.EmployeeProfile.ExtendedProfile.Email;
                    empProfileExtended.HomePhone = bookingTypes.EmployeeProfile.ExtendedProfile.HomePhone;
                    empProfileExtended.FaxNumber = bookingTypes.EmployeeProfile.ExtendedProfile.FaxNumber;
                    empProfileExtended.WorkPhone = bookingTypes.EmployeeProfile.ExtendedProfile.WorkPhone;

                    empJAResponse.EmpProfileExtended = empProfileExtended;
                }
                #endregion
                empJAResponse.AllowImpersonation = bookingTypes.AllowImpersonation;
                empJAResponse.ImpersonateType = bookingTypes.ImpersonateType;
                empJAResponse.LanguageCode = bookingTypes.LanguageCode;
                empJAResponse.MachineName = bookingTypes.MachineName;
                empJAResponse.TransactionId = bookingTypes.TransactionId;

            }
            else
            {
                _logger.LogError("eRes - GetEmployeeJA {Response}", empJAResponse.Exception);
            }

            return empJAResponse;
        }

        public MOBCacheDataResponse GetCacheDataID(string strServerUpdateTimes, int intWaitTime, int intExpireTime, string strGUID, string strDeviceID, string strMPNumber, int intAppId, string strAppVersion, string strDataDescription)
        {
            MOBCacheDataResponse mobReturn = new MOBCacheDataResponse
            {
                Id = 0,
                BlnRefresh = true
            };

            try
            {
                DateTime dteLastServerUpdate = System.DateTime.Now;
                var data = new CacheData();
                mobReturn.CacheData = _cachingService.SaveCache<CacheData>("TravelTypeCacheData", data, _headers.ContextValues.TransactionId, new System.TimeSpan(3, 0, 0, 0)).ToString();

                if (strServerUpdateTimes != null && strServerUpdateTimes.Length > 0)
                {
                    string[] arrStrings = strServerUpdateTimes.Split('|');
                    List<DateTime> lstDates = new List<DateTime>();
                    DateTime dteToday = System.DateTime.Now;
                    string strDate = "";
                    DateTime dteActive = dteToday;
                    if (arrStrings.Length > 0)
                    {
                        int intUpdateTimesCount = arrStrings.Length + 1;
                        for (int i = 0; i < intUpdateTimesCount; i++)
                        {
                            if (i < intUpdateTimesCount - 1)
                            {
                                strDate = dteActive.Month.ToString() + "/" + dteActive.Day.ToString() + "/" + dteActive.Year.ToString() + " " + arrStrings[i];
                                lstDates.Add(Convert.ToDateTime(strDate));
                            }
                            else
                            {
                                dteActive = dteToday.AddDays(-1);
                                strDate = dteActive.Month.ToString() + "/" + dteActive.Day.ToString() + "/" + dteActive.Year.ToString() + " " + arrStrings[0];
                                lstDates.Add(Convert.ToDateTime(strDate));
                            }
                        }
                        if (lstDates != null && lstDates.Count > 0)
                        {
                            for (int x = 0; x < lstDates.Count; x++)
                            {
                                if (System.DateTime.Now > lstDates[x])
                                {
                                    dteLastServerUpdate = lstDates[x];
                                    if (mobReturn.LastUpdateDateTime > dteLastServerUpdate.AddMinutes(intWaitTime))
                                    {
                                        mobReturn.BlnRefresh = false;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception) { }

            return mobReturn;
        }

        public async Task<EmployeeTravelProfile> GetEmployeeProfile(int applicationId, string applicationVersion, string deviceId
           , string employeeId, string token, string sessionId)
        {
            try
            {
                string jsonResponse = await _employeeProfileService.GetEmployeeProfile(token, employeeId, sessionId).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    return JsonConvert.DeserializeObject<EmployeeTravelProfile>(jsonResponse);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError("eRes - GetEmployeeProfile {Exception}", ex.ToString());
            }

            return default(EmployeeTravelProfile);
        }
    }
}
