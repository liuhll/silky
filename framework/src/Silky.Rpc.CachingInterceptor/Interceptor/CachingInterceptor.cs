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
            var serviceEntryDescriptor = invocation.GetServiceEntryDescriptor();
            if (serviceEntry?.GovernanceOptions.EnableCachingInterceptor == true)
            {
                await InterceptForServiceEntryAsync(invocation, serviceEntry);
            }
            else if (serviceEntryDescriptor?.GovernanceOptions.EnableCachingInterceptor == true)
            {
                await InterceptForServiceEntryDescriptorAsync(invocation, serviceEntryDescriptor);
            }
            else
            {
                await invocation.ProceedAsync();
            }
        }

        private async Task InterceptForServiceEntryDescriptorAsync(ISilkyMethodInvocation invocation,
            ServiceEntryDescriptor serviceEntryDescriptor)
        {
            var serviceKey = invocation.GetServiceKey();
            var parameters = invocation.GetServiceEntryDescriptorParameters();

            async Task<object> GetResultFirstFromCache(string cacheName, string cacheKey)
            {
                _distributedCache.UpdateCacheName(cacheName);
                return await _distributedCache.GetOrAddAsync(cacheKey,
                    async () =>
                    {
                        await invocation.ProceedAsync();
                        return invocation.ReturnValue;
                    });
            }

            if (serviceEntryDescriptor.CachingInterceptorDescriptors?.Any() == true)
            {
                var cachingInterceptorDescriptors = serviceEntryDescriptor.CachingInterceptorDescriptors;
                var removeCachingInterceptorDescriptors =
                    cachingInterceptorDescriptors.Where(p => p.CachingMethod == CachingMethod.Remove);
                foreach (var removeCachingInterceptProvider in removeCachingInterceptorDescriptors)
                {
                    _distributedCache.SetIgnoreMultiTenancy(removeCachingInterceptProvider.IgnoreMultiTenancy);
                    var removeCacheKey =
                        CacheKeyHelper.GetCachingInterceptKey(parameters, removeCachingInterceptProvider, serviceKey);
                    if (removeCachingInterceptProvider.IsRemoveMatchKeyProvider)
                    {
                        await _distributedCache.RemoveMatchKeyAsync(removeCacheKey);
                    }
                    else
                    {
                        await _distributedCache.RemoveAsync(removeCacheKey);
                    }
                }

                var getCachingInterceptProviderDescriptor = cachingInterceptorDescriptors
                    .Where(p => p.CachingMethod == CachingMethod.Get).FirstOrDefault();
                var updateCachingInterceptProviderDescriptor = cachingInterceptorDescriptors
                    .Where(p => p.CachingMethod == CachingMethod.Update).FirstOrDefault();
                if (getCachingInterceptProviderDescriptor != null)
                {
                    if (serviceEntryDescriptor.IsDistributeTransaction)
                    {
                        await invocation.ProceedAsync();
                    }
                    else
                    {
                        _distributedCache.SetIgnoreMultiTenancy(
                            getCachingInterceptProviderDescriptor.IgnoreMultiTenancy);
                        var getCacheKey = CacheKeyHelper.GetCachingInterceptKey(parameters,
                            getCachingInterceptProviderDescriptor, serviceKey);

                        invocation.ReturnValue = await GetResultFirstFromCache(
                            getCachingInterceptProviderDescriptor.CacheName,
                            getCacheKey);
                    }
                }
                else if (updateCachingInterceptProviderDescriptor != null)
                {
                    if (serviceEntryDescriptor.IsDistributeTransaction)
                    {
                        await invocation.ProceedAsync();
                    }
                    else
                    {
                        _distributedCache.SetIgnoreMultiTenancy(updateCachingInterceptProviderDescriptor
                            .IgnoreMultiTenancy);
                        var updateCacheKey = CacheKeyHelper.GetCachingInterceptKey(parameters,
                            updateCachingInterceptProviderDescriptor, serviceKey);

                        await _distributedCache.RemoveAsync(updateCacheKey,
                            updateCachingInterceptProviderDescriptor.CacheName,
                            hideErrors: true);
                        invocation.ReturnValue = await GetResultFirstFromCache(
                            updateCachingInterceptProviderDescriptor.CacheName,
                            updateCacheKey);
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

        private async Task InterceptForServiceEntryAsync(ISilkyMethodInvocation invocation, ServiceEntry serviceEntry)
        {
            var serviceKey = invocation.GetServiceKey();
            var parameters = invocation.GetParameters();

            async Task<object> GetResultFirstFromCache(string cacheName, string cacheKey, ServiceEntry entry)
            {
                _distributedCache.UpdateCacheName(cacheName);
                return await _distributedCache.GetOrAddAsync(cacheKey,
                    serviceEntry.MethodInfo.GetReturnType(),
                    async () => await entry.Executor(serviceKey, parameters));
            }

            var removeCachingInterceptProviders = serviceEntry.RemoveCachingInterceptProviders();
            if (removeCachingInterceptProviders.Any())
            {
                var index = 1;
                foreach (var removeCachingInterceptProvider in removeCachingInterceptProviders)
                {
                    _distributedCache.SetIgnoreMultiTenancy(removeCachingInterceptProvider.IgnoreMultiTenancy);
                    var removeCacheKey =
                        serviceEntry.GetCachingInterceptKey(parameters, removeCachingInterceptProvider, serviceKey);
                    await _distributedCache.RemoveAsync(removeCacheKey,
                        removeCachingInterceptProvider.CacheName,
                        true);
                    Logger.LogWithMiniProfiler(MiniProfileConstant.Caching.Name,
                        MiniProfileConstant.Caching.State.RemoveCaching + index,
                        $"Remove the cache with key {removeCacheKey}");
                    index++;
                }
            }

            var removeMatchKeyCachingInterceptProviders =
                serviceEntry.RemoveMatchKeyCachingInterceptProviders();
            if (removeMatchKeyCachingInterceptProviders.Any())
            {
                var index = 1;
                foreach (var removeMatchKeyCachingInterceptProvider in removeMatchKeyCachingInterceptProviders)
                {
                    _distributedCache.SetIgnoreMultiTenancy(removeMatchKeyCachingInterceptProvider
                        .IgnoreMultiTenancy);
                    var removeCacheKey =
                        serviceEntry.GetCachingInterceptKey(parameters, removeMatchKeyCachingInterceptProvider,
                            serviceKey);
                    await _distributedCache.RemoveMatchKeyAsync(removeCacheKey);
                    Logger.LogWithMiniProfiler(MiniProfileConstant.Caching.Name,
                        MiniProfileConstant.Caching.State.RemoveCaching + index,
                        $"RemoveMatchKey the cache with key {removeCacheKey}");
                    index++;
                }
            }

            var getCachingInterceptProvider = serviceEntry.GetGetCachingInterceptProvider();
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
                        serviceEntry.GetGetCachingInterceptProvider(), serviceKey);
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
                    var updateCacheKey =
                        serviceEntry.GetCachingInterceptKey(parameters, updateCachingInterceptProvider, serviceKey);

                    Logger.LogWithMiniProfiler(MiniProfileConstant.Caching.Name,
                        MiniProfileConstant.Caching.State.UpdateCaching,
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
    }
}