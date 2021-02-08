using System.Threading;
using System.Threading.Tasks;
using Lms.Rpc.Messages;

namespace Lms.Rpc.Transport
{
    public interface ITransportClient
    {
        Task<RemoteResultMessage> SendAsync(RemoteInvokeMessage message, int timeout);
    }
}