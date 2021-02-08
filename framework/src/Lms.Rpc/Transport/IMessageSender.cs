using System.Threading.Tasks;
using Lms.Rpc.Messages;

namespace Lms.Rpc.Transport
{
    public interface IMessageSender
    {
        Task SendAsync(TransportMessage message);
        
        Task SendAndFlushAsync(TransportMessage message);
    }
}