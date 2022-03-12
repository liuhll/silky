using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Silky.Rpc.Transport.Codec;
using Silky.Rpc.Transport.Messages;

namespace Silky.DotNetty.Handlers;

public class EncoderHandler : MessageToByteEncoder<TransportMessage>
{
    private readonly ITransportMessageEncoder _transportMessageEncoder;

    public EncoderHandler(ITransportMessageEncoder transportMessageEncoder)
    {
        _transportMessageEncoder = transportMessageEncoder;
    }

    protected override void Encode(IChannelHandlerContext context, TransportMessage message, IByteBuffer output)
    {
        var bodyArray = _transportMessageEncoder.Encode(message);
        output.WriteBytes(bodyArray);
    }
}