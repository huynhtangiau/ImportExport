using System;
using ImportExport.Service.Interfaces;
using ImportExport.Service.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ImportExport.Service
{
    public static class DependencyInjectionServices
    {
        public static  void ServicesRegister(this IServiceCollection services)
        {
            services.AddScoped<IRefundService, RefundService>();
            services.AddScoped<ILicenseService, LicenseService>();
            
        }
    }
}
