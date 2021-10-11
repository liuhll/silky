using System;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServiceGenerator : ITransientDependency
    {
        Service CreateService((Type, bool) serviceTypeInfo);

        Service CreateWsService(Type wsServiceType);
    }
}