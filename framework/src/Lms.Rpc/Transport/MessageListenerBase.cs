using System.Threading.Tasks;
using Lms.Rpc.Messages;

namespace Lms.Rpc.Transport
{
    public abstract class MessageListenerBase : IMessageListener
    {
        public event ReceivedDelegate Received;
        
        public abstract Task Listen();
        
        public Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            return Received == null ? Task.CompletedTask : Received(sender, message);
        }
    }
}