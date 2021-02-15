using System.Runtime.CompilerServices;
using Lms.Codec.Message;
using Lms.Rpc.Messages;
using Lms.Rpc.Transport.Codec;

namespace Lms.Codec
{
    public class MessagePackTransportMessageEncoder : ITransportMessageEncoder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] Encode(TransportMessage message)
        {
            var transportMessage = new MessagePackTransportMessage(message);
            return SerializerUtilitys.Serialize(transportMessage);
        }
    }
}