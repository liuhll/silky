using System;
using System.Linq;
using System.Threading.Tasks;
using Lms.Caching;
using Lms.Core.DependencyInjection;
using Lms.Core.DynamicProxy;
using Lms.Core.Exceptions;
using Lms.Rpc.Configuration;
using Lms.Rpc.Runtime.Server;
using Lms.Rpc.Transport.CachingIntercept;
using Microsoft.Extensions.Options;

namespace Lms.Rpc.Proxy.Interceptors
{
    public class RpcClientProxyInterceptor : LmsInterceptor, ITransientDependency
    {
        private readonly IServiceIdGenerator _serviceIdGenerator;
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly ICurrentServiceKey _currentServiceKey;
        private readonly IDistributedInterceptCache _distributedCache;

        public RpcClientProxyInterceptor(
            IServiceIdGenerator serviceIdGenerator,
            IServiceEntryLocator serviceEntryLocator,
            ICurrentServiceKey currentServiceKey,
            IDistributedCache<object> distributedCache)
        {
            _serviceIdGenerator = serviceIdGenerator;
            _serviceEntryLocator = serviceEntryLocator;
            _currentServiceKey = currentServiceKey;
            _distributedCache = distributedCache;
        }

        public async override Task InterceptAsync(ILmsMethodInvocation invocation)
        {
            var servcieId = _serviceIdGenerator.GenerateServiceId(invocation.Method);
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(servcieId);
            try
            {
                if (serviceEntry.GovernanceOptions.CacheEnabled)
                {
                    var cachingInterceptProvider = serviceEntry.CachingInterceptProvider;
                    if (cachingInterceptProvider != null)
                    {
                        switch (cachingInterceptProvider.CachingMethod)
                        {
                            case CachingMethod.Get:
                                var getCachingInterceptProvider =
                                    cachingInterceptProvider as IGetCachingInterceptProvider;

                                break;
                            case CachingMethod.Remove:

                                foreach (var removeRemoveCachingKey in
                                    (cachingInterceptProvider as IRemoveCachingInterceptProvider)
                                    .RemoveRemoveCachingKeyInfos)
                                {
                                    var removeCacheKey = serviceEntry.GetCachingInterceptKey(invocation.Arguments,
                                        removeRemoveCachingKey.RemoveKeyTemplete);
                                    _distributedCache.UpdateCacheName(removeRemoveCachingKey.CacheName);
                                    await _distributedCache.RemoveAsync(removeCacheKey, true);
                                    invocation.ReturnValue =
                                        await serviceEntry.Executor(_currentServiceKey.ServiceKey,
                                            invocation.Arguments);
                                }

                                break;
                        }
                    }
                    else
                    {
                        invocation.ReturnValue =
                            await serviceEntry.Executor(_currentServiceKey.ServiceKey, invocation.Arguments);
                    }
                }
                else
                {
                    invocation.ReturnValue =
                        await serviceEntry.Executor(_currentServiceKey.ServiceKey, invocation.Arguments);
                }
            }
            catch (Exception e)
            {
                if (!e.IsBusinessException() && serviceEntry.FallBackExecutor != null)
                {
                    await invocation.ProceedAsync();
                }
                else
                {
                    throw;
                }
            }
        }
    }
}