using Silky.Core.Extensions;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Endpoint;

namespace Silky.MassTransit.Extensions;

public static class RpcContextExtensions
{
    public static void SetMqInvokeAddressInfo(this RpcContext rpcContext)
    {
        var localRpcEndpointDescriptor = SilkyEndpointHelper.GetLocalRpcEndpoint();
        rpcContext.SetInvokeAttachment(AttachmentKeys.ClientHost, localRpcEndpointDescriptor.Host);
        rpcContext.SetInvokeAttachment(AttachmentKeys.ClientServiceProtocol, localRpcEndpointDescriptor.ServiceProtocol.ToString());
        rpcContext.SetInvokeAttachment(AttachmentKeys.ClientPort, localRpcEndpointDescriptor.Port.ToString());
        if (RpcContext.Context.GetLocalHost().IsNullOrEmpty())
        {
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.LocalAddress, localRpcEndpointDescriptor.Host);
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.LocalPort, localRpcEndpointDescriptor.Port.ToString());
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.LocalServiceProtocol,
                localRpcEndpointDescriptor.ServiceProtocol.ToString());
        }
    }
}