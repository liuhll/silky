using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Silky.Core;
using Silky.Core.Extensions;
using Silky.Core.Serialization;

namespace Silky.HealthChecks.Rpc
{
    public static class SilkyRpcHealthCheckEndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapSilkyRpcHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapHealthChecks("/rpc-health", new HealthCheckOptions()
            {
                Predicate = (check) => check.Name.Equals(SilkyRpcHealthCheckBuilderExtensions.SILKYRPC_NAME),
                ResponseWriter = ResponseWriter
            }).RequireAuthorization();
            //.RequireAuthorization();
        }

        private static Task ResponseWriter(HttpContext httpContext, HealthReport healthReport)
        {
            httpContext.Response.ContentType = "application/json;charset=utf-8";
            var serializer = EngineContext.Current.Resolve<ISerializer>();

            var silkyRpcHealthCheckResult = new SilkyRpcHealthCheckResult()
            {
                Status = healthReport.Status,
                TotalDuration = healthReport.TotalDuration,
                HealthData = healthReport.Entries.SelectMany(p => p.Value.Data.Values.Select(p => (ServerHealthData)p))
                    .ToArray()
            };
            var responseResultData = serializer.Serialize(silkyRpcHealthCheckResult);
            httpContext.Response.ContentLength = responseResultData.GetBytes().Length;
            httpContext.Response.StatusCode = 200;

            return httpContext.Response.WriteAsync(responseResultData);
        }
    }
}