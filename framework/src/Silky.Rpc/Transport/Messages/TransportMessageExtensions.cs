using Silky.Core.Runtime.Rpc;

namespace Silky.Rpc.Transport.Messages
{
    public static class TransportMessageExtensions
    {
        public static bool IsInvokeMessage(this TransportMessage message)
        {
            return message.ContentType == TransportMessageType.RemoteInvokeMessage;
        }

        public static bool IsResultMessage(this TransportMessage message)
        {
            return message.ContentType == TransportMessageType.RemoteResultMessage;
        }
        
        public static void SetRpcMessageId(this TransportMessage message)
        {
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.MessageId, message.Id);
        }
        
    }
}