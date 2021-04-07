using System.Threading.Tasks;
using Silky.Lms.Rpc.Messages;

namespace Silky.Lms.Rpc.Transport
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