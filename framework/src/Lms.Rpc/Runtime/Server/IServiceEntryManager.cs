using System;
using System.Collections.Generic;
using Lms.Core.DependencyInjection;

namespace Lms.Rpc.Runtime.Server
{
    public interface IServiceEntryManager : ISingletonDependency
    {
        IReadOnlyList<ServiceEntry> GetLocalEntries();
        
        IReadOnlyList<ServiceEntry> GetAllEntries();

        void Update(ServiceEntry serviceEntry);

        event EventHandler<ServiceEntry> OnUpdate;

    }
}