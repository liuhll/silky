using System;
using System.Collections.Generic;
using Silky.Lms.Core.DependencyInjection;

namespace Silky.Lms.Rpc.Runtime.Server.ServiceDiscovery
{
    public interface IServiceEntryGenerator : ITransientDependency
    {
        IEnumerable<ServiceEntry> CreateServiceEntry((Type,bool) serviceType);
    }
}