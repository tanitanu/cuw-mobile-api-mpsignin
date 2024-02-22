using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using United.Mobile.Model.Common;
using United.Mobile.Model.Internal.Common;
using United.Mobile.Model.Shopping;

namespace United.Mobile.Test.MPAccountProfileTests.Api
{
    public class TestDataSet
    {
        //public Object[] set1()
        //{
        //    var mOBCustomerProfileRequestJson = TestDataGenerator.GetFileContent("MOBCustomerProfileRequest.json");
        //    var mOBCustomerProfileRequest = JsonConvert.DeserializeObject<List<MOBCPProfileRequest>>(mOBCustomerProfileRequestJson);

        //    var sessionDataJson = TestDataGenerator.GetFileContent("SessionData.json");
        //    var sessionData = JsonConvert.DeserializeObject<Session>(sessionDataJson);

        //    var mOBCustomerProfileResponseJson = TestDataGenerator.GetFileContent("MOBCustomerProfileResponse.json");
        //    var mOBCustomerProfileResponse = JsonConvert.DeserializeObject<MOBCustomerProfileResponse>(mOBCustomerProfileResponseJson);

        //    var reservationjson = TestDataGenerator.GetFileContent("Reservation.json");
        //    var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

        //    var ProfileResponseJson = TestDataGenerator.GetFileContent("ProfileResponse.json");
        //    var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponseJson);

        //    var mOBCPProfileResponseJson = TestDataGenerator.GetFileContent("MOBCustomerProfileResponse.json");
        //    var mOBCPProfileResponse = JsonConvert.DeserializeObject<MOBCPProfileResponse>(mOBCPProfileResponseJson);

        //    var mOBShoppingCartjson = TestDataGenerator.GetFileContent("MOBShoppingCart.json");
        //    var mOBShoppingCart = JsonConvert.DeserializeObject<MOBShoppingCart>(mOBShoppingCartjson);



        //    return new object[] { mOBCustomerProfileRequest[0], sessionData, mOBCustomerProfileResponse, reservation[0], ProfileResponse[0], mOBCPProfileResponse, mOBShoppingCart };
        //}

        //public Object[] set2()
        //{
        //    var mOBCustomerProfileRequestJson = TestDataGenerator.GetFileContent("MOBCustomerProfileRequest.json");
        //    var mOBCustomerProfileRequest = JsonConvert.DeserializeObject<List<MOBCPProfileRequest>>(mOBCustomerProfileRequestJson);

        //    var sessionjson = TestDataGenerator.GetFileContent("Session.json");
        //    var session = JsonConvert.DeserializeObject<Session>(sessionjson);

        //    var reservationjson = TestDataGenerator.GetFileContent("Reservation.json");
        //    var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

        //    var selectTripjson = TestDataGenerator.GetFileContent("SelectTrip.json");
        //    var selectTrip = JsonConvert.DeserializeObject<List<SelectTrip>>(selectTripjson);

        //    var mOBCPProfileResponseJson = TestDataGenerator.GetFileContent("MOBCustomerProfileResponse.json");
        //    var mOBCPProfileResponse = JsonConvert.DeserializeObject<MOBCPProfileResponse>(mOBCPProfileResponseJson);

        //    var ProfileResponseJson = TestDataGenerator.GetFileContent("ProfileResponse.json");
        //    var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponseJson);

        //    var mOBShoppingCartjson = TestDataGenerator.GetFileContent("MOBShoppingCart.json");
        //    var mOBShoppingCart = JsonConvert.DeserializeObject<MOBShoppingCart>(mOBShoppingCartjson);


        //    return new object[] { mOBCustomerProfileRequest[0], session, reservation[0], selectTrip[0], mOBCPProfileResponse, ProfileResponse[0], mOBShoppingCart };
        //}

        //public Object[] set3()
        //{
        //    var mPSignedInInsertUpdateTravelerJson = TestDataGenerator.GetFileContent("MOBUpdateTravelerRequest.json");
        //    var mPSignedInInsertUpdateTraveler = JsonConvert.DeserializeObject<List<MOBUpdateTravelerRequest>>(mPSignedInInsertUpdateTravelerJson);

        //    var sessionjson = TestDataGenerator.GetFileContent("Session.json");
        //    var session = JsonConvert.DeserializeObject<Session>(sessionjson);

        //    var reservationjson = TestDataGenerator.GetFileContent("Reservation.json");
        //    var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

        //    var ProfileResponseJson = TestDataGenerator.GetFileContent("ProfileResponse.json");
        //    var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponseJson);

        //    var saveResponseJson = TestDataGenerator.GetFileContent("SaveResponse.json");
        //    var saveResponse = JsonConvert.DeserializeObject<United.Services.Customer.Common.SaveResponse>(saveResponseJson);




