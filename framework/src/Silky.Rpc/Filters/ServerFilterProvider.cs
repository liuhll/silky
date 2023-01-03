using System;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Filters
{
    public class ServerFilterProvider : IServerFilterProvider, ISingletonDependency
    {
        public IFilterMetadata[] GetServerFilters(ServiceEntry serviceEntry, Type instanceType)
        {
            // var globalFilter = EngineContext.Current.ResolveAll<IServerFilter>();
            // var serverFilters = new List<IServerFilter>();
            // serverFilters.AddRange(globalFilter);
            // serverFilters.AddRange(serviceEntry.ServerFilters);
            //
            // var serviceInstanceFilters = instanceType.GetCustomAttributes().OfType<IServerFilter>();
            // serverFilters.AddRange(serviceInstanceFilters);
            //
            // var implementationMethod = instanceType.GetTypeInfo()
            //     .GetCompareMethod(serviceEntry.MethodInfo, serviceEntry.MethodInfo.Name);
            // var implementationMethodFilters = implementationMethod.GetCustomAttributes().OfType<IServerFilter>();
            // serverFilters.AddRange(implementationMethodFilters);
            // var filters = serverFilters.ToArray();
            // return filters;

            // 实现如何获取服务端该服务条目设置的过滤器
            return null;
        }
    }
}