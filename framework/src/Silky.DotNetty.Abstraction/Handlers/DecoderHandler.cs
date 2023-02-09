using System.Collections.Generic;
using System.Linq;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Silky.Core.Extensions;
using Silky.DotNetty.Abstraction;
using Silky.Rpc.Transport.Codec;

namespace Silky.DotNetty.Handlers;

public class DecoderHandler : ChannelHandlerAdapter
{
    private readonly ITransportMessageDecoder _transportMessageDecoder;

    public DecoderHandler(ITransportMessageDecoder transportMessageDecoder)
    {
        _transportMessageDecoder = transportMessageDecoder;
    }

    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        try
        {
            var buffer = (IByteBuffer)message;
            var data = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(data);
            if (data.SequenceEqual(HeartBeat.Semaphore.GetBytes())) return;
            var transportMessage = _transportMessageDecoder.Decode(data);
            context.FireChannelRead(transportMessage);
        }
        finally
        {
            ReferenceCountUtil.Release(message);
        }
    }
}