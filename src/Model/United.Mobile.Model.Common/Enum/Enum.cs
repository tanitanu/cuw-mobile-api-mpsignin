using System;
using System.Runtime.Serialization;

namespace United.Mobile.Model.Common.Enum
{
    public enum TravelType
    {
        RA,//Revenue Booking/AwardTravel
        YA,//Young adult
        CLB,//Coroporateleisurebooking
        CB,
        D, // eRes Deviation Travel (Authorization Required)
        T, // eRes Training Travel (Authorization Required)
        E, // eRes Emergency Travel
        TPSearch = MOBTripPlannerType.TPSearch,
        TPBooking = MOBTripPlannerType.TPBooking
    }

    public enum MOBTripPlannerType
    {
        TPSearch,
        TPBooking
    }
    [Serializable()]
    public enum MOBMemberType
    {
        [EnumMember(Value = "0")]
        GENERAL,
        [EnumMember(Value = "")]
        None = GENERAL,
        [EnumMember(Value = "1")]
        PremierSilver,
        [EnumMember(Value = "2")]
        PremierGold,
        [EnumMember(Value = "3")]
        PremierPlatinium,
        [EnumMember(Value = "4")]
        Premier1K,
        [EnumMember(Value = "5")]
        PremierGlobalServices,
        [EnumMember(Value = "-1")]
        CHairManCircle
    }
}
