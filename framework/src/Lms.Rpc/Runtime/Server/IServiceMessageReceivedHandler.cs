using System.Threading.Tasks;
using Lms.Core.DependencyInjection;
using Lms.Rpc.Messages;
using Lms.Rpc.Transport;

namespace Lms.Rpc.Runtime.Server
{
    public interface IServiceMessageReceivedHandler : IScopedDependency
    {
        Task Handle(string messageId, IMessageSender sender, RemoteInvokeMessage message);
    }
}