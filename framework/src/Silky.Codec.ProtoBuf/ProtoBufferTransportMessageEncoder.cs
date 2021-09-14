using System.Runtime.CompilerServices;
using Silky.Codec.Message;
using Silky.Rpc.Transport.Codec;
using Silky.Rpc.Transport.Messages;

namespace Silky.Codec
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