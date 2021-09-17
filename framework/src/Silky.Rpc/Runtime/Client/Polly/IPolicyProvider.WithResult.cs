using Polly;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Runtime.Client
{
    public interface IPolicyWithResultProvider : IScopedDependency
    {
        IAsyncPolicy<object> Create(ServiceEntry serviceEntry, object[] parameters);
    }
}