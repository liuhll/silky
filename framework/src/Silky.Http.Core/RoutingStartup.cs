using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Silky.Http.Core
{
    public class RoutingStartup : ISilkyStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder application)
        {
            application.UseRouting();
        }

        public int Order { get; } = Int32.MinValue;
    }
}