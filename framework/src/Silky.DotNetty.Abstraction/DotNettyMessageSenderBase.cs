using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Silky.Core.Exceptions;
using Silky.Core.Logging;
using Silky.Rpc.Runtime;
using Silky.Rpc.Transport.Messages;

namespace Silky.DotNetty.Abstraction
{
    public abstract class DotNettyMessageSenderBase : IMessageSender
    {
        public ILogger<DotNettyMessageSenderBase> Logger { get; set; }

        protected DotNettyMessageSenderBase()
        {
            Logger = NullLogger<DotNettyMessageSenderBase>.Instance;
        }

        public virtual async Task SendMessageAsync(TransportMessage message, bool flush)
        {
            try
            {
                if (flush)
                {
                    await SendAndFlushAsync(message);
                }
                else
                {
                    await SendAsync(message);
                }
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                throw new CommunicationException(e.Message, e);
            }
        }

        protected abstract Task SendAsync(TransportMessage message);

        protected abstract Task SendAndFlushAsync(TransportMessage message);
    }
}