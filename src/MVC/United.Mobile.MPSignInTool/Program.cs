using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using United.Ebs.Logging;

namespace United.Mobile.MPSignInTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder1(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args)
             .UseServiceProviderFactory(new AutofacServiceProviderFactory())
          .ConfigureAppConfiguration((hostContext, config) =>
          {
              config.AddJsonFile("appsettings.json", optional: false);
              config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
          })
          .ConfigureWebHostDefaults(webBuilder =>
          {
              webBuilder.ConfigureServices((context, services) =>
              {
                  services.AddHttpContextAccessor();
                  context.ConfigureEbsLogger(services);
              })
          .ConfigureLogging(x =>
          {
              x.ClearProviders();
              x.AddEbsLogger();
          });
              webBuilder.UseStartup<Startup>();
          });
    }
}