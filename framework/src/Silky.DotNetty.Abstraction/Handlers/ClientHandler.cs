using System.Net;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Silky.Rpc.Endpoint.Monitor;
using Silky.Rpc.Runtime;
using Silky.Rpc.Transport.Messages;

namespace Silky.DotNetty.Handlers
{
    public class ClientHandler : ChannelHandlerAdapter
    {
        private readonly IMessageListener _messageListener;
        private readonly IRpcEndpointMonitor _rpcEndpointMonitor;

        public ClientHandler(IMessageListener messageListener,
            IRpcEndpointMonitor rpcEndpointMonitor)
        {
            _messageListener = messageListener;
            _rpcEndpointMonitor = rpcEndpointMonitor;
        }

        public override async void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is TransportMessage transportMessage)
            {
                if (transportMessage.IsResultMessage())
                {
                    await _messageListener.OnReceived(null, transportMessage);
                }
            }
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            if (context.Channel.RemoteAddress is IPEndPoint remoteAddress)
            {
                _rpcEndpointMonitor.RemoveRpcEndpoint(remoteAddress.Address.MapToIPv4(), remoteAddress.Port);
            }
        }
    }
}