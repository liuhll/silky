using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Silky.Caching.Configuration;
using Silky.Core.Modularity;

namespace Silky.Caching
{
    public class CachingModule : SilkyModule
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            services.AddSingleton(typeof(IDistributedCache<>), typeof(DistributedCache<>));
            services.AddSingleton(typeof(IDistributedCache<,>), typeof(DistributedCache<,>));
            services.AddTransient(typeof(IDistributedInterceptCache), typeof(DistributedInterceptCache));
            services.AddOptions<SilkyDistributedCacheOptions>()
                .Bind(configuration.GetSection(SilkyDistributedCacheOptions.SilkyDistributedCache));
        }
    }
}