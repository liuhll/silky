using System;
using Silky.Lms.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Lms.Caching.Configuration;

namespace Silky.Lms.Caching
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
            services.AddOptions<LmsDistributedCacheOptions>()
                .Bind(configuration.GetSection(LmsDistributedCacheOptions.LmsDistributedCache));
        }

        public int Order { get; } = 2;
    }
}