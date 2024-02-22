using Microsoft.AspNetCore.Mvc;
using System;

namespace United.Mobile.MPSignInTool.Controllers
{
    public class ApiController : Controller
    {

        [HttpGet]
        public string HealthCheck()
        {
            return "Healthy";
        }

        [HttpGet]
        public virtual string Version()
        {
            string serviceVersionNumber = null;

            try
            {
                serviceVersionNumber = Environment.GetEnvironmentVariable("SERVICE_VERSION_NUMBER");
            }
            catch
            {
                // Suppress any exceptions
            }
            finally
            {
                serviceVersionNumber = (null == serviceVersionNumber) ? "0.0.0" : serviceVersionNumber;
            }

            return serviceVersionNumber;
        }

        [HttpGet]
        public virtual string ApiEnvironment()
        {
            try
            {
                return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            }
            catch
            {
                // Suppress any exceptions
            }
            return "Unknown";
        }
    }
}