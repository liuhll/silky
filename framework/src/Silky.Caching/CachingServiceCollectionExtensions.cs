using Silky.Caching;
using Silky.Caching.Configuration;
using Silky.Core;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CachingServiceCollectionExtensions
    {
        public static IServiceCollection AddSilkyCaching(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            services.AddSingleton(typeof(IDistributedCache<>), typeof(DistributedCache<>));
            services.AddSingleton(typeof(IDistributedCache<,>), typeof(DistributedCache<,>));
            services.AddOptions<SilkyDistributedCacheOptions>()
                .Bind(
                    EngineContext.Current.Configuration.GetSection(SilkyDistributedCacheOptions.SilkyDistributedCache));
            return services;
        }
    }
}