using Microsoft.AspNetCore.Builder;

namespace Silky.Lms.WebSocketServer.Middleware
{
    public static class WebSocketsProxyMiddlewareExtensions
    {
        public static IApplicationBuilder UseWebSocketsProxyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebSocketsProxyMiddleware>();
        }
    }
}