using System;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Silky.Core;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.Rpc.Transport.Messages;

namespace Silky.DotNetty.Handlers
{
    public class ServerHandler : ChannelHandlerAdapter
    {
        private readonly Func<IChannelHandlerContext, TransportMessage, Task> _readMessageAction;

        public ILogger<ServerHandler> Logger { get; set; }

        public ServerHandler(Func<IChannelHandlerContext, TransportMessage, Task> readMessageAction)
        {
            _readMessageAction = readMessageAction;
            Logger = EngineContext.Current.Resolve<ILogger<ServerHandler>>();
        }

        public override async void ChannelRead(IChannelHandlerContext context, object message)
        {
            try
            {
                var transportMessage = (TransportMessage)message;
                await _readMessageAction(context, transportMessage);
            }
            catch (Exception e)
            {
                Logger.LogException(e);
            }
        }
    }
}