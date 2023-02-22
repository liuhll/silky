using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Core.Modularity;

namespace Silky.Caching.StackExchangeRedis
{
    [DependsOn(typeof(CachingModule))]
    public class RedisCachingModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var redisEnabled = configuration.GetValue<bool>("DistributedCache:Redis:IsEnabled");
            if (redisEnabled)
            {
                var redisCacheOptions = configuration.GetRedisCacheOptions();
                services.AddRedisCaching(redisCacheOptions);
            }
        }
    }
}