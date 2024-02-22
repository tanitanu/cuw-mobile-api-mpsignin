using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.Shopping
{
    [Serializable]
    public class MOBTripInsuranceInfo
    {
        public double Amount { get; set; }
        public string QuoteHeader { get; set; } = string.Empty;

        public string QuoteCompanyName { get; set; } = string.Empty;

        public string QuoteDisplayAmount { get; set; } = string.Empty;

        public string CurrencyCode { get; set; } = string.Empty;

        public List<string> ListOfBenifits { get; set; }

        public string Tnc { get; set; } = string.Empty;

        public string TncLink { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string ProductId { get; set; } = string.Empty;

        public List<TravelOptionSubItem> ListOfPriceDetails { get; set; }
        public MOBTripInsuranceInfo()
        {
            ListOfBenifits = new List<string>();
            ListOfPriceDetails = new List<TravelOptionSubItem>();
        }
    }
    [Serializable]
    public class TripInsuranceFile
    {
        public MOBTPIInfo TripInsuranceInfo { get; set; }
        public MakeReservationRequest RegisterFOPForTPIRequest { get; set; }
        public MakeReservationRequest RegisterFormOfPaymentForSecondaryPaymentRequest { get; set; }
        public MakeReservationResponse RegisterFormOfPaymentForSecondaryPaymentResponse { get; set; }
        public MOBRegisterFOPForTPIResponse RegisterFOPForTPIResponse { get; set; }
        public string AccountNumberToken { get; set; } = string.Empty;

        public string ConfirmationResponseDetailMessage1 { get; set; } = string.Empty;

        public string ConfirmationResponseDetailMessage2 { get; set; } = string.Empty;

        public MOBTPIInfoInBookingPath TripInsuranceBookingInfo { get; set; }
    }
}
