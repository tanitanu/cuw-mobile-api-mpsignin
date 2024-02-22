using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json.Converters;
using Serilog;
using System;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Ebs.Logging.Enrichers;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.MPSignIn;
using United.Mobile.DataAccess.MPSignInDatabaseManager;
using United.Mobile.DataAccess.OnPremiseSQLSP;
using United.Mobile.Model;
using United.Mobile.MPSignInCommon.Domain;
using United.Service.Presentation.ReferenceDataModel;
using United.Utility.Helper;
using United.Utility.Http;
using United.Utility.Middleware;

namespace United.Mobile.MPSignInCommon.Api
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
            try
            {
                services.AddControllers()
                    .AddJsonOptions(opts => opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
                services.AddControllers().AddNewtonsoftJson(option =>
                {
                    option.SerializerSettings.Converters.Add(new StringEnumConverter());
                });
                services.AddControllers();
                services.AddHttpContextAccessor();
                services.AddScoped<IHeaders, Headers>();
                services.AddScoped<IMPSignInCommonBusiness, MPSignInCommonBusiness>();
                services.AddScoped<IDynamoDBHelperService, DynamoDBHelperService>();
                services.AddScoped<ISQLSPService, SQLSPService>();
                services.AddScoped<IDynamoDBService, DynamoDBService>();
                services.AddScoped<IHashPin, HashPin>();
                services.AddScoped<IDynamoDBUtility, DynamoDBUtility>();
                services.AddScoped<CacheLogWriter>();
                services.AddScoped(typeof(ICacheLog<>), typeof(CacheLog<>));
                services.AddSingleton<IFeatureSettings, FeatureSettings>();
                services.AddSingleton<IAuroraMySqlService, AuroraMySqlService>();
                services.AddSingleton<IAWSSecretManager, AWSSecretManager>();
                services.AddSingleton<IDataSecurity, DataSecurity>();
                services.AddSingleton<IUtility, United.Common.Helper.Utility>();


            }
            catch (Exception ex)
            {
                Log.Logger.Fatal(ex, "ConfigureServices-Unexpected error occurred while starting services");
            }

        }

        public void ConfigureContainer(ContainerBuilder container)
        {
            try
            {
                container.Register(c => new ResilientClient(Configuration.GetSection("DynamoDBClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("DynamoDBClientKey");
                container.RegisterType<DynamoDBService>().As<IDynamoDBService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("OnPremSqlClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("OnPremSqlClientKey");
                container.RegisterType<SQLSPService>().As<ISQLSPService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("dpTokenConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("dpTokenConfigKey");
                container.RegisterType<DPService>().As<IDPService>().WithAttributeFiltering();
                container.Register(c => new ResilientClient(Configuration.GetSection("cachingConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("cachingConfigKey");
                container.RegisterType<CachingService>().As<ICachingService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("DatabaseManagerClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("DatabaseManagerClientKey");
                container.RegisterType<DatabaseManagerService>().As<IDatabaseManagerService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("dpTokenValidateConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("dpTokenValidateKey");
                container.RegisterType<DPTokenValidationService>().As<IDPTokenValidationService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("CMSContentDataClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("CMSContentDataClientKey");
                container.RegisterType<CMSContentService>().As<ICMSContentService>().WithAttributeFiltering();

            }
            catch (Exception ex)
            {
                Log.Logger.Fatal(ex, "ConfigureContainer-Unexpected error occurred while starting services");
            }

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApplicationEnricher applicationEnricher, IFeatureSettings featureSettings, IHostApplicationLifetime applicationLifetime)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                app.MapWhen(context => string.IsNullOrEmpty(context.Request.Path) || string.Equals("/", context.Request.Path), appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        await context.Response.WriteAsync("Welcome from MPSignInCommon Microservice").ConfigureAwait(false);
                    });
                });
                applicationEnricher.Add(Constants.ServiceNameText, Program.Namespace);
                applicationEnricher.Add(Constants.EnvironmentText, env.EnvironmentName);
                applicationEnricher.Add(Constants.ApplicationIdText, Program.AppName);
                if (Configuration.GetValue<bool>("Globalization"))
                {
                    var cultureInfo = new CultureInfo("en-US");
                    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
                }
                if (Configuration.GetValue<bool>("EnableFeatureSettingsChanges"))
                {
                    applicationLifetime.ApplicationStarted.Register(async () => await OnStart(featureSettings));
                    applicationLifetime.ApplicationStopping.Register(async () => await OnShutDown(featureSettings));
                }
                app.UseMiddleware<RequestResponseLoggingMiddleware>();

                app.UseHttpsRedirection();

                app.UseRouting();

                app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal(ex, "Configure-Unexpected error occurred while starting services");
            }

        }
        private async Task OnStart(IFeatureSettings featureSettings)
        {
            try
            {
                await featureSettings.LoadFeatureSettings(United.Mobile.Model.Common.ServiceNames.MPSIGNINCOMMON.ToString());
            }
            catch (Exception) { }

        }
        private async Task OnShutDown(IFeatureSettings featureSettings)
        {
            try
            {
                await featureSettings.DeleteContainerIPAdress(United.Mobile.Model.Common.ServiceNames.MPSIGNINCOMMON.ToString(), StaticDataLoader._ipAddress);
            }
            catch { }
        }
    }
}
