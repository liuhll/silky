using System;
using System.Threading.Tasks;
using Silky.Core.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Runtime.Server;
using Silky.Rpc.Security;
using Silky.Rpc.Utils;

namespace Silky.HealthChecks.Rpc.ServerCheck
{
    public class DefaultServerHealthCheck : IServerHealthCheck
    {
        private const string healthCheckServiceEntryId = "Silky.Rpc.AppServices.IRpcAppService.IsHealth_Get";
        private readonly IServiceEntryLocator _serviceEntryLocator;
        private readonly ILocalExecutor _localExecutor;
        private readonly IRemoteExecutor _remoteExecutor;
   

        public DefaultServerHealthCheck(IServiceEntryLocator serviceEntryLocator,
            ILocalExecutor localExecutor,
            IRemoteExecutor remoteExecutor)
        {
            _serviceEntryLocator = serviceEntryLocator;
            _localExecutor = localExecutor;
            _remoteExecutor = remoteExecutor;
        }

        public Task<bool> IsHealth(string address)
        {
            var serviceEntry = _serviceEntryLocator.GetServiceEntryById(healthCheckServiceEntryId);
            try
            {
                return ServiceEntryExec<bool>(address, serviceEntry);
            }
            catch (Exception e)
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> IsHealth(IRpcEndpoint rpcEndpoint)
        {
            var address = rpcEndpoint.GetAddress();
            if (rpcEndpoint.ServiceProtocol == ServiceProtocol.Tcp)
            {
                return IsHealth(address);
            }

            if (rpcEndpoint.ServiceProtocol.IsHttp())
            {
                return Task.FromResult<bool>(UrlCheck.UrlIsValid($"{rpcEndpoint.ServiceProtocol}://{address}", out _));
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

        private bool IsLocalAddress(string address)
        {
            var localAddress = RpcEndpointHelper.GetLocalTcpEndpoint().GetAddress();
            return localAddress.Equals(address);
        }

        private async Task<T> ServiceEntryExec<T>(string address, ServiceEntry serviceEntry)
        {
            T result = default(T);
            if (IsLocalAddress(address))
            {
                result =
                    (T)await _localExecutor.Execute(serviceEntry, Array.Empty<object>(), null);
            }
            else
            {
                RpcContext.Context.SetAttachment(AttachmentKeys.SelectedServerEndpoint, address);
                result =
                    (T)await _remoteExecutor.Execute(serviceEntry, Array.Empty<object>(), null);
            }

            return result;
        }
    }
}