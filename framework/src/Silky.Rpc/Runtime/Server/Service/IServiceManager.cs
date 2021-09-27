using System;
using System.Collections.Generic;
using Silky.Core.DependencyInjection;
using Silky.Core.Rpc;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServiceManager : ISingletonDependency
    {
        IReadOnlyList<Service> GetLocalService();

        IReadOnlyCollection<Service> GetLocalService(ServiceProtocol serviceProtocol);

        IReadOnlyList<Service> GetAllService();

        IReadOnlyCollection<Service> GetAllService(ServiceProtocol serviceProtocol);

        bool IsLocalService(string serviceId);

        Service GetService(string serviceId);

        Service GeLocalService(string serviceId);


        void Update(Service service);

        event EventHandler<Service> OnUpdate;
    }
}