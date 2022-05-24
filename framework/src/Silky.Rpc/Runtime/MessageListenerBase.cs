using System.Threading;
using System.Threading.Tasks;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime
{
    public abstract class MessageListenerBase : IMessageListener
    {
        public event ReceivedDelegate Received;


        public Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            ThreadPool.QueueUserWorkItem(CallBack, new { Sender = sender, Message = message });
            return Task.CompletedTask;
        }

        private async void CallBack(object state)
        {
            dynamic dy = state;
            if (Received != null)
            {
                await Received((IMessageSender)dy.Sender, (TransportMessage)dy.Message);
            }
        }
    }
}