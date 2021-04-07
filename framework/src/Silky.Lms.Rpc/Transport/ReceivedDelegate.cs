using System.Threading.Tasks;
using Silky.Lms.Rpc.Messages;

namespace Silky.Lms.Rpc.Transport
{
    public delegate Task ReceivedDelegate(IMessageSender sender, TransportMessage message);
}