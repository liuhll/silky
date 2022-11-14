using Microsoft.Extensions.Configuration;
using Silky.Rpc.Auditing;
using Silky.Rpc.Auditing.Filters;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime.Server;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RpcServiceCollectionExtensions
    {
        public static IServiceCollection AddAuditing(this IServiceCollection services, IConfiguration configuration)
            {
                services.AddOptions<AuditingOptions>()
                    .Bind(configuration.GetSection(AuditingOptions.Auditing));
                services.AddSingleton<IAuditSerializer, JsonNetAuditSerializer>();
                services.AddScoped<IServerFilter, AuditingFilter>();
                return services;
            }
    }
}