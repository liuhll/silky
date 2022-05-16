using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServiceEntryManager : ISingletonDependency
    {
        IReadOnlyList<ServiceEntry> GetLocalEntries();

        IReadOnlyList<ServiceEntry> GetAllEntries();

        IReadOnlyCollection<ServiceEntry> GetServiceEntries(string serviceId);

        ServiceEntry GetServiceEntry(string serviceEntryId);

        bool HasHttpProtocolServiceEntry();

        void Update(ServiceEntry serviceEntry);

        event EventHandler<ServiceEntry> OnUpdate;
    }
}