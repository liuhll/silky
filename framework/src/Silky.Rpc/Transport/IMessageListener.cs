using System.Threading.Tasks;
using Silky.Rpc.Messages;

namespace Silky.Rpc.Transport
{
    public interface IMessageListener
    {
        event ReceivedDelegate Received;

        Task OnReceived(IMessageSender sender, TransportMessage message);
    }
}