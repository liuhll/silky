using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Silky.HealthChecks.Rpc
{
    public static class SilkyRpcHealthCheckEndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapSilkyRpcHealthChecks(this IEndpointRouteBuilder endpoints,
            string pattern = "/api/silkyrpc/health")
        {
            return endpoints.MapHealthChecks(pattern, new HealthCheckOptions()
            {
                Predicate = (check) => check.Name.Equals(SilkyRpcHealthCheckBuilderExtensions.SILKYRPC_NAME),
                ResponseWriter = SilkyRpcApiResponseWriter.ResponseWriter
            }).RequireAuthorization();
        }
        
        public static IEndpointConventionBuilder MapSilkyGatewayHealthChecks(this IEndpointRouteBuilder endpoints,
            string pattern = "/api/silkygateway/health")
        {
            return endpoints.MapHealthChecks(pattern, new HealthCheckOptions()
            {
                Predicate = (check) => check.Name.Equals(SilkyRpcHealthCheckBuilderExtensions.SILKYGATEWAT_NAME),
                ResponseWriter = SilkyRpcApiResponseWriter.ResponseWriter
            }).RequireAuthorization();
        }
    }
}