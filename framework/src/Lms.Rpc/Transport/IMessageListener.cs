using System.Threading.Tasks;
using Lms.Rpc.Messages;

namespace Lms.Rpc.Transport
{
    public interface IMessageListener
    {
        event ReceivedDelegate Received;

        Task OnReceived(IMessageSender sender, TransportMessage message);
    }
}