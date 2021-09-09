using System;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server.ServiceDiscovery
{
    public interface IServiceGenerator : ITransientDependency
    {
        ServiceInfo CreateService((Type,bool) serviceTypeInfo);
        
        ServiceInfo CreateWsService(Type wsServiceType);
    }
}