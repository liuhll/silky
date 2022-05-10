using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Core.Convertible;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Session;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.CachingInterceptor
{
    public static class ServiceEntryExtensions
    {
        public static string GetCachingInterceptKey(this ServiceEntry serviceEntry, [NotNull] object[] parameters,
            [NotNull] ICachingInterceptProvider cachingInterceptProvider,string serviceKey)
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
            var cacheKeyProviders = new List<CacheKeyProvider>();
            var index = 0;
            var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
            foreach (var parameterDescriptor in serviceEntry.ParameterDescriptors)
            {
                if (parameterDescriptor.CacheKeys.Any())
                {
                    if (parameterDescriptor.IsSampleOrNullableType)
                    {
                        var cacheKey = parameterDescriptor.CacheKeys.First();
                        var cacheKeyProvider = new CacheKeyProvider()
                        {
                            PropName = cacheKey.PropName,
                            Index = cacheKey.Index,
                            Value = parameters[index]?.ToString()
                        };
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
                            var cacheKeyProvider = new CacheKeyProvider()
                            {
                                PropName = cacheKey.PropName,
                                Index = cacheKey.Index,
                                Value = cacheKeyProp.GetValue(parameterValue)?.ToString()
                            };
                            cacheKeyProviders.Add(cacheKeyProvider);
                        }
                    }
                }

                index++;
            }

            var templeteAgrs = cacheKeyProviders.OrderBy(p => p.Index).ToList().Select(ckp => ckp.Value).ToArray();
            cachingInterceptKey = string.Format(templete, templeteAgrs);
            if (!serviceKey.IsNullOrEmpty())
            {
                cachingInterceptKey = $"serviceKey:{serviceKey}:" + cachingInterceptKey;
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

        
    }
}