using Microsoft.AspNetCore.Builder;

namespace Silky.Http.Core.Middlewares
{
    public static class WebSocketsProxyMiddlewareExtensions
    {
        public static IApplicationBuilder UseWebSocketsProxyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Http.Core.Middlewares.WebSocketsProxyMiddleware>();
        }
    }
}