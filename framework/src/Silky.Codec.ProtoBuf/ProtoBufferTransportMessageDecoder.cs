using System.Runtime.CompilerServices;
using Silky.Codec.Message;
using Silky.Rpc.Transport.Codec;
using Silky.Rpc.Transport.Messages;

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