        //    return new object[] { mPSignedInInsertUpdateTraveler[0], session, reservation[0], ProfileResponse[0], saveResponse };
        //}

        //public Object[] set2_1()
        //{
        //    var mOBCustomerProfileRequestJson = TestDataGenerator.GetFileContent("MOBCustomerProfileRequest.json");
        //    var mOBCustomerProfileRequest = JsonConvert.DeserializeObject<List<MOBCPProfileRequest>>(mOBCustomerProfileRequestJson);

        //    var sessionjson = TestDataGenerator.GetFileContent("Session.json");
        //    var session = JsonConvert.DeserializeObject<Session>(sessionjson);

        //    var reservationjson = TestDataGenerator.GetFileContent("Reservation.json");
        //    var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

        //    var selectTripjson = TestDataGenerator.GetFileContent("SelectTrip.json");
        //    var selectTrip = JsonConvert.DeserializeObject<List<SelectTrip>>(selectTripjson);

        //    var mOBCPProfileResponseJson = TestDataGenerator.GetFileContent("MOBCustomerProfileResponse.json");
        //    var mOBCPProfileResponse = JsonConvert.DeserializeObject<MOBCPProfileResponse>(mOBCPProfileResponseJson);

        //    var ProfileResponseJson = TestDataGenerator.GetFileContent("ProfileResponse.json");
        //    var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponseJson);

        //    var mOBShoppingCartjson = TestDataGenerator.GetFileContent("MOBShoppingCart.json");
        //    var mOBShoppingCart = JsonConvert.DeserializeObject<MOBShoppingCart>(mOBShoppingCartjson);


        //    return new object[] { mOBCustomerProfileRequest[0], session, reservation[1], selectTrip[0], mOBCPProfileResponse, ProfileResponse[0], mOBShoppingCart };
        //}

        //public Object[] set3_1()
        //{
        //    var mPSignedInInsertUpdateTravelerJson = TestDataGenerator.GetFileContent("MOBUpdateTravelerRequest.json");
        //    var mPSignedInInsertUpdateTraveler = JsonConvert.DeserializeObject<List<MOBUpdateTravelerRequest>>(mPSignedInInsertUpdateTravelerJson);

        //    var sessionjson = TestDataGenerator.GetFileContent("Session.json");
        //    var session = JsonConvert.DeserializeObject<Session>(sessionjson);

        //    var reservationjson = TestDataGenerator.GetFileContent("Reservation.json");
        //    var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

        //    var ProfileResponseJson = TestDataGenerator.GetFileContent("ProfileResponse.json");
        //    var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponseJson);

        //    var mOBCustomerProfileResponseJson = TestDataGenerator.GetFileContent("MOBCustomerProfileResponse.json");
        //    var mOBCustomerProfileResponse = JsonConvert.DeserializeObject<MOBCPProfileResponse>(mOBCustomerProfileResponseJson);

        //    var saveResponseJson = TestDataGenerator.GetFileContent("SaveResponse.json");
        //    var saveResponse = JsonConvert.DeserializeObject<United.Services.Customer.Common.SaveResponse>(saveResponseJson);




        //    return new object[] { mPSignedInInsertUpdateTraveler[1], session, reservation[1], ProfileResponse[0], mOBCustomerProfileResponse, saveResponse };
        //}

        //public Object[] set4()
        //{
        //    //var updateTravelerInfoRequestJson = TestDataGenerator.GetFileContent("MOBUpdateTravelerInfoRequest.json");
        //    //var updateTravelerInfoRequest = JsonConvert.DeserializeObject<MOBUpdateTravelerInfoRequest>(updateTravelerInfoRequestJson);

        //    // var frequentFlyerRewardProgramsRequest = TestDataGenerator.GetXmlData<FrequentFlyerRewardProgramsRequest>("FrequentFlyerRewardProgramsRequest.xml");

        //    var sessionjson = TestDataGenerator.GetFileContent("Session.json");
        //    var session = JsonConvert.DeserializeObject<Session>(sessionjson);

        //    var reservationjson = TestDataGenerator.GetFileContent("Reservation.json");
        //    var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

        //    // var rewardProgramResponseJson = TestDataGenerator.GetFileContent("RewardProgramResponse.json");
        //    // var rewardProgramResponse = JsonConvert.DeserializeObject<Collection<Program>>(rewardProgramResponseJson);


        //    return new object[] { session, reservation[0] };
        //}

