using System.Collections.Generic;
using System.Linq;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Silky.Core.Extensions;
using Silky.DotNetty.Abstraction;
using Silky.Rpc.Transport.Codec;

namespace Silky.DotNetty.Handlers;

public class DecoderHandler : ByteToMessageDecoder
{
    private readonly ITransportMessageDecoder _transportMessageDecoder;

    public DecoderHandler(ITransportMessageDecoder transportMessageDecoder)
    {
        _transportMessageDecoder = transportMessageDecoder;
    }

    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
    {
        var data = new byte[input.ReadableBytes];
        input.ReadBytes(data);
        if (data.SequenceEqual(HeartBeat.Semaphore.GetBytes())) return;
        var message = _transportMessageDecoder.Decode(data);
        output.Add(message);
    }
}