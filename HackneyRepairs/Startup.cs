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
            services.AddMvc();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Version = "v1", Title = "Hackney Repairs API", 
                    Description="This is the Hackney Repairs API which allows client applications </br>" +
                        "to securely access publicly available information on repairs to Hackney properties, </br>" +
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
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                string basePath = Environment.GetEnvironmentVariable("ASPNETCORE_APPL_PATH");
                if (basePath == null) basePath = "/";
                c.SwaggerEndpoint($"{basePath}swagger/v1/swagger.json", "HackneyRepairsAPI");
                c.RoutePrefix = string.Empty;
            });
        }
    }
}
