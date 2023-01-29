using System.Threading.Tasks;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime
{
    public abstract class MessageListenerBase : IMessageListener
    {
        public event ReceivedDelegate Received;


        public Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received != null)
            {
                return Received(sender, message);
            }
            return Task.CompletedTask;
        }
        
    }
}