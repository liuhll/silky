using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Runtime.Session;
using Silky.Core.Serialization;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.CachingInterceptor;

public static class CacheKeyHelper
{
    public static string GetCachingInterceptKey(object parameters,
        [NotNull] CachingInterceptorDescriptor cachingInterceptProvider, string serviceKey)

    {
        var cacheKeyProviders = new List<CacheKeyProvider>();
        var cachingInterceptKey = string.Empty;
        foreach (var cacheKeyProviderDescriptor in cachingInterceptProvider.CacheKeyProviders)
        {
            var cacheKeyProvider = new CacheKeyProvider()
            {
                Index = cacheKeyProviderDescriptor.Index,
                PropName = cacheKeyProviderDescriptor.PropName,
                Value = GetCacheKeyValue(cacheKeyProviderDescriptor, parameters)
            };
            cacheKeyProviders.Add(cacheKeyProvider);
        }

        var templeteAgrs = cacheKeyProviders.OrderBy(p => p.Index).ToList().Select(ckp => ckp.Value).ToArray();
        cachingInterceptKey = string.Format(cachingInterceptProvider.KeyTemplete, templeteAgrs);

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

    private static string GetCacheKeyValue(CacheKeyProviderDescriptor cacheKeyProviderDescriptor, object parameters)
    {
        var serializer = EngineContext.Current.Resolve<ISerializer>();
        if (parameters is IDictionary<string, object> dictParameters)
        {
            if (cacheKeyProviderDescriptor.IsSampleOrNullableType &&
                dictParameters.TryOrdinalIgnoreCaseGetValue(cacheKeyProviderDescriptor.PropName, out var value))
            {
                return value?.ToString();
            }

            var cacheKeyValue = string.Empty;
            foreach (var dictParameter in dictParameters)
            {
                dynamic dictParameterValue = dictParameter.Value;
                var cacheKeyValueProp = dictParameterValue.GetType().GetProperty(cacheKeyProviderDescriptor.PropName);
                if (cacheKeyValueProp != null)
                {
                    cacheKeyValue = cacheKeyValueProp.GetValue(dictParameterValue, null);
                    break;
                }
            }

            if (cacheKeyValue == null)
            {
                throw new SilkyException(
                    $"Failed to get the value of the cache interception key:{cacheKeyProviderDescriptor.PropName}");
            }

            return cacheKeyValue;
        }

        else if (parameters is IDictionary<ParameterFrom, object> httpParameters)
        {
            if (httpParameters.TryGetValue(cacheKeyProviderDescriptor.From, out var httpParameterValue))
            {
                if (cacheKeyProviderDescriptor.IsSampleOrNullableType)
                {
                    return httpParameterValue?.ToString();
                }

                var httpParameterDictValue =
                    serializer.Deserialize<IDictionary<string, object>>(httpParameterValue.ToString());
                if (httpParameterDictValue.TryOrdinalIgnoreCaseGetValue(cacheKeyProviderDescriptor.PropName,
                        out var cacheKeyValue))
                {
                    return cacheKeyValue?.ToString();
                }
                else
                {
                    throw new SilkyException(
                        $"Failed to get the value of the cache interception key:{cacheKeyProviderDescriptor.PropName}");
                }
            }
        }

        else if (parameters is object[] sortedParameters)
        {
            var sortedParameteValue = sortedParameters[cacheKeyProviderDescriptor.ParameterIndex];
            if (cacheKeyProviderDescriptor.IsSampleOrNullableType)
            {
                return sortedParameteValue?.ToString();
            }

            dynamic parameterValue = sortedParameteValue;
            var cacheKeyValueProp = parameterValue.GetType().GetProperty(cacheKeyProviderDescriptor.PropName);
            if (cacheKeyValueProp == null)
            {
                throw new SilkyException(
                    $"Failed to get the value of the cache interception key:{cacheKeyProviderDescriptor.PropName}");
            }
            var cacheKeyValue = cacheKeyValueProp.GetValue(parameterValue, null);
            return cacheKeyValue?.ToString();
        }

        throw new SilkyException(
            $"Failed to get the value of the cache interception key:{cacheKeyProviderDescriptor.PropName}");
    }
}