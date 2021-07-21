using Microsoft.AspNetCore.Builder;

namespace Silky.WebSocketServer.Middleware
{
    public static class WebSocketsProxyMiddlewareExtensions
    {
        public static IApplicationBuilder UseWebSocketsProxyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebSocketsProxyMiddleware>();
        }
    }
}