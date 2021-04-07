using System.Collections.Generic;
using Silky.Lms.Core.DependencyInjection;

namespace Silky.Lms.Rpc.Runtime.Server
{
    public interface IServiceEntryProvider : ITransientDependency
    {
        IReadOnlyList<ServiceEntry> GetEntries();
    }
}