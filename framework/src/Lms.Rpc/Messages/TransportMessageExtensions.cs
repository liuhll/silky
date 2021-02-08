using Lms.Core;
using Lms.Rpc.Transport;

namespace Lms.Rpc.Messages
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