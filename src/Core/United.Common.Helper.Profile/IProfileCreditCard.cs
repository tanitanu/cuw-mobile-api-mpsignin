using System.Collections.Generic;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
using Reservation = United.Mobile.Model.Shopping.Reservation;

namespace United.Common.Helper.Profile
{
    public interface IProfileCreditCard
    {
        Task<List<MOBCreditCard>> PopulateCorporateCreditCards(List<Services.Customer.Common.CreditCard> creditCards, bool isGetCreditCardDetailsCall, List<MOBAddress> addresses, Reservation persistedReservation, string sessionId);
        Task<List<MOBCreditCard>> PopulateCreditCards(List<Services.Customer.Common.CreditCard> creditCards, bool isGetCreditCardDetailsCall, List<Mobile.Model.Common.MOBAddress> addresses, string sessionId);
        bool IsValidFOPForTPIpayment(string cardType);
        Task<List<MOBCreditCard>> PopulateCorporateCreditCards(bool isGetCreditCardDetailsCall, List<MOBAddress> addresses, Reservation persistedReservation, MOBCPProfileRequest request);
        Task<List<MOBCreditCard>> PopulateCreditCards(bool isGetCreditCardDetailsCall, List<MOBAddress> addresses, MOBCPProfileRequest request);
    }
}
