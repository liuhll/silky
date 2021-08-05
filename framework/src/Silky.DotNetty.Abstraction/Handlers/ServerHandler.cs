using System;
using System.Net;
using DotNetty.Transport.Channels;
using Silky.Rpc.Address.HealthCheck;
using Silky.Rpc.Messages;

namespace Silky.DotNetty.Handlers
{
    public class ServerHandler : ChannelHandlerAdapter
    {
        private readonly Action<IChannelHandlerContext, TransportMessage> _readMessageAction;
        private readonly IHealthCheck _healthCheck;
        public ServerHandler(Action<IChannelHandlerContext, TransportMessage> readMessageAction,
            IHealthCheck healthCheck)
        {
            _readMessageAction = readMessageAction;
            _healthCheck = healthCheck;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var transportMessage = (TransportMessage)message;
            _readMessageAction(context, transportMessage);
        }
        
    }
}