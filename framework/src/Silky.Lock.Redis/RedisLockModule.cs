using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Caching.StackExchangeRedis;
using Silky.Core.Modularity;

namespace Silky.Lock.Redis;

[DependsOn(typeof(LockModule))]
public class RedisLockModule : SilkyModule
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRedisLock(configuration.GetRedisCacheOptions());
    }
}