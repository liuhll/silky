using System.Threading.Tasks;
using Silky.Rpc.Messages;

namespace Silky.Rpc.Transport
{
    public delegate Task ReceivedDelegate(IMessageSender sender, TransportMessage message);
}