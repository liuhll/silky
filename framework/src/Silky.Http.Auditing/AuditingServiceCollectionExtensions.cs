using Silky.Http.Auditing;

namespace Microsoft.Extensions.DependencyInjection;

public static class AuditingServiceCollectionExtensions
{
    public static IServiceCollection AddAuditing<T>(this IServiceCollection services)
        where T : class, IAuditingStore
    {
        services.AddScoped<IAuditingStore, T>();
        return services;
    }
}