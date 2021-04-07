using Silky.Lms.Core;
using Silky.Lms.Rpc.Transport;
using Silky.Lms.Rpc.Transport.Codec;

namespace Silky.Lms.Rpc.Messages
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
    }
}