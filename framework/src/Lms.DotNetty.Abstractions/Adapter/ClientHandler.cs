using DotNetty.Transport.Channels;
using Lms.Rpc.Messages;
using Lms.Rpc.Transport;

namespace Lms.DotNetty.Adapter
{
    public class ClientHandler : ChannelHandlerAdapter
    {
        private readonly IMessageListener _messageListener;
        private readonly IMessageSender _messageSender;
        public ClientHandler(IMessageListener messageListener, IMessageSender messageSender)
        {
            _messageListener = messageListener;
            _messageSender = messageSender;
        }

        public async override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var transportMessage = (TransportMessage)message;
            await _messageListener.OnReceived(_messageSender, transportMessage);
        }
    }
}