using System;
using System.Collections.Generic;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServiceManager : ISingletonDependency
    {
        IReadOnlyList<Service> GetLocalService();

        IReadOnlyList<Service> GetAllService();

        IReadOnlyCollection<string> GetAllApplications();

        bool IsLocalService(string serviceId);

        Service GetService(string serviceId);

        void Update(Service service);

        event EventHandler<Service> OnUpdate;
    }
}