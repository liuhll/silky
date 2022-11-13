using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Silky.Core;
using Silky.Core.Convertible;
using Silky.Core.Exceptions;
using Silky.Core.Extensions;
using Silky.Rpc.Auditing;

namespace Silky.Rpc.Runtime.Server
{
    public static class ServiceEntryExtensions
    {
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
                if (parameter.IsSampleOrNullableType)
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
                    if (serviceEntry.ParameterDescriptors[i].Type == typeof(IFormFile) ||
                        serviceEntry.ParameterDescriptors[i].Type == typeof(IFormFileCollection))
                    {
                        continue;
                    }

                    var typeConvertibleService = EngineContext.Current.Resolve<ITypeConvertibleService>();
                    parameters[i] =
                        typeConvertibleService.Convert(parameters[i], serviceEntry.ParameterDescriptors[i].Type);
                }
            }

            return parameters;
        }

        public static bool IsTransactionServiceEntry([NotNull] this ServiceEntry serviceEntry)
        {
            Check.NotNull(serviceEntry, nameof(serviceEntry));
            return serviceEntry.CustomAttributes.Any(p =>
                p.GetType().GetTypeInfo().FullName == "Silky.Transaction.TransactionAttribute");
        }

        public static ServiceDescriptor GetServiceDescriptor(this ServiceEntry serviceEntry)
        {
            var serverManager = EngineContext.Current.Resolve<IServerManager>();
            var serviceDescriptor = serverManager.GetServiceDescriptor(serviceEntry.ServiceId);
            return serviceDescriptor;
        }

        public static bool DisableAuditing(this ServiceEntry serviceEntry)
        {
            if (serviceEntry == null)
            {
                return false;
            }

            var disableAuditing = serviceEntry.CustomAttributes.OfType<DisableAuditingAttribute>().Any()
                                  || serviceEntry.ServiceType.GetCustomAttributes(true)
                                      .OfType<DisableAuditingAttribute>()
                                      .Any();
            return disableAuditing;
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

        public static bool IsEnableAuditing(this ServiceEntry serviceEntry, bool isEnable)
        {
            return isEnable && !serviceEntry.DisableAuditing();
        }

        public static ICollection<ICachingInterceptProvider> GetAllCachingInterceptProviders(
            this ServiceEntry serviceEntry)
        {
            return serviceEntry.CustomAttributes.OfType<ICachingInterceptProvider>().ToList();
        }

        public static ICachingInterceptProvider GetGetCachingInterceptProvider(this ServiceEntry serviceEntry)
        {
            return serviceEntry.CustomAttributes.OfType<IGetCachingInterceptProvider>().FirstOrDefault();
        }

        public static ICachingInterceptProvider[] GetUpdateCachingInterceptProviders(this ServiceEntry serviceEntry)
        {
            return serviceEntry.CustomAttributes.OfType<IUpdateCachingInterceptProvider>().ToArray();
        }

        public static IReadOnlyCollection<IRemoveCachingInterceptProvider> GetRemoveCachingInterceptProviders(
            this ServiceEntry serviceEntry)
        {
            return serviceEntry.CustomAttributes.OfType<IRemoveCachingInterceptProvider>().ToArray();
        }
        
        public static ICachingInterceptProvider[] GetAllRemoveCachingInterceptProviders(
            this ServiceEntry serviceEntry)
        {
            return serviceEntry.CustomAttributes.OfType<ICachingInterceptProvider>().Where(p=> p.CachingMethod == CachingMethod.Remove).ToArray();
        }

        public static IRemoveMatchKeyCachingInterceptProvider[]
            GetRemoveMatchKeyCachingInterceptProviders(
                this ServiceEntry serviceEntry)
        {
            return serviceEntry.CustomAttributes.OfType<IRemoveMatchKeyCachingInterceptProvider>().ToArray();
        }

        public static bool NeedHttpProtocolSupport(this ServiceEntry serviceEntry)
        {
            if (typeof(IActionResult).IsAssignableFrom(serviceEntry.ReturnType))
            {
                return true;
            }

            foreach (var parameterDescriptor in serviceEntry.ParameterDescriptors)
            {
                if (parameterDescriptor.IsSupportFileParameter())
                {
                    return true;
                }
            }

            return false;
        }
        
        public static bool IsSilkyAppService(this ServiceEntry serviceEntry)
        {
            return "Silky.Http.Dashboard.AppService.ISilkyAppService".Equals(serviceEntry.ServiceType.FullName);
        }


        private static string GetHashKey(object[] parameterValues, ParameterDescriptor parameterDescriptor, int index,
            ITypeConvertibleService typeConvertibleService)
        {
            string hashKey;
            if (parameterDescriptor.IsSampleOrNullableType)
            {
                var propVal = parameterValues[index];
                if (propVal == null)
                {
                    throw new SilkyException("The value specified by hashKey is not allowed to be empty");
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
                    throw new SilkyException("The value of the attribute specified by hashKey cannot be empty");
                }

                hashKey = propValue.ToString();
            }

            return hashKey;
        }
    }
}