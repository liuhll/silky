using System;
using Silky.Core;
using Silky.Http.Core.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Http.Core.Configuration;
using Silky.WebSocket.Middleware.Middleware;

namespace Silky.Http.Core
{
    public class HttpServerSilkyStartup : ISilkyStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<GatewayOptions>()
                .Bind(configuration.GetSection(GatewayOptions.Gateway));
            services.AddResponseCaching();
           
        }

        public async void Configure(IApplicationBuilder application)
        {
            application.UseResponseCaching();
            application.UseHttpsRedirection();
            application.UseWebSockets();
            application.MapWhen(httpContext => httpContext.WebSockets.IsWebSocketRequest,
                wenSocketsApp => { wenSocketsApp.UseWebSocketsProxyMiddleware(); });
            application.UseSilkyExceptionHandler();
            application.UseSilky();
            application.RegisterHttpRoutes();
        }

        public int Order { get; } = Int32.MaxValue;
    }
}