using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Endpoint;

namespace Silky.Rpc.Transport.Messages
{
    public static class RemoteInvokeMessageExtensions
    {
        public static void SetRpcAttachments(this RemoteInvokeMessage message, bool isGateway = false)
        {
            foreach (var messageAttachment in message.Attachments)
            {
                if (messageAttachment.Key.Equals(AttachmentKeys.SelectedServerEndpoint)
                    || messageAttachment.Key.Equals(AttachmentKeys.SelectedServerHost)
                    || messageAttachment.Key.Equals(AttachmentKeys.SelectedServerPort)
                    || messageAttachment.Key.Equals(AttachmentKeys.SelectedServerServiceProtocol)
                )
                {
                    continue;
                }

                RpcContext.Context.SetInvokeAttachment(messageAttachment.Key, messageAttachment.Value);
            }

            var localRpcEndpoint = RpcEndpointHelper.GetLocalTcpEndpoint();
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.LocalAddress, localRpcEndpoint.Host);
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.LocalPort, localRpcEndpoint.Port);
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.LocalServiceProtocol, localRpcEndpoint.ServiceProtocol);
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.IsGateway, isGateway);
        }
    }
}