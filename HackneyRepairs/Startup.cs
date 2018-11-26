using System.Configuration;
using HackneyRepairs.Tests;
using HackneyRepairs.DbContext;
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
using System.IO;
using HackneyRepairs.Extension;
using System.Reflection;

namespace HackneyRepairs
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();
			TestStatus.IsRunningInTests = false;    
		}

		public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<UhtDbContext>(options =>
                                                options.UseSqlServer(Configuration.GetSection("UhtDb").Value));
            services.AddDbContext<UhwDbContext>(options =>
                                                options.UseSqlServer(Configuration.GetSection("UhwDb").Value));
            services.AddDbContext<UHWWarehouseDbContext>(options =>
                                                         options.UseSqlServer(Configuration.GetSection("UhWarehouseDb").Value));
            services.AddDbContext<DRSDbContext>(options =>
                                                options.UseMySql(Configuration.GetSection("DRSDb").Value));
            
            services.AddMvc();
            services.AddSwaggerGen(c =>
            {
				c.SwaggerDoc("v1", new Info { Version = "v1", Title = $"Hackney Repairs API - {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}",
                    Description="This is the Hackney Repairs API which allows client applications " +
                        "to securely access publicly available information on repairs to Hackney properties, " +
                        "and to raise new repair requests." });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddCors(option => {
                option.AddPolicy("AllowAny", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

			});
			services.AddCustomServices();
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
			switch (environment)
			{
				case "Production":
					app.UseSwagger(
                        c => c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Host = "api.hackney.gov.uk/unboxedhackneyrepairs/")
                    );
					app.UseSwaggerUI(c =>
                    {
                        string basePath = Environment.GetEnvironmentVariable("ASPNETCORE_APPL_PATH");
                        if (basePath == null) basePath = "/unboxedhackneyrepairs/";
                        c.SwaggerEndpoint($"{basePath}swagger/v1/swagger.json", "Hackney Repairs API");
                    });
					break;
				case "Test":
					app.UseSwagger(
                        c => c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Host = "sandboxapi.hackney.gov.uk/unboxedhackneyrepairs/")
                    );
					app.UseSwaggerUI(c =>
                    {
                        string basePath = Environment.GetEnvironmentVariable("ASPNETCORE_APPL_PATH");
                        if (basePath == null) basePath = "/unboxedhackneyrepairs/";
                        c.SwaggerEndpoint($"{basePath}swagger/v1/swagger.json", "Hackney Repairs API");
                    });
                    break;
				case "Development":
					app.UseSwagger(
						c => c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Host = "sandboxapi.hackney.gov.uk/unboxedhackneyrepairs_dev/")
                    );
                    app.UseSwaggerUI(c =>
                    {
                        string basePath = Environment.GetEnvironmentVariable("ASPNETCORE_APPL_PATH");
                        if (basePath == null) basePath = "/unboxedhackneyrepairs_dev/";
                        c.SwaggerEndpoint($"{basePath}swagger/v1/swagger.json", "Hackney Repairs API");
                    });
                    break;
				case "Local":
					app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        string basePath = Environment.GetEnvironmentVariable("ASPNETCORE_APPL_PATH");
                        if (basePath == null) basePath = "/";
                        c.SwaggerEndpoint($"{basePath}swagger/v1/swagger.json", "Hackney Repairs API");
                    });
                    break;
				default:
					app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        string basePath = Environment.GetEnvironmentVariable("ASPNETCORE_APPL_PATH");
                        if (basePath == null) basePath = "/";
                        c.SwaggerEndpoint($"{basePath}swagger/v1/swagger.json", "Hackney Repairs API");
                    });
					break;
			}
		}
	}
}
