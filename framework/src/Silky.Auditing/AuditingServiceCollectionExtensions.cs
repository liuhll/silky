using Microsoft.Extensions.Configuration;
using Silky.Auditing;
using Silky.Auditing.Filters;
using Silky.Rpc.Configuration;
using Silky.Rpc.Runtime.Server;

namespace Microsoft.Extensions.DependencyInjection;

public static class AuditingServiceCollectionExtensions
{
    public static IServiceCollection AddAuditing(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AuditingOptions>()
            .Bind(configuration.GetSection(AuditingOptions.Auditing));
        services.AddTransient<IAuditSerializer, JsonNetAuditSerializer>();
        services.AddScoped<IServerFilter, AuditingFilter>();
        return services;
    }
}