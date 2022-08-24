using Silky.Core.DependencyInjection;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Transport
{
    public interface ITransportClientFactory : ISingletonDependency
    {
        ITransportClient GetClient(IRpcEndpoint rpcEndpoint);
    }
}