﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.DependencyInjection;
using Silky.Core.DynamicProxy;
using Silky.Core.Extensions;
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
            if (serviceEntry?.GovernanceOptions.EnableCachingInterceptor == true &&
                serviceEntry?.CachingInterceptorDescriptors?.Any() == true)
            {
                await InterceptForServiceEntryAsync(invocation, serviceEntry);
            }
            else if (serviceEntryDescriptor?.GovernanceOptions.EnableCachingInterceptor == true &&
                     serviceEntryDescriptor?.CachingInterceptorDescriptors?.Any() == true)
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
            var proceed = ProceedType.UnProceed;

            async Task InvocationProceedAsync(ISilkyMethodInvocation invocation)
            {
                if (proceed == ProceedType.UnProceed)
                {
                    await invocation.ProceedAsync();
                    proceed = ProceedType.ForCache;
                }
            }

            async Task<object> GetResultFirstFromCache(string cacheName, string cacheKey)
            {
                _distributedCache.UpdateCacheName(cacheName);
                var result = await _distributedCache.GetOrAddAsync(cacheKey,
                    async () =>
                    {
                        await InvocationProceedAsync(invocation);
                        return invocation.ReturnValue;
                    });
                if (proceed == ProceedType.UnProceed)
                {
                    proceed = ProceedType.ForCache;
                }

                return result;
            }

            var cachingInterceptorDescriptors = serviceEntryDescriptor.CachingInterceptorDescriptors;
            var removeCachingInterceptorDescriptors =
                cachingInterceptorDescriptors.Where(p => p.CachingMethod == CachingMethod.Remove);
            var getCachingInterceptProviderDescriptor =
                cachingInterceptorDescriptors.FirstOrDefault(p => p.CachingMethod == CachingMethod.Get);
            var updateCachingInterceptProviderDescriptors =
                cachingInterceptorDescriptors.Where(p => p.CachingMethod == CachingMethod.Update);

            if (getCachingInterceptProviderDescriptor != null)
            {
                if (serviceEntryDescriptor.IsDistributeTransaction)
                {
                    await InvocationProceedAsync(invocation);
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

            await InvocationProceedAsync(invocation);

            foreach (var updateCachingInterceptProviderDescriptor in
                     updateCachingInterceptProviderDescriptors)
            {
                _distributedCache.SetIgnoreMultiTenancy(updateCachingInterceptProviderDescriptor
                    .IgnoreMultiTenancy);
                var updateCacheKey = CacheKeyHelper.GetCachingInterceptKey(parameters,
                    updateCachingInterceptProviderDescriptor, serviceKey);
                await _distributedCache.RemoveForInterceptAsync(updateCacheKey,
                    updateCachingInterceptProviderDescriptor.CacheName, false);
                _distributedCache.UpdateCacheName(updateCachingInterceptProviderDescriptor.CacheName);
                await _distributedCache.SetAsync(updateCacheKey, invocation.ReturnValue);
            }

            foreach (var removeCachingInterceptProvider in removeCachingInterceptorDescriptors)
            {
                _distributedCache.SetIgnoreMultiTenancy(removeCachingInterceptProvider.IgnoreMultiTenancy);
                var removeCacheKey =
                    CacheKeyHelper.GetCachingInterceptKey(parameters, removeCachingInterceptProvider, serviceKey);
                await _distributedCache.RemoveForInterceptAsync(removeCacheKey,
                    removeCachingInterceptProvider.CacheName,
                    removeCachingInterceptProvider.IsRemoveMatchKeyProvider);
            }
        }

        private async Task InterceptForServiceEntryAsync(ISilkyMethodInvocation invocation, ServiceEntry serviceEntry)
        {
            var serviceKey = invocation.GetServiceKey();
            var parameters = invocation.GetParameters();
            var proceed = ProceedType.UnProceed;

            async Task InvocationProceedAsync(ISilkyMethodInvocation invocation)
            {
                if (proceed == ProceedType.UnProceed)
                {
                    await invocation.ProceedAsync();
                    proceed = ProceedType.ForExec;
                }
            }

            async Task<object> GetResultFirstFromCache(string cacheName, string cacheKey, ServiceEntry entry)
            {
                _distributedCache.UpdateCacheName(cacheName);
                var result = await _distributedCache.GetOrAddAsync(cacheKey,
                    serviceEntry.MethodInfo.GetReturnType(),
                    async () =>
                    {
                        await InvocationProceedAsync(invocation);
                        return invocation.ReturnValue;
                    });
                if (proceed == ProceedType.UnProceed)
                {
                    proceed = ProceedType.ForCache;
                }

                return result;
            }

            var removeCachingInterceptProviders = serviceEntry.GetAllRemoveCachingInterceptProviders();
            var getCachingInterceptProvider = serviceEntry.GetGetCachingInterceptProvider();
            var updateCachingInterceptProviders = serviceEntry.GetUpdateCachingInterceptProviders();

            if (getCachingInterceptProvider != null)
            {
                if (serviceEntry.IsTransactionServiceEntry())
                {
                    Logger.LogDebug(
                        $"Cache interception is invalid in distributed transaction processing for the serviceEntry method {serviceEntry.GetCacheName()}");

                    await invocation.ProceedAsync();
                    proceed = ProceedType.ForExec;
                }
                else
                {
                    _distributedCache.SetIgnoreMultiTenancy(getCachingInterceptProvider.IgnoreMultiTenancy);
                    var getCacheKeyInfo = CacheKeyHelper.GetCachingInterceptKey(serviceEntry, parameters,
                        serviceEntry.GetGetCachingInterceptProvider(), serviceKey);
                    Logger.LogDebug(
                        $"Ready to get data from the cache service:[cacheName=>{serviceEntry.GetCacheName()};cacheKey=>{getCacheKeyInfo.Item1}]");
                    if (getCacheKeyInfo.Item2)
                    {
                        await InvocationProceedAsync(invocation);
                    }
                    else
                    {
                        invocation.ReturnValue = await GetResultFirstFromCache(
                            serviceEntry.GetCacheName(),
                            getCacheKeyInfo.Item1,
                            serviceEntry);
                    }
                }
            }

            await InvocationProceedAsync(invocation);

            foreach (var updateCachingInterceptProvider in updateCachingInterceptProviders)
            {
                var updateCacheKeyInfo =
                    CacheKeyHelper.GetCachingInterceptKey(serviceEntry, parameters,
                        updateCachingInterceptProvider,
                        serviceKey);
                _distributedCache.SetIgnoreMultiTenancy(updateCachingInterceptProvider.IgnoreMultiTenancy);

                if (updateCacheKeyInfo.Item2)
                {
                    await _distributedCache.RemoveForInterceptAsync(updateCacheKeyInfo.Item1,
                        updateCachingInterceptProvider.CachingInterceptorDescriptor.CacheName, false);
                }
                else
                {
                    await _distributedCache.SetAsync(updateCacheKeyInfo.Item1, invocation.ReturnValue);
                }
            }

            foreach (var removeCachingInterceptProvider in removeCachingInterceptProviders)
            {
                _distributedCache.SetIgnoreMultiTenancy(removeCachingInterceptProvider.IgnoreMultiTenancy);
                var removeCacheKeyInfo =
                    CacheKeyHelper.GetCachingInterceptKey(serviceEntry, parameters, removeCachingInterceptProvider,
                        serviceKey);

                await _distributedCache.RemoveForInterceptAsync(removeCacheKeyInfo.Item1,
                    removeCachingInterceptProvider.CachingInterceptorDescriptor.CacheName,
                    removeCachingInterceptProvider is IRemoveMatchKeyCachingInterceptProvider);
            }
        }

        private enum ProceedType
        {
            UnProceed,
            ForCache,
            ForExec,
        }
    }
}