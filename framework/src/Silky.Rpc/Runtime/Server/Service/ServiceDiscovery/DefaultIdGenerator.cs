using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Silky.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Rpc.Routing;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultIdGenerator : IIdGenerator
    {
        private readonly ServiceEntryIdCache _serviceEntryIdCache;
        public ILogger<DefaultIdGenerator> Logger { get; set; }

        public DefaultIdGenerator(ServiceEntryIdCache serviceEntryIdCache)
        {
            _serviceEntryIdCache = serviceEntryIdCache;
            Logger = NullLogger<DefaultIdGenerator>.Instance;
        }

        /// <summary>
        /// 生成一个服务Id。
        /// </summary>
        /// <param name="method">本地方法信息。</param>
        /// <param name="httpMethod"></param>
        /// <returns>对应方法的唯一服务Id。</returns>
        public string GenerateServiceEntryId([NotNull] MethodInfo method, HttpMethod httpMethod)
        {
            Check.NotNull(method, nameof(method));

            if (EngineContext.Current.ApplicationOptions.UsingServiceShortName)
            {
                return GetShortServiceEntryId(method, httpMethod);
            }
            else
            {
                return GetLongServiceEntryId(method, httpMethod);
            }

           
        }

        private string GetLongServiceEntryId(MethodInfo method, HttpMethod httpMethod)
        {
            var type = method.DeclaringType;
            if (type == null)
                throw new ArgumentNullException(nameof(method.DeclaringType),
                    "The definition type of the method cannot be empty.");

            var id = $"{type.FullName}.{method.Name}";
            var parameters = method.GetParameters();
            if (parameters.Any())
            {
                id += "." + string.Join(".", parameters.Select(i => i.Name));
            }

            id += $":{httpMethod.ToString()}";

            Logger.LogDebug("Generate ServiceEntry [{0}] for the method {1}", id, method.Name);
            return id;
        }

        private string GetShortServiceEntryId(MethodInfo method, HttpMethod httpMethod)
        {
            var type = method.DeclaringType;
            if (type == null)
                throw new ArgumentNullException(nameof(method.DeclaringType),
                    "The definition type of the method cannot be empty.");
            var serviceRoute = type.GetCustomAttribute<ServiceRouteAttribute>()!;
            if (type.GetCustomAttribute<SilkyAppServiceAttribute>() != null)
            {
                var id = $"{serviceRoute.GetServiceName(type)}.{method.Name}:{httpMethod.ToString()}";
                return id;
            }
            else
            {
                var id = $"{EngineContext.Current.HostName}.{serviceRoute.GetServiceName(type)}.{method.Name}:{httpMethod.ToString()}";
                return id;
            }

           
        }

        public string GetDefaultServiceEntryId([NotNull]MethodInfo method)
        {
            if (_serviceEntryIdCache.TryGetServiceEntryId(method, out var id))
            {
                return id;
            }

            var httpMethodInfos = method.GetHttpMethodInfos();
            var defaultHttpMethod = httpMethodInfos.First().HttpMethod;
            id = GenerateServiceEntryId(method, defaultHttpMethod);
            _serviceEntryIdCache.UpdateServiceEntryIdCache(method, id);
            return id;
        }

        public string GenerateServiceId([NotNull]Type serviceType)
        {
            Check.NotNull(serviceType, nameof(serviceType));
          
            return serviceType.FullName;
        }
    }
}