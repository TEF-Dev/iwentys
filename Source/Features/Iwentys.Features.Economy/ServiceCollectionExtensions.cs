﻿using Iwentys.Features.Economy.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Iwentys.Features.Economy
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIwentysEconomyFeatureServices(this IServiceCollection services)
        {
            services.AddScoped<BarsPointTransactionLogService>();

            return services;
        }
    }
}