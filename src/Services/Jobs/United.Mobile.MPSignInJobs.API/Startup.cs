using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.OnPremiseSQLSP;
using United.Mobile.MPSignInJobs.Domain;
using United.Utility.Http;

namespace United.Mobile.Jobs.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddTransient<ISQL2DynamoDBJobBusiness, SQL2DynamoDBJobBusiness>();
            services.AddHostedService<Device_2DynamoDBJob>();
            services.AddHostedService<Device_History_2DynamoDBJob>();
            services.AddHostedService<DevicePushToken_2DynamoDBJob>();
            services.AddHostedService<MileagePlusValidation_CSS_2DynamoDBJob>();
            services.AddHostedService<MileagePlusValidation_2DynamoDBJob>();
            services.AddHostedService<TSA_Flagged_Account_2DynamoDBJob>();
            services.AddHostedService<IsVBQWMDisplayed_2DynamoDBJob>();
            services.AddHostedService<EResBetaTester_2DynamoDBJob>(); 
        }

        public void ConfigureContainer(ContainerBuilder container)
        {
            container.Register(c => new ResilientClient(Configuration.GetSection("OnPremSQLDataMigrationService").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("SQL2DynamoMigrationClientKey");
            container.RegisterType<SQL2DynamoMigrationService>().As<ISQL2DynamoMigrationService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("CloudSQLDataMigrationService").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("CloudSQL2DynamoMigrationClientKey");
            container.RegisterType<CloudSQL2DynamoMigrationService>().As<ICloudSQL2DynamoMigrationService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("cachingConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("cachingConfigKey");
            container.RegisterType<CachingService>().As<ICachingService>().WithAttributeFiltering();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.MapWhen(context => string.IsNullOrEmpty(context.Request.Path) || string.Equals("/", context.Request.Path), appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    await context.Response.WriteAsync("Welcome from MPSignIn Jobs microservice").ConfigureAwait(false);
                });
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
