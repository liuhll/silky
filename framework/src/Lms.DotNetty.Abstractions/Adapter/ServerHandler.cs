using System;
using DotNetty.Transport.Channels;
using Lms.Rpc.Messages;

namespace Lms.DotNetty.Adapter
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

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            base.ChannelInactive(context);
        }
    }
}