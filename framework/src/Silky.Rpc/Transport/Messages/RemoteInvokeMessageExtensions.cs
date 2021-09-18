using Silky.Core.Rpc;

namespace Silky.Rpc.Transport.Messages
{
    public static class RemoteInvokeMessageExtensions
    {
        public static void SetRpcAttachments(this RemoteInvokeMessage message, bool isGateway = false)
        {
            foreach (var messageAttachment in message.Attachments)
            {
                if (!messageAttachment.Key.Equals(AttachmentKeys.SelectedServerEndpoint))
                {
                    RpcContext.Context.SetAttachment(messageAttachment.Key,messageAttachment.Value);
                }
            }
            RpcContext.Context.SetAttachment(AttachmentKeys.IsGateway, isGateway);
        }
    }
}