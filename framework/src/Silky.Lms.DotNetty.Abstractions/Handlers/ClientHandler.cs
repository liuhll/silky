using System.Net;
using DotNetty.Transport.Channels;
using Silky.Lms.Rpc.Address.HealthCheck;
using Silky.Lms.Rpc.Messages;
using Silky.Lms.Rpc.Transport;

namespace Silky.Lms.DotNetty.Handlers
{
    public class ClientHandler : ChannelHandlerAdapter
    {
        private readonly IMessageListener _messageListener;
        private readonly IHealthCheck _healthCheck;

        public ClientHandler(IMessageListener messageListener, IHealthCheck healthCheck)
        {
            _messageListener = messageListener;
            _healthCheck = healthCheck;
        }

        public async override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var transportMessage = (TransportMessage)message;
            await _messageListener.OnReceived(null, transportMessage);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            var remoteServerEndpoint = context.Channel.RemoteAddress as IPEndPoint;
            if (remoteServerEndpoint != null)
            {
                _healthCheck.RemoveAddress(remoteServerEndpoint.Address.MapToIPv4(), remoteServerEndpoint.Port);
            }
        }
    }
}