using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Silky.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Silky.Rpc.Runtime.Server
{
    public class DefaultServiceIdGenerator : IServiceIdGenerator
    {
        private ConcurrentDictionary<MethodInfo, string> m_serviceIdCache = new();
        
        public ILogger<DefaultServiceIdGenerator> Logger { get; set; }

        public DefaultServiceIdGenerator()
        {
            Logger = NullLogger<DefaultServiceIdGenerator>.Instance;
        }

        /// <summary>
        /// 生成一个服务Id。
        /// </summary>
        /// <param name="method">本地方法信息。</param>
        /// <returns>对应方法的唯一服务Id。</returns>
        public string GenerateServiceId([NotNull] MethodInfo method)
        {
            Check.NotNull(method, nameof(method));
            var type = method.DeclaringType;
            if (type == null)
                throw new ArgumentNullException(nameof(method.DeclaringType), "The definition type of the method cannot be empty.");

            if (m_serviceIdCache.TryGetValue(method, out var id))
            {
                return id;
            }

            id = $"{type.FullName}.{method.Name}".Replace(".", "_");
            var parameters = method.GetParameters();
            if (parameters.Any())
            {
                id += "_" + string.Join("_", parameters.Select(i => i.Name));
            }

            Logger.LogDebug($"Generate serviceId {id} for method {method.DeclaringType?.FullName}.{method.Name}");
            return m_serviceIdCache.GetOrAdd(method, id);
        }
    }
}