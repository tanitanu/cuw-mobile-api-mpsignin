using System.Collections.Generic;
using System.Threading.Tasks;

namespace United.Mobile.MPSignInTool.Domain
{
    public interface IMPSignInToolBusiness
    {
        Task<HealthCheckReport> GetAllServiceDetails();
        Task<List<ServiceDetails>> GetQAServiceDetails();
        Task<List<ServiceDetails>> GetDevelopmentServiceDetails();
        Task<List<ServiceDetails>> GetStageServiceDetails();
        Task<List<ServiceDetails>> GetProdServiceDetails();
    }
}