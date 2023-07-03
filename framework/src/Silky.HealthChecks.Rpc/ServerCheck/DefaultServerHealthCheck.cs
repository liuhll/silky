using System;
using System.Net.Http;
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
                    await _appointAddressInvoker.Invoke<bool>(address, Utils.GetHealthCheckServiceEntryId(),
                        Array.Empty<object>());
            }
            catch (Exception e)
            {
                _logger.LogWarning("Address[{0}] UnHealth, exception message {1}", address, e.Message);
                return false;
            }
        }

        public async Task<bool> IsHealth(ISilkyEndpoint silkyEndpoint)
        {
            var address = silkyEndpoint.GetAddress();
            if (silkyEndpoint.ServiceProtocol == ServiceProtocol.Rpc)
            {
                return await IsHealth(address);
            }

            if (silkyEndpoint.ServiceProtocol.IsHttp())
            {
                if (await IsHealthGateway(silkyEndpoint.ToString()))
                {
                    return true;
                }
            }

            return SocketCheck.TestConnection(silkyEndpoint.Host, silkyEndpoint.Port);
        }

        private async Task<bool> IsHealthGateway(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(url);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                await httpClient.GetAsync("/api/silky/health");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"address is unhealth, reason:{e.Message}");
                return false;
            }
        }

        public async Task<bool> IsHealth(SilkyEndpointDescriptor silkyEndpointDescriptor)
        {
            var address = silkyEndpointDescriptor.GetAddress();
            if (silkyEndpointDescriptor.ServiceProtocol == ServiceProtocol.Rpc)
            {
                return await IsHealth(address);
            }

            if (silkyEndpointDescriptor.ServiceProtocol.IsHttp())
            {
                if (await IsHealthGateway(silkyEndpointDescriptor.ToString()))
                {
                    return true;
                }
            }

            return SocketCheck.TestConnection(silkyEndpointDescriptor.Host,
                silkyEndpointDescriptor.Port);
        }
    }
}