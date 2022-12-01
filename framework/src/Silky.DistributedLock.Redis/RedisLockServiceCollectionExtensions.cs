using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Silky.Core.DependencyInjection;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection;

public static class RedisLockServiceCollectionExtensions
{
    public static IServiceCollection AddRedisLock(this IServiceCollection services,
        RedisCacheOptions redisCacheOption, int lockStoreDbIndex = 15)
    {
        if (!services.IsAdded<IDistributedLockProvider>())
        {
            var connection = ConnectionMultiplexer.Connect(redisCacheOption.Configuration);
            var distributedLockProvider =
                new RedisDistributedSynchronizationProvider(connection.GetDatabase(lockStoreDbIndex));
            services.AddSingleton<IDistributedLockProvider>(distributedLockProvider);
        }

        return services;
    }
}