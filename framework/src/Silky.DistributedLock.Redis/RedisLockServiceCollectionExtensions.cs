using Silky.Core.DependencyInjection;
using Silky.DistributedLock.Redis;

namespace Microsoft.Extensions.DependencyInjection;

public static class RedisLockServiceCollectionExtensions
{
    public static IServiceCollection AddRedisLock(this IServiceCollection services)
    {
        if (!services.IsAdded<IRedisDistributedLockProvider>())
        {
            services.AddSingleton<IRedisDistributedLockProvider, DefaultRedisDistributedLockProvider>();
        }

        return services;
    }
}