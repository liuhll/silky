using System;
using Silky.Lms.Core;
using Silky.Lms.HttpServer.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Lms.HttpServer.Configuration;

namespace Silky.Lms.HttpServer
{
    public class HttpServerLmsStartup : ILmsStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<GatewayOptions>()
                .Bind(configuration.GetSection(GatewayOptions.Gateway));
        }

        public void Configure(IApplicationBuilder application)
        {
            application.UseLmsExceptionHandler();
            application.UseLms();
        }

        public int Order { get; } = Int32.MinValue;
    }
}