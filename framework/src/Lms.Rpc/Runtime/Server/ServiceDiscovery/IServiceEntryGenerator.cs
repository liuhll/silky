using System;
using System.Collections.Generic;
using Lms.Core.DependencyInjection;

namespace Lms.Rpc.Runtime.Server.ServiceDiscovery
{
    public interface IServiceEntryGenerator : ITransientDependency
    {
        IEnumerable<ServiceEntry> CreateServiceEntry((Type,bool) serviceType);
    }
}