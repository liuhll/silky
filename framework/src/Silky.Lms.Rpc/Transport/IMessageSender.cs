using System.Threading.Tasks;
using Silky.Lms.Rpc.Messages;

namespace Silky.Lms.Rpc.Transport
{
    public interface IMessageSender
    {
        Task SendAsync(TransportMessage message);
        
        Task SendAndFlushAsync(TransportMessage message);
    }
}