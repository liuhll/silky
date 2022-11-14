using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Silky.Core;
using Silky.Core.Convertible;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Core.Extensions.Collections.Generic;
using Silky.Core.Runtime.Session;
using Silky.Core.Serialization;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.CachingInterceptor;

public static class CacheKeyHelper
{
    public static (string, bool) GetCachingInterceptKey(object parameters,
        [NotNull] CachingInterceptorDescriptor cachingInterceptProvider, string serviceKey)
    {
        Check.NotNull(cachingInterceptProvider, nameof(cachingInterceptProvider));
        var template = cachingInterceptProvider.KeyTemplate;
        if (template.IsNullOrEmpty())
        {
            throw new SilkyException(
                "The KeyTemplate specified by the cache interception is not allowed to be empty",
                StatusCode.CachingInterceptError);
        }

        var cacheKeyProviders = GetCacheKeyProviders(cachingInterceptProvider, parameters);
        return (GetCachingInterceptKey(cacheKeyProviders, cachingInterceptProvider, serviceKey),
            cacheKeyProviders.Any(p => p.Value.IsNullOrEmpty()));
    }


    public static (string, bool) GetCachingInterceptKey([NotNull] ServiceEntry serviceEntry,
        [NotNull] object[] parameters,
        [NotNull] ICachingInterceptProvider cachingInterceptProvider, string serviceKey)
    {
        Check.NotNull(serviceEntry, nameof(serviceEntry));
        Check.NotNull(parameters, nameof(parameters));
        Check.NotNull(cachingInterceptProvider, nameof(cachingInterceptProvider));
        var template = cachingInterceptProvider.KeyTemplate;
        if (template.IsNullOrEmpty())
        {
            throw new SilkyException(
                "The KeyTemplate specified by the cache interception is not allowed to be empty",
                StatusCode.CachingInterceptError);
        }

        var cacheKeyProviders = GetCacheKeyProviders(serviceEntry, cachingInterceptProvider, parameters);

        return (GetCachingInterceptKey(cacheKeyProviders, cachingInterceptProvider.CachingInterceptorDescriptor,
                serviceKey),
            cacheKeyProviders.Any(p => p.Value.IsNullOrEmpty()));
    }


    private static string GetCachingInterceptKey(CacheKeyProvider[] cacheKeyProviders,
        CachingInterceptorDescriptor cachingInterceptProvider, string serviceKey)
    {
        if (cachingInterceptProvider.CachingMethod != CachingMethod.Update &&
            cacheKeyProviders.Any(p => p.Value.IsNullOrEmpty()))
        {
            throw new SilkyException(
                $"Failed to get parameter value of cache interception with {cachingInterceptProvider.KeyTemplate} - {cachingInterceptProvider.CachingMethod}.",
                StatusCode.CachingInterceptError);
        }

        if (cachingInterceptProvider.CachingMethod == CachingMethod.Update &&
            cachingInterceptProvider.IgnoreWhenCacheKeyNull == false &&
            cacheKeyProviders.Any(p => p.Value.IsNullOrEmpty()))
        {
            throw new SilkyException(
                $"Failed to get parameter value of cache interception with {cachingInterceptProvider.KeyTemplate} - {cachingInterceptProvider.CachingMethod}.",
                StatusCode.CachingInterceptError);
        }

        var cachingInterceptKey = cachingInterceptProvider.GetCacheKeyType() == CacheKeyType.Attribute
            ? ParserAttributeCacheKey(cacheKeyProviders, cachingInterceptProvider.KeyTemplate)
            : ParserNamedCacheKey(cacheKeyProviders, cachingInterceptProvider.KeyTemplate);

        if (!serviceKey.IsNullOrEmpty())
        {
            cachingInterceptKey = $"serviceKey:{serviceKey}:" + cachingInterceptKey;
        }

        if (!cachingInterceptProvider.OnlyCurrentUserData) return cachingInterceptKey;
        var session = NullSession.Instance;
        if (!session.IsLogin())
        {
            throw new SilkyException(
                "If the cached data is specified to be related to the currently logged in user, then you must log in to the system to allow the use of cache interception",
                StatusCode.CachingInterceptError);
        }

        cachingInterceptKey += $":userId:{session.UserId}";

        return cachingInterceptKey;
    }

