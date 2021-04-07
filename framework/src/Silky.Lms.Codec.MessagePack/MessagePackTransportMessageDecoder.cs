using System.Runtime.CompilerServices;
using Silky.Lms.Codec.Message;
using Silky.Lms.Rpc.Messages;
using Silky.Lms.Rpc.Transport.Codec;

namespace Silky.Lms.Codec
{
    public class MessagePackTransportMessageDecoder : ITransportMessageDecoder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TransportMessage Decode(byte[] data)
        {
            var message = SerializerUtilitys.Deserialize<MessagePackTransportMessage>(data);
            return message.GetTransportMessage();
        }
    }
}