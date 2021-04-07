using System.Runtime.CompilerServices;
using Silky.Lms.Codec.Message;
using Silky.Lms.Rpc.Messages;
using Silky.Lms.Rpc.Transport.Codec;

namespace Silky.Lms.Codec
{
    public class ProtoBufferTransportMessageEncoder : ITransportMessageEncoder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] Encode(TransportMessage message)
        {
            var transportMessage = new ProtoBufTransportMessage(message);
            return SerializerUtilitys.Serialize(transportMessage);
        }
    }
}