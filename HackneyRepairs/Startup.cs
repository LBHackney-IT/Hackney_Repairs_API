using HackneyRepairs.DbContext;
using HackneyRepairs.Infrastructure;
using HackneyRepairs.Extension;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Logging;
using HackneyRepairs.Settings;
using HackneyRepairs.Tests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using StackExchange.Redis;

namespace HackneyRepairs
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();
			TestStatus.IsRunningInTests = false;    
		}

		public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ConfigurationSettings>(Configuration);
            var settings = Configuration.Get<ConfigurationSettings>();

            // Add framework services.
            services.AddDbContext<UhtDbContext>(options =>
                                                options.UseSqlServer(Configuration.GetSection("UhtDb").Value));
            services.AddDbContext<UhwDbContext>(options =>
                                                options.UseSqlServer(Configuration.GetSection("UhwDb").Value));
            services.AddDbContext<UHWWarehouseDbContext>(options =>
                                                         options.UseSqlServer(Configuration.GetSection("UhWarehouseDb").Value));
            services.AddDbContext<UhWebDbContext>(options =>
                                                         options.UseSqlServer(Configuration.GetSection("UhWebDb").Value));
            services.AddDbContext<DRSDbContext>(options =>
                                                options.UseMySql(Configuration.GetSection("DRSDb").Value));
            //services.AddSingleton(cacheManager => new CacheManager(Configuration.GetSection("RepairsCacheEndpoint").Value));   
            services.AddSingleton(CacheManager => new CacheManager(new ConfigurationOptions { EndPoints = { Configuration.GetSection("RepairsCacheEndpoint").Value }, Password = Configuration.GetSection("RepairsCachePassword").Value }));

            services.AddMvc();
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Token",
                  new ApiKeyScheme
                  {
                      In = "header",
                      Description = "Your Hackney API Key",
                      Name = "X-Api-Key",
                      Type = "apiKey"
                  });

                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Token", Enumerable.Empty<string>() }
                });

                c.SwaggerDoc("v1", new Info { Title = $"Hackney Repairs API - {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}", Version = "v1", Description = "This is the Hackney Repairs API which allows client applications to securely access publicly available information on repairs to Hackney properties, and to raise new repair requests." });
                //c.SwaggerDoc("v1", new Info 
                //{ 
                //    Version = "v1", Title = $"Hackney Repairs API - {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}",
                //    Description = "This is the Hackney Repairs API which allows client applications " +
                //        "to securely access publicly available information on repairs to Hackney properties, " +
                //        "and to raise new repair requests." 
                //});
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddCors(option => 
            {
                option.AddPolicy("AllowAny", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
			});
			services.AddCustomServices();

	        if (!string.IsNullOrEmpty(settings.SentrySettings?.Url))
	        {
		        var sentryLoggerProvider =
			        new SentryLoggerProvider(settings.SentrySettings?.Url, settings.SentrySettings?.Environment);
		        services.AddTransient<IExceptionLogger>(_ => sentryLoggerProvider.CreateExceptionLogger());

		        services.AddLogging(configure => { configure.AddProvider(sentryLoggerProvider); });
	        }
	        else
	        {
	            services.AddTransient<IExceptionLogger, NullExceptionLogger>();
	        }
        }

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddNLog();
			env.ConfigureNLog("NLog.config");
			app.UseCors("AllowAny");
			app.UseMvc();
			app.UseDeveloperExceptionPage();

			string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string routePrefix = Environment.GetEnvironmentVariable("SWAGGER_ROUTE_PREFIX");
            string swaggerEndpoint = Environment.GetEnvironmentVariable("SWAGGER_ENDPOINT");

            if (swaggerEndpoint != null && routePrefix != null)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
              {
                  c.SwaggerEndpoint(swaggerEndpoint, "Hackney Repairs API");
                  c.RoutePrefix = routePrefix;
              });
            }
            else
            {
                switch (environment)
                {
                    case "Production":
                        app.UseSwagger(
                            c => c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Host = "api.hackney.gov.uk/unboxedhackneyrepairs/"));
                        app.UseSwaggerUI(c =>
                        {
                            string basePath = Environment.GetEnvironmentVariable("ASPNETCORE_APPL_PATH");
                            if (basePath == null) basePath = "/unboxedhackneyrepairs/";
                            c.SwaggerEndpoint($"{basePath}swagger/v1/swagger.json", "Hackney Repairs API");
                            c.RoutePrefix = string.Empty;
                        });
                        break;
                    case "Test":
                        app.UseSwagger(
                            c => c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Host = "sandboxapi.hackney.gov.uk/unboxedhackneyrepairs/"));
                        app.UseSwaggerUI(c =>
                        {
                            string basePath = Environment.GetEnvironmentVariable("ASPNETCORE_APPL_PATH");
                            if (basePath == null) basePath = "/unboxedhackneyrepairs/";
                            c.SwaggerEndpoint($"{basePath}swagger/v1/swagger.json", "Hackney Repairs API");
                            c.RoutePrefix = string.Empty;
                        });
                        break;
                    case "Development":
                        app.UseSwagger();
                        app.UseSwaggerUI(c =>
                        {
                            c.SwaggerEndpoint("v1/swagger.json", "Hackney Repairs API");
                            c.RoutePrefix = "swagger";
                        });
                        //app.UseSwaggerUI(c =>
                        //{
                        //    string basePath = Environment.GetEnvironmentVariable("ASPNETCORE_APPL_PATH");
                        //    if (basePath == null) basePath = "/unboxedhackneyrepairs_dev/";
                        //    c.SwaggerEndpoint($"{basePath}swagger/v1/swagger.json", "Hackney Repairs API");
                        //    c.RoutePrefix = string.Empty;
                        //});
                        break;
                    case "Local":
                        app.UseSwagger();
                        app.UseSwaggerUI(c =>
                        {
                            string basePath = Environment.GetEnvironmentVariable("ASPNETCORE_APPL_PATH");
                            if (basePath == null) basePath = "/";
                            c.SwaggerEndpoint($"{basePath}swagger/v1/swagger.json", "Hackney Repairs API");
                            c.RoutePrefix = string.Empty;
                        });
                        break;
                    default:
                        app.UseSwagger();
                        app.UseSwaggerUI(c =>
                        {
                            string basePath = Environment.GetEnvironmentVariable("ASPNETCORE_APPL_PATH");
                            if (basePath == null) basePath = "/";
                            c.SwaggerEndpoint($"{basePath}swagger/v1/swagger.json", "Hackney Repairs API");
                            c.RoutePrefix = string.Empty;
                        });
                        break;
                }
            }
		}
	}
}
