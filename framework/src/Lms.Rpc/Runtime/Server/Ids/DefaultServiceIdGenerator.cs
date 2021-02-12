using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lms.Rpc.Runtime.Server.Ids
{
    public class DefaultServiceIdGenerator : IServiceIdGenerator
    {
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
        public string GenerateServiceId(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            var type = method.DeclaringType;
            if (type == null)
                throw new ArgumentNullException(nameof(method.DeclaringType), "方法的定义类型不能为空。");

            var id = $"{type.FullName}.{method.Name}".Replace(".","_");
            var parameters = method.GetParameters();
            if (parameters.Any())
            {
                id += "_" + string.Join("_", parameters.Select(i => i.Name));
            }
            Logger.LogDebug($"为方法：{method}生成服务Id：{id}。");
            return id;
        }

    }
}