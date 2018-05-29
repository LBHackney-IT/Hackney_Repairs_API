﻿using HackneyRepairs.Factories;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Logging;
using HackneyRepairs.Repository;
using HackneyRepairs.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HackneyRepairs.Extension
{
    public static class ServiceConfiguration
    {
        public static void AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
            services.AddTransient<IUhtRepository, UhtRepository>();
            services.AddTransient<IUhwRepository, UhwRepository>();
            services.AddTransient(typeof(IUHWWarehouseRepository), typeof(UHWWarehouseRepository));
        }
    }
}
