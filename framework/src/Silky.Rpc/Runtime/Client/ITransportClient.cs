using System.Threading;
using System.Threading.Tasks;
using Silky.Rpc.Messages;

namespace Silky.Rpc.Runtime.Client
{
    public interface ITransportClient
    {
        Task<RemoteResultMessage> SendAsync(RemoteInvokeMessage message, int timeout = Timeout.Infinite);
    }
}