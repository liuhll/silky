using System.Threading;
using System.Threading.Tasks;
using Polly;
using Silky.Core.DependencyInjection;
using Silky.Rpc.Transport.Messages;

namespace Silky.Rpc.Runtime.Server
{
    public interface IServerMessageReceivedHandler : IScopedDependency
    {
        Task<RemoteResultMessage> Handle(RemoteInvokeMessage message, Context ctx, CancellationToken cancellationToken);
    }
}