    private static string ParserNamedCacheKey(CacheKeyProvider[] cacheKeyProviders, string keyTemplate)
    {
        var keyTemplateParameters = Regex.Matches(keyTemplate, CacheKeyConstants.CacheKeyParameterRegex)
            .Select(q => q.Value.RemoveCurlyBraces());
        var index = 0;
        foreach (var keyTemplateParameter in keyTemplateParameters)
        {
            var cacheKeyProvider = cacheKeyProviders.FirstOrDefault(p =>
                p.PropName.Equals(keyTemplateParameter, StringComparison.OrdinalIgnoreCase));
            if (cacheKeyProvider == null)
            {
                throw new SilkyException(
                    $"Failed to parse parameter {keyTemplate} from cache key providers",
                    StatusCode.CachingInterceptError);
            }

            keyTemplate = keyTemplate.Replace("{" + keyTemplateParameter + "}", cacheKeyProvider.Value);
            index++;
        }

        return keyTemplate;
    }

    private static string ParserAttributeCacheKey(CacheKeyProvider[] cacheKeyProviders, string keyTemplate)
    {
        var templateArgs = cacheKeyProviders.Select(ckp => ckp.Value).ToArray();
        var cachingInterceptKey = string.Format(keyTemplate, templateArgs);
        return cachingInterceptKey;
    }

    private static CacheKeyProvider[] GetCacheKeyProviders(CachingInterceptorDescriptor cachingInterceptProvider,
        object parameters)
    {
        var cacheKeyProviders = new List<CacheKeyProvider>();
        foreach (var cacheKeyProviderDescriptor in cachingInterceptProvider.CacheKeyProviderDescriptors)
        {
            var cacheKeyProvider = new CacheKeyProvider()
            {
                Index = cacheKeyProviderDescriptor.Index,
                PropName = cacheKeyProviderDescriptor.PropName,
                CacheKeyType = cacheKeyProviderDescriptor.CacheKeyType,
                Value = GetCacheKeyValue(cacheKeyProviderDescriptor, parameters)
            };
            cacheKeyProviders.Add(cacheKeyProvider);
        }

        return cacheKeyProviders.OrderBy(p => p.Index).ToArray();
    }

    private static CacheKeyProvider[] GetCacheKeyProviders(ServiceEntry serviceEntry,
        ICachingInterceptProvider cachingInterceptProvider, object[] parameters)
    {
        var cacheKeyProviders = new List<CacheKeyProvider>();


        foreach (var cacheKeyProviderDescriptor in cachingInterceptProvider.CachingInterceptorDescriptor
                     .CacheKeyProviderDescriptors)
        {
            var cacheKeyProvider = new CacheKeyProvider()
            {
                Index = cacheKeyProviderDescriptor.Index,
                PropName = cacheKeyProviderDescriptor.PropName,
                CacheKeyType = cacheKeyProviderDescriptor.CacheKeyType,
                Value = GetCacheKeyValue(serviceEntry, cacheKeyProviderDescriptor, parameters)
            };
            cacheKeyProviders.Add(cacheKeyProvider);
        }

        return cacheKeyProviders.ToArray();
    }

