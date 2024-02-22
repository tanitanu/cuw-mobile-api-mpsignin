using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.Tools;
using United.Mobile.MPSignInTool.DataAccess;
using United.Mobile.MPSignInTool.DataAccess.CustomImplementation;
using United.Mobile.MPSignInTool.Domain;
using United.Mobile.MPSignInTool.Domain.HealthCheck;
using United.Mobile.MPSignInTool.Models;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.MPSignInTool
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
            services.AddTransient<IUserStore<AppUser>, UserStore>();
            services.AddTransient<IRoleStore<AppRole>, RoleStore>();

            services.AddIdentity<AppUser, AppRole>(config =>
            {
                // User defined password policy settings.  
                config.Password.RequiredLength = 4;
                config.Password.RequireDigit = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
                config.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";
            }).AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Name = "ProjectCookie";
                config.LoginPath = "/mpsignintoolsservice/signin"; // User defined login path  
                config.LogoutPath = "/mpsignintoolsservice/";
                config.AccessDeniedPath = "/mpsignintoolsservice/SignIn/AccessDenied";
                config.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            });

            services.AddScoped<IMPSignInToolBusiness, MPSignInToolBusiness>();
            services.AddScoped<IQAHealthCheckReport, QAHealthCheckReport>();
            services.AddScoped<IDevHealthCheckReport, DevHealthCheckReport>();
            services.AddScoped<IStageHealthCheckReport, StageHealthCheckReport>();
            services.AddScoped<IProdHealthCheckReport, ProdHealthCheckReport>();
            services.AddScoped<ICatalogBusiness, CatalogBusiness>();

            services.AddScoped<IDynamoSearchToolBusiness, DynamoSearchToolBusiness>();
            services.AddScoped<IAccountManager, AccountManager>();
            services.AddScoped<IDbHelper, DbHelper>();
            services.AddScoped<CacheLogWriter>();
            services.AddScoped(typeof(ICacheLog<>), typeof(CacheLog<>));
            services.AddControllersWithViews();            
        }

        public void ConfigureContainer(ContainerBuilder container)
        {
            try
            {
                container.Register(c => new ResilientClient(Configuration.GetSection("DevelopmentClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("DevelopmentClientKey");
                container.RegisterType<DevelopmentService>().As<IDevelopmentService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("QAClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("QAClientKey");
                container.RegisterType<QAService>().As<IQAService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("StageClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("StageClientKey");
                container.RegisterType<StageService>().As<IStageService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("ProdClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("ProdClientKey");
                container.RegisterType<ProdService>().As<IProdService>().WithAttributeFiltering();


                container.Register(c => new ResilientClient(Configuration.GetSection("OnPremSQLDevelopmentClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("OnPremSQLDevelopmentClientKey");
                container.RegisterType<DevelopmentOnPremService>().As<IDevelopmentOnPremService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("OnPremSQLQAClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("OnPremSQLQAClientKey");
                container.RegisterType<QAOnPremService>().As<IQAOnPremService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("OnPremSQLStageClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("OnPremSQLStageClientKey");
                container.RegisterType<StageOnPremService>().As<IStageOnPremService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("OnPremSQLPRODClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("OnPremSQLPRODClientKey");
                container.RegisterType<ProdOnPremService>().As<IProdOnPremService>().WithAttributeFiltering();

                container.Register(c => new ResilientClient(Configuration.GetSection("DynamoDBClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("DynamoDBClientKey");
                container.RegisterType<DynamoDBService>().As<IDynamoDBService>().WithAttributeFiltering();

            }
            catch (Exception ex)
            {
                Log.Logger.Fatal(ex, "ConfigureContainer-Unexpected error occurred while starting services");
            }

        }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "mpsignintoolsservice/{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
