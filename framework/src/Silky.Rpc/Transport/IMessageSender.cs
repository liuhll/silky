using System.Threading.Tasks;
using Silky.Rpc.Messages;

namespace Silky.Rpc.Transport
{
    public interface IMessageSender
    {
        Task SendAsync(TransportMessage message);
        
        Task SendAndFlushAsync(TransportMessage message);
    }
}