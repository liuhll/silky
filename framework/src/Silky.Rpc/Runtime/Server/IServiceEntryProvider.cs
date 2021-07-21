using System.Collections.Generic;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServiceEntryProvider : ITransientDependency
    {
        IReadOnlyList<ServiceEntry> GetEntries();
    }
}