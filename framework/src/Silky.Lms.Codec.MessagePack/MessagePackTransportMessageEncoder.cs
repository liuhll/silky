using System.Runtime.CompilerServices;
using Silky.Lms.Codec.Message;
using Silky.Lms.Rpc.Messages;
using Silky.Lms.Rpc.Transport.Codec;

namespace Silky.Lms.Codec
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