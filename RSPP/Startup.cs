
//using BHP.Controllers.RequestProposal;
using RSPP.Models.DB;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Rotativa.AspNetCore;
using System;
using RSPP.Exceptions;
using RSPP.UnitOfWorks;
using RSPP.UnitOfWorks.Interfaces;

namespace RSPP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<RSPPdbContext>(options => options
            .UseSqlServer(Configuration.GetConnectionString("RSPPConnectionString"))
            //.EnableSensitiveDataLogging()
            );
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            //AddHostedService

            //services.AddHostedService<PaymentConfirmationService>();
            //services.AddHostedService<ExpiryCertificateReminderService>();
            services.AddDistributedMemoryCache();

            services.ConfigureApplicationCookie(options => options.LoginPath = "/Home/Index");


            //services.ConfigureApplicationCookie(options =>
            //{
            //    // Cookie settings
            //    options.Cookie.HttpOnly = true;
            //    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            //    options.LoginPath = "/Identity/Account/Login";
            //    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            //    options.SlidingExpiration = true;
            //});


            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);//You can set Time   
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;

            });

            services.Configure<IISOptions>(options =>
            {
                options.AutomaticAuthentication = false;
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddControllersWithViews(options => options.Filters.Add(new ExceptionHandlingFilter()))
                .AddRazorRuntimeCompilation();

            services.AddControllersWithViews().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/Home/Index");
                });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [Obsolete]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Microsoft.AspNetCore.Hosting.IHostingEnvironment env2)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error404");
                //app.UseDeveloperExceptionPage();
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.Use(async (context, next) =>
            {

                await next();

                if (context.Response.StatusCode == 404)
                {
                    var requestedPath = context.Request.Host + context.Request.Path;
                    context.Request.Headers.Add("4o4Path", $" Method=> {context.Request.Method}  Path=>{requestedPath}");
                    context.Request.Path = "/Home/Error404";

                    await next();
                }
            });

            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

            });


            RotativaConfiguration.Setup(env2);
        }
    }
}

