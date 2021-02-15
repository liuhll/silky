using System.Runtime.CompilerServices;
using Lms.Codec.Message;
using Lms.Rpc.Messages;
using Lms.Rpc.Transport.Codec;

namespace Lms.Codec
{
    public class ProtoBufferTransportMessageDecoder : ITransportMessageDecoder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TransportMessage Decode(byte[] data)
        {
            var message = SerializerUtilitys.Deserialize<ProtoBufTransportMessage>(data);
            return message.GetTransportMessage();
        }
    }
}