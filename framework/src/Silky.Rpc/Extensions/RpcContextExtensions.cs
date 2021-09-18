using Silky.Core.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Endpoint.Descriptor;

namespace Silky.Rpc.Extensions
{
    public static class RpcContextExtensions
    {
        public static void SetRcpInvokeAddressInfo(this RpcContext rpcContext, RpcEndpointDescriptor serverRpcEndpoint)
        {
            rpcContext
                .SetAttachment(AttachmentKeys.SelectedServerHost, serverRpcEndpoint.Host);
            rpcContext
                .SetAttachment(AttachmentKeys.SelectedServerPort, serverRpcEndpoint.Port.ToString());
            rpcContext
                .SetAttachment(AttachmentKeys.SelectedServerServiceProtocol,
                    serverRpcEndpoint.ServiceProtocol.ToString());


            var localRpcEndpointDescriptor = AddressHelper.GetLocalRpcEndpointDescriptor();
            rpcContext.SetAttachment(AttachmentKeys.ClientHost, localRpcEndpointDescriptor.Host);
            rpcContext.SetAttachment(AttachmentKeys.ClientServiceProtocol, localRpcEndpointDescriptor.ServiceProtocol);
            rpcContext.SetAttachment(AttachmentKeys.ClientPort, localRpcEndpointDescriptor.Port);
        }
        
    }
}