    private static string GetCacheKeyValue(ServiceEntry serviceEntry,
        CacheKeyProviderDescriptor cacheKeyProviderDescriptor, object[] parameters)
    {
        if (cacheKeyProviderDescriptor.IsSampleOrNullableType)
        {
            return parameters[cacheKeyProviderDescriptor.ParameterIndex]?.ToString();
        }

        var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
        var parameterDescriptor =
            serviceEntry.ParameterDescriptors[cacheKeyProviderDescriptor.ParameterIndex];
        var parameterValue =
            typeConvertibleService.Convert(parameters[cacheKeyProviderDescriptor.ParameterIndex],
                parameterDescriptor.Type);
        var cacheKeyProp = parameterDescriptor.Type.GetProperty(cacheKeyProviderDescriptor.PropName);
        return cacheKeyProp.GetValue(parameterValue)?.ToString();
    }


    private static string GetCacheKeyValue(CacheKeyProviderDescriptor cacheKeyProviderDescriptor, object parameters)

    {
        var serializer = EngineContext.Current.Resolve<ISerializer>();
        switch (parameters)
        {
            case IDictionary<string, object> dictParameters when cacheKeyProviderDescriptor.IsSampleOrNullableType &&
                                                                 dictParameters.TryOrdinalIgnoreCaseGetValue(
                                                                     cacheKeyProviderDescriptor.PropName,
                                                                     out var value):
                return value?.ToString();
            case IDictionary<string, object> dictParameters when !cacheKeyProviderDescriptor.IsSampleOrNullableType:
            {
                var cacheKeyValue = string.Empty;
                foreach (var dictParameter in dictParameters)
                {
                    if (dictParameter.Value.GetType() == typeof(string) && dictParameter.Value.ToString().IsValidJson())
                    {
                        var httpParameterDictValue =
                            serializer.Deserialize<IDictionary<string, object>>(dictParameter.Value.ToString());
                        if (httpParameterDictValue.TryOrdinalIgnoreCaseGetValue(cacheKeyProviderDescriptor.PropName,
                                out var keyValue))
                        {
                            cacheKeyValue = keyValue?.ToString();
                            break;
                        }
                    }

                    dynamic dictParameterValue = dictParameter.Value;
                    var cacheKeyValueProp =
                        dictParameterValue.GetType().GetProperty(cacheKeyProviderDescriptor.PropName);
                    if (cacheKeyValueProp != null)
                    {
                        cacheKeyValue = cacheKeyValueProp.GetValue(dictParameterValue, null);
                        break;
                    }
                }

                return cacheKeyValue;
            }
            case IDictionary<ParameterFrom, object> httpParameters
                when httpParameters.TryGetValue(cacheKeyProviderDescriptor.From, out var httpParameterValue):
            {
                if (httpParameterValue == null)
                {
                    throw new SilkyException(
                        $"Failed to get the value of the cache interception key value:{cacheKeyProviderDescriptor.From}");
                }

                var httpParameterValueLine = httpParameterValue.ToString();
                if (cacheKeyProviderDescriptor.IsSampleOrNullableType && !httpParameterValueLine.IsValidJson())
                {
                    return httpParameterValue.ToString();
                }

                var httpParameterDictValue =
                    serializer.Deserialize<IDictionary<string, object>>(httpParameterValueLine);
                if (httpParameterDictValue.TryOrdinalIgnoreCaseGetValue(cacheKeyProviderDescriptor.PropName,
                        out var cacheKeyValue))
                {
                    return cacheKeyValue?.ToString();
                }

                return null;
            }
            case object[] sortedParameters:
            {
                var sortedParameterValue = sortedParameters[cacheKeyProviderDescriptor.ParameterIndex];
                if (cacheKeyProviderDescriptor.IsSampleOrNullableType)
                {
                    return sortedParameterValue?.ToString();
                }

                dynamic parameterValue = sortedParameterValue;
                var cacheKeyValueProp = parameterValue.GetType().GetProperty(cacheKeyProviderDescriptor.PropName);
                if (cacheKeyValueProp == null)
                {
                    return cacheKeyValueProp;
                }

                var cacheKeyValue = cacheKeyValueProp.GetValue(parameterValue, null);
                return cacheKeyValue?.ToString();
            }
            default:
                return null;
        }
    }
}