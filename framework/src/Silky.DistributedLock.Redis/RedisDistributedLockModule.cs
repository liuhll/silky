using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Caching.StackExchangeRedis;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Modularity;

namespace Silky.DistributedLock.Redis;

public class RedisDistributedLockModule : SilkyModule
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var redisEnabled = configuration.GetValue<bool>("DistributedCache:Redis:IsEnabled");
        if (redisEnabled)
        {
            services.AddRedisLock(configuration.GetRedisCacheOptions());
        }
    }
}