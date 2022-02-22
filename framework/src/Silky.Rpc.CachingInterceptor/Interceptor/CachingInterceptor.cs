using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.DependencyInjection;
using Silky.Core.DynamicProxy;
using Silky.Core.Extensions;
using Silky.Core.Logging;
using Silky.Core.MiniProfiler;
using Silky.Rpc.Extensions;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.CachingInterceptor
{
    public class CachingInterceptor : SilkyInterceptor, ITransientDependency
    {
        private readonly IDistributedInterceptCache _distributedCache;
        private ILogger<CachingInterceptor> Logger { get; set; }

        public CachingInterceptor(IDistributedInterceptCache distributedCache)
        {
            _distributedCache = distributedCache;
            Logger = NullLogger<CachingInterceptor>.Instance;
        }

        public override async Task InterceptAsync(ISilkyMethodInvocation invocation)
        {
            var serviceEntry = invocation.GetServiceEntry();
            var serviceKey = invocation.GetServiceKey();
            var parameters = invocation.GetParameters();

            async Task<object> GetResultFirstFromCache(string cacheName, string cacheKey, ServiceEntry entry)
            {
                _distributedCache.UpdateCacheName(cacheName);
                return await _distributedCache.GetOrAddAsync(cacheKey,
                    serviceEntry.MethodInfo.GetReturnType(),
                    async () => await entry.Executor(serviceKey, parameters));
            }

            if (serviceEntry.GovernanceOptions.EnableCachingInterceptor)
            {
                
                var removeCachingInterceptProviders = serviceEntry.RemoveCachingInterceptProviders();
                if (removeCachingInterceptProviders.Any())
                {
                    var index = 1;
                    foreach (var removeCachingInterceptProvider in removeCachingInterceptProviders)
                    {
                        _distributedCache.SetIgnoreMultiTenancy(removeCachingInterceptProvider.IgnoreMultiTenancy);
                        var removeCacheKey =
                            serviceEntry.GetCachingInterceptKey(parameters, removeCachingInterceptProvider);
                        await _distributedCache.RemoveAsync(removeCacheKey, removeCachingInterceptProvider.CacheName,
                            true);
                        Logger.LogWithMiniProfiler(MiniProfileConstant.Caching.Name,
                            MiniProfileConstant.Caching.State.RemoveCaching + index,
                            $"Remove the cache with key {removeCacheKey}");
                        index++;
                    }
                }

                var getCachingInterceptProvider = serviceEntry.GetCachingInterceptProvider();
                var updateCachingInterceptProvider = serviceEntry.UpdateCachingInterceptProvider();
                if (getCachingInterceptProvider != null)
                {
                    if (serviceEntry.IsTransactionServiceEntry())
                    {
                        Logger.LogWithMiniProfiler(MiniProfileConstant.Caching.Name,
                            MiniProfileConstant.Caching.State.GetCaching,
                            $"Cache interception is invalid in distributed transaction processing");
                        await invocation.ProceedAsync();
                    }
                    else
                    {
                        _distributedCache.SetIgnoreMultiTenancy(getCachingInterceptProvider.IgnoreMultiTenancy);
                        var getCacheKey = serviceEntry.GetCachingInterceptKey(parameters,
                            serviceEntry.GetCachingInterceptProvider());
                        Logger.LogWithMiniProfiler(MiniProfileConstant.Caching.Name,
                            MiniProfileConstant.Caching.State.GetCaching,
                            $"Ready to get data from the cache service:[cacheName=>{serviceEntry.GetCacheName()};cacheKey=>{getCacheKey}]");
                        invocation.ReturnValue = await GetResultFirstFromCache(
                            serviceEntry.GetCacheName(),
                            getCacheKey,
                            serviceEntry);
                    }
                }
                else if (updateCachingInterceptProvider != null)
                {
                    if (serviceEntry.IsTransactionServiceEntry())
                    {
                        Logger.LogWithMiniProfiler(MiniProfileConstant.Caching.Name,
                            MiniProfileConstant.Caching.State.UpdateCaching,
                            $"Cache interception is invalid in distributed transaction processing");
                        await invocation.ProceedAsync();
                    }
                    else
                    {
                        _distributedCache.SetIgnoreMultiTenancy(updateCachingInterceptProvider.IgnoreMultiTenancy);
                        var updateCacheKey = serviceEntry.GetCachingInterceptKey(parameters, updateCachingInterceptProvider);
                           
                        Logger.LogWithMiniProfiler(MiniProfileConstant.Caching.Name, MiniProfileConstant.Caching.State.UpdateCaching,
                            $"The cacheKey for updating the cache data is[cacheName=>{serviceEntry.GetCacheName()};cacheKey=>{updateCacheKey}]");
                        await _distributedCache.RemoveAsync(updateCacheKey, serviceEntry.GetCacheName(),
                            hideErrors: true);
                        invocation.ReturnValue = await GetResultFirstFromCache(
                            serviceEntry.GetCacheName(),
                            updateCacheKey,
                            serviceEntry);
                    }
                }
                else
                {
                    await invocation.ProceedAsync();
                }
            }
            else
            {
                await invocation.ProceedAsync();
            }
        }
    }
}