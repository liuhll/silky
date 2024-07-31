using Silky.Http.Core.Middlewares;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseSilkyWrapperResponse(this IApplicationBuilder application)
        {
            application.UseMiddleware<WrapperResponseMiddleware>();
        }
        
        public static IApplicationBuilder UseSilkyWebSocketsProxy(this IApplicationBuilder application)
        {
            application.UseWebSockets();
            application.MapWhen(httpContext => httpContext.WebSockets.IsWebSocketRequest,
                wenSocketsApp => { wenSocketsApp.UseWebSocketsProxyMiddleware(); });
            return application;
        }

        private static IApplicationBuilder UseWebSocketsProxyMiddleware(this IApplicationBuilder application)
        {
            return application.UseMiddleware<WebSocketsProxyMiddleware>();
        }
    }
}