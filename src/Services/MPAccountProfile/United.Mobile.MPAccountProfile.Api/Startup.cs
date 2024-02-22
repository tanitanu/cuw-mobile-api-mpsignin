using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json.Converters;
using Serilog;
using System;
using System.Globalization;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.EmployeeReservation;
using United.Common.Helper.Profile;
using United.Ebs.Logging.Enrichers;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.CSLSerivce;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.MerchandizeService;
using United.Mobile.DataAccess.MPSignIn;
using United.Mobile.DataAccess.OnPremiseSQLSP;
using United.Mobile.Model;
using United.Mobile.MPAccountProfile.Domain;
using United.Service.Presentation.ReferenceDataModel;
using United.Utility.Helper;
using United.Utility.Http;
using United.Utility.Middleware;

namespace United.Mobile.MPAccountProfile.Api
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
                services.AddControllers();
                services.AddControllers()
                     .AddJsonOptions(opts => opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
                services.AddControllers().AddNewtonsoftJson(option =>
                 {
                     option.SerializerSettings.Converters.Add(new StringEnumConverter());
                 });
                services.AddScoped<IMPAccountProfileBusiness, MPAccountProfileBusiness>();
                services.AddScoped<ISessionHelperService, SessionHelperService>();
                services.AddScoped<ICustomerProfile, CustomerProfile>();
                services.AddScoped<IMileagePlus, MileagePlus>();
                services.AddScoped<IShoppingSessionHelper, ShoppingSessionHelper>();
                services.AddScoped<IHeaders, Headers>();
                services.AddScoped<IHashPin, HashPin>();
                services.AddScoped<IEmployeeReservations, EmployeeReservations>();
                services.AddScoped<IMPTraveler, MPTraveler>();
                services.AddScoped<ICorporateProfile, CorporateProfile>();
                services.AddScoped<IProfileCreditCard, ProfileCreditCard>();
                services.AddScoped<IDynamoDBHelperService, DynamoDBHelperService>();
                services.AddScoped<IDynamoDBUtility, DynamoDBUtility>();
                services.AddScoped<IMerchandizingServices, Common.Helper.Profile.MerchandizingServices>();
                services.AddScoped<IUtility, Common.Helper.Utility>();
                services.AddScoped<CacheLogWriter>();
                services.AddScoped(typeof(ICacheLog<>), typeof(CacheLog<>));
                services.AddSingleton<IFeatureSettings, FeatureSettings>();
                services.AddSingleton<IAuroraMySqlService, AuroraMySqlService>();
                services.AddSingleton<IAWSSecretManager, AWSSecretManager>();
                services.AddSingleton<IDataSecurity, DataSecurity>();
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal(ex, "Unexpected error occurred while starting services");
            }
        }

        public void ConfigureContainer(ContainerBuilder container)
        {
            try
            {
                container.Register(c => new ResilientClient(Configuration.GetSection("cachingConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("cachingConfigKey");
                container.RegisterType<CachingService>().As<ICachingService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("dpDiscoveryDocumentConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("dpDiscoveryDocumentConfigKey");
                container.RegisterType<DataPowerFactory>().As<IDataPowerFactory>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("SessionOnCloudConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("sessionOnCloudConfigKey");
                container.RegisterType<SessionOnCloudService>().As<ISessionOnCloudService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("dpTokenConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("dpTokenConfigKey");
                container.RegisterType<DPService>().As<IDPService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("DynamoDBClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("DynamoDBClientKey");
                container.RegisterType<DynamoDBService>().As<IDynamoDBService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("LoyaltyUCBClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("LoyaltyUCBClientKey");
                container.RegisterType<LoyaltyUCBService>().As<ILoyaltyUCBService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("CustomerPreferencesClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("CustomerPreferencesClientKey");
                container.RegisterType<CustomerPreferencesService>().As<ICustomerPreferencesService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("UtilitiesServiceClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("UtilitiesServiceClientKey");
                container.RegisterType<UtilitiesService>().As<IUtilitiesService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("MPSecurityQuestionsClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("MPSecurityQuestionsClientKey");
                container.RegisterType<MPSecurityQuestionsService>().As<IMPSecurityQuestionsService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("MPSecurityCheckDetailsClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("MPSecurityCheckDetailsClientKey");
                container.RegisterType<MPSecurityCheckDetailsService>().As<IMPSecurityCheckDetailsService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("CustomerDataClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("CustomerDataClientKey");
                container.RegisterType<CustomerDataService>().As<ICustomerDataService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("MyAccountFutureFlightCreditClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("MyAccountFutureFlightCreditClientKey");
                container.RegisterType<MPFutureFlightCredit>().As<IMPFutureFlightCredit>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("EmployeeServiceClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("EmployeeServiceClientKey");
                container.RegisterType<EmployeeService>().As<IEmployeeService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("LoyaltyAccountClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("LoyaltyAccountClientKey");
                container.RegisterType<LoyaltyAccountService>().As<ILoyaltyAccountService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("LoyaltyWebClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("LoyaltyWebClientKey");
                container.RegisterType<LoyaltyWebService>().As<ILoyaltyWebService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("PNRRetrievalClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("PNRRetrievalClientKey");
                container.RegisterType<PNRRetrievalService>().As<IPNRRetrievalService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("DataVaultTokenClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("DataVaultTokenClientKey");
                container.RegisterType<DataVaultService>().As<IDataVaultService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("OnPremSqlClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("OnPremSqlClientKey");
                container.RegisterType<SQLSPService>().As<ISQLSPService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("eResEmployeeProfileClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("eResEmployeeProfileClientKey");
                container.RegisterType<EResEmployeeProfileService>().As<IEResEmployeeProfileService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("UnitedClubMembershipService").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("UnitedClubMembershipKey");
                container.RegisterType<UnitedClubMembershipService>().As<IUnitedClubMembershipService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("CMSContentDataClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("CMSContentDataClientKey");
                container.RegisterType<CMSContentService>().As<ICMSContentService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("SubscriptionsClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("SubscriptionsClientKey");
                container.RegisterType<MerchOffersService>().As<IMerchOffersService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("dpTokenValidateConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("dpTokenValidateKey");
                container.RegisterType<DPTokenValidationService>().As<IDPTokenValidationService>().WithAttributeFiltering();
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal(ex, "Unexpected error occurred while starting services");
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
                        await context.Response.WriteAsync("Welcome from MP Account Profile Microservice").ConfigureAwait(false);
                    });
                });

                applicationEnricher.Add(Constants.ServiceNameText, Program.Namespace);
                applicationEnricher.Add(Constants.EnvironmentText, env.EnvironmentName);

                if (Configuration.GetValue<bool>("Globalization"))
                {
                    var cultureInfo = new CultureInfo("en-US");
                    // cultureInfo.NumberFormat.CurrencySymbol = "";
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
                Log.Logger.Fatal(ex, "Unexpected error occurred while starting services");
            }
        }
        private async Task OnStart(IFeatureSettings featureSettings)
        {
            try
            {
                await featureSettings.LoadFeatureSettings(United.Mobile.Model.Common.ServiceNames.MPACCOUNTPROFILE.ToString());
            }
            catch (Exception) { }

        }
        private async Task OnShutDown(IFeatureSettings featureSettings)
        {
            try
            {
                await featureSettings.DeleteContainerIPAdress(United.Mobile.Model.Common.ServiceNames.MPACCOUNTPROFILE.ToString(), StaticDataLoader._ipAddress);
            }
            catch { }
        }
    }
}
