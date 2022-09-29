using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silky.Core;
using Silky.Core.DependencyInjection;
using Silky.Core.Extensions;

namespace Silky.Rpc.Runtime.Server
{
    public class ServerFilterProvider : ISingletonDependency
    {
        public IServerFilter[] GetServerFilters(ServiceEntry serviceEntry, Type instanceType)
        {
            var globalFilter = EngineContext.Current.ResolveAll<IServerFilter>();
            var serverFilters = new List<IServerFilter>();
            serverFilters.AddRange(globalFilter);
            serverFilters.AddRange(serviceEntry.ServerFilters);

            var serviceInstanceFilters = instanceType.GetCustomAttributes().OfType<IServerFilter>();
            serverFilters.AddRange(serviceInstanceFilters);

            var implementationMethod = instanceType.GetTypeInfo()
                .GetCompareMethod(serviceEntry.MethodInfo, serviceEntry.MethodInfo.Name);
            var implementationMethodFilters = implementationMethod.GetCustomAttributes().OfType<IServerFilter>();
            serverFilters.AddRange(implementationMethodFilters);
            var filters = serverFilters.OrderBy(p => p.Order).ToArray();
            return filters;
        }
    }
}