        //public Object[] set4_1()
        //{
        //    //var updateTravelerInfoRequestJson = TestDataGenerator.GetFileContent("MOBUpdateTravelerInfoRequest.json");
        //    //var updateTravelerInfoRequest = JsonConvert.DeserializeObject<MOBUpdateTravelerInfoRequest>(updateTravelerInfoRequestJson);

        //    // var frequentFlyerRewardProgramsRequest = TestDataGenerator.GetXmlData<FrequentFlyerRewardProgramsRequest>("FrequentFlyerRewardProgramsRequest.xml");

        //    var sessionjson = TestDataGenerator.GetFileContent("Session.json");
        //    var session = JsonConvert.DeserializeObject<Session>(sessionjson);

        //    var reservationjson = TestDataGenerator.GetFileContent("Reservation.json");
        //    var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);



        //    return new object[] { session, reservation[0] };
        //}

        //public Object[] set5()
        //{
        //    var mPSignedInInsertUpdateTravelerJson = TestDataGenerator.GetFileContent("MOBUpdateTravelerRequest.json");
        //    var mPSignedInInsertUpdateTraveler = JsonConvert.DeserializeObject<List<MOBUpdateTravelerRequest>>(mPSignedInInsertUpdateTravelerJson);

        //    var sessionjson = TestDataGenerator.GetFileContent("Session.json");
        //    var session = JsonConvert.DeserializeObject<Session>(sessionjson);

        //    var reservationjson = TestDataGenerator.GetFileContent("Reservation.json");
        //    var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

        //    var ProfileResponseJson = TestDataGenerator.GetFileContent("ProfileResponse1.json");
        //    var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponseJson);

        //    //var mOBCustomerProfileResponseJson = TestDataGenerator.GetFileContent("MOBCustomerProfileResponse.json");
        //    //var mOBCustomerProfileResponse = JsonConvert.DeserializeObject<MOBCPProfileResponse>(mOBCustomerProfileResponseJson);

        //    var mOBCPProfileResponseJson = TestDataGenerator.GetFileContent("MOBCustomerProfileResponse.json");
        //    var mOBCPProfileResponse = JsonConvert.DeserializeObject<MOBCPProfileResponse>(mOBCPProfileResponseJson);

        //    var mOBShoppingCartjson = TestDataGenerator.GetFileContent("MOBShoppingCart.json");
        //    var mOBShoppingCart = JsonConvert.DeserializeObject<MOBShoppingCart>(mOBShoppingCartjson);






        //    return new object[] { mPSignedInInsertUpdateTraveler[0], session, reservation[0], ProfileResponse[0], mOBCPProfileResponse, mOBShoppingCart };
        //}

        //public Object[] set6()
        //{
        //    var updateTravelerInfoRequestJson = TestDataGenerator.GetFileContent("MOBUpdateTravelerInfoRequest.json");
        //    var updateTravelerInfoRequest = JsonConvert.DeserializeObject<MOBUpdateTravelerInfoRequest>(updateTravelerInfoRequestJson);

        //    var sessionjson = TestDataGenerator.GetFileContent("Session.json");
        //    var session = JsonConvert.DeserializeObject<Session>(sessionjson);

        //    var reservationjson = TestDataGenerator.GetFileContent("Reservation.json");
        //    var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

        //    var ProfileResponseJson = TestDataGenerator.GetFileContent("ProfileResponse1.json");
        //    var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponseJson);

        //    //var mOBCustomerProfileResponseJson = TestDataGenerator.GetFileContent("MOBCustomerProfileResponse.json");
        //    //var mOBCustomerProfileResponse = JsonConvert.DeserializeObject<MOBCPProfileResponse>(mOBCustomerProfileResponseJson);

        //    var mOBCPProfileResponseJson = TestDataGenerator.GetFileContent("MOBCustomerProfileResponse.json");
        //    var mOBCPProfileResponse = JsonConvert.DeserializeObject<MOBCPProfileResponse>(mOBCPProfileResponseJson);

        //    var mOBShoppingCartjson = TestDataGenerator.GetFileContent("MOBShoppingCart.json");
        //    var mOBShoppingCart = JsonConvert.DeserializeObject<MOBShoppingCart>(mOBShoppingCartjson);

        //    var mOBUpdateTravelerInfoResponseJson = TestDataGenerator.GetFileContent("MOBUpdateTravelerInfoResponse.json");
        //    var mOBUpdateTravelerInfoResponse = JsonConvert.DeserializeObject<MOBUpdateTravelerInfoResponse>(mOBUpdateTravelerInfoResponseJson);





        //    return new object[] { updateTravelerInfoRequest, session, reservation[0], ProfileResponse[0], mOBCPProfileResponse, mOBShoppingCart, mOBUpdateTravelerInfoResponse };
        //}

    }
}
