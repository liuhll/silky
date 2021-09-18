using Silky.Core.Extensions;
using Silky.Core.Rpc;
using Silky.Rpc.Address;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Descriptor;
using Silky.Rpc.Runtime.Server;

namespace Silky.Rpc.Extensions
{
    public static class RpcContextExtensions
    {
        public static void SetRcpInvokeAddressInfo(this RpcContext rpcContext, RpcEndpointDescriptor serverRpcEndpoint,
            RpcEndpointDescriptor clientRpcEndpoint)
        {
            rpcContext
                .SetAttachment(AttachmentKeys.ServerAddress, serverRpcEndpoint.Address);
            rpcContext
                .SetAttachment(AttachmentKeys.ServerPort, serverRpcEndpoint.Port.ToString());
            rpcContext
                .SetAttachment(AttachmentKeys.ServerServiceProtocol, serverRpcEndpoint.ServiceProtocol.ToString());
            rpcContext.SetAttachment(AttachmentKeys.ClientAddress,
                clientRpcEndpoint.Address);
            rpcContext.SetAttachment(AttachmentKeys.ClientServiceProtocol,
                clientRpcEndpoint.ServiceProtocol.ToString());
        }

        public static ServiceProtocol GetServerServiceProtocol(this RpcContext rpcContext)
        {
            var serverServiceProtocol = rpcContext.GetAttachment(AttachmentKeys.ServerServiceProtocol);
            return serverServiceProtocol.To<ServiceProtocol>();
        }

        public static IRpcEndpoint GetSelectedServerRpcEndpoint(this RpcContext rpcContext)
        {
            var serverAddress = rpcContext.GetServerAddress();
            if (serverAddress == null)
            {
                return null;
            }
            var serviceProtocol = rpcContext.GetServerServiceProtocol();
            var serverPort = rpcContext.GetServerPort();

            return AddressHelper.CreateRpcEndpoint(serverAddress, serverPort, serviceProtocol);
        }
    }
}