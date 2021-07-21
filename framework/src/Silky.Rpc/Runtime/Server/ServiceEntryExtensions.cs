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
using Silky.Rpc.Routing.Descriptor;
using Silky.Rpc.Runtime.Server.Parameter;
using Silky.Rpc.Runtime.Session;
using Silky.Rpc.Transport.CachingIntercept;
using Silky.Rpc.Utils;

namespace Silky.Rpc.Runtime.Server
{
    public static class ServiceEntryExtensions
    {
        public static ServiceRouteDescriptor CreateLocalRouteDescriptor(this ServiceEntry serviceEntry)
        {
            if (!serviceEntry.IsLocal)
            {
                throw new SilkyException("只允许本地服务条目生成路由描述符");
            }

            return new ServiceRouteDescriptor()
            {
                ServiceDescriptor = serviceEntry.ServiceDescriptor,
                AddressDescriptors = new[]
                    {NetUtil.GetRpcAddressModel().Descriptor},
            };
        }


        public static string GetHashKeyValue(this ServiceEntry serviceEntry, object[] parameterValues)
        {
            var hashKey = string.Empty;
            if (!serviceEntry.ParameterDescriptors.Any())
            {
                hashKey = serviceEntry.Router.RoutePath + serviceEntry.Router.HttpMethod;
            }

            var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
            if (serviceEntry.ParameterDescriptors.Any(p => p.IsHashKey))
            {
                var index = 0;
                foreach (var parameterDescriptor in serviceEntry.ParameterDescriptors)
                {
                    if (parameterDescriptor.IsHashKey)
                    {
                        hashKey = GetHashKey(parameterValues, parameterDescriptor, index, typeConvertibleService);
                        break;
                    }

                    index++;
                }
            }
            else
            {
                var index = 0;
                foreach (var parameterDescriptor in serviceEntry.ParameterDescriptors)
                {
                    hashKey = GetHashKey(parameterValues, parameterDescriptor, index, typeConvertibleService);
                    break;
                }
            }

            return hashKey;
        }

        public static IDictionary<string, object> CreateDictParameters(this ServiceEntry serviceEntry,
            [NotNull] object[] parameters)
        {
            Check.NotNull(parameters, nameof(parameters));
            var dictionaryParms = new Dictionary<string, object>();
            var index = 0;
            var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
            foreach (var parameter in serviceEntry.ParameterDescriptors)
            {
                if (parameter.IsSample)
                {
                    dictionaryParms[parameter.Name] = parameters[index];
                }
                else
                {
                    dictionaryParms[parameter.Name] = typeConvertibleService.Convert(parameters[index], parameter.Type);
                }

                index++;
            }

            return dictionaryParms;
        }

        public static object[] ConvertParameters(this ServiceEntry serviceEntry, object[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] != null && parameters[i].GetType() != serviceEntry.ParameterDescriptors[i].Type)
                {
                    var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
                    parameters[i] =
                        typeConvertibleService.Convert(parameters[i], serviceEntry.ParameterDescriptors[i].Type);
                }
            }

            return parameters;
        }

        public static string GetCachingInterceptKey(this ServiceEntry serviceEntry, [NotNull] object[] parameters,
            [NotNull] ICachingInterceptProvider cachingInterceptProvider)
        {
            Check.NotNull(parameters, nameof(parameters));
            Check.NotNull(cachingInterceptProvider, nameof(cachingInterceptProvider));
            var templete = cachingInterceptProvider.KeyTemplete;
            if (templete.IsNullOrEmpty())
            {
                throw new SilkyException("缓存拦截指定的KeyTemplete不允许为空", StatusCode.CachingInterceptError);
            }

            var cachingInterceptKey = string.Empty;
            var cacheKeyProviders = new List<ICacheKeyProvider>();
            var index = 0;
            var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
            foreach (var parameterDescriptor in serviceEntry.ParameterDescriptors)
            {
                if (parameterDescriptor.CacheKeys.Any())
                {
                    if (parameterDescriptor.IsSample)
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
            var currentServiceKey = EngineContext.Current.Resolve<ICurrentServiceKey>();
            if (!currentServiceKey.ServiceKey.IsNullOrEmpty())
            {
                cachingInterceptKey = $"serviceKey:{currentServiceKey.ServiceKey}:" + cachingInterceptKey;
            }

            if (cachingInterceptProvider.OnlyCurrentUserData)
            {
                var session = NullSession.Instance;
                if (!session.IsLogin())
                {
                    throw new SilkyException("缓存数据如果指定与当前登录用户相关,那么必须登录系统才允许使用缓存拦截", StatusCode.CachingInterceptError);
                }

                cachingInterceptKey = cachingInterceptKey + $":userId:{session.UserId}";
            }

            return cachingInterceptKey;
        }

        public static bool IsTransactionServiceEntry([NotNull] this ServiceEntry serviceEntry)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            return serviceEntry.CustomAttributes.Any(p =>
                p.GetType().GetTypeInfo().FullName == "Silky.Transaction.TransactionAttribute");
        }

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

        private static string GetHashKey(object[] parameterValues, ParameterDescriptor parameterDescriptor, int index,
            ITypeConvertibleService typeConvertibleService)
        {
            string hashKey;
            if (parameterDescriptor.IsSample)
            {
                var propVal = parameterValues[index];
                if (propVal == null)
                {
                    throw new SilkyException("hashKey指定的值不允许为空");
                }

                hashKey = propVal.ToString();
            }
            else
            {
                var parameterValue =
                    typeConvertibleService.Convert(parameterValues[index], parameterDescriptor.Type);
                var hashKeyProp = parameterDescriptor.Type
                    .GetProperties().First();
                var hashKeyProviderProps = parameterDescriptor.Type
                    .GetProperties()
                    .Where(p => p.GetCustomAttributes().OfType<IHashKeyProvider>().Any());
                if (hashKeyProviderProps.Any())
                {
                    hashKeyProp = hashKeyProviderProps.First();
                }

                var propValue = hashKeyProp.GetValue(parameterValue);
                if (propValue == null)
                {
                    throw new SilkyException("hashKey指定的属性的值不允许为空");
                }

                hashKey = propValue.ToString();
            }

            return hashKey;
        }
    }
}