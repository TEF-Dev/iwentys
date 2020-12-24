﻿using Iwentys.Features.Companies.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Iwentys.Features.Companies
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIwentysCompanyFeatureServices(this IServiceCollection services)
        {
            services.AddScoped<CompanyService>();

            return services;
        }
    }
}