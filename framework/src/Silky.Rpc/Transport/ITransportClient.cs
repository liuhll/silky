using System.Threading;
using System.Threading.Tasks;
using Silky.Core.FilterMetadata;
using Silky.Rpc.Runtime.Client;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Transport
{
    public interface ITransportClient : IClientFilterMetadata
    {
        
        Task<RemoteResultMessage> SendAsync(RemoteInvokeMessage message, string messageId,
            int timeout = Timeout.Infinite);

        IClientMessageSender MessageSender { get; set; }
    }
}