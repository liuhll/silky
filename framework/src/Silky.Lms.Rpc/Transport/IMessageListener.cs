using System.Threading.Tasks;
using Silky.Lms.Rpc.Messages;

namespace Silky.Lms.Rpc.Transport
{
    public interface IMessageListener
    {
        event ReceivedDelegate Received;

        Task OnReceived(IMessageSender sender, TransportMessage message);
    }
}