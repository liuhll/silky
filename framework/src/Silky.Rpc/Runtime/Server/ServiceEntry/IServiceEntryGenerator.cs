using System;
using System.Collections.Generic;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServiceEntryGenerator : ITransientDependency
    {
        IEnumerable<ServiceEntry> CreateServiceEntry((Type, bool) serviceType);
    }
}