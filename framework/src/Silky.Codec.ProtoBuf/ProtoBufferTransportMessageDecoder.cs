using System.Runtime.CompilerServices;
using Silky.Codec.Message;
using Silky.Rpc.Messages;
using Silky.Rpc.Transport.Codec;

namespace Silky.Codec
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