using Lms.Core;
using Lms.Core.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Lms.Caching.StackExchangeRedis
{
    public class RedisCachingConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
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

                services.Replace(ServiceDescriptor.Singleton<IDistributedCache, LmsRedisCache>());
            }
        }

        public int Order { get; } = 3;
    }
}