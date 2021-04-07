using System.Threading;
using System.Threading.Tasks;
using Silky.Lms.Rpc.Messages;

namespace Silky.Lms.Rpc.Transport
{
    public interface ITransportClient
    {
        Task<RemoteResultMessage> SendAsync(RemoteInvokeMessage message, int timeout = Timeout.Infinite);
    }
}