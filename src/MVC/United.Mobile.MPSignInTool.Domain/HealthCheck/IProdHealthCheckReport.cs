using System.Threading.Tasks;

namespace United.Mobile.MPSignInTool.Domain.HealthCheck
{
    public interface IProdHealthCheckReport
    {
        Task SetCatalog();
        Task<ServiceDetails> GetServiceDetails(ServiceDetails objServiceDetails);
    }
}