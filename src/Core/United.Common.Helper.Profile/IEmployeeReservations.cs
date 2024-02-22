using System.Threading.Tasks;
using United.Mobile.Model.Common;
using United.Service.Presentation.PersonModel;

namespace United.Common.Helper.EmployeeReservation
{
    public interface IEmployeeReservations
    {
        MOBEmpTravelTypeAndJAProfileResponse GetTravelTypesAndJAProfile(MOBEmpTravelTypeAndJAProfileRequest request);
        Task<EmployeeTravelProfile> GetEmployeeProfile(int applicationId, string applicationVersion, string deviceId, string employeeId, string token, string sessionId);
    }
}
