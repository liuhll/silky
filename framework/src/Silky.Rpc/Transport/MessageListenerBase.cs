using System.Threading.Tasks;
using Silky.Rpc.Messages;

namespace Silky.Rpc.Transport
{
    public abstract class MessageListenerBase : IMessageListener
    {
        public event ReceivedDelegate Received;
        
        
        public Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            return Received == null ? Task.CompletedTask : Received(sender, message);
        }
    }
}