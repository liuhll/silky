using System.Threading;
using System.Threading.Tasks;
using Silky.Rpc.Runtime;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Transport
{
    public interface ITransportClient
    {
        Task<RemoteResultMessage> SendAsync(RemoteInvokeMessage message, string messageId,
            int timeout = Timeout.Infinite);

        IMessageSender MessageSender { get; set; }
    }
}