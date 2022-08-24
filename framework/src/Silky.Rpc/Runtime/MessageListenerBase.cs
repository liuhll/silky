using System.Threading.Tasks;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime
{
    public abstract class MessageListenerBase : IMessageListener
    {
        public event ReceivedDelegate Received;


        public async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received != null)
            {
                await Received(sender, message);
            }
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