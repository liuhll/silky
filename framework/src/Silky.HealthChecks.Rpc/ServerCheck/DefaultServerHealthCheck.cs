using System;
using System.Threading.Tasks;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Utils;

namespace Silky.HealthChecks.Rpc.ServerCheck
{
    public class DefaultServerHealthCheck : IServerHealthCheck
    {
        private readonly IAppointAddressInvoker _appointAddressInvoker;

        public DefaultServerHealthCheck(IAppointAddressInvoker appointAddressInvoker)
        {
            _appointAddressInvoker = appointAddressInvoker;
        }

        private Task<bool> IsHealth(string address)
        {
            try
            {
                return
                    _appointAddressInvoker.Invoke<bool>(address, HealthCheckConstants.HealthCheckServiceEntryId, Array.Empty<object>());
            }
            catch (Exception e)
            {
                return Task.FromResult<bool>(false);
            }
        }

        public Task<bool> IsHealth(IRpcEndpoint rpcEndpoint)
        {
            var address = rpcEndpoint.GetAddress();
            if (rpcEndpoint.ServiceProtocol == ServiceProtocol.Tcp)
            {
                return IsHealth(address);
            }

            return Task.FromResult<bool>(SocketCheck.TestConnection(rpcEndpoint.Host, rpcEndpoint.Port));
        }

        public Task<bool> IsHealth(RpcEndpointDescriptor rpcEndpointDescriptor)
        {
            var address = rpcEndpointDescriptor.GetHostAddress();
            if (rpcEndpointDescriptor.ServiceProtocol == ServiceProtocol.Tcp)
            {
                return IsHealth(address);
            }

            if (rpcEndpointDescriptor.ServiceProtocol.IsHttp())
            {
                return Task.FromResult<bool>(UrlCheck.UrlIsValid($"{rpcEndpointDescriptor.ServiceProtocol}://{address}",
                    out var _));
            }

            return Task.FromResult<bool>(SocketCheck.TestConnection(rpcEndpointDescriptor.Host,
                rpcEndpointDescriptor.Port));
        }
    }
}