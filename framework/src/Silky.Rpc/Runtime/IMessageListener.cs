using System.Threading.Tasks;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime
{
    public interface IMessageListener
    {
        event ReceivedDelegate Received;

        Task OnReceived(IMessageSender sender, TransportMessage message);
    }
}