using Lms.Core;
using Lms.WebSocketServer.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.WebSocketServer
{
    public class WebSocketServerLmsStartup : ILmsStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        public int Order { get; } = 9999;

        public void Configure(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.MapWhen(httpContext => httpContext.WebSockets.IsWebSocketRequest,
                wenSocketsApp => { wenSocketsApp.UseWebSocketsProxyMiddleware(); });
        }
    }
}