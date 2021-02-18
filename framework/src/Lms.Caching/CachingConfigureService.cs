using System;
using Lms.Caching.Configuration;
using Lms.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Caching
{
    public class CachingConfigureService : IConfigureService
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            services.AddSingleton(typeof(IDistributedCache<>), typeof(DistributedCache<>));
            services.AddSingleton(typeof(IDistributedCache<,>), typeof(DistributedCache<,>));

            services.Configure<LmsDistributedCacheOptions>(cacheOptions =>
            {
                //cacheOptions.GlobalCacheEntryOptions.SlidingExpiration = TimeSpan.FromMinutes(20);
            });
        }

        public int Order { get; } = 2;
    }
}