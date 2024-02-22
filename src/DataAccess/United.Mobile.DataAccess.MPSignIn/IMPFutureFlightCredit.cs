using System.Threading.Tasks;

namespace United.Mobile.DataAccess.CSLSerivce
{
    public interface IMPFutureFlightCredit
    {
        Task<T> GetMPFutureFlightCredit<T>(string token, string callsource, string mileagePlusNumber, string sessionId);
    }
}
