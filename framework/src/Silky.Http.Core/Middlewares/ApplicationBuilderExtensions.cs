using Microsoft.Extensions.DependencyInjection;
using Silky.Http.Core.Middlewares;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseSilkyErrorHandling(this IApplicationBuilder application)
        {
            application.UseMiddleware<SilkyErrorHandlingMiddleware>();
        }

        public static async void UseSilkyHttpServer(this IApplicationBuilder application)
        {
            var serverRegisterProvider =
                application.ApplicationServices.GetRequiredService<IServerProvider>();
            serverRegisterProvider.AddHttpServices();

            var serverRouteRegister =
                application.ApplicationServices.GetRequiredService<IServerRegister>();
            await serverRouteRegister.RegisterServer();

            var invokeMonitor =
                application.ApplicationServices.GetService<IInvokeMonitor>();
            if (invokeMonitor != null)
            {
                await invokeMonitor.ClearCache();
            }

            var serverHandleMonitor =
                application.ApplicationServices.GetService<IServerHandleMonitor>();
            if (serverHandleMonitor != null)
            {
                await serverHandleMonitor.ClearCache();
            }
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