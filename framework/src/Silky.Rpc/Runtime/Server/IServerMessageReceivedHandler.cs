using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServerMessageReceivedHandler : IScopedDependency
    {
        Task Handle(string messageId, IMessageSender sender, RemoteInvokeMessage message);
    }
}