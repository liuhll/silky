using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Caching.StackExchangeRedis;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Modularity;

namespace Silky.Transaction.Repository.Redis
{
    [DependsOn(typeof(RedisCachingModule))]
    public class RedisTransactionRepositoryModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var redisEnabled = configuration.GetValue<bool>("DistributedCache:Redis:IsEnabled");

            if (!redisEnabled)
            {
                throw new SilkyException("The redis cache service is unavailable");
            }

            var redisCacheOptions = configuration.GetSection("DistributedCache:Redis").Get<RedisCacheOptions>();
            if (redisCacheOptions == null || redisCacheOptions.Configuration.IsNullOrEmpty())
            {
                throw new SilkyException("You must specify the Configuration of the redis service");
            }

            services.AddRedisTransactionRepository(redisCacheOptions);
        }
    }
}