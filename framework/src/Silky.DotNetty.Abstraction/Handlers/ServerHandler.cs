using System;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Silky.Rpc.Transport.Messages;

namespace Silky.DotNetty.Handlers
{
    public class ServerHandler : ChannelHandlerAdapter
    {
        private readonly Func<IChannelHandlerContext, TransportMessage, Task> _readMessageAction;

        public ServerHandler(Func<IChannelHandlerContext, TransportMessage, Task> readMessageAction)
        {
            _readMessageAction = readMessageAction;
        }

        public override async void ChannelRead(IChannelHandlerContext context, object message)
        {
            var transportMessage = (TransportMessage)message;
            await _readMessageAction(context, transportMessage);
        }
    }
}