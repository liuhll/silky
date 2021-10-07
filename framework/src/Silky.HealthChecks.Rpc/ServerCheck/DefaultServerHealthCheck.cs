using System.Threading.Tasks;
using Silky.Core.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Utils;

namespace Silky.HealthChecks.Rpc.ServerCheck
{
    public class DefaultServerHealthCheck : IServerHealthCheck
    {
        public Task<bool> IsHealth(IRpcEndpoint rpcEndpoint)
        {
            var address = rpcEndpoint.GetAddress();


            if (rpcEndpoint.ServiceProtocol.IsHttp())
            {
                return Task.FromResult<bool>(UrlCheck.UrlIsValid($"{rpcEndpoint.ServiceProtocol}://{address}", out _));
            }

            return Task.FromResult<bool>(SocketCheck.TestConnection(rpcEndpoint.Host, rpcEndpoint.Port));
        }

        public Task<bool> IsHealth(RpcEndpointDescriptor rpcEndpointDescriptor)
        {
            if (rpcEndpointDescriptor.ServiceProtocol.IsHttp())
            {
                var address = rpcEndpointDescriptor.GetHostAddress();
                return Task.FromResult<bool>(UrlCheck.UrlIsValid($"{rpcEndpointDescriptor.ServiceProtocol}://{address}",
                    out var _));
            }

            return Task.FromResult<bool>(SocketCheck.TestConnection(rpcEndpointDescriptor.Host,
                rpcEndpointDescriptor.Port));
        }

        private bool IsLocalAddress(string address)
        {
            var localAddress = RpcEndpointHelper.GetLocalTcpEndpoint().GetAddress();
            return localAddress.Equals(address);
        }
    }
}