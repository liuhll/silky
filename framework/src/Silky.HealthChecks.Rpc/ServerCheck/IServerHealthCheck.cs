using System.Threading.Tasks;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.HealthChecks.Rpc.ServerCheck
{
    public interface IServerHealthCheck
    {
        Task<bool> IsHealth(ISilkyEndpoint silkyEndpoint);

        Task<bool> IsHealth(SilkyEndpointDescriptor silkyEndpointDescriptor);
    }
}