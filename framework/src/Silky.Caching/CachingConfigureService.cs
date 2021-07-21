using System;
using Silky.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Caching.Configuration;

namespace Silky.Caching
{
    public class CachingConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            services.AddSingleton(typeof(IDistributedCache<>), typeof(DistributedCache<>));
            services.AddSingleton(typeof(IDistributedCache<,>), typeof(DistributedCache<,>));
            services.AddTransient(typeof(IDistributedInterceptCache), typeof(DistributedInterceptCache));
            services.AddOptions<SilkyDistributedCacheOptions>()
                .Bind(configuration.GetSection(SilkyDistributedCacheOptions.SilkyDistributedCache));
        }

        public int Order { get; } = 2;
    }
}