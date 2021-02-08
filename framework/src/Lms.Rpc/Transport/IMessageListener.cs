using System.Threading.Tasks;
using Lms.Rpc.Messages;

namespace Lms.Rpc.Transport
{
    public interface IMessageListener
    {
        Task Listen();
        Task OnReceived(IMessageSender sender, TransportMessage message);
    }
}