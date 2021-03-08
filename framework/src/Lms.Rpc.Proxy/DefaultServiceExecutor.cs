using System.Linq;
using System.Threading.Tasks;
using Lms.Caching;
using Lms.Core.Extensions;
using Lms.Rpc.Runtime;
using Lms.Rpc.Runtime.Server;

namespace Lms.Rpc.Proxy
{
    public class DefaultServiceExecutor : IServiceExecutor
    {
        private readonly IDistributedInterceptCache _distributedCache;

        public DefaultServiceExecutor(IDistributedInterceptCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<object> Execute(ServiceEntry serviceEntry, object[] parameters, string serviceKey = null)
        {
            async Task<object> GetResultFirstFromCache(string cacheName, string cacheKey, ServiceEntry entry)
            {
                _distributedCache.UpdateCacheName(cacheName);
                return await _distributedCache.GetOrAddAsync(cacheKey,
                    serviceEntry.MethodInfo.GetReturnType(),
                    async () => entry.Executor(serviceKey, parameters));
            }

            if (serviceEntry.GovernanceOptions.CacheEnabled)
            {
                var removeCachingInterceptProviders = serviceEntry.RemoveCachingInterceptProviders;
                if (removeCachingInterceptProviders.Any())
                {
                    foreach (var removeCachingInterceptProvider in removeCachingInterceptProviders)
                    {
                        var removeCacheKey = serviceEntry.GetCachingInterceptKey(parameters,
                            removeCachingInterceptProvider.KeyTemplete);
                        _distributedCache.UpdateCacheName(removeCachingInterceptProvider.CacheName);
                        await _distributedCache.RemoveAsync(removeCacheKey, true);
                    }
                }

                if (serviceEntry.GetCachingInterceptProvider != null)
                {
                    var getCacheKey = serviceEntry.GetCachingInterceptKey(parameters,
                        serviceEntry.GetCachingInterceptProvider.KeyTemplete);
                    return await GetResultFirstFromCache(serviceEntry.GetCachingInterceptProvider.CacheName,
                        getCacheKey,
                        serviceEntry);
                }
                else if (serviceEntry.UpdateCachingInterceptProvider != null)
                {
                    var updateCacheKey = serviceEntry.GetCachingInterceptKey(parameters,
                        serviceEntry.UpdateCachingInterceptProvider.KeyTemplete);
                    await _distributedCache.RemoveAsync(updateCacheKey);
                    return await GetResultFirstFromCache(serviceEntry.GetCachingInterceptProvider.CacheName,
                        updateCacheKey,
                        serviceEntry);
                }
                else
                {
                    return serviceEntry.Executor(serviceKey, parameters);
                }
            }
            else
            {
                return serviceEntry.Executor(serviceKey, parameters);
            }
        }
    }
}