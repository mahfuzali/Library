﻿using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


namespace Library.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
