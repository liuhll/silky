using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Silky.Core.Extensions;
using Silky.Core.Modularity;

namespace Silky.Caching.StackExchangeRedis
{
    [DependsOn(typeof(CachingModule))]
    public class RedisCachingModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            var redisEnabled = configuration["DistributedCache:Redis:IsEnabled"];
            
            if (redisEnabled.IsNullOrEmpty() || bool.Parse(redisEnabled))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    var redisConfiguration = configuration["DistributedCache:Redis:Configuration"];
                    if (!redisConfiguration.IsNullOrEmpty())
                    {
                        options.Configuration = redisConfiguration;
                    }
                });

                services.Replace(ServiceDescriptor.Singleton<IDistributedCache, SilkyRedisCache>());
            }
        }
    }
}