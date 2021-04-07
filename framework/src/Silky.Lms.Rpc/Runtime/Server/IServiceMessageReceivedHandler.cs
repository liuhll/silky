using System.Threading.Tasks;
using Silky.Lms.Core.DependencyInjection;
using Silky.Lms.Rpc.Messages;
using Silky.Lms.Rpc.Transport;

namespace Silky.Lms.Rpc.Runtime.Server
{
    public interface IServiceMessageReceivedHandler : IScopedDependency
    {
        Task Handle(string messageId, IMessageSender sender, RemoteInvokeMessage message);
    }
}