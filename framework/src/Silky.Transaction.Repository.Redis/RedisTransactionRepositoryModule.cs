using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Caching.StackExchangeRedis;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Modularity;

namespace Silky.Transaction.Repository.Redis
{
    [DependsOn(typeof(RedisCachingModule))]
    public class RedisTransactionRepositoryModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var redisEnabled = configuration.GetValue<bool>("DistributedCache:Redis:IsEnabled");
            if (redisEnabled)
            {
                services.AddRedisTransactionRepository(configuration.GetRedisCacheOptions());
            }
        }
    }
}