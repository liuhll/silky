using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Caching.StackExchangeRedis;
using Silky.Core.Modularity;

namespace Silky.Transaction.Repository.Redis
{
    [DependsOn(typeof(RedisCachingModule))]
    public class RedisTransactionRepositoryModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddRedisTransactionRepository(configuration.GetRedisCacheOptions());
        }
        
    }
}