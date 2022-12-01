using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<DefaultServerHealthCheck> _logger;

        public DefaultServerHealthCheck(IAppointAddressInvoker appointAddressInvoker,
            ILogger<DefaultServerHealthCheck> logger)
        {
            _appointAddressInvoker = appointAddressInvoker;
            _logger = logger;
        }

        private async Task<bool> IsHealth(string address)
        {
            try
            {
                return
                    await _appointAddressInvoker.Invoke<bool>(address, HealthCheckConstants.HealthCheckServiceEntryId,
                        Array.Empty<object>());
            }
            catch (Exception e)
            {
                _logger.LogWarning("Address[{0}] UnHealth, exception messagess {1}", address, e.Message);
                return false;
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
                return Task.FromResult<bool>(UrlUtil.IsHealth(
                    $"{silkyEndpointDescriptor.ServiceProtocol}://{address}",
                    out var _));
            }

            return Task.FromResult<bool>(SocketCheck.TestConnection(silkyEndpointDescriptor.Host,
                silkyEndpointDescriptor.Port));
        }
    }
}