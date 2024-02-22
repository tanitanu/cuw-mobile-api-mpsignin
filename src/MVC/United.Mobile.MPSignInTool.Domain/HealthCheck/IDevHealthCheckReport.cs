using System.Threading.Tasks;

namespace United.Mobile.MPSignInTool.Domain.HealthCheck
{
    public interface IDevHealthCheckReport
    {
        Task SetCatalog();
        Task<ServiceDetails> GetServiceDetails(ServiceDetails objServiceDetails);
    }
}