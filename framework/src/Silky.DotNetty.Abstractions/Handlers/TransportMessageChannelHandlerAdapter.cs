using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Silky.Rpc.Transport.Codec;

namespace Silky.DotNetty.Handlers
{
    public class TransportMessageChannelHandlerAdapter : ChannelHandlerAdapter
    {
        private readonly ITransportMessageDecoder _transportMessageDecoder;

        public TransportMessageChannelHandlerAdapter(ITransportMessageDecoder transportMessageDecoder)
        {
            _transportMessageDecoder = transportMessageDecoder;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = (IByteBuffer)message;
            var data = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(data);
            var transportMessage = _transportMessageDecoder.Decode(data);
            context.FireChannelRead(transportMessage);
            ReferenceCountUtil.Release(buffer);

        }
    }
}