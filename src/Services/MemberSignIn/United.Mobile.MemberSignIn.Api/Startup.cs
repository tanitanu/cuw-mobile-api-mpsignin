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
using Serilog;
using System;
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
using United.Mobile.DataAccess.MPSignIn;
using United.Mobile.DataAccess.OnPremiseSQLSP;
using United.Mobile.MemberSignIn.Domain;
using United.Mobile.Model;
using United.Service.Presentation.ReferenceDataModel;
using United.Utility.Helper;
using United.Utility.Http;
using United.Utility.Middleware;

namespace United.Mobile.MemberSignIn.Api
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
                services.AddHttpContextAccessor();
                services.AddDirectoryBrowser();

                services.AddScoped<IMemberSignInBusiness, MemberSignInBusiness>();
                services.AddScoped<ISessionHelperService, SessionHelperService>();
                services.AddScoped<ICustomerProfile, CustomerProfile>();
                services.AddScoped<IMPTraveler, MPTraveler>();
                services.AddScoped<IMileagePlusTFACSL, MileagePlusTFACSL>();
                services.AddScoped<IMileagePlusCSL, MileagePlusCSL>();
                services.AddScoped<IMileagePlus, MileagePlus>();
                services.AddScoped<IShoppingSessionHelper, ShoppingSessionHelper>();
                services.AddScoped<ISecurityQuestion, SecurityQuestion>();
                services.AddScoped<IHeaders, Headers>();
                services.AddScoped<IHashPin, HashPin>();
                services.AddScoped<IEmployeeReservations, EmployeeReservations>();
                services.AddScoped<ICorporateProfile, CorporateProfile>();
                services.AddScoped<IProfileCreditCard, ProfileCreditCard>();
                services.AddScoped<IDynamoDBHelperService, DynamoDBHelperService>();
                services.AddScoped<IDynamoDBUtility, DynamoDBUtility>();
                services.AddScoped<IUtility, Common.Helper.Utility>();
                services.AddScoped<ICatalog, Common.Helper.Catalog>();
                services.AddScoped<CacheLogWriter>();
                services.AddScoped(typeof(ICacheLog<>), typeof(CacheLog<>));
                services.AddSingleton<IFeatureSettings, FeatureSettings>();
                services.AddSingleton<IAuroraMySqlService, AuroraMySqlService>();
                services.AddSingleton<IAWSSecretManager, AWSSecretManager>();
                services.AddSingleton<IDataSecurity, DataSecurity>();
                services.AddScoped<IMPNumberToPnrBussiness, MPNumberToPnrBussiness>();
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
                container.Register(c => new ResilientClient(Configuration.GetSection("dpTokenConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("dpTokenConfigKey");
                container.RegisterType<DPService>().As<IDPService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("dpDiscoveryDocumentConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("dpDiscoveryDocumentConfigKey");
                container.RegisterType<DataPowerFactory>().As<IDataPowerFactory>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("SessionOnCloudConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("sessionOnCloudConfigKey");
                container.RegisterType<SessionOnCloudService>().As<ISessionOnCloudService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("cachingConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("cachingConfigKey");
                container.RegisterType<CachingService>().As<ICachingService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("DynamoDBClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("DynamoDBClientKey");
                container.RegisterType<DynamoDBService>().As<IDynamoDBService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("CustomerPreferencesClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("CustomerPreferencesClientKey");
                container.RegisterType<CustomerPreferencesService>().As<ICustomerPreferencesService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("MPSecurityQuestionsClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("MPSecurityQuestionsClientKey");
                container.RegisterType<MPSecurityQuestionsService>().As<IMPSecurityQuestionsService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("LoyaltyAWSClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("LoyaltyAWSClientKey");
                container.RegisterType<LoyaltyAWSService>().As<ILoyaltyAWSService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("RavenClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("RavenClientKey");
                container.RegisterType<RavenService>().As<IRavenService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("RavenCloudClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("RavenCloudClientKey");
                container.RegisterType<RavenCloudService>().As<IRavenCloudService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("PNRRetrievalClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("PNRRetrievalClientKey");
                container.RegisterType<PNRRetrievalService>().As<IPNRRetrievalService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("DataVaultTokenClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("DataVaultTokenClientKey");
                container.RegisterType<DataVaultService>().As<IDataVaultService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("UtilitiesServiceClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("UtilitiesServiceClientKey");
                container.RegisterType<UtilitiesService>().As<IUtilitiesService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("LoyaltyUCBClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("LoyaltyUCBClientKey");
                container.RegisterType<LoyaltyUCBService>().As<ILoyaltyUCBService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("LoyaltyAccountClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("LoyaltyAccountClientKey");
                container.RegisterType<LoyaltyAccountService>().As<ILoyaltyAccountService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("LoyaltyWebClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("LoyaltyWebClientKey");
                container.RegisterType<LoyaltyWebService>().As<ILoyaltyWebService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("EmployeeServiceClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("EmployeeServiceClientKey");
                container.RegisterType<EmployeeService>().As<IEmployeeService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("MyAccountFutureFlightCreditClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("MyAccountFutureFlightCreditClientKey");
                container.RegisterType<MPFutureFlightCredit>().As<IMPFutureFlightCredit>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("OnPremSqlClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("OnPremSqlClientKey");
                container.RegisterType<SQLSPService>().As<ISQLSPService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("MPSecurityCheckDetailsClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("MPSecurityCheckDetailsClientKey");
                container.RegisterType<MPSecurityCheckDetailsService>().As<IMPSecurityCheckDetailsService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("UnitedClubMembershipService").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("UnitedClubMembershipKey");
                container.RegisterType<UnitedClubMembershipService>().As<IUnitedClubMembershipService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("CustomerDataClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("CustomerDataClientKey");
                container.RegisterType<CustomerDataService>().As<ICustomerDataService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("CustomerProfileClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("CustomerProfileClientKey");
                container.RegisterType<CustomerProfileService>().As<ICustomerProfileService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("MemberInfoRecommendationClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("MemberInfoRecommendationClientKey");
                container.RegisterType<MemberInfoRecommendationService>().As<IMemberInfoRecommendationService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("CMSContentDataClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("CMSContentDataClientKey");
                container.RegisterType<CMSContentService>().As<ICMSContentService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("dpTokenValidateConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("dpTokenValidateKey");
                container.RegisterType<DPTokenValidationService>().As<IDPTokenValidationService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("contactPointConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("contactPointConfigKey");
                container.RegisterType<ContactPointService>().As<IContactPointService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("MemberProfileClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("MemberProfileClientKey");
                container.RegisterType<MemberProfileService>().As<IMemberProfileService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("UCBProfileClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("UCBProfileClientKey");
                container.RegisterType<UCBProfileService>().As<IUCBProfileService>().WithAttributeFiltering();

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
                        await context.Response.WriteAsync("Welcome from MemberSign in Microservice").ConfigureAwait(false);
                    });
                });

                applicationEnricher.Add(Constants.ServiceNameText, Program.Namespace);
                applicationEnricher.Add(Constants.EnvironmentText, env.EnvironmentName);
                if (Configuration.GetValue<bool>("EnableFeatureSettingsChanges"))
                {
                    applicationLifetime.ApplicationStarted.Register(async () => await OnStart(featureSettings));
                    applicationLifetime.ApplicationStopping.Register(async () => await OnShutDown(featureSettings));
                }
                app.UseStaticFiles();
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Images")),
                    RequestPath = "/membersigninservice/Images"
                });
                app.UseDirectoryBrowser(new DirectoryBrowserOptions
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Images")),
                    RequestPath = "/membersigninservice/Images"
                });

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
                await featureSettings.LoadFeatureSettings(United.Mobile.Model.Common.ServiceNames.MEMBERSIGNIN.ToString());
            }
            catch (Exception) { }

        }
        private async Task OnShutDown(IFeatureSettings featureSettings)
        {
            try
            {
                await featureSettings.DeleteContainerIPAdress(United.Mobile.Model.Common.ServiceNames.MEMBERSIGNIN.ToString(), StaticDataLoader._ipAddress);
            }
            catch { }
        }
    }
}
