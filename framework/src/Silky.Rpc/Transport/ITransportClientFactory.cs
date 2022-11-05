using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Transport
{
    public interface ITransportClientFactory : ISingletonDependency
    {
        Task<ITransportClient> GetClient(ISilkyEndpoint silkyEndpoint);
    }
}