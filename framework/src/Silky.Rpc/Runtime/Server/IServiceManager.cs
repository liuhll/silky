using System;
using System.Collections.Generic;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServiceManager : ISingletonDependency
    {
        IReadOnlyCollection<Service> GetLocalService();
        
        IReadOnlyCollection<Service> GetLocalService(ServiceProtocol serviceProtocol);

        IReadOnlyCollection<Service> GetAllService();
        
        IReadOnlyCollection<Service> GetAllService(ServiceProtocol serviceProtocol);

        IReadOnlyCollection<string> GetAllApplications();

        bool IsLocalService(string serviceId);

        Service GetService(string serviceId);

        void Update(Service service);

        event EventHandler<Service> OnUpdate;
    }
}