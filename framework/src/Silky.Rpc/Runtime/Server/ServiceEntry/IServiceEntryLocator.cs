using Silky.Core.DependencyInjection;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServiceEntryLocator : ISingletonDependency
    {
        ServiceEntry GetServiceEntryById(string id);

        ServiceEntry GetLocalServiceEntryById(string id);
    }
}