using System.Collections.Generic;
using Lms.Core.DependencyInjection;

namespace Lms.Rpc.Runtime.Server.ServiceEntry
{
    public interface IServiceEntryManager : ISingletonDependency
    {
        IReadOnlyList<ServiceEntry> GetLocalEntries();
        
        IReadOnlyList<ServiceEntry> GetAllEntries();
    }
}