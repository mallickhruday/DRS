﻿using AutoMapper;
using FluentValidation.AspNetCore;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Serilog;
using StructureMap;
using System;
using System.IO;
using Web.Engine;
using Web.Engine.Filters;
using Web.Engine.Services;
using Web.Engine.Services.Hangfire;
using Web.Engine.Services.Hangfire.Jobs;
using Web.Engine.ViewEngine;
using Web.Models;

namespace Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.LiterateConsole()
                .WriteTo.Async(a => a.RollingFile(Path.Combine(env.ContentRootPath, "logs/log-{Date}.txt")))
                .CreateLogger();

            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            DefaultConnectionString = Configuration.GetConnectionString("DefaultConnection");
        }

        private static IConfigurationRoot Configuration { get; set; }
        private static string DefaultConnectionString { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(DefaultConnectionString, o => o.EnableRetryOnFailure()));

            services.Configure<DRSConfig>(Configuration.GetSection("DRS"));
            services.Configure<Engine.Services.Lucene.Config>(Configuration.GetSection("DRS"));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddHangfire(x => x.UseSqlServerStorage(DefaultConnectionString));

            services
                .AddMvc(
                    options =>
                    {
                        options.Conventions.Add(new FeatureConvention());
                        options.Filters.Add(new ValidateModelStateFilter());
                        options.Filters.Add(new ApiExceptionFilter());
                    })
                .AddRazorOptions(options =>
                {
                    // {0} - Action Name
                    // {1} - Controller Name
                    // {2} - Area Name
                    // {3} - Feature Name
                    //options.AreaViewLocationFormats.Clear();
                    options.AreaViewLocationFormats.Add("/Areas/{2}/Features/{3}/{1}/{0}.cshtml");
                    options.AreaViewLocationFormats.Add("/Areas/{2}/Features/{3}/{0}.cshtml");
                    options.AreaViewLocationFormats.Add("/Areas/{2}/Features/Shared/{0}.cshtml");
                    options.AreaViewLocationFormats.Add("/Areas/Shared/{0}.cshtml");
                    // replace normal view location entirely
                    //options.ViewLocationFormats.Clear();
                    options.ViewLocationFormats.Add("/Features/{3}/{1}/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Features/{3}/{0}.cshtml");
                    options.ViewLocationFormats.Add("/Features/Shared/{0}.cshtml");

                    options.ViewLocationExpanders.Add(new FeatureViewLocationExpander());
                })
                .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver())
                .AddFluentValidation(cfg => cfg.RegisterValidatorsFromAssemblyContaining<Startup>());

            services.AddAutoMapper(typeof(Startup));

            Mapper.AssertConfigurationIsValid();

            services.AddMediatR(typeof(Startup));

            var container = new Container(cfg => cfg.AddRegistry<WebRegistry>());

            // populates structuremap with .NET services

            container.Populate(services);

            return container.GetInstance<IServiceProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime,
            IServiceProvider serviceProvider)
        {
            loggerFactory.AddSerilog();
            // Ensure any buffered events are sent at shutdown
            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();

                //app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                //{
                //    HotModuleReplacement = true,
                //    ReactHotModuleReplacement = true
                //});

                // prepopulates database

                app.EnsureSampleData();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseIdentity();

            // Hangfire

            GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(serviceProvider));

            app.UseHangfireServer();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new DashboardAuthorizationFilter() }
            });

            RecurringJob
                .AddOrUpdate<IndexRevisionsJob>(nameof(IndexRevisionsJob)
                , j => j.Run()
                , "*/5 * * * *", TimeZoneInfo.Local);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}");

                //routes.MapSpaFallbackRoute(
                //    "spa-fallback",
                //    "{controller=Documents}/{action=Index}");
            });
        }
    }
}