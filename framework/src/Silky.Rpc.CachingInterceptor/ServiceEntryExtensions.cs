using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Silky.Caching;
using Silky.Core;
using Silky.Core.Convertible;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Session;
using Silky.Rpc.CachingInterceptor.Providers;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.CachingInterceptor
{
    public static class ServiceEntryExtensions
    {
        public static string GetCacheName([NotNull] this ServiceEntry serviceEntry)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            var returnType = serviceEntry.ReturnType;
            var cacheNameAttribute = returnType.GetCustomAttributes().OfType<CacheNameAttribute>().FirstOrDefault();
            if (cacheNameAttribute != null)
            {
                return cacheNameAttribute.Name;
            }

            return returnType.FullName.RemovePostFix("CacheItem");
        }

        public static string GetCachingInterceptKey(this ServiceEntry serviceEntry, [NotNull] object[] parameters,
            [NotNull] ICachingInterceptProvider cachingInterceptProvider)
        {
            Check.NotNull(parameters, nameof(parameters));
            Check.NotNull(cachingInterceptProvider, nameof(cachingInterceptProvider));
            var templete = cachingInterceptProvider.KeyTemplete;
            if (templete.IsNullOrEmpty())
            {
                throw new SilkyException(
                    "The KeyTemplete specified by the cache interception is not allowed to be empty",
                    StatusCode.CachingInterceptError);
            }

            var cachingInterceptKey = string.Empty;
            var cacheKeyProviders = new List<ICacheKeyProvider>();
            var index = 0;
            var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
            foreach (var parameterDescriptor in serviceEntry.ParameterDescriptors)
            {
                if (parameterDescriptor.CacheKeys.Any())
                {
                    if (parameterDescriptor.IsSampleOrNullableType)
                    {
                        var cacheKeyProvider = parameterDescriptor.CacheKeys.First();
                        cacheKeyProvider.Value = parameters[index]?.ToString();
                        cacheKeyProviders.Add(cacheKeyProvider);
                    }
                    else
                    {
                        var parameterValue =
                            typeConvertibleService.Convert(parameters[index], parameterDescriptor.Type);
                        foreach (var cacheKey in parameterDescriptor.CacheKeys)
                        {
                            var cacheKeyProp = parameterDescriptor.Type.GetProperty(cacheKey.PropName);
                            Debug.Assert(cacheKeyProp != null, nameof(cacheKeyProp));
                            cacheKey.Value = cacheKeyProp.GetValue(parameterValue)?.ToString();
                            cacheKeyProviders.Add(cacheKey);
                        }
                    }
                }

                index++;
            }

            var templeteAgrs = cacheKeyProviders.OrderBy(p => p.Index).ToList().Select(ckp => ckp.Value).ToArray();
            cachingInterceptKey = string.Format(templete, templeteAgrs);
            var currentServiceKey = EngineContext.Current.Resolve<IServiceKeyExecutor>();
            if (!currentServiceKey.ServiceKey.IsNullOrEmpty())
            {
                cachingInterceptKey = $"serviceKey:{currentServiceKey.ServiceKey}:" + cachingInterceptKey;
            }

            if (cachingInterceptProvider.OnlyCurrentUserData)
            {
                var session = NullSession.Instance;
                if (!session.IsLogin())
                {
                    throw new SilkyException(
                        "If the cached data is specified to be related to the currently logged in user, then you must log in to the system to allow the use of cache interception",
                        StatusCode.CachingInterceptError);
                }

                cachingInterceptKey = cachingInterceptKey + $":userId:{session.UserId}";
            }

            return cachingInterceptKey;
        }

        public static ICachingInterceptProvider GetCachingInterceptProvider(this ServiceEntry serviceEntry)
        {
            return serviceEntry.CustomAttributes.OfType<IGetCachingInterceptProvider>().FirstOrDefault();
        }

        public static ICachingInterceptProvider UpdateCachingInterceptProvider(this ServiceEntry serviceEntry)
        {
            return serviceEntry.CustomAttributes.OfType<IUpdateCachingInterceptProvider>().FirstOrDefault();
        }

        public static IReadOnlyCollection<IRemoveCachingInterceptProvider> RemoveCachingInterceptProviders(
            this ServiceEntry serviceEntry)
        {
            return serviceEntry.CustomAttributes.OfType<IRemoveCachingInterceptProvider>().ToArray();
        }
        
        public static IReadOnlyCollection<IRemoveMatchKeyCachingInterceptProvider> RemoveMatchKeyCachingInterceptProviders(
            this ServiceEntry serviceEntry)
        {
            return serviceEntry.CustomAttributes.OfType<IRemoveMatchKeyCachingInterceptProvider>().ToArray();
        }
    }
}