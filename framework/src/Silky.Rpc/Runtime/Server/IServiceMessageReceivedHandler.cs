using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Messages;
using Silky.Rpc.Transport;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServiceMessageReceivedHandler : IScopedDependency
    {
        Task Handle(string messageId, IMessageSender sender, RemoteInvokeMessage message);
    }
}