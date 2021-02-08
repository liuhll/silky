using System.Threading.Tasks;
using Lms.Rpc.Messages;

namespace Lms.Rpc.Transport
{
    public delegate Task ReceivedDelegate(IMessageSender sender, TransportMessage message);
}