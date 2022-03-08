using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Silky.HealthChecks.Rpc;

namespace Microsoft.AspNetCore.Builder
{
    public static class SilkyRpcHealthCheckApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSilkyRpcHealthCheck(this IApplicationBuilder app,
            string path = "/silkyrpc/healthz")
        {
            app.UseHealthChecks(path, new HealthCheckOptions
            {
                Predicate = (check) => check.Name.Equals(SilkyRpcHealthCheckBuilderExtensions.SILKYRPC_NAME),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            return app;
        }
        
        public static IApplicationBuilder UseSilkyGatewayHealthCheck(this IApplicationBuilder app,
            string path = "/silkygateway/healthz")
        {
            app.UseHealthChecks(path, new HealthCheckOptions
            {
                Predicate = (check) => check.Name.Equals(SilkyRpcHealthCheckBuilderExtensions.SILKYGATEWAT_NAME),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            return app;
        }

        public static IApplicationBuilder UseSilkyRpcHealthCheckApi(this IApplicationBuilder app,
            string path = "/api/silkyrpc/healthz")
        {
            app.UseHealthChecks(path, new HealthCheckOptions
            {
                Predicate = (check) => check.Name.Equals(SilkyRpcHealthCheckBuilderExtensions.SILKYRPC_NAME),
                ResponseWriter = SilkyRpcApiResponseWriter.ResponseWriter
            });
            return app;
        }
        
        public static IApplicationBuilder UseSilkyGatewayHealthCheckApi(this IApplicationBuilder app,
            string path = "/silkygateway/healthz")
        {
            app.UseHealthChecks(path, new HealthCheckOptions
            {
                Predicate = (check) => check.Name.Equals(SilkyRpcHealthCheckBuilderExtensions.SILKYGATEWAT_NAME),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            return app;
        }
    }
}