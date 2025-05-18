using System;
using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Transport
{
    public interface ITransportClientFactory : ISingletonDependency, IDisposable
    {
        Task<ITransportClient> GetClient(ISilkyEndpoint silkyEndpoint);

        void RemoveClient(ISilkyEndpoint silkyEndpoint);
    }
}