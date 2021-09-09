using System.Collections.Generic;
using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServiceProvider : ITransientDependency
    {
        IReadOnlyList<ServiceEntry> GetEntries();

        IReadOnlyCollection<ServiceInfo> GetServices();
    }
}