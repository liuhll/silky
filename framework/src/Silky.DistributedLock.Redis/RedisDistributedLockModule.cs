using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;

namespace Silky.DistributedLock.Redis;

public class RedisDistributedLockModule : SilkyModule
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRedisLock();
    }
}