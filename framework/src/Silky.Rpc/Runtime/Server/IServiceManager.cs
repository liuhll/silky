using System;
using System.Collections.Generic;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServiceManager : ISingletonDependency
    {
        IReadOnlyList<ServiceInfo> GetLocalService();

        IReadOnlyList<ServiceInfo> GetAllService();

        bool IsLocalService(string serviceId);

        ServiceInfo GetService(string serviceId);

        void Update(ServiceInfo serviceInfo);

        event EventHandler<ServiceInfo> OnUpdate;
    }
}