using System.Linq;
using System.Threading.Tasks;
using Silky.Lms.Caching;
using Silky.Lms.Core;
using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.Core.DynamicProxy;
using Silky.Lms.Core.Extensions;
using Silky.Lms.Rpc.Runtime.Server;

namespace Silky.Lms.Rpc.Interceptors
{
    public class CachingInterceptor : LmsInterceptor, ITransientDependency
    {
        private readonly IDistributedInterceptCache _distributedCache;

        public CachingInterceptor(IDistributedInterceptCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public override async Task InterceptAsync(ILmsMethodInvocation invocation)
        {
            var serviceEntry = invocation.ArgumentsDictionary["serviceEntry"] as ServiceEntry;
            var serviceKey = invocation.ArgumentsDictionary["serviceKey"] as string;
            var parameters = invocation.ArgumentsDictionary["parameters"] as object[];

            async Task<object> GetResultFirstFromCache(string cacheName, string cacheKey, ServiceEntry entry)
            {
                _distributedCache.UpdateCacheName(cacheName);
                return await _distributedCache.GetOrAddAsync(cacheKey,
                    serviceEntry.MethodInfo.GetReturnType(),
                    async () => await entry.Executor(serviceKey, parameters));
            }

            if (serviceEntry.GovernanceOptions.CacheEnabled)
            {
                EngineContext.Current.PrintToMiniProfiler(MiniProfileConstant.Caching.Name,
                    MiniProfileConstant.Caching.State.CacheEnabled,
                    $"缓存拦截可用");

                var removeCachingInterceptProviders = serviceEntry.RemoveCachingInterceptProviders;
                if (removeCachingInterceptProviders.Any())
                {
                    var index = 1;
                    foreach (var removeCachingInterceptProvider in removeCachingInterceptProviders)
                    {
                        var removeCacheKey =
                            serviceEntry.GetCachingInterceptKey(parameters, removeCachingInterceptProvider);
                        await _distributedCache.RemoveAsync(removeCacheKey, removeCachingInterceptProvider.CacheName,
                            true);
                        EngineContext.Current.PrintToMiniProfiler(MiniProfileConstant.Caching.Name,
                            MiniProfileConstant.Caching.State.RemoveCaching + index,
                            $"移除key为{removeCacheKey}的缓存");
                        index++;
                    }
                }

                if (serviceEntry.GetCachingInterceptProvider != null)
                {
                    if (serviceEntry.IsTransactionServiceEntry())
                    {
                        EngineContext.Current.PrintToMiniProfiler(MiniProfileConstant.Caching.Name,
                            MiniProfileConstant.Caching.State.GetCaching,
                            $"分布式事务缓存拦截无效");
                        await invocation.ProceedAsync();
                    }
                    else
                    {
                        var getCacheKey = serviceEntry.GetCachingInterceptKey(parameters,
                            serviceEntry.GetCachingInterceptProvider);
                        EngineContext.Current.PrintToMiniProfiler(MiniProfileConstant.Caching.Name,
                            MiniProfileConstant.Caching.State.GetCaching,
                            $"准备从缓存服务中获取数据:[cacheName=>{serviceEntry.GetCacheName()};cacheKey=>{getCacheKey}]");
                        invocation.ReturnValue = await GetResultFirstFromCache(
                            serviceEntry.GetCacheName(),
                            getCacheKey,
                            serviceEntry);
                    }
                }
                else if (serviceEntry.UpdateCachingInterceptProvider != null)
                {
                    if (serviceEntry.IsTransactionServiceEntry())
                    {
                        EngineContext.Current.PrintToMiniProfiler(MiniProfileConstant.Caching.Name,
                            MiniProfileConstant.Caching.State.UpdateCaching,
                            $"分布式事务缓存拦截无效");
                        await invocation.ProceedAsync();
                    }
                    else
                    {
                        var updateCacheKey = serviceEntry.GetCachingInterceptKey(parameters,
                            serviceEntry.UpdateCachingInterceptProvider);
                        EngineContext.Current.PrintToMiniProfiler(MiniProfileConstant.Caching.Name,
                            MiniProfileConstant.Caching.State.UpdateCaching,
                            $"更新缓存数据的cacheKey为[cacheName=>{serviceEntry.GetCacheName()};cacheKey=>{updateCacheKey}]");
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
                    EngineContext.Current.PrintToMiniProfiler(MiniProfileConstant.Caching.Name,
                        MiniProfileConstant.Caching.State.NotSet,
                        $"没有设置缓存拦截");
                    await invocation.ProceedAsync();
                }
            }
            else
            {
                EngineContext.Current.PrintToMiniProfiler(MiniProfileConstant.Caching.Name,
                    MiniProfileConstant.Caching.State.CacheEnabled,
                    $"缓存拦截不可用");
                await invocation.ProceedAsync();
            }
        }
    }
}