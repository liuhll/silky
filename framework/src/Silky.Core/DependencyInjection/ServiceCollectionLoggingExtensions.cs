using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Silky.Core.Logging;

namespace Silky.Core.DependencyInjection;

public static class ServiceCollectionLoggingExtensions
{
    public static ILogger<T> GetInitLogger<T>(this IServiceCollection services)
    {
        return services.GetSingletonInstance<IInitLoggerFactory>().Create<T>();
    }
}