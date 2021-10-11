using System;
using DotNetty.Transport.Channels;
using Silky.Rpc.Transport.Messages;

namespace Silky.DotNetty.Handlers
{
    public class ServerHandler : ChannelHandlerAdapter
    {
        private readonly Action<IChannelHandlerContext, TransportMessage> _readMessageAction;

        public ServerHandler(Action<IChannelHandlerContext, TransportMessage> readMessageAction)
        {
            _readMessageAction = readMessageAction;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var transportMessage = (TransportMessage)message;
            _readMessageAction(context, transportMessage);
        }
    }
}