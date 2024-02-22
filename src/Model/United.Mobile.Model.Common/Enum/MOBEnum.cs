using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public enum MOBSHOPResponseStatus
    {
        [EnumMember(Value = "1")]
        ReshopUnableToComplete,
        [EnumMember(Value = "2")]
        ReshopChangePending,
        [EnumMember(Value = "3")]
        ReshopBENonElgible,
        [EnumMember(Value = "4")]
        ReshopUnableToChange,
        [EnumMember(Value = "5")]
        PcuUpgradeFailed,
        [EnumMember(Value = "6")]
        FailedToGetBagChargeInfo,
        [EnumMember(Value = "7")]
        ReshopAgencyCheckinEligible,
        [EnumMember(Value = "8")]
        ReshopCheckinEligible,
        [EnumMember(Value = "9")]
        ReshopChangeOfferBEBuyOut,
        [EnumMember(Value = "10")]
        ReshopOTFShopEligible,
    }


    [Serializable]
    public enum MOBMPErrorScreenType
    {
        [EnumMember(Value = "None")]
        None,
        [EnumMember(Value = "Common")]
        Common,
        [EnumMember(Value = "AccountNotFound")]
        AccountNotFound,
        [EnumMember(Value = "Duplicate")]
        Duplicate,
        [EnumMember(Value = "MultipleAccount")]
        MultipleAccount,
        [EnumMember(Value = "UnableToReset")]
        UnableToReset,
        [EnumMember(Value = "ResendActivationEmail")]
        ResendActivationEmail
    }


    [Serializable]
    public enum MOBMPSecurityUpdatePath
    {
        [EnumMember(Value = "None")]
        None,
        [EnumMember(Value = "VerifyPrimaryEmail")]
        VerifyPrimaryEmail,
        [EnumMember(Value = "NoPrimayEmailExist")]
        NoPrimayEmailExist,
        [EnumMember(Value = "UpdatePassword")]
        UpdatePassword,
        [EnumMember(Value = "UpdateSecurityQuestions")]
        UpdateSecurityQuestions,
        [EnumMember(Value = "SignInBackWithNewPassWord")]
        SignInBackWithNewPassWord,
        [EnumMember(Value = "ForgotMileagePlusNumber")]
        ForgotMileagePlusNumber,
        [EnumMember(Value = "ForgotMPPassWord")]
        ForgotMPPassWord,
        [EnumMember(Value = "ValidateSecurityQuestions")]
        ValidateSecurityQuestions,
        [EnumMember(Value = "IncorrectSecurityQuestion")]
        IncorrectSecurityQuestion,
        [EnumMember(Value = "IncorrectUserDetails")]
        IncorrectUserDetails,
        [EnumMember(Value = "UnableToResetOnline")]
        UnableToResetOnline,
        [EnumMember(Value = "AccountLocked")]
        AccountLocked,
        [EnumMember(Value = "MultipleAccount")]
        MultipleAccount,
        [EnumMember(Value = "ValidateTFASecurityQuestions")]
        ValidateTFASecurityQuestions,
        [EnumMember(Value = "TFAAccountLocked")]
        TFAAccountLocked,
        [EnumMember(Value = "TFAForgotPasswordEmail")]
        TFAForgotPasswordEmail,
        [EnumMember(Value = "TFAAccountResetEmail")]
        TFAAccountResetEmail,
        [EnumMember(Value = "TFAInvalidAccountResetEmail")]
        TFAInvalidAccountResetEmail,
        [EnumMember(Value = "RevenueBookingPath")]
        RevenueBookingPath
        // when forceSignOut is true for update later is disabled at ValidateMPSignIn() as its time to update the Security Data and will be forced to update data to move forward (As of now here too other than Revenue Booking) - 
        // - than after update password success then need to force client to sign in back with re-enter the new password and sign in.
        //“VerifyPrimaryEmail” means need to verify saved primary email
        //“NoPrimayEmailExist” means no primary email exists
        //“UpdatePassword” means a valid password does not exist
        //“UpdateSecurityQuestions” means the 5 security questions and answers do not exist
    }

    [Serializable]
    public enum MOBMPSignInPath
    {
        [EnumMember(Value = "None")]
        None,
        [EnumMember(Value = "")]
        General = None,
        [EnumMember(Value = "MyAccountPath")]
        MyAccountPath,
        [EnumMember(Value = "AwardTBookingPath")]
        AwardTBookingPath,
        [EnumMember(Value = "RevenueBookingPath")]
        RevenueBookingPath,
        [EnumMember(Value = "CorporateBookingPath")]
        CorporateBookingPath,
        [EnumMember(Value = "CorporateChangePath")]
        CorporateChangePath,
        [EnumMember(Value = "logInAncillary")]
        logInAncillary,
        [EnumMember(Value = "HomeScreenLogInStandby")]
        HomeScreenLogInStandby,
        [EnumMember(Value = "HomeScreenLogInDismiss")]
        HomeScreenLogInDismiss,
        [EnumMember(Value = "homeScreenLoginSoftSignedInChaseSSO")]
        homeScreenLoginSoftSignedInChaseSSO,
        [EnumMember(Value = "HomeScreenLoginSweeptake")]
        HomeScreenLoginSweeptake,
        [EnumMember(Value = "BusinessTravelBookingPath")]
        BusinessTravelBookingPath,
        [EnumMember(Value = "ShopByMapPath")]
        ShopByMapPath,
        [EnumMember(Value = "PersonalTravelBookingPath")]
        PersonalTravelBookingPath,
        [EnumMember(Value = "EmpDiscountReshopNav")]
        EmpDiscountReshopNav,
        [EnumMember(Value = "MyAccountSummary")]
        MyAccountSummary,
        [EnumMember(Value = "CorporateLeisureBookingPath")]
        CorporateLeisureBookingPath,
        [EnumMember(Value = "ReshopChangeRewardsNav")]
        ReshopChangeRewardsNav,
        [EnumMember(Value = "ReshopCancelRewardsNav")]
        ReshopCancelRewardsNav,
        [EnumMember(Value = "YoungAdultBookingPath")]
        YoungAdultBookingPath,
        [EnumMember(Value = "UpgradeCabinWebSSORedirect")]
        UpgradeCabinWebSSORedirect,
        [EnumMember(Value = "EmpResBookingPath")]
        EmpResBookingPath,
        [EnumMember(Value = "PostBooking")]
        PostBooking,
        [EnumMember(Value = "BusinessTravelEmergencyBookingPath")]
        BusinessTravelEmergencyBookingPath,
        [EnumMember(Value = "BusinessTravelDeviationBookingPath")]
        BusinessTravelDeviationBookingPath,
        [EnumMember(Value = "BusinessTravelTrainingBookingPath")]
        BusinessTravelTrainingBookingPath,
        [EnumMember(Value = "couponingBookingPath")]
        couponingBookingPath,
        [EnumMember(Value = "EditSearchPath")]
        EditSearchPath,
        [EnumMember(Value = "businessCQtrainingTravel")]
        businessCQtrainingTravel,
        [EnumMember(Value = "businessNonCQTrainingTravel")]
        businessNonCQTrainingTravel,
        [EnumMember(Value = "TrainingTravelCQMain")]
        TrainingTravelCQMain,
        [EnumMember(Value = "LogInPSSACancel")]
        LogInPSSACancel,
        [EnumMember(Value = "LogInPSSAStandby")]
        LogInPSSAStandby,
        [EnumMember(Value = "LogInAwardCancel")]
        LogInAwardCancel,
        [EnumMember(Value = "LogInCorp")]
        LogInCorp,
        [EnumMember(Value = "LogInMyTripSSO")]
        LogInMyTripSSO,
        [EnumMember(Value = "LogInUpgradeSSO")]
        LogInUpgradeSSO,
        [EnumMember(Value = "LogInUpgradeStoreFront")]
        LogInUpgradeStoreFront,
        [EnumMember(Value = "LogInUpgradeStoreFrontAsGuest")]
        LogInUpgradeStoreFrontAsGuest,
        [EnumMember(Value = "LogInUpgradeStoreFrontBalanceInfo")]
        LogInUpgradeStoreFrontBalanceInfo,
        [EnumMember(Value = "LogInUpgradeReview")]
        LogInUpgradeReview,
        [EnumMember(Value = "LogInWebSSORedirect")]
        LogInWebSSORedirect,
        [EnumMember(Value = "LogInCancelSSORedirect")]
        LogInCancelSSORedirect,
        [EnumMember(Value = "LogInReshopSameDayChange")]
        LogInReshopSameDayChange,
        [EnumMember(Value = "Dismiss")]
        Dismiss,
        [EnumMember(Value = "ContinueAsGuest")]
        ContinueAsGuest,
        [EnumMember(Value = "LoggedIn")]
        LoggedIn,
        [EnumMember(Value = "HomeScreenLoginBookaCar")]
        HomeScreenLoginBookaCar,
        [EnumMember(Value = "EmpDiscountBookingNav")]
        EmpDiscountBookingNav,
        [EnumMember(Value = "MoneyPlusMilesBookingPath")]
        MoneyPlusMilesBookingPath,
        [EnumMember(Value = "TravelCreditsBookingPath")]
        TravelCreditsBookingPath
    }

    [Serializable()]
    public enum PBSegmentType
    {
        [EnumMember(Value = "0")]
        Regular = 0,
        [EnumMember(Value = "1")]
        AlreadyPurchased = 1,
        [EnumMember(Value = "2")]
        InEligible = 2,
        [EnumMember(Value = "3")]
        Included = 3
    }



    [Serializable()]
    public enum MOBPBSegmentType
    {
        [EnumMember(Value = "0")]
        Regular = 0,
        [EnumMember(Value = "1")]
        AlreadyPurchased = 1,
        [EnumMember(Value = "2")]
        InEligible = 2,
        [EnumMember(Value = "3")]
        Included = 3
    }

    public enum ServiceNames
    {
        MEMBERSIGNIN,
        MPACCOUNTPROFILE,
        MPAUTHENTICATION,
        MPREWARDS,
        MPSIGNINCOMMON
    }




}
