using System.Threading.Tasks;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime
{
    public delegate Task ReceivedDelegate(IMessageSender sender, TransportMessage message);
}