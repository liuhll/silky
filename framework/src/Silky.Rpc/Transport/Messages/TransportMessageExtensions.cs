using Silky.Core;
using Silky.Core.Runtime.Rpc;
using Silky.Rpc.Endpoint;
using Silky.Rpc.Transport.Codec;

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

        public static byte[] Encode(this TransportMessage message)
        {
            var transportMessageEncoder = EngineContext.Current.Resolve<ITransportMessageEncoder>();
            return transportMessageEncoder.Encode(message);
        }

        public static void SetRpcMessageId(this TransportMessage message)
        {
            RpcContext.Context.SetInvokeAttachment(AttachmentKeys.MessageId, message.Id);
        }
        
    }
}