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

        public Task<bool> IsHealth(ISilkyEndpoint silkyEndpoint)
        {
            var address = silkyEndpoint.GetAddress();
            if (silkyEndpoint.ServiceProtocol == ServiceProtocol.Rpc)
            {
                return IsHealth(address);
            }

            return Task.FromResult<bool>(SocketCheck.TestConnection(silkyEndpoint.Host, silkyEndpoint.Port));
        }

        public Task<bool> IsHealth(SilkyEndpointDescriptor silkyEndpointDescriptor)
        {
            var address = silkyEndpointDescriptor.GetAddress();
            if (silkyEndpointDescriptor.ServiceProtocol == ServiceProtocol.Rpc)
            {
                return IsHealth(address);
            }

            if (silkyEndpointDescriptor.ServiceProtocol.IsHttp())
            {
                return Task.FromResult<bool>(UrlCheck.UrlIsValid($"{silkyEndpointDescriptor.ServiceProtocol}://{address}",
                    out var _));
            }

            return Task.FromResult<bool>(SocketCheck.TestConnection(silkyEndpointDescriptor.Host,
                silkyEndpointDescriptor.Port));
        }
